using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    private Rigidbody2D _rb;
    public float dropForce = 3f; // item nay nen khi rot ra
    //public float rangerCollect = 1f;
    Transform Player;
    public float DistanceCollect = 2f;
    public float speedCollect = 5f;
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.AddForce(new Vector2(Random.Range(-0.3f, 0.3f), 1) * dropForce, ForceMode2D.Impulse);
        Player = GetPosPlayer.Instance.PlayerPos;
    }
    private void Update()
    {
        Destroy(gameObject, 60f);
        Playercollect();
    }

    void Playercollect()
    {
        if (Player != null)
        {
            if (Vector2.Distance(transform.position, Player.position) <= DistanceCollect)
            {
                transform.position = Vector2.MoveTowards(transform.position, Player.position, speedCollect * Time.deltaTime);
            }
        }
    }

}
