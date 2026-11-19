using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Camera va reset cho san thu dieu khien doc lap.</summary>
public sealed class PrototypePlayground : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera followCamera;
    [SerializeField] private PrototypeTrainingDummy[] dummies = new PrototypeTrainingDummy[0];
    private Vector2 spawn;
    private readonly PrototypeCameraFollow cameraRig = new PrototypeCameraFollow();
    private PrototypePause pause;
    private PrototypeHud hud;
    private PrototypePlayerHealth health;
    private PrototypeMeleeEnemy enemy;
    private float respawnAt = float.PositiveInfinity;
    private SpriteRenderer dashCharge;
    private TextMesh dashLabel;
    private SpriteRenderer wallContact;
    private PrototypeTerrain terrain;
    private PrototypeCombatRoom combatRoom;
    private PrototypeBash bash;
    private PrototypeBashCourse bashCourse;
    private PrototypeProgress progress;
    public PrototypeProgress Progress => progress;

    /// <summary>Dong bo nang luc Bash, mau toi da va hien thi vat pham/cua theo tien trinh da luu.</summary>
    public void ApplyProgress()
    {
        if (progress == null) return;
        if (bash != null) bash.Unlocked = progress.BashAvailable;
        if (health != null) health.SetMaxBonus(progress.MaxHealthBonus + PrototypeShards.VitalityBonus);
        if (bashCourse != null) bashCourse.RefreshProgress();
    }

    public void OnCheckpointActivated(string id)
    {
        if (progress != null) progress.SetCheckpoint(id);
        if (bossArena != null) bossArena.SetCheckpointActive(id == PrototypeBossArena.BossCheckpointId);
        if (skyArena != null) skyArena.SetCheckpointActive(id == PrototypeSkyArena.CheckpointId);
        if (rootArena != null) rootArena.SetCheckpointActive(id == PrototypeRootArena.CheckpointId);
    }

    private void OnRootBossDefeated()
    {
        if (progress != null) progress.CollectItem(PrototypeProgress.RootBossDefeatedId);
    }

    private PrototypeRootArena rootArena;
    public PrototypeRootBoss RootBoss => rootArena != null ? rootArena.Boss : null;
    private bool RootDone => progress != null && progress.HasItem(PrototypeProgress.RootBossDefeatedId);

    private void OnSkyBossDefeated()
    {
        if (progress != null) progress.CollectItem(PrototypeProgress.SkyBossDefeatedId);
    }

    private PrototypeSkyArena skyArena;
    public PrototypeStormBoss SkyBoss => skyArena != null ? skyArena.Boss : null;
    private bool SkyDone => progress != null && progress.HasItem(PrototypeProgress.SkyBossDefeatedId);

    private void OnBossDefeated()
    {
        if (progress != null) progress.CollectItem(PrototypeProgress.BossDefeatedId);
    }

    private PrototypeBossArena bossArena;
    public PrototypeBoss Boss => bossArena != null ? bossArena.Boss : null;
    private bool BossDone => progress != null && progress.HasItem(PrototypeProgress.BossDefeatedId);
    private bool resetting;
    public Vector2 RespawnPosition => progress != null && progress.CheckpointId == PrototypeRootArena.CheckpointId
        ? PrototypeRootArena.CheckpointSpawn
        : progress != null && progress.CheckpointId == PrototypeSkyArena.CheckpointId
        ? PrototypeSkyArena.CheckpointSpawn
        : progress != null && progress.CheckpointId == PrototypeBossArena.BossCheckpointId
        ? PrototypeBossArena.CheckpointSpawn
        : terrain != null && terrain.CheckpointActive ? terrain.CheckpointSpawn : spawn;

    private PrototypeSpiritArc arc;
    public PrototypeSpiritArc Arc => arc;
    private PrototypeSpiritSlots spiritSlots;
    public PrototypeSpiritSlots Slots => spiritSlots;
    private PrototypeShards shards;
    public PrototypeShards Shards => shards;
    private PrototypeSpiritCasts casts;
    public PrototypeSpiritCasts Casts => casts;
    private PrototypeSpiritSentry sentry;
    public PrototypeSpiritSentry Sentry => sentry;
    private PrototypeSpiritSmash smash;
    public PrototypeSpiritSmash Smash => smash;
    private PrototypeHazards hazards;
    public PrototypeHazards Hazards => hazards;
    private int previousFrameRate;
    private int previousVSync;
    private float previousFixedStep;
    public const int TargetFrameRate = 60;

    /// <summary>Game chay 60 FPS: khoa frame rate, tat vSync va dong bo buoc vat ly voi frame de bot rung hinh.</summary>
    private void ApplyFrameRate()
    {
        previousFrameRate = Application.targetFrameRate;
        previousVSync = QualitySettings.vSyncCount;
        previousFixedStep = Time.fixedDeltaTime;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
        Time.fixedDeltaTime = 1f / TargetFrameRate;
    }

    private void Start()
    {
        ApplyFrameRate();
        spawn = player.transform.position;
        health = player.GetComponent<PrototypePlayerHealth>();
        if (health == null) health = player.gameObject.AddComponent<PrototypePlayerHealth>();
        health.Died += OnPlayerDied;
        var square = player.transform.Find("Animated Shape").GetComponent<SpriteRenderer>().sprite;
        terrain = gameObject.AddComponent<PrototypeTerrain>();
        terrain.Initialize(this, player, health, square);
        player.Motor.Crushed += ResetRun;
        dashCharge = PrototypeGeometry.Rectangle(player.transform, square, "Air Dash Charge",
            new Vector2(.18f, .1f), new Vector2(0f, -.9f), Color.cyan);
        dashLabel = PrototypeGeometry.Label(player.transform, "Air Dash Label", new Vector2(0f, -1.1f), .055f);
        wallContact = PrototypeGeometry.Rectangle(player.transform, square, "Wall Contact",
            new Vector2(.06f, .35f), Vector2.zero, Color.green);
        // Runtime practice corridor on the existing far landing; no scene asset edits.
        foreach (float x in new[] { 19f, 22f })
        {
            var wall = PrototypeGeometry.Rectangle(transform, square, "Wall Jump Practice " + x,
                new Vector2(.5f, 5.5f), new Vector2(x, 2.6f), new Color(.3f, .38f, .42f), 0);
            wall.gameObject.AddComponent<BoxCollider2D>();
        }
        var wallHint = PrototypeGeometry.Label(transform, "Wall Jump Hint", new Vector2(20.5f, 5.8f), .09f);
        wallHint.text = "WALL CLIMB\nHold Shift (RT) on the wall to grab, W / S to climb\nSpace: wall jump | or hold toward wall + Space and alternate";
        var g = new GameObject("Prototype Melee Enemy");
        g.tag = "Enemy";
        g.transform.position = new Vector3(-4.5f,.55f,0);
        g.AddComponent<Rigidbody2D>();
        g.AddComponent<BoxCollider2D>();
        enemy = g.AddComponent<PrototypeMeleeEnemy>();
        enemy.Initialize(player, health, square);
        combatRoom=gameObject.AddComponent<PrototypeCombatRoom>();
        combatRoom.Initialize(player,health,square);
        bash = player.GetComponent<PrototypeBash>();
        if (bash == null) bash = player.gameObject.AddComponent<PrototypeBash>();
        bash.Initialize(player, followCamera, square);
        player.SetBash(bash);
        spiritSlots = player.GetComponent<PrototypeSpiritSlots>();
        if (spiritSlots == null) spiritSlots = player.gameObject.AddComponent<PrototypeSpiritSlots>();
        spiritSlots.Initialize(player.Controls);
        shards = player.GetComponent<PrototypeShards>();
        if (shards == null) shards = player.gameObject.AddComponent<PrototypeShards>();
        shards.Initialize();
        arc = player.GetComponent<PrototypeSpiritArc>();
        if (arc == null) arc = player.gameObject.AddComponent<PrototypeSpiritArc>();
        arc.Initialize(player, followCamera, square);
        smash = player.GetComponent<PrototypeSpiritSmash>();
        if (smash == null) smash = player.gameObject.AddComponent<PrototypeSpiritSmash>();
        smash.Initialize(player, square);
        sentry = player.GetComponent<PrototypeSpiritSentry>();
        if (sentry == null) sentry = player.gameObject.AddComponent<PrototypeSpiritSentry>();
        sentry.Initialize(player, square, arc);
        casts = player.GetComponent<PrototypeSpiritCasts>();
        if (casts == null) casts = player.gameObject.AddComponent<PrototypeSpiritCasts>();
        casts.Initialize(player, square, arc);
        progress = gameObject.AddComponent<PrototypeProgress>();
        progress.Load();
        bashCourse = gameObject.AddComponent<PrototypeBashCourse>();
        bashCourse.Initialize(player, health, square, combatRoom, progress, ApplyProgress);
        bossArena = gameObject.AddComponent<PrototypeBossArena>();
        bossArena.Initialize(player, health, square, OnCheckpointActivated, OnBossDefeated);
        bossArena.ResetArena(BossDone);
        skyArena = gameObject.AddComponent<PrototypeSkyArena>();
        skyArena.Initialize(player, health, square, OnCheckpointActivated, OnSkyBossDefeated);
        skyArena.ResetArena(SkyDone);
        rootArena = gameObject.AddComponent<PrototypeRootArena>();
        rootArena.Initialize(player, health, square, OnCheckpointActivated, OnRootBossDefeated);
        rootArena.ResetArena(RootDone);
        // Checkpoint khong con ton tai trong save -> ve diem dau an toan va xoa dau cu.
        if (progress.CheckpointId == PrototypeTerrain.MainCheckpointId) terrain.RestoreCheckpoint();
        else if (progress.CheckpointId == PrototypeBossArena.BossCheckpointId)
        {
            terrain.RestoreCheckpoint();
            bossArena.SetCheckpointActive(true);
        }
        else if (progress.CheckpointId == PrototypeSkyArena.CheckpointId)
        {
            terrain.RestoreCheckpoint();
            skyArena.SetCheckpointActive(true);
        }
        else if (progress.CheckpointId == PrototypeRootArena.CheckpointId)
        {
            terrain.RestoreCheckpoint();
            rootArena.SetCheckpointActive(true);
        }
        else if (!string.IsNullOrEmpty(progress.CheckpointId)) progress.ClearCheckpoint();
        ApplyProgress();
        hazards = gameObject.AddComponent<PrototypeHazards>();
        hazards.Initialize(player, health, square);
        var finishObject = new GameObject("Finish Flag");
        finishObject.transform.SetParent(transform, false);
        finishObject.transform.position = new Vector2(169.6f, .1f);
        finishObject.AddComponent<PrototypeFinish>().Initialize(square, this, player);
        pause = gameObject.AddComponent<PrototypePause>();
        pause.Initialize(this, player);
        hud = gameObject.AddComponent<PrototypeHud>();
        hud.Initialize(this, player, health, progress, terrain, combatRoom);
        // Dat camera ngay tai Player de khong truot tu vi tri trong scene.
        followCamera.transform.position = cameraRig.Snap(player.Motor.Position, 1);
        if (terrain.CheckpointActive) ResetRun();
        var hint = PrototypeGeometry.Label(transform, "Combat Route Hint", new Vector2(-3.7f,2.6f), .085f);
        hint.text = "MELEE ENEMY < LEFT\nSpace x2: Double jump | Ctrl/RMB: Dash\nHold toward wall: Slide | Shift: Grab + W/S climb | Space: Wall jump\nF5: Respawn | Ctrl+F5: Restart | E: Checkpoint\nBASH COURSE > RIGHT: hold R or C, aim mouse, release";
    }

    private void OnPlayerDied() => respawnAt = Time.time + 1.1f;

    private void OnDestroy()
    {
        if (previousFixedStep > 0f)
        {
            Application.targetFrameRate = previousFrameRate;
            QualitySettings.vSyncCount = previousVSync;
            Time.fixedDeltaTime = previousFixedStep;
        }
        if (health != null) health.Died -= OnPlayerDied;
        if (player != null) player.Motor.Crushed -= ResetRun;
    }

    private void Update()
    {
        bool paused = pause != null && pause.Paused;
        bool focused = Application.isFocused || PlayerInputReader.IgnoreFocusLoss;
        if (!paused && !Completed && Time.timeScale > 0f) RunTime += Time.unscaledDeltaTime;
        if (focused && Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame) DebugVisible = !DebugVisible;
        // Phong thu cuoi (combat trial) xong thi ghi vao tien trinh de F5/thoat khong mat.
        if (combatRoom != null && combatRoom.Completed && progress != null
            && !progress.HasItem(PrototypeProgress.TrialClearId)) progress.CollectItem(PrototypeProgress.TrialClearId);
        if (paused) return;
        // Vietnamese input methods may synthesize Backspace while composing text.
        // Keep the reset shortcut outside text-editing keys.
        if (focused && Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed) RestartRun();
            else ResetRun();
        }
        else if (player.transform.position.y < -7f || Time.time >= respawnAt) ResetRun();
    }

    // ---------------- M7: hoan thanh, muc tieu, debug ----------------

    public bool Completed { get; private set; }
    public float RunTime { get; private set; }
    public float FinishTime { get; private set; }
    public bool DebugVisible { get; private set; }
    public event System.Action Finished;
    public bool CanFinish => !Completed && !player.IsDead && progress != null
        && progress.HasItem(PrototypeProgress.TrialClearId) && BossDone && SkyDone && RootDone;

    public string ObjectiveText
    {
        get
        {
            if (Completed) return "Route complete. Esc > Restart route to play again.";
            if (terrain != null && !terrain.CheckpointActive) return "Activate the checkpoint (E) near x=9.";
            if (progress == null || !progress.HasItem(PrototypeProgress.TrialClearId))
                return "Clear the 3-wave combat trial to the left.";
            if (!progress.BashAvailable) return "Take the Bash orb (cyan) right of the walls.";
            if (!progress.IsDoorOpen(PrototypeProgress.ShortcutDoorId))
                return "Cross the gap with Bash, break the gate with a returned shot, pass the door.";
            if (!BossDone) return "Enter the arena and defeat the Warden. Bash its purple shots back.";
            if (!SkyDone) return "The east wall is open. Defeat the Stormwing in the sky arena.";
            if (!RootDone) return "The next wall is open. Crack the Gloomroot's shell with Smash or a Bashed seed.";
            return "Reach the flag at the end.";
        }
    }

    /// <summary>Vi tri the gioi cua muc tieu hien tai de HUD chi huong; false khi khong con muc tieu.</summary>
    public bool TryGetObjectivePosition(out Vector2 position)
    {
        position = Vector2.zero;
        if (Completed) return false;
        float x = player.Motor.Position.x;
        if (terrain != null && !terrain.CheckpointActive) position = new Vector2(9f, .65f);
        else if (progress == null || !progress.HasItem(PrototypeProgress.TrialClearId)) position = new Vector2(-15f, 1f);
        else if (!progress.BashAvailable) position = new Vector2(25.5f, .5f);
        else if (!progress.IsDoorOpen(PrototypeProgress.ShortcutDoorId))
        {
            bool gateBroken = bashCourse != null && bashCourse.Barrier != null && bashCourse.Barrier.Broken;
            if (x < 31f) position = new Vector2(35f, 2.8f);
            else if (!gateBroken) position = x < 50f ? new Vector2(46f, 2.8f) : new Vector2(58f, 2.6f);
            else position = new Vector2(63f, 1.45f);
        }
        else if (!BossDone) position = new Vector2(Mathf.Max(80f, x), .6f);
        else if (!SkyDone) position = new Vector2(Mathf.Max(106f, x), .6f);
        else if (!RootDone) position = new Vector2(Mathf.Max(142f, x), .6f);
        else position = new Vector2(169.6f, .1f);
        return true;
    }

    /// <summary>Hoan thanh tuyen: chi mot lan, chi khi con song va phong thu cuoi da xong.</summary>
    public bool Complete()
    {
        if (!CanFinish) return false;
        Completed = true;
        FinishTime = RunTime;
        // Ngung quai/dan va tra thoi gian ve trang thai hop le.
        SetEnemiesEnabled(false);
        if (bash != null) bash.Cancel();
        var stop = GetComponent<DangDang.Combat.HitStop>();
        if (stop != null) stop.Release();
        if (pause == null || !pause.Paused) Time.timeScale = 1f;
        Finished?.Invoke();
        return true;
    }

    private void SetEnemiesEnabled(bool on)
    {
        if (enemy != null) enemy.gameObject.SetActive(on);
        if (combatRoom != null)
        {
            if (!on) combatRoom.ResetRoom(false);
            combatRoom.enabled = on;
        }
        if (bashCourse != null) bashCourse.SetShooterActive(on);
    }

    public void ResetRun()
    {
        if (resetting) return;
        resetting = true;
        try
        {
            Vector2 destination = RespawnPosition;
            player.ResetMotion(destination);
            if (terrain != null) terrain.ResetTerrain();
            var stop = GetComponent<DangDang.Combat.HitStop>();
            if (stop != null) stop.Release();
            foreach (var dummy in dummies) if (dummy != null) dummy.ResetDummy();
            // Shard Vitality doi luc nao thi chi co hieu luc tu lan hoi sinh nay.
            if (health != null && progress != null) health.SetMaxBonus(progress.MaxHealthBonus + PrototypeShards.VitalityBonus);
            if (health != null) health.ResetHealth();
            if (enemy != null) enemy.ResetEnemy();
            if (combatRoom != null) combatRoom.ResetRoom(false);
            if (bashCourse != null) bashCourse.ResetCourse();
            if (hazards != null) hazards.ResetAll();
            if (arc != null) arc.ResetEnergy();
            if (smash != null) smash.Cancel();
            if (sentry != null) sentry.Clear();
            if (bossArena != null) bossArena.ResetArena(BossDone);
            if (skyArena != null) skyArena.ResetArena(SkyDone);
            if (rootArena != null) rootArena.ResetArena(RootDone);
            respawnAt = float.PositiveInfinity;
            // Hoi sinh: snap camera va xoa smoothing cu.
            followCamera.transform.position = cameraRig.Snap(destination, 1);
        }
        finally { resetting = false; }
    }

    public void RestartRun()
    {
        // Choi lai toan tuyen: xoa tien trinh prototype (save rieng), khong dong vao save game cu.
        Completed = false;
        RunTime = 0f;
        FinishTime = 0f;
        SetEnemiesEnabled(true);
        if (progress != null) progress.ResetAll();
        if (terrain != null) terrain.ClearCheckpoint();
        if (bossArena != null) bossArena.SetCheckpointActive(false);
        if (skyArena != null) skyArena.SetCheckpointActive(false);
        if (rootArena != null) rootArena.SetCheckpointActive(false);
        ApplyProgress();
        if (combatRoom != null) combatRoom.ResetRoom(true);
        ResetRun();
    }

    private void LateUpdate()
    {
        bool available = player.Motor.Grounded || player.Motor.AirDashAvailable;
        dashCharge.color = available ? Color.cyan : Color.gray;
        dashLabel.text = !available ? "DASH: 0" : player.DashReady ? "DASH: 1 READY" : "DASH: 1 WAIT";
        dashLabel.transform.rotation = Quaternion.identity;
        wallContact.enabled = player.Locomotion == PlayerLocomotion.WallSlide || player.Locomotion == PlayerLocomotion.WallClimb;
        wallContact.transform.localPosition = new Vector3(.37f * player.Motor.WallDirection
            * (player.Motor.FacingRight ? 1f : -1f), 0f, 0f);
        followCamera.transform.position = cameraRig.Step(followCamera.transform.position, player.Motor.Position,
            player.Motor.FacingRight ? 1 : -1, Time.deltaTime);
    }
}
