using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] string nameHPDataPlayerNow = "PlayerHPNow";
    [SerializeField] string nameHPDataPlayerMax = "PlayerHPMax";

    [SerializeField] string nameMPDataPlayerNow = "PlayerMPNow";
    [SerializeField] string nameMPDataPlayerMax = "PlayerMPMax";

    [SerializeField] string playerPosX = "FirstPlayPosX";
    [SerializeField] string playerPosY = "FirstPlayPosY";

    private void OnApplicationQuit()
    {

        //luu thong tin nhan vat khi thoat
        Debug.Log("luu thong tin game va thoat game");

        //luu hp
        PlayerPrefs.SetInt(nameHPDataPlayerNow, HealthPlayer.Instance.NowHeal);
        PlayerPrefs.SetInt(nameHPDataPlayerMax, HealthPlayer.Instance.HealthPlayerMax);
        
        //luu mana
        PlayerPrefs.SetInt(nameMPDataPlayerNow, ManaManager.Instance.NowMana);
        PlayerPrefs.SetInt(nameMPDataPlayerMax, ManaManager.Instance.ManaPlayerMax);

        //luu vi tri
        PlayerPrefs.SetFloat(playerPosX, GetPosPlayer.Instance.PlayerPos.position.x);
        PlayerPrefs.SetFloat(playerPosY, GetPosPlayer.Instance.PlayerPos.position.y + 1); //tang khoang cach len cao 1 don vi
    }
}
