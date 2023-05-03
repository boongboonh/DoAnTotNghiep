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
    
    
    [Header("name tree 3")]
    [SerializeField] string TelePostX3 = "tree3PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY3 = "tree3PostY";   //vi tri y dich chuy player

    [Header("name tree 4")]
    [SerializeField] string TelePostX4 = "tree4PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY4 = "tree4PostY";   //vi tri y dich chuy player

    [Header("name tree 5")]
    [SerializeField] string TelePostX5 = "tree5PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY5 = "tree5PostY";   //vi tri y dich chuy player

    public void TeleTreePos()
    {
        if (treeNumber == -1) return;

        switch (treeNumber)
        {
            case 1:
                {
                    TeleToTree(TelePostX1, TelePostY1);
                    break;
                }
            case 2:
                {
                    TeleToTree(TelePostX2, TelePostY2);
                    break;
                }
            case 3:
                {
                    TeleToTree(TelePostX3, TelePostY3);
                    break;
                }
            case 4:
                {
                    TeleToTree(TelePostX4, TelePostY4);
                    break;
                }
            case 5:
                {
                    TeleToTree(TelePostX5, TelePostY5);
                    break;
                }
            default:
                {
                    Debug.Log("loi tham so tree tele");
                    break;
                }
        }
    }

   /* private void TeleToTree1()
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
    private void TeleTotree2()
    {
        findPlayer(TelePostX2, TelePostY2);
        hideButton();
        exit();
    }
*/
    private void TeleToTree(string xTree, string yTree)
    {
        findPlayer(xTree, yTree);
        hideButton();
        exit();
    }

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
        other.transform.position = new Vector2(PlayerPrefs.GetFloat(posX), PlayerPrefs.GetFloat(posY));
    }

    //an nut dich chuyen
    private void hideButton()
    {
        gameObject.SetActive(false);
    }
}
