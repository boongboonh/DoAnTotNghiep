using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCuaNhayMiniHealth : EnemyHealth
{
    [SerializeField] private float timelife = 15f;// thoi gian ton tai
    private float timer = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        timer = 0f;
    }

    protected void Update()
    {
        timeLife();
    }

    void timeLife()
    {
        timer += Time.deltaTime;
        if(timer>= timelife)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void dropItem()
    {
        for (int i = 0; i < ItemsDrop.Length; i++)
        {
            if (Random.Range(0, 3) == 1)
            {
                Instantiate(ItemsDrop[i], transform.position + new Vector3(Random.Range(-0.7f, 0.7f), 1, 0), Quaternion.identity);
            }
        }
    }
}
