using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaser : MonoBehaviour
{
    [SerializeField] LayerMask layerMaskToHit;
    [SerializeField] GameObject cube;

    [Header("sound lazer")]
    [SerializeField] AudioSource laserSound;


    private void Update()
    {

        //chay am thanh
        playSoundLaser();

        //tinhs lai goc

        Vector2 dir = transform.position - cube.transform.position;


        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 50f, layerMaskToHit);
        if (hit.collider == null)
        {
            transform.localScale = new Vector3(3f, transform.localScale.y,1);
            return;
        }

        if (hit.collider.tag == "Ground")
        {
            scaleLaser(hit);
        }
        

        if(hit.collider.tag == "Player")
        {
            scaleLaser(hit);
            Debug.Log("laser hit player");
            hit.collider.gameObject.GetComponent<HealthPlayer>().killPlayer();
        }
    }

    //chay am thanh
    private void playSoundLaser()
    {
        if (laserSound.isPlaying) return;
        laserSound.Play();

    }
    private void scaleLaser(RaycastHit2D hit)
    {
        transform.localScale = new Vector3(hit.distance / 10, transform.localScale.y, 1);
    }
}
