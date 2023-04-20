using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] string nameHPDataPlayerNow = "PlayerHPNow";
    [SerializeField] string nameHPDataPlayerMax = "PlayerHPMax";

    [SerializeField] string nameMPDataPlayerNow = "PlayerMPNow";
    [SerializeField] string nameMPDataPlayerMax = "PlayerMPMax";

    private void OnApplicationQuit()
    {

        //luu thong tin nhan vat khi thoat
       

        PlayerPrefs.SetInt(nameHPDataPlayerNow, HealthPlayer.Instance.NowHeal);
        PlayerPrefs.SetInt(nameHPDataPlayerMax, HealthPlayer.Instance.HealthPlayerMax);
        
        PlayerPrefs.SetInt(nameMPDataPlayerNow, ManaManager.Instance.NowMana);
        PlayerPrefs.SetInt(nameMPDataPlayerMax, ManaManager.Instance.ManaPlayerMax);

    }
}
