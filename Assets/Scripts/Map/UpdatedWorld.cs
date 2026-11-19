using GF.Save;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class UpdatedWorld : MonoBehaviour
{
    public const string Meadow = "MeadowUpdate";
    public const string Cave = "VioletCaveUpdate";
    public const string SceneName = "LoadMapScene";

    private const string MapKey = "nameMapCurrent";
    private const string XKey = "FirstPlayPosX";
    private const string YKey = "FirstPlayPosY";
    private const string CheckpointXKey = "checkPointPosX";
    private const string CheckpointYKey = "checkPointPosY";

    [SerializeField] private Rigidbody2D player;
    [SerializeField] private CinemachineVirtualCamera followCamera;

    private string currentMap;
    private bool switching;

    public static void StartNewGame() => RememberSpawn(Meadow, Entrance(Meadow));

    private void Awake()
    {
        currentMap = PlayerPrefsSaveSystem.LoadString(MapKey);
        if (!IsValidMap(currentMap) || !IsInRegion(currentMap, ReadPosition()))
        {
            StartNewGame();
            currentMap = Meadow;
        }
    }

    private IEnumerator Start()
    {
        switching = true;
        yield return LoadRegion(currentMap);
        MovePlayer(ReadPosition() + Vector2.up);
        switching = false;
    }

    public void Travel(string destination)
    {
        if (switching || !IsValidMap(destination) || destination == currentMap) return;
        StartCoroutine(SwitchRegion(destination));
    }

    private IEnumerator SwitchRegion(string destination)
    {
        switching = true;
        string previous = currentMap;
        yield return LoadRegion(destination);
        currentMap = destination;
        Vector2 spawn = Entrance(destination);
        RememberSpawn(destination, spawn);
        PlayerPrefsSaveSystem.SetInt("isCreateCheckPoint", 1);
        MovePlayer(spawn + Vector2.up);
        yield return SceneManager.UnloadSceneAsync(previous);
        switching = false;
    }

    private IEnumerator LoadRegion(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    private void LateUpdate()
    {
        if (switching || !player.gameObject.activeInHierarchy) return;
        if (player.position.y < (currentMap == Meadow ? -9f : -18f))
            MovePlayer(Entrance(currentMap) + Vector2.up);
    }

    private void MovePlayer(Vector2 destination)
    {
        Vector2 offset = destination - player.position;
        player.linearVelocity = Vector2.zero;
        player.position = destination;
        player.transform.position = destination;
        followCamera.OnTargetObjectWarped(player.transform, offset);
        followCamera.PreviousStateIsValid = false;
    }

    private static Vector2 ReadPosition() => new Vector2(PlayerPrefsSaveSystem.LoadFloat(XKey), PlayerPrefsSaveSystem.LoadFloat(YKey));

    private static bool IsValidMap(string sceneName) => sceneName == Meadow || sceneName == Cave;

    private static bool IsInRegion(string sceneName, Vector2 position)
    {
        if (sceneName == Meadow) return position.x > -1f && position.x < 66f && position.y > -7f && position.y < 15f;
        return position.x > 99f && position.x < 163f && position.y > -16f && position.y < 15f;
    }

    private static Vector2 Entrance(string sceneName) =>
        sceneName == Meadow ? new Vector2(3f, 0.68f) : new Vector2(103f, 0.68f);

    private static void RememberSpawn(string sceneName, Vector2 position)
    {
        PlayerPrefsSaveSystem.SetString(MapKey, sceneName);
        PlayerPrefsSaveSystem.SetFloat(XKey, position.x);
        PlayerPrefsSaveSystem.SetFloat(YKey, position.y);
        PlayerPrefsSaveSystem.SetFloat(CheckpointXKey, position.x);
        PlayerPrefsSaveSystem.SetFloat(CheckpointYKey, position.y);
    }
}
