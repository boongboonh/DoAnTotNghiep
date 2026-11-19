using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class telePoint : InteractableZone
{
    [Header("name data")]
    [SerializeField] string nameTele = "teleTree1";     //ten luu du lieu
    [SerializeField] string TelePostX = "tree1PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY = "tree1PostY";   //vi tri y dich chuy player


    [Header("setting")]
    [SerializeField] GameObject clickTuto;
    [SerializeField] GameObject EffectPower;
    private bool isActive = false;

    [Header("audio sound tele tree")]
    [SerializeField] private AudioSource openTree;

    private void Start()
    {
        if (PlayerPrefsSaveSystem.LoadInt(nameTele) != 1) return;
        isActive = true;
        EffectPower.SetActive(true);
    }

    protected override bool CanInteract => !isActive;

    protected override void OnPlayerEnter(Collider2D player)
    {
        if (isActive) return;
        clickTuto.SetActive(true);
    }

    protected override void OnInteract(Collider2D player)
    {
        isActive = true;
        EffectPower.SetActive(true);
        PlayerPrefsSaveSystem.SetInt(nameTele, 1);

        //chay am thanh
        AudioManager.PlaySource(openTree);

        clickTuto.SetActive(false);

        //luu thong tin vi tri cay de teleport
        PlayerPrefsSaveSystem.SetFloat(TelePostX, transform.parent.position.x);
        PlayerPrefsSaveSystem.SetFloat(TelePostY, transform.parent.position.y + 1);
    }

    protected override void OnPlayerExit(Collider2D player)
    {
        clickTuto.SetActive(false);
    }
}
