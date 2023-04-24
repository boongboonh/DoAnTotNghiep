using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{

    public static PlayerSounds instance;

    [Header("player sound")]
    [SerializeField] private AudioSource runAudio;
    [SerializeField] private AudioSource jumpAudio;
    [SerializeField] private AudioSource dashAudio;

    [Header("Sword sound")]
    [SerializeField] private AudioSource swordChop;
    [SerializeField] private AudioSource swordHit;

    [Header("bullet sound")]
    [SerializeField] private AudioSource shoot;
    [SerializeField] private AudioSource bulletHit;

    [Header("create Checkpoint")]
    [SerializeField] private AudioSource checkpointSound;

    [Header("hurt and death")]
    [SerializeField] private AudioSource hurtPlayer;
    [SerializeField] private AudioSource deathPlayer;

    private void Awake()
    {
        instance = this;
    }

    public void PlayRunAudio()
    {
        if (!runAudio.isPlaying) //kiem tra am thanh co dang duoc phat hay khong
        {
            runAudio.Play(); // neu chua thi phat am thanh
        }
    }
    public void PlayJumpAudio()     //am thanh nhay
    {
        jumpAudio.Play();
    }   
    
    public void PlayDashAudio()     //am thanh dash
    {
        dashAudio.Play();
    }    
    public void SwordChopAudio()    //am thanh kiem chem
    {
        swordChop.Play();
    }    

    public void SwordHitAudio()     //am thanh kiem chem trung
    {
        swordHit.Play();
    }
    
    public void ShootAudio()     //am thanh ban dan
    {
        shoot.Play();
    }
    
    public void BulletHitAudio()     //am thanh dan trung
    {
        bulletHit.Play();
    }  

    public void CheckpointSoundAudio()     //am thanh dan trung
    {
        checkpointSound.Play();
    }
    public void HurtPlayerAudio()     //am thanh take dame
    {
        hurtPlayer.Play();
    }
    public void DeathPlayerAudio()     //am thanh chet
    {
        deathPlayer.Play();
    }

}
