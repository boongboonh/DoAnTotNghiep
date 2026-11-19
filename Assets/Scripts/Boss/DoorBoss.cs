using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBoss : MonoBehaviour
{
    private string nameState = "StateDoor";
    Animator animator;
    bool canActive = true;
    [Header("audio sound")]
    [SerializeField] AudioSource OpenDoorSound;
    [SerializeField] AudioSource CloseDoorSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (PlayerPrefsSaveSystem.LoadInt("BoosDie") == 1)
        {
            canActive = false;
            OpenedDoorBoss();                   //neu boss da bi tieu diet thi mo cua
            return;
        }else
        {
            canActive = true;
        }

        EventManager.Instance.DoorClosed += CloseDoorBoss;
        EventManager.Instance.DoorOpened += OpenDoorBoss;

    }

    public void OpenDoorBoss()
    {
        if (canActive == false) return;

        //chay am thanh
        SoundOpen();
        animator.SetInteger(nameState, 0);
    }

    public void CloseDoorBoss()
    {
        if (canActive == false) return;

        if (!checkBoss()) return;

        //chay am thanh
        SoundClose();
        animator.SetInteger(nameState, 1);
    }
    
    private void OpenedDoorBoss()
    {
        animator.SetInteger(nameState, 2);
    }

    private void OnDestroy()
    {
        EventManager.Instance.DoorClosed -= CloseDoorBoss;
        EventManager.Instance.DoorOpened -= OpenDoorBoss;
    }

    private void SoundOpen()
    {
        AudioManager.PlaySource(OpenDoorSound, true);
    }
    private void SoundClose()
    {
        AudioManager.PlaySource(CloseDoorSound, true);
    }

    private bool checkBoss()
    {
        if (PlayerPrefsSaveSystem.LoadInt("BoosDie") == 1) return false;
        else return true;
    }
}
