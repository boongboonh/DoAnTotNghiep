using GF.Save;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dieu phoi nguon nhua thoi gian. Tu dang ky luc chay game (khong can dat vao scene):
/// quai chet -> ti le roi, boss chet -> roi chac mot lan. Nhua roi ra la <see cref="SapPickup"/> lay tu
/// Resources/<see cref="SapEconomy.PickupResourcePath"/>; neu chua co prefab thi cong thang vao nguoi choi.
/// UI nghe <see cref="Gained"/> de hien thong bao "+N nhua".
/// </summary>
public static class SapDirector
{
    /// <summary>(so nhua, vi tri the gioi) - ban moi khi nguoi choi that su nhan nhua.</summary>
    public static event Action<int, Vector3> Gained;

    private static SapPickup pickupPrefab;
    private static bool prefabLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        prefabLoaded = false;
        pickupPrefab = null;
        EnemyHealth.Died -= OnEnemyDied;
        EnemyHealth.Died += OnEnemyDied;
        bossHealth.Died -= OnBossDied;
        bossHealth.Died += OnBossDied;
    }

    /// <summary>Cong nhua vao nguoi choi va bao cho UI. Pickup va cac nguon khac goi vao day.</summary>
    public static void Collect(int amount, Vector3 position)
    {
        if (amount <= 0) return;
        SkillProgressManager.Instance.AddSap(amount);
        Gained?.Invoke(amount, position);
    }

    /// <summary>Tao nhua tai vi tri (cuc nhat duoc neu co prefab, khong thi cong thang).</summary>
    public static void Drop(int amount, Vector3 position)
    {
        if (amount <= 0) return;
        var prefab = GetPrefab();
        if (prefab == null)
        {
            Collect(amount, position);
            return;
        }
        var drop = UnityEngine.Object.Instantiate(prefab, position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0.8f, 0f), Quaternion.identity);
        drop.SetAmount(amount);
    }

    private static SapPickup GetPrefab()
    {
        if (!prefabLoaded)
        {
            pickupPrefab = Resources.Load<SapPickup>(SapEconomy.PickupResourcePath);
            prefabLoaded = true;
        }
        return pickupPrefab;
    }

    private static void OnEnemyDied(EnemyHealth enemy)
    {
        if (enemy == null || !SapEconomy.RollEnemyDrop()) return;
        Drop(SapEconomy.EnemyAmount, enemy.transform.position);
    }

    private static void OnBossDied(bossHealth boss)
    {
        if (boss == null) return;
        string key = SapEconomy.BossKeyPrefix + SceneManager.GetActiveScene().name + "_" + boss.name;
        if (PlayerPrefsSaveSystem.LoadInt(key, 0) == 1) return;
        PlayerPrefsSaveSystem.SetInt(key, 1);
        PlayerPrefsSaveSystem.Flush();
        Drop(SapEconomy.BossAmount, boss.transform.position);
    }
}
