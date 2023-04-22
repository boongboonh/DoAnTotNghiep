using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleButton : MonoBehaviour
{
    public int treeNumber = -1;         //cay so bao nhieu se duoc truyen vao

    [SerializeField] GameObject MapController;

    [Header("name tree 1")]
    [SerializeField] string TelePostX1 = "tree1PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY1 = "tree1PostY";   //vi tri y dich chuy player
    
    
    [Header("name tree 2")]
    [SerializeField] string TelePostX2 = "tree2PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY2 = "tree2PostY";   //vi tri y dich chuy player



    public void TeleTreePos()
    {
        if (treeNumber == -1) return;
        if(treeNumber == 1)
        {
            TeleToTree1();
        }

        if(treeNumber == 2)
        {
            TeleTotree2();
        }

        //bo xung
    }

    private void TeleToTree1()
    {
        findPlayer(TelePostX1, TelePostY1);
        hideButton();
        exit();
    }
    private void TeleTotree2()
    {
        findPlayer(TelePostX2, TelePostY2);
        hideButton();
        exit();
    }

    //bo xung them cac cay


    private void exit()//tat hien thi mini map
    {
        MapController.GetComponent<MapController>().ExitMinimap();
    }

    private void findPlayer(string posX, string posY)
    {
        //dich chuyen player
        GameObject player = GameObject.FindWithTag("Player");
        transformObject(player, posX, posY);
        
        //dich chuyen bullet
        GameObject BulletPlayer = GameObject.FindWithTag("BulletPlayer");
        transformObject(BulletPlayer, posX, posY);

    }

    //dich chuyen doi tuong tim duoc den vi tri
    private void transformObject(GameObject other, string posX, string posY)
    {
        other.transform.parent.position = new Vector2(PlayerPrefs.GetFloat(posX), PlayerPrefs.GetFloat(posY));
    }


    //an nut dich chuyen
    private void hideButton()
    {
        gameObject.SetActive(false);
    }

}
