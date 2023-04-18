using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*public class healthBar : MonoBehaviour
{
    [SerializeField] Slider slider;

    private int ValueHealth;

    [SerializeField] GameObject boss;
    public int hp = 20;

    private int currentHP = 20;
    private int updateHP = 20;

    private void Start()
    {
        int hp = boss.GetComponent<bossHealth>().HealthBossMax;
        setMaxHealth(hp);
    }

    private void Update()
    {
        if (boss.GetComponent<bossHealth>().healthEnemy == currentHP) return;       //neu hp trung thi bo qua
        updateHP = boss.GetComponent<bossHealth>().healthEnemy;                     //luu hp hien tai
        currentHP = updateHP;                                                       //cap nhat hp moi
        HealthValueBoss = updateHP;                                                 //hien thi
    }

    public int HealthValueBoss
    {
        get { return ValueHealth; }
        set
        {
            if (ValueHealth == value) return;       //neu gia tri lap thi khong cap nhat
            ValueHealth = value;
            OnPropertyChanged(nameof(HealthValueBoss));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (propertyName == "HealthValueBoss")
        {
            setHealth(HealthValueBoss);
        }
        Debug.Log("run update ui bar");
    }

    private void setMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    private void setHealth(int health)
    {
        slider.value = health;
    }
}*/


public class healthBar : MonoBehaviour
{
    [SerializeField] Slider slider;

    [SerializeField] private GameObject boss;
    private int currentHP = 0;
    private int _healthValueBoss;

    public delegate void HealthChangedEventHandler(int newHealthValue);

    public event HealthChangedEventHandler OnHealthChanged;

    private void Start()
    {
        int hp = boss.GetComponent<bossHealth>().HealthBossMax;
        setMaxHealth(hp);
    }

    private void Update()
    {
        int updateHP = boss.GetComponent<bossHealth>().healthEnemy;
        if (updateHP == currentHP) return;
        currentHP = updateHP;
        HealthValueBoss = updateHP;
    }

    public int HealthValueBoss
    {
        get { return _healthValueBoss; }
        set
        {
            if (value != _healthValueBoss)
            {
                _healthValueBoss = value;
                setHealth(_healthValueBoss);
                OnHealthChanged?.Invoke(_healthValueBoss);
            }
        }
    }

    private void setMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    private void setHealth(int health)
    {
        slider.value = health;
    }
}
