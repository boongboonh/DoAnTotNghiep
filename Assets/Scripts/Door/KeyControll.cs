using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyControll : InteractableZone
{

    [SerializeField] string nameDoorPlayerPrefs = "doorKey1";
    [SerializeField] string nameAnimatorKey = "OpenKey";
    [SerializeField] string nameAnimatorDoor = "OpenDoor";
    [SerializeField] string nameAniKeyOpened = "OpenedKey";
    [SerializeField] string nameAniDoorOpened = "OpenedDoor";


    [SerializeField] bool isOpen = false;

    [SerializeField] private GameObject door;
    [SerializeField] private GameObject clickButton;

    
    Animator animator;
    [Header("sound door")]
    [SerializeField] private AudioSource KeyOpenSound;
    [SerializeField] private AudioSource DoorOpenSound;


    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log(PlayerPrefsSaveSystem.LoadInt(nameDoorPlayerPrefs));   //loi
        if (PlayerPrefsSaveSystem.LoadInt(nameDoorPlayerPrefs) == 0) return;
        isOpen = true;                                              //cua da mo
        animator.SetTrigger(nameAniKeyOpened);                      //chay ani mo
        door.GetComponent<Animator>().SetTrigger(nameAniDoorOpened);//chay ani mo
    }

    protected override bool CanInteract => !isOpen;

    protected override void OnPlayerEnter(Collider2D player)
    {
        if (isOpen) return;
        clickButton.SetActive(true);
    }

    protected override void OnInteract(Collider2D player)
    {
        PlayerPrefsSaveSystem.SetInt(nameDoorPlayerPrefs, 1);
        openDoor();
    }

    private void openDoor()
    {
        AudioManager.PlaySource(KeyOpenSound);
        animator.SetTrigger(nameAnimatorKey);//chay animation mo khoa

        AudioManager.PlaySource(DoorOpenSound);
        door.GetComponent<Animator>().SetTrigger(nameAnimatorDoor); //chay animation mo cua
    }

    protected override void OnPlayerExit(Collider2D player)
    {
        clickButton.SetActive(false);
    }


    public void opened()
    {
        isOpen = true;
    }
}
