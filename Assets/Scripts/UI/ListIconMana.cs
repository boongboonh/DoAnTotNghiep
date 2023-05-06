using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ListIconMana : ListIcon
{

    private int playerManaClone;
    private int playerManaNowClone;

    protected override void Start()
    {
        base.Start();
        AddIconMana();
    }

    private void Update()
    {
        this.UpdateIconMana();
    }
    protected virtual void AddIconMana()
    {
        playerManaClone = ManaManager.Instance.ManaPlayerMax;
        playerManaNowClone = playerManaClone;

        for (int i = 0; i < playerManaClone; i++)
        {
            icons[i].gameObject.SetActive(true);
        }
    }

    protected virtual void UpdateIconMana()// cap nhat mau khi co thay doi du lieu tu hp nguwoi choi
    {
        if (playerManaNowClone == ManaManager.Instance.NowMana) return;
        
            playerManaNowClone = ManaManager.Instance.NowMana;

            for(int i=0; i< playerManaClone; i++)
            {
                //icons[i].gameObject.SetActive(true);
                if (i < ManaManager.Instance.NowMana)
                {
                    icons[i].GetComponent<Image>().color = Color.white;
                    LeanTween.scale(icons[i], new Vector3(1.5f, 1.5f, 1f), 0.25f).setEaseShake();
                }
                else
                {
                    //icons[i].gameObject.SetActive(false);
                    icons[i].GetComponent<Image>().color = Color.gray;
                }
            }

            //khi chi so mana goc cua nhan vat thay doi cap nhat lai
        if (playerManaClone == ManaManager.Instance.ManaPlayerMax) return;
            AddIconMana();
    }
}
