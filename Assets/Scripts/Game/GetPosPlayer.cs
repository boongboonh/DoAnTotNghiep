using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPosPlayer : MonoBehaviour
{
    protected static GetPosPlayer instance;
    public static GetPosPlayer Instance { get => instance; }// truyen du lieu

    [SerializeField] private Transform playerGameObject;

    protected Transform playerPos;
    public Transform PlayerPos { get => playerPos; }// truyen du lieu vi tri nguowi choi

    private void Awake()
    {
        if (GetPosPlayer.instance != null) Debug.LogError("Erro only getPosPlayer run");
        GetPosPlayer.instance = this;
    }

    private void Update()
    {
        this.FindPlayerPos();
    }

    protected virtual void FindPlayerPos()
    {
        Transform player = playerGameObject;
        if (player == null) return;
        playerPos = player;
    }
}
