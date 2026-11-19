using System.Text;
using UnityEngine;

/// <summary>
/// HUD prototype (HUD-01) va debug overlay (DBG-01, F3), ve bang IMGUI, khong can Canvas/package.
/// Debug chi doc trang thai, khong tieu thu input gameplay va khong ghi tien trinh.
/// </summary>
public sealed class PrototypeHud : MonoBehaviour
{
    private PrototypePlayground arena;
    private PlayerController player;
    private PrototypePlayerHealth health;
    private PrototypeProgress progress;
    private PrototypeTerrain terrain;
    private PrototypeCombatRoom room;
    private float cacheAt;
    private PrototypeMeleeEnemy[] melee = new PrototypeMeleeEnemy[0];
    private PrototypeSpecialEnemy[] special = new PrototypeSpecialEnemy[0];
    private GUIStyle label, big, small;
    private readonly StringBuilder text = new StringBuilder();

    public void Initialize(PrototypePlayground owner, PlayerController controller, PrototypePlayerHealth vitality,
        PrototypeProgress state, PrototypeTerrain ground, PrototypeCombatRoom combat)
    {
        arena = owner; player = controller; health = vitality; progress = state; terrain = ground; room = combat;
    }

    private void EnsureStyles()
    {
        if (label != null) return;
        label = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
        label.normal.textColor = Color.white;
        big = new GUIStyle(label) { fontSize = 60, alignment = TextAnchor.MiddleCenter };
        small = new GUIStyle(GUI.skin.label) { fontSize = 22 };
        small.normal.textColor = new Color(.85f, .85f, .85f);
    }

    private static void Box(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    private static string Clock(float t) => ((int)(t / 60f)).ToString("00") + ":" + ((int)(t % 60f)).ToString("00");

    private void OnGUI()
    {
        if (arena == null || player == null) return;
        EnsureStyles();
        float s = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float w = Screen.width / s, h = Screen.height / s;

        // Nen toi de doc ro tren moi phong nen va nhan the gioi.
        Box(new Rect(12, 8, 760, 232), new Color(0f, 0f, 0f, .55f));
        Box(new Rect(w - 312, 8, 300, 50), new Color(0f, 0f, 0f, .55f));
        Box(new Rect(12, h - 52, w - 24, 44), new Color(0f, 0f, 0f, .55f));

        // --- HP ---
        int max = health.Maximum, cur = health.Health;
        GUI.Label(new Rect(24, 16, 300, 40), "HP", label);
        for (int i = 0; i < max; i++) Box(new Rect(90 + i * 44, 24, 38, 26), i < cur ? new Color(.9f, .2f, .25f) : new Color(.25f, .25f, .25f));

        // --- Luot / nang luc ---
        bool dashReady = player.Motor.Grounded || player.Motor.AirDashAvailable;
        GUI.Label(new Rect(24, 60, 500, 36), "AIR DASH: " + (dashReady ? "READY" : "USED"), label);
        Box(new Rect(24, 96, 20, 20), dashReady ? Color.cyan : Color.gray);
        string bashState = !progress.BashAvailable ? "LOCKED"
            : player.Bash != null && player.Bash.Phase == PrototypeBash.BashPhase.Aim ? "AIMING"
            : player.Bash != null && player.Bash.Candidate != null ? "READY (hold R)" : "UNLOCKED";
        GUI.Label(new Rect(24, 120, 500, 36), "BASH: " + bashState, label);
        GUI.Label(new Rect(24, 156, 500, 36), "CHECKPOINT: " + (terrain != null && terrain.CheckpointActive ? "ACTIVE" : "NONE"), label);

        // --- Nang luong Spirit Arc: 3 o, moi o 4 vach (1 vach = 1 phat ban) ---
        var arcSystem = arena.Arc;
        if (arcSystem != null)
        {
            Box(new Rect(w - 312, 62, 300, 54), new Color(0f, 0f, 0f, .55f));
            var energyLabel = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(w - 306, 66, 120, 24), arcSystem.Aiming ? "ARC AIM" : "ARC  Q/LT", energyLabel);
            for (int cell = 0; cell < 3; cell++)
            {
                float fill = Mathf.Clamp01(arcSystem.Energy - cell);
                float cx = w - 300 + cell * 96;
                Box(new Rect(cx, 92, 90, 18), new Color(.15f, .15f, .15f));
                Box(new Rect(cx, 92, 90 * fill, 18), arcSystem.Aiming ? new Color(1f, .9f, .45f) : new Color(.35f, .85f, 1f));
                for (int q = 1; q < 4; q++) Box(new Rect(cx + 90 * q / 4f - 1, 92, 2, 18), new Color(0f, 0f, 0f, .6f));
            }
        }

        var smashSystem = arena.Smash;
        if (smashSystem != null)
        {
            Box(new Rect(w - 312, 120, 300, 26), new Color(0f, 0f, 0f, .55f));
            float ready = smashSystem.Active ? 1f : 1f - Mathf.Clamp01(smashSystem.CooldownRemaining / 1.4f);
            Box(new Rect(w - 192, 126, 174 * ready, 14), smashSystem.Active ? new Color(1f, .8f, .4f) : (ready >= 1f ? new Color(.9f, .75f, .5f) : new Color(.5f, .4f, .3f)));
            GUI.Label(new Rect(w - 306, 122, 110, 24), "SMASH  F/B", new GUIStyle(small) { fontStyle = FontStyle.Bold });
        }

        // --- Thoi gian va muc tieu ---
        GUI.Label(new Rect(w - 300, 16, 280, 40), "TIME " + Clock(arena.RunTime), label);
        GUI.Label(new Rect(24, 196, w - 48, 34), "GOAL: " + arena.ObjectiveText, small);

        // --- Phim (khop binding hien tai) ---
        GUI.Label(new Rect(24, h - 44, w - 48, 34),
            PlayerInputReader.GamepadConnected
                ? "PAD  Stick/D-pad Move   A Jump (x2, wall)   RB Dash   X Edge (Up/Down+X)   LT Arc   B Smash   Y hold Bash (stick aim)   RT hold Grab (climb)   LB Interact   Down+A Drop   Start Pause"
                : "A/D Move   Space Jump (x2, wall)   Ctrl/RMB Dash   LMB Edge x5 (W up / S down)   Q Arc   F Smash   R/C Bash   Shift Grab + W/S Climb   E Interact   S+Space Drop   Esc Pause   F3 Debug",
            small);

        if (arena.Completed)
        {
            Box(new Rect(0, h / 2f - 110, w, 220), new Color(0f, 0f, 0f, .65f));
            GUI.Label(new Rect(0, h / 2f - 90, w, 90), "ROUTE COMPLETE", big);
            var c = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, h / 2f, w, 50), "Time " + Clock(arena.FinishTime) + "   |   Esc > Restart route, or Ctrl+F5 to play again", c);
        }

        DrawGuidance(s, w, h);
        if (arena.DebugVisible) DrawDebug(s, w);
    }

    private void RefreshCache()
    {
        if (Time.unscaledTime < cacheAt) return;
        cacheAt = Time.unscaledTime + .25f;
        melee = FindObjectsOfType<PrototypeMeleeEnemy>();
        special = FindObjectsOfType<PrototypeSpecialEnemy>();
        shots = FindObjectsOfType<PrototypeProjectile>();
    }

    // ---------------- Canh bao va chi huong ----------------

    private PrototypeProjectile[] shots = new PrototypeProjectile[0];
    private int lastHealth = -1;
    private float hitFlashUntil;

    /// <summary>Toa do GUI (da nhan ty le) cua diem the gioi; z &lt; 0 la phia sau camera.</summary>
    private bool ToGui(Vector2 world, float s, out Vector2 gui)
    {
        gui = Vector2.zero;
        var cam = Camera.main;
        if (cam == null) return false;
        Vector3 p = cam.WorldToScreenPoint(world);
        gui = new Vector2(p.x / s, (Screen.height - p.y) / s);
        return p.z > 0f;
    }

    /// <summary>Mui ten o mep man hinh chi huong toi diem ngoai man hinh.</summary>
    private void EdgeArrow(Vector2 gui, float w, float h, Color color, string tag)
    {
        const float margin = 56f;
        Vector2 center = new Vector2(w / 2f, h / 2f);
        Vector2 dir = gui - center;
        if (dir.sqrMagnitude < 1f) return;
        float scale = Mathf.Min((w / 2f - margin) / Mathf.Max(.01f, Mathf.Abs(dir.x)),
            (h / 2f - margin) / Mathf.Max(.01f, Mathf.Abs(dir.y)));
        Vector2 p = center + dir * Mathf.Min(1f, scale);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var old = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, p);
        Box(new Rect(p.x - 22, p.y - 5, 40, 10), color);
        GUIUtility.RotateAroundPivot(150f, p + new Vector2(18, 0));
        Box(new Rect(p.x + 18, p.y - 4, 22, 8), color);
        GUI.matrix = old;
        GUIUtility.RotateAroundPivot(angle, p);
        GUIUtility.RotateAroundPivot(-150f, p + new Vector2(18, 0));
        Box(new Rect(p.x + 18, p.y - 4, 22, 8), color);
        GUI.matrix = old;
        if (!string.IsNullOrEmpty(tag))
        {
            var tagStyle = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            tagStyle.normal.textColor = color;
            GUI.Label(new Rect(Mathf.Clamp(p.x - 50, 4f, w - 104f), p.y + (dir.y > 0 ? -58f : 28f), 100, 26), tag, tagStyle);
        }
    }

    private void Mark(Vector2 gui, string text, Color color, float yOffset)
    {
        var style = new GUIStyle(big) { fontSize = 80 };
        style.normal.textColor = color;
        GUI.Label(new Rect(gui.x - 40, gui.y - yOffset - 20f, 80, 90), text, style);
    }

    private void DrawGuidance(float s, float w, float h)
    {
        RefreshCache();
        float t = Time.unscaledTime;
        bool dead = health.IsDead;
        Vector2 playerPos = player.Motor.Position;

        // --- Bi thuong: nhay do toan man hinh, mo dan ---
        if (lastHealth >= 0 && health.Health < lastHealth && !dead) hitFlashUntil = t + .45f;
        lastHealth = health.Health;
        float flash = Mathf.Clamp01((hitFlashUntil - t) / .45f);
        if (flash > 0f) Edges(w, h, new Color(1f, .05f, .05f, .22f * flash), 80f);

        // --- Mau thap: vien do nhap nhay + chu ---
        if (!dead && health.Health <= 1 && health.Maximum > 1)
        {
            float pulse = .1f + .08f * Mathf.Sin(t * 7f);
            Edges(w, h, new Color(1f, 0f, 0f, pulse), 48f);
            var warn = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 34 };
            warn.normal.textColor = Mathf.Repeat(t, .8f) < .5f ? new Color(1f, .35f, .35f) : Color.white;
            GUI.Label(new Rect(w / 2f - 200, 130, 400, 44), "LOW HP", warn);
        }

        // --- Quai: dau '!' khi sap danh, mui ten mep man hinh khi ngoai tam nhin ---
        foreach (var e in melee) if (e != null && e.isActiveAndEnabled) Threat(e.transform.position, e.State, playerPos, s, w, h, t, 5f);
        foreach (var e in special) if (e != null && e.isActiveAndEnabled) Threat(e.transform.position, e.State, playerPos, s, w, h, t, 9f);

        // --- Boss: canh bao don va thanh mau (kieu Hollow Knight, ten + 2 pha) ---
        var boss = arena.Boss;
        if (boss != null && boss.Active)
        {
            Threat(boss.Position, boss.State, playerPos, s, w, h, t, 12f);
            float frac = Mathf.Clamp01(boss.Health / (float)PrototypeBoss.MaxHealth);
            float bw = Mathf.Min(900f, w - 200f);
            float bx = (w - bw) / 2f;
            Box(new Rect(bx - 4, 60, bw + 8, 30), new Color(0f, 0f, 0f, .75f));
            Box(new Rect(bx, 64, bw * frac, 22), boss.Phase == 2 ? new Color(.9f, .2f, .35f) : new Color(.75f, .45f, .95f));
            Box(new Rect(bx + bw * .5f - 2, 60, 4, 30), new Color(1f, 1f, 1f, .5f));
            var nameStyle = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(bx, 30, bw, 28), "THE WARDEN" + (boss.Phase == 2 ? "  -  ENRAGED" : ""), nameStyle);
            if (boss.Current == PrototypeBoss.Mode.Stagger)
            {
                var hit = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                hit.normal.textColor = new Color(.5f, .9f, 1f);
                GUI.Label(new Rect(bx, 94, bw, 28), "STUNNED", hit);
            }
        }

        // --- Dan dich dang bay ---
        foreach (var p in shots)
        {
            if (p == null || p.Faction != CombatFaction.Enemy || !p.isActiveAndEnabled) continue;
            bool onScreen = ToGui(p.transform.position, s, out var g) && g.x > 0 && g.x < w && g.y > 0 && g.y < h;
            if (onScreen)
            {
                if (Vector2.Distance(p.transform.position, playerPos) < 6f) Mark(g, "!", new Color(1f, .6f, .1f), 60f);
            }
            else EdgeArrow(g, w, h, new Color(1f, .6f, .1f, .9f), "SHOT");
        }

        // --- Bash: thanh thoi gian ngam, do dan khi sap tu phong ---
        if (player.Bash != null && player.Bash.Phase == PrototypeBash.BashPhase.Aim)
        {
            float frac = player.Bash.AimRemaining / Mathf.Max(.01f, player.Bash.AimSecondsMax);
            Box(new Rect(w / 2f - 160, h - 120, 320, 14), new Color(0f, 0f, 0f, .7f));
            Box(new Rect(w / 2f - 158, h - 118, 316 * frac, 10), frac < .33f ? new Color(1f, .25f, .2f) : Color.cyan);
            var aimTip = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(w / 2f - 300, h - 148, 600, 28), "Aim: mouse / WASD / stick   |   release the Bash button to launch", aimTip);
        }

        // --- Muc tieu: mui ten o mep man hinh hoac dau nhay tren dau muc tieu ---
        if (!dead && arena.TryGetObjectivePosition(out var goal))
        {
            Vector2 gui;
            bool visible = ToGui(goal, s, out gui) && gui.x > 40 && gui.x < w - 40 && gui.y > 40 && gui.y < h - 40;
            Color c = new Color(.3f, 1f, 1f, .95f);
            if (!visible) EdgeArrow(gui, w, h, c, Mathf.RoundToInt(Vector2.Distance(goal, playerPos)) + "m");
            else
            {
                float bob = Mathf.Abs(Mathf.Sin(t * 4f)) * 14f;
                var style = new GUIStyle(big) { fontSize = 40 };
                style.normal.textColor = c;
                GUI.Label(new Rect(gui.x - 40, gui.y - 110 - bob, 80, 50), "v", style);
            }
        }
    }

    private void Threat(Vector2 world, PrototypeEnemyState state, Vector2 playerPos, float s, float w, float h, float t, float alertRange)
    {
        if (state == PrototypeEnemyState.Dead || state == PrototypeEnemyState.Idle) return;
        bool onScreen = ToGui(world, s, out var g) && g.x > 0 && g.x < w && g.y > 0 && g.y < h;
        if (onScreen)
        {
            if (state == PrototypeEnemyState.Windup && Mathf.Repeat(t, .2f) < .13f) Mark(g, "!", new Color(1f, .9f, .1f), 120f);
            else if (state == PrototypeEnemyState.Attack) Mark(g, "!", Color.red, 120f);
        }
        else if (Vector2.Distance(world, playerPos) < alertRange * 2f)
            EdgeArrow(g, w, h, state == PrototypeEnemyState.Windup ? new Color(1f, .85f, .1f, .95f) : new Color(1f, .3f, .3f, .85f), "");
    }

    private static void Edges(float w, float h, Color c, float thickness)
    {
        // Vien man hinh gia lap vignette: nhieu lop mong, cang ra ngoai cang dam.
        for (int i = 0; i < 4; i++)
        {
            Color layer = c; layer.a = c.a * (1f - i * .25f);
            float d = thickness * (i + 1) / 4f;
            Box(new Rect(0, 0, w, d), layer);
            Box(new Rect(0, h - d, w, d), layer);
            Box(new Rect(0, 0, d, h), layer);
            Box(new Rect(w - d, 0, d, h), layer);
        }
    }

    private void DrawDebug(float s, float w)
    {
        RefreshCache();
        var m = player.Motor;
        var c = player.Controls;
        text.Length = 0;
        text.AppendLine("DEBUG (F3)");
        text.AppendLine("state " + player.Locomotion + "  idle " + player.IdleTime.ToString("0.0"));
        text.AppendLine("vel " + m.Velocity.x.ToString("0.0") + ", " + m.Velocity.y.ToString("0.0") + "  pos " + m.Position.x.ToString("0.0") + ", " + m.Position.y.ToString("0.0"));
        text.AppendLine("grounded " + m.Grounded + "  wall " + m.WallDirection + "  face " + (m.FacingRight ? "R" : "L"));
        text.AppendLine("airJump " + m.AirJumpAvailable + "  airDash " + m.AirDashAvailable);
        text.AppendLine("buffer J " + c.JumpBuffered + "  D " + c.DashBuffered + "  A " + c.AttackBuffered);
        if (player.Combat != null)
            text.AppendLine("combat " + player.Combat.Phase + " step " + player.Combat.StepIndex + " queued " + player.Combat.ChainQueued);
        if (player.Bash != null)
            text.AppendLine("bash " + player.Bash.Phase + " dir " + player.Bash.AimDirection.x.ToString("0.00") + "," + player.Bash.AimDirection.y.ToString("0.00"));
        text.AppendLine("hp " + health.Health + "/" + health.Maximum + "  timeScale " + Time.timeScale.ToString("0.00"));
        foreach (var e in melee) if (e != null) text.AppendLine("melee " + e.State + " hp " + e.Health);
        foreach (var e in special) if (e != null) text.AppendLine(e.Kind + " " + e.State + " hp " + e.Health);
        if (room != null) text.AppendLine("trial wave " + (room.WaveIndex + 1) + " active " + room.Active + " shots " + room.ProjectileCount);
        GUI.Box(new Rect(w - 620, 70, 600, 36 + 28 * (text.ToString().Split('\n').Length)), GUIContent.none);
        GUI.Label(new Rect(w - 610, 76, 580, 700), text.ToString(), small);

        // Hurtbox/hitbox cua Player, quai va vung danh dang hoat dong.
        var body = player.GetComponent<Collider2D>();
        if (body != null) Outline(body.bounds, new Color(.2f, 1f, .3f), s);
        foreach (var e in melee) if (e != null) Outline(e.GetComponent<Collider2D>().bounds, Color.red, s);
        foreach (var e in special) if (e != null) Outline(e.GetComponent<Collider2D>().bounds, Color.magenta, s);
        if (player.Combat != null && player.Combat.Active)
            Outline(new Bounds(player.Combat.HitCenter, player.Combat.HitSize), Color.yellow, s);
    }

    private void Outline(Bounds b, Color color, float s)
    {
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 a = cam.WorldToScreenPoint(b.min), z = cam.WorldToScreenPoint(b.max);
        float x = a.x / s, y = (Screen.height - z.y) / s;
        float width = (z.x - a.x) / s, height = (z.y - a.y) / s;
        const float t = 2f;
        Box(new Rect(x, y, width, t), color);
        Box(new Rect(x, y + height - t, width, t), color);
        Box(new Rect(x, y, t, height), color);
        Box(new Rect(x + width - t, y, t, height), color);
    }
}
