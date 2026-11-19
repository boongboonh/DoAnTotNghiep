using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Bot choi thu khu Bash trong Play Mode bang input that (phim R + vi tri chuot), de agent tu kiem tra.
/// Dung: AgentBashBot.Start() trong Play Mode, doi Done roi doc Log. Khong anh huong build.
/// </summary>
public static class AgentBashBot
{
    private enum Step { Setup, Settle, Press, WaitAim, Hold, Release, WaitDone, Next, Cross, Cross2, Cross3, Finished }

    private static Step step;
    private static double until;
    private static int index;
    private static readonly StringBuilder log = new StringBuilder();
    private static PlayerController player;
    private static PrototypeBash bash;
    private static PrototypeBashAnchor[] anchors;
    private static Vector2[] aimPoints;
    private static Vector2 aim;
    private static bool running;

    /// <summary>Do tre (giay that) giua hai lan Bash trong chuoi, de do cua so noi chuoi.</summary>
    public static double ChainDelay = 0;
    public static bool Done => !running;
    public static string Log => log.ToString();

    public static void Start()
    {
        player = Object.FindObjectOfType<PlayerController>();
        bash = player != null ? player.GetComponent<PrototypeBash>() : null;
        if (bash == null) { log.AppendLine("no player/bash"); return; }
        bash.Unlocked = true; // bot choi khong qua orb: mo trong bo nho, khong dong vao save
        anchors = Object.FindObjectsOfType<PrototypeBashAnchor>();
        System.Array.Sort(anchors, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        // Moi lan Bash nham vao diem neo ke tiep; lan cuoi nham xuong san ben kia ho.
        aimPoints = new Vector2[]
        {
            anchors[0].transform.position, anchors[1].transform.position,
            anchors[2].transform.position, new Vector2(52f, 1.5f)
        };
        log.Clear();
        index = 0;
        step = Step.Setup;
        running = true;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    // ---- Chay kich ban phim theo thoi gian that (do tam vuot ho khi chua co Bash) ----
    private static System.Collections.Generic.List<(double t, Key[] keys)> script;
    private static double scriptStart;
    private static int scriptIndex;
    private static float maxX, startX;
    private static Key[] heldKeys;

    public static void RunScript(Vector2 startPos, System.Collections.Generic.List<(double t, Key[] keys)> steps)
    {
        player = Object.FindObjectOfType<PlayerController>();
        log.Clear();
        player.ResetMotion(startPos);
        script = steps; scriptIndex = 0; heldKeys = null;
        scriptStart = EditorApplication.timeSinceStartup + 1.2;
        startX = startPos.x; maxX = startPos.x;
        running = true;
        EditorApplication.update -= ScriptTick;
        EditorApplication.update += ScriptTick;
    }

    private static void ScriptTick()
    {
        if (!EditorApplication.isPlaying || player == null) { running = false; EditorApplication.update -= ScriptTick; return; }
        double t = EditorApplication.timeSinceStartup - scriptStart;
        if (t < 0) return;
        // Giu phim hien tai moi lan update de ban phim that khong ghi de trang thai gia lap.
        if (heldKeys != null) InputSystem.QueueStateEvent(Keyboard.current, heldKeys.Length == 0 ? new KeyboardState() : new KeyboardState(heldKeys));
        maxX = Mathf.Max(maxX, player.Motor.Position.x);
        while (scriptIndex < script.Count && t >= script[scriptIndex].t)
        {
            var keys = script[scriptIndex].keys;
            heldKeys = keys;
            InputSystem.QueueStateEvent(Keyboard.current, keys.Length == 0 ? new KeyboardState() : new KeyboardState(keys));
            Report("t=" + script[scriptIndex].t + " keys=" + keys.Length + " pos=" + player.Motor.Position + " loco=" + player.Locomotion);
            scriptIndex++;
        }
        if (scriptIndex >= script.Count && t > script[script.Count - 1].t + 2.0)
        {
            heldKeys = null;
            Report("END maxX=" + maxX + " pos=" + player.Motor.Position + " dead=" + player.IsDead);
            running = false; EditorApplication.update -= ScriptTick;
        }
    }

    // ---- Che do phan dan: dung truoc cong, cho quai ban, Bash vien dan ve phia cong ----
    private static int shotPhase;
    private static double shotDeadline;
    private static PrototypeProjectile targetShot;
    private static PrototypeBashBarrier gate;

    public static void StartShot()
    {
        player = Object.FindObjectOfType<PlayerController>();
        bash = player.GetComponent<PrototypeBash>();
        gate = Object.FindObjectOfType<PrototypeBashBarrier>();
        bash.Unlocked = true;
        log.Clear();
        player.ResetMotion(new Vector2(55.5f, .55f));
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        shotPhase = 0;
        shotDeadline = EditorApplication.timeSinceStartup + 25;
        until = EditorApplication.timeSinceStartup + 1.0;
        running = true;
        EditorApplication.update -= ShotTick;
        EditorApplication.update += ShotTick;
    }

    private static void ShotTick()
    {
        if (!EditorApplication.isPlaying || player == null) { running = false; EditorApplication.update -= ShotTick; return; }
        double now = EditorApplication.timeSinceStartup;
        if (now > shotDeadline) { Report("TIMEOUT phase=" + shotPhase + " broken=" + gate.Broken); End(); return; }
        if (holdR) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.R));
        if (Waiting) return;
        switch (shotPhase)
        {
            case 0: // cho vien dan quai thuong trong tam
                foreach (var s in Object.FindObjectsOfType<PrototypeProjectile>())
                {
                    if (s.Bashed || s.Spent) continue;
                    if (Vector2.Distance(s.transform.position, player.Motor.Position) > 4.5f) continue;
                    targetShot = s;
                    Report("shot seen at " + s.transform.position + " dir=" + s.Direction + " player=" + player.Motor.Position
                        + " candidate=" + (bash.Candidate != null));
                    // Nham ra xa (trai-tren): dan bay nguoc lai ve phia cong.
                    SetMouseWorld(player.Motor.Position + new Vector2(-5f, 1.3f));
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.R));
                    shotPhase = 1; Wait(.12);
                    return;
                }
                break;
            case 1:
                Report("press: phase=" + bash.Phase + " loco=" + player.Locomotion + " dir=" + bash.AimDirection
                    + " count=" + bash.SuccessCount + " shotBashed=" + (targetShot != null && targetShot.Bashed)
                    + " rKey=" + Keyboard.current.rKey.isPressed + " unscaledDt=" + Time.unscaledDeltaTime);
                if (bash.Phase != PrototypeBash.BashPhase.Aim) { Report("FAIL no aim (shot dead or out of range?)"); shotPhase = 0; InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState()); Wait(.5); return; }
                SetMouseWorld(player.Motor.Position + new Vector2(-5f, 1.3f));
                shotPhase = 2; Wait(.2); break;
            case 2:
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                shotPhase = 3; Wait(.15); break;
            case 3:
                Report("released: shotBashed=" + (targetShot != null && targetShot.Bashed) + " faction="
                    + (targetShot != null ? targetShot.Faction.ToString() : "gone") + " count=" + bash.SuccessCount);
                shotPhase = 4; Wait(2.0); break;
            case 4:
                Report("result: gateBroken=" + gate.Broken + " shotSpent=" + (targetShot == null || targetShot.Spent)
                    + " player=" + player.Motor.Position + " scale=" + Time.timeScale);
                End(); break;
        }
    }

    private static void End()
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        running = false;
        EditorApplication.update -= ShotTick;
    }

    private static void Wait(double seconds) => until = EditorApplication.timeSinceStartup + seconds;
    private static bool Waiting => EditorApplication.timeSinceStartup < until;

    private static bool holdR;
    private static void SetKey(bool r)
    {
        holdR = r;
        var kb = Keyboard.current;
        InputSystem.QueueStateEvent(kb, r ? new KeyboardState(Key.R) : new KeyboardState());
    }

    private static void SetMouseWorld(Vector2 world)
    {
        var cam = Camera.main;
        Vector3 sp = cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));
        var state = new MouseState { position = new Vector2(sp.x, sp.y) };
        InputSystem.QueueStateEvent(Mouse.current, state);
    }

    private static void Report(string text) => log.AppendLine(Time.frameCount + " | " + text);

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || player == null) { running = false; EditorApplication.update -= Tick; return; }
        if (holdR) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.R));
        if (Waiting) return;
        switch (step)
        {
            case Step.Setup:
                SetKey(false);
                foreach (var a in anchors) a.ResetAnchor();
                player.ResetMotion(new Vector2(31f, .55f));
                Report("setup at 31,.55");
                step = Step.Settle; Wait(1.2); break;
            case Step.Settle:
                Report("settled pos=" + player.Motor.Position + " grounded=" + player.Motor.Grounded
                    + " candidate=" + (bash.Candidate != null));
                step = Step.Press; break;
            case Step.Press:
                aim = aimPoints[index];
                SetMouseWorld(aim);
                step = Step.WaitAim; Wait(.15);
                SetKey(true);
                break;
            case Step.WaitAim:
                Report("press #" + index + ": phase=" + bash.Phase + " loco=" + player.Locomotion
                    + " scale=" + Time.timeScale + " dir=" + bash.AimDirection + " pos=" + player.Motor.Position);
                if (bash.Phase != PrototypeBash.BashPhase.Aim) { Report("FAIL: did not enter aim"); step = Step.Finished; break; }
                SetMouseWorld(aim);
                step = Step.Hold; Wait(.35); break;
            case Step.Hold:
                Report("aiming: dir=" + bash.AimDirection + " vel=" + player.Motor.Velocity);
                SetKey(false);
                step = Step.Release; Wait(.1); break;
            case Step.Release:
                Report("released: phase=" + bash.Phase + " scale=" + Time.timeScale + " count=" + bash.SuccessCount
                    + " airDash=" + player.Motor.AirDashAvailable);
                step = Step.WaitDone; break;
            case Step.WaitDone:
                if (bash.Phase != PrototypeBash.BashPhase.None) break;
                Report("after bash #" + index + ": phase=" + bash.Phase + " pos=" + player.Motor.Position
                    + " vel=" + player.Motor.Velocity + " scale=" + Time.timeScale);
                step = Step.Next; break;
            case Step.Next:
                index++;
                if (index >= aimPoints.Length - 1) { step = Step.Cross; }
                else { step = Step.Press; Wait(ChainDelay); }
                break;
            case Step.Cross:
                // Het neo: air dash roi nhay doi qua nua ho con lai.
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.LeftCtrl, Key.D));
                Report("cross: dash from " + player.Motor.Position + " airDash=" + player.Motor.AirDashAvailable
                    + " airJump=" + player.Motor.AirJumpAvailable);
                step = Step.Cross2; Wait(.3); break;
            case Step.Cross2:
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Space, Key.D));
                Report("cross: air jump from " + player.Motor.Position + " loco=" + player.Locomotion);
                step = Step.Cross3; Wait(.12); break;
            case Step.Cross3:
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.D));
                step = Step.Finished; Wait(1.8); break;
            case Step.Finished:
                SetKey(false);
                Report("final pos=" + player.Motor.Position + " grounded=" + player.Motor.Grounded
                    + " dead=" + player.IsDead + " success=" + bash.SuccessCount + " scale=" + Time.timeScale);
                running = false;
                EditorApplication.update -= Tick;
                break;
        }
    }
}
