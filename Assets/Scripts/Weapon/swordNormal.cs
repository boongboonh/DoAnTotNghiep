using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordNormal : MonoBehaviour
{
    [SerializeField] private int ATK = 1;

    [SerializeField] private GameObject effectChop;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
           

            //neeu cham vao quai, quais awn 1 dame
            Debug.Log("enemyTakeDame");
            Vector2 posHit = gameObject.GetComponent<Collider2D>().ClosestPoint(collision.transform.position);

            GameObject cloneEffectChop = Instantiate(effectChop, posHit, Quaternion.identity);
            Destroy(cloneEffectChop, 1f);

            collision.GetComponent<EnemyHealth>().EnemyTakeDame(ATK);

            //am thanh
            PlayerSounds.instance.SwordHitAudio();
        }
    }
}
