using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private float Speed;
    private bool RestoreTime;
    [SerializeField] private float ChangeTime = 0.01f; //thoi giam bi scale ve muc nay
    [SerializeField] private float RestoreSpeed = 2f; // toos ddo hoi time scale
    [SerializeField] private float timeDelay = 0.2f; // thoi gian lam cham

    // effect
    /*public GameObject ImpactEffect;
    private Animator Anim;
*/
    private void Start()
    {
        RestoreTime = false;
        //Anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (RestoreTime)
        {
            if (Time.timeScale < 1f)
            {
                Time.timeScale += Time.deltaTime * Speed;
            }
            else
            {
                Time.timeScale = 1f;
                RestoreTime = false;
            }
        }
    }

    // function to call an other script
    // goi ben health khi nhan dame
    public void StopTimeCall()
    {
        StopTime(ChangeTime, RestoreSpeed, timeDelay);
    }
    private void StopTime(float ChangeTime, float RestoreSpeed,float Delay)
    {
        Speed = RestoreSpeed;
        if (Delay > 0)
        {
            StopCoroutine(StartTimeAgain(Delay));
            StartCoroutine(StartTimeAgain(Delay));
        }
        else
        {
            RestoreTime = true;
        }

        //sinh hieu ung
        /*Instantiate(ImpactEffect,transform.position, Quaternion.identity);
        Anim.SetBool("takeDame", true);*/


        Time.timeScale = ChangeTime;
    }

    IEnumerator StartTimeAgain(float amt)
    {
        yield return new WaitForSecondsRealtime(amt);
        RestoreTime = true;
    }

}
