using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    public static WeaponAttack instance;

    [SerializeField] private string ANIMATORNAME = "Idle";
    [HideInInspector] public bool canReceiveInput;
    [HideInInspector] public bool inputReceived;

    [SerializeField] private float timeEndCombo = .5f;

    private float time = 0f;
    private Animator animator;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
       
        canReceiveInput = true;
    }
    private void Update()
    {
        attack();
        coldownEndCombo();
    }

    private void attack()
    {
        if (!canReceiveInput) return;

        if (Input.GetMouseButtonDown(0))
        {
            inputReceived = true;
            canReceiveInput = false;
        }
    }

    public void inputManager()
    {
        if (!canReceiveInput)
        {
            canReceiveInput = true;
        }
        else
        {
            canReceiveInput = false;
        }
    }
    //tinh coldown delay ddoi combo
    public void coldownEndCombo()
    {
        if (time >= timeEndCombo + 1f) return;

        if (time >= timeEndCombo)
        {
            animator.SetTrigger(ANIMATORNAME);
        }
        time += Time.deltaTime;
    }


    //goi khi ket thuc 1 combo
    public void startRunColdown()
    {
        time = 0f;
    }
}
