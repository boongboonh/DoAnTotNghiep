using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cho phep agent choi thu trong Play Mode khi cua so Unity khong co focus.
/// Chi doi trang thai trong luc Play Mode (khong ghi vao asset/ProjectSettings) va tra lai khi thoat.
///   - Input System: bo qua mat focus, phim/chuot luon den Game View.
///   - Application.runInBackground = true.
///   - PlayerInputReader.IgnoreFocusLoss = true (code game khong tu xoa input khi mat focus).
/// Bat/tat bang menu Dang Dang > Agent Play Setup (mac dinh bat).
/// </summary>
[InitializeOnLoad]
public static class AgentPlaySetup
{
    private const string PrefKey = "DangDang.AgentPlaySetup";
    private const string MenuPath = "Dang Dang/Agent Play Setup (play without focus)";

    private static bool applied;
    private static InputSettings.BackgroundBehavior oldBackground;
    private static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    private static bool oldRunInBackground;

    static AgentPlaySetup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (EditorApplication.isPlaying) Apply();
    }

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void Toggle() => Enabled = !Enabled;

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) Apply();
        else if (state == PlayModeStateChange.ExitingPlayMode) Restore();
    }

    private static void Apply()
    {
        if (applied || !Enabled) return;
        var settings = InputSystem.settings;
        if (settings == null) return;
        applied = true;
        oldBackground = settings.backgroundBehavior;
        oldEditorInput = settings.editorInputBehaviorInPlayMode;
        oldRunInBackground = Application.runInBackground;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        PlayerInputReader.IgnoreFocusLoss = true;
    }

    private static void Restore()
    {
        PlayerInputReader.IgnoreFocusLoss = false;
        if (!applied) return;
        applied = false;
        var settings = InputSystem.settings;
        if (settings != null)
        {
            settings.backgroundBehavior = oldBackground;
            settings.editorInputBehaviorInPlayMode = oldEditorInput;
        }
        Application.runInBackground = oldRunInBackground;
    }
}
