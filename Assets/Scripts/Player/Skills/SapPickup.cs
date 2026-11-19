using GF.Save;
using UnityEngine;

/// <summary>
/// Cuc nhua thoi gian nhat duoc khi nguoi choi cham vao (trigger hoac va cham, tag Player).
/// Dung cho nhua quai/boss roi ra (do <see cref="SapDirector"/> tao) va cho nhua dat san trong map.
/// Dat <see cref="saveId"/> (duy nhat theo cuc) de nhat mot lan duy nhat qua cac lan tai/choi lai.
/// Co the them <see cref="ItemDrop"/> + Rigidbody2D vao prefab de co hieu ung roi va hut ve nguoi choi.
/// </summary>
public class SapPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int amount = 1;
    [Tooltip("De trong = cuc roi tu quai, khong luu. Dien id duy nhat = cuc dat san, chi nhat mot lan.")]
    [SerializeField] private string saveId = "";
    [SerializeField] private AudioSource collectSound;

    private bool collected;

    public int Amount => amount;
    public void SetAmount(int value) => amount = Mathf.Max(1, value);

    private string Key => SapEconomy.PickupKeyPrefix + saveId;
    private bool Persisted => !string.IsNullOrEmpty(saveId);

    private void Start()
    {
        if (Persisted && PlayerPrefsSaveSystem.LoadInt(Key, 0) == 1) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => TryCollect(collision.gameObject);

    private void TryCollect(GameObject who)
    {
        if (collected || !who.CompareTag("Player")) return;
        collected = true;

        if (Persisted)
        {
            PlayerPrefsSaveSystem.SetInt(Key, 1);
            PlayerPrefsSaveSystem.Flush();
        }
        if (collectSound != null && collectSound.clip != null)
            AudioManager.PlayClipAt(collectSound.clip, transform.position);

        SapDirector.Collect(amount, transform.position);
        Destroy(gameObject);
    }
}
