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


    protected override void OnEnable()
    {
        nowMana = manaPlayerMax;
    }

    protected override void Awake()
    {
        base.Awake();
        if (ManaManager.instance != null) Debug.LogError("Erro only mana Player run");
        ManaManager.instance = this;
    }
    protected override void Start()
    {
        nowMana = manaPlayerMax;

    }
    public bool checkMana()
    {
        if(nowMana >=0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
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

}
