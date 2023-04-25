using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMenuController : MonoBehaviour
{
    [Header("sound background")]
    [SerializeField] List<AudioSource> listSound;
    bool startRandomSound = true;
    int numberSoundRun = -1;

    private void Start()
    {
        numberSoundRun = RandomNotDuplicate(listSound.Count);   //lay 1 bien dau vao
        playSound(numberSoundRun);                              //chay nhac
        startRandomSound = false;                               //khong cho random nua
    }
    private void FixedUpdate()
    {
        if (listSound[numberSoundRun].isPlaying) return;        //kiem tra da phat het nhac chua;
        startRandomSound = true;
        Debug.Log("ket thuc bai");
        //random so bai
        starRandom();

        //chay bai vua random
        playSound(numberSoundRun);
    }

    //chay sound truyen vao
    private void playSound(int numberSound)
    {
        listSound[numberSound].Play();
    }

    //random sound
    private void starRandom()
    {
        if (!startRandomSound) return;

        numberSoundRun = RandomNotDuplicate(listSound.Count);
        Debug.Log("so sound background : "+ listSound.Count);
    }

    //random khong trung lap
    private int RandomNotDuplicate( int numberRanger)
    {
        if (numberRanger <=1 ) return 0; //neu chi co 1 phan tu

        int newValue = Random.Range(0, numberRanger);

        if (numberSoundRun == newValue)
        {
            return RandomNotDuplicate(numberRanger);  //lap lai
        }
        else
        {
            Debug.Log("bai random : " + newValue);
            return newValue;
        }
    }

}
