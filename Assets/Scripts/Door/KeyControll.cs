using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyControll : MonoBehaviour
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
        Debug.Log(PlayerPrefs.GetInt(nameDoorPlayerPrefs));   //loi
        if (PlayerPrefs.GetInt(nameDoorPlayerPrefs) == 0) return;
        isOpen = true;                                              //cua da mo
        animator.SetTrigger(nameAniKeyOpened);                      //chay ani mo
        door.GetComponent<Animator>().SetTrigger(nameAniDoorOpened);//chay ani mo
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpen) return;
        if (collision.CompareTag("Player"))
        {
            clickButton.SetActive(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isOpen) return;
        if (collision.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                PlayerPrefs.SetInt(nameDoorPlayerPrefs, 1);
                openDoor();
            }
        }
    }

    private void openDoor()
    {
        KeyOpenSound.Play();
        animator.SetTrigger(nameAnimatorKey);//chay animation mo khoa

        DoorOpenSound.Play();
        door.GetComponent<Animator>().SetTrigger(nameAnimatorDoor); //chay animation mo cua
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            clickButton.SetActive(false);
        }
    }


    public void opened()
    {
        isOpen = true;
    }
}
