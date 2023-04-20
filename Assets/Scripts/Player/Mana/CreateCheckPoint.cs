using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCheckPoint : MonoBehaviour
{
    public float timeHolder = 1f;
    public bool isDistanceToCreate = true;

    private float timer;
    private bool press = false;
    private GameObject checkPointFind;
    ManaManager mana;
    private void Start()
    {
        mana = GetComponent<ManaManager>();
        gameObject.transform.parent.position = new Vector2(PlayerPrefs.GetFloat("FirstPlayPosX"), PlayerPrefs.GetFloat("FirstPlayPosY"));

        timer = timeHolder;
        
       
    }

    private void Update()
    {
        //kiem tra neu diem tiep theo qua gan se ko tao moi
        if (!isDistanceToCreate) return;

        //check mat dat
        if (!GetComponent<PlayerMove>().IsGround()) return;

        //check mana
        if (mana.NowMana <= 0) return;


        //kiem tra nhan phim + chay time giu
        if (Input.GetKeyDown(KeyCode.F))
        {
            press = true;
            timer = timeHolder;
        }

        //kiem tra giu phims
        if (Input.GetKey(KeyCode.F) && timer > 0)
        {
            timer -= Time.deltaTime;
        }

        //kiem tra nha nut
        if (Input.GetKeyUp(KeyCode.F))
        {
            press = false;
        }

        if (timer <= 0 && press)
        {
            press = false;
            mana.UseMana();

            //luu vi tri 
            PlayerPrefs.SetFloat("FirstPlayPosX", gameObject.transform.position.x);
            PlayerPrefs.SetFloat("FirstPlayPosY", gameObject.transform.position.y);
        }

    }


}
