using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleBoxEndGame : InteractableZone
{
    [SerializeField] private GameObject TextTuto;

    [SerializeField] AudioSource openBoxSound;
    [SerializeField] AudioSource closeBoxSound;
    private bool isActive = false;

    protected override bool CanInteract => !isActive;

    protected override void OnPlayerEnter(Collider2D player)
    {
        TextTuto.SetActive(true);

        //am thanh
        playSoundOpen();
    }

    protected override void OnInteract(Collider2D player)
    {
        isActive = true;
        TextTuto.SetActive(false);
        player.GetComponent<PlayerMove>().canMove = false;
        Invoke("loadEndGame2", 3f);

        Debug.Log(" chay story 2");
    }

    protected override void OnPlayerExit(Collider2D player)
    {
        TextTuto.SetActive(false);

        //am thanh
        playSoundClose();
    }
    private void loadEndGame2()
    {
        SceneManager.LoadScene("StoryEnd2");
    }


    //am thanh
    private void playSoundOpen()
    {
        AudioManager.PlaySource(openBoxSound, true);
    }
    private void playSoundClose()
    {
        AudioManager.PlaySource(closeBoxSound, true);
    }
}
