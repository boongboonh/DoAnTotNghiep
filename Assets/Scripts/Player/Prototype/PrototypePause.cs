using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pause prototype (PAU-01): Esc dung gameplay (timeScale 0) va timer; menu IMGUI: tiep tuc, ve checkpoint, choi lai toan tuyen.
/// Lenh bam trong luc pause khong duoc ghi (PlayerInputReader tu bo khi timeScale 0) va buffer bi xoa khi tiep tuc.
/// </summary>
public sealed class PrototypePause : MonoBehaviour
{
    private PrototypePlayground arena;
    private PlayerController player;
    private bool restartArmed;
    private float resumeScale = 1f;
    public bool Paused { get; private set; }

    public void Initialize(PrototypePlayground owner, PlayerController controller)
    {
        arena = owner; player = controller;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (!(Application.isFocused || PlayerInputReader.IgnoreFocusLoss)) return;
        bool toggle = (kb != null && kb.escapeKey.wasPressedThisFrame) || player.Controls.PausePressed;
        if (!toggle && Paused && Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) toggle = true;
        if (toggle) { if (Paused) Resume(); else Pause(); }
    }

    public void Pause()
    {
        if (Paused || arena == null) return;
        // Huy ngam Bash va hitstop truoc de tra dung time scale, roi moi dung hang.
        if (player.Bash != null) player.Bash.Cancel();
        var stop = arena.GetComponent<DangDang.Combat.HitStop>();
        if (stop != null) stop.Release();
        resumeScale = Mathf.Max(.01f, DangDang.Combat.HitStop.BaseScale);
        player.Controls.ResetInput();
        Time.timeScale = 0f;
        Paused = true;
        restartArmed = false;
    }

    public void Resume()
    {
        if (!Paused) return;
        Paused = false;
        restartArmed = false;
        Time.timeScale = resumeScale;
        // Bo moi thu da bam trong luc o menu.
        player.Controls.ResetInput();
    }

    private void OnDisable()
    {
        if (Paused) Resume();
    }

    private void OnGUI()
    {
        if (!Paused) return;
        float s = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float w = Screen.width / s, h = Screen.height / s;
        var old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, .6f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = old;
        var title = new GUIStyle(GUI.skin.label) { fontSize = 56, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        var button = new GUIStyle(GUI.skin.button) { fontSize = 34 };
        var note = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(w / 2f - 400, h / 2f - 250, 800, 90), "PAUSED", title);
        float x = w / 2f - 300;
        if (GUI.Button(new Rect(x, h / 2f - 120, 600, 80), "Continue (Esc)", button)) Resume();
        if (GUI.Button(new Rect(x, h / 2f - 20, 600, 80), "Back to checkpoint", button))
        {
            Resume();
            arena.ResetRun();
        }
        string restart = restartArmed ? "Click again: RESET ALL PROTOTYPE PROGRESS" : "Restart route (resets progress)";
        if (GUI.Button(new Rect(x, h / 2f + 80, 600, 80), restart, button))
        {
            if (!restartArmed) restartArmed = true;
            else
            {
                Resume();
                arena.RestartRun();
            }
        }
        GUI.Label(new Rect(x, h / 2f + 180, 600, 60),
            "Restart clears Bash, items, doors and checkpoint of the prototype save.", note);
    }
}
