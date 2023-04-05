using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPosPlayer : MonoBehaviour
{
    protected static GetPosPlayer instance;
    public static GetPosPlayer Instance { get => instance; }// truyen du lieu


    [SerializeField] protected Transform playerPos;
    public Transform PlayerPos { get => playerPos; }// truyen du lieu vi tri nguowi choi

    /*
        public static GetPosPlayer Instance { get; private set; }

        public Transform PlayerPos;
    */

    private void Awake()
    {
        if (GetPosPlayer.instance != null) Debug.LogError("Erro only getPosPlayer run");
        GetPosPlayer.instance = this;
    }

    /*private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Another instance of GetPosPlayer already exists. This instance will be destroyed.");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }*/

    private void Update()
    {
        this.FindPlayerPos();
    }

    protected virtual void FindPlayerPos()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        playerPos = player.transform;
    }
}
