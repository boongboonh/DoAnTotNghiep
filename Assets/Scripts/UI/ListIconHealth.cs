using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ListIconHealth : ListIcon
{

    private int playerHealthClone;
    private int playerHealthNowClone;

    protected override void Start()
    {
        base.Start();
        AddIconHealth();
    }

    private void Update()
    {
        this.UpdateIconHealth();
    }
    protected virtual void AddIconHealth()
    {
        playerHealthClone = HealthPlayer.Instance.HealthPlayerMax;
        playerHealthNowClone = playerHealthClone;

        for (int i = 0; i < playerHealthClone; i++)
        {
            icons[i].gameObject.SetActive(true);
        }
    }

    protected virtual void UpdateIconHealth()// cap nhat mau khi co thay doi du lieu tu hp nguwoi choi
    {
        if (playerHealthNowClone == HealthPlayer.Instance.NowHeal) return;

            playerHealthNowClone = HealthPlayer.Instance.NowHeal;

            for(int i=0; i< playerHealthClone; i++)
            {
                //icons[i].gameObject.SetActive(true);
                if (i < HealthPlayer.Instance.NowHeal)
                {
                    icons[i].GetComponent<Image>().color = Color.white;
                    LeanTween.cancel(icons[i]);
                    LeanTween.scale(icons[i], new Vector3(1.5f, 1.5f, 1f), 0.25f).setEaseShake().setIgnoreTimeScale(true);
                }
                else
                {
                    //icons[i].gameObject.SetActive(false);
                    icons[i].GetComponent<Image>().color = Color.gray;
                }
            }

            // khi chi so mau chua nhan vat thay doi cap nhat lai icon ui
        if (playerHealthClone == HealthPlayer.Instance.HealthPlayerMax) return;
            AddIconHealth();
    }
}
