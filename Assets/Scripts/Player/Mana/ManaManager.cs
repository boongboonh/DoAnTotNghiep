using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaManager : BinhBehaviour
{
    [SerializeField] private int manaPlayerMax = 3;

    [SerializeField] private int nowMana;

    private static ManaManager instance;

    public static ManaManager Instance { get => instance; }
    public int ManaPlayerMax { get => manaPlayerMax;}
    public int NowMana { get => nowMana; }

    [Header("Name PlayerPrefs")]
    [SerializeField] private string NumberPlay = "NumberPlay";
    [SerializeField] string nameMPDataPlayerNow = "PlayerMPNow";
    [SerializeField] string nameMPDataPlayerMax = "PlayerMPMax";

    protected override void OnEnable()
    {
        fullMana();
    }

    protected override void Awake()
    {
        base.Awake();
        if (ManaManager.instance != null) Debug.LogError("Erro only mana Player run");
        ManaManager.instance = this;
    }
    protected override void Start()
    {
        setMPPlayeAgain();

    }
    /*public bool checkMana()
    {
        if(nowMana >=0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/
    public void ManaHealing()
    {
        if (nowMana < manaPlayerMax)
        {
            nowMana++;
        }
    }

    public void UseMana()
    {
        if (nowMana > 0)
        {
            nowMana--;
        }
    }

    public void addMana()
    {
        if (manaPlayerMax < 9)
        {
            manaPlayerMax++;
            nowMana++;
        }
    }

    public void fullMana()
    {
        nowMana = manaPlayerMax;
    }


    //kiem tra lan choi va cai mana cu
    private void setMPPlayeAgain()
    {
        if (PlayerPrefs.GetInt(NumberPlay) != 1) //neu lan choi thu nhat
        {
            nowMana = manaPlayerMax;
        }
        else
        {
            nowMana = PlayerPrefs.GetInt(nameMPDataPlayerNow);
            manaPlayerMax = PlayerPrefs.GetInt(nameMPDataPlayerMax);
        }
    }

}
