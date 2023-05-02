using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        int updateHP = boss.GetComponent<bossHealth>().healthBoss;
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
