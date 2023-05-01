using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBoss : MonoBehaviour
{
    private string nameState = "StateDoor";
    Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OpenDoorBoss()
    {
        animator.SetInteger(nameState, 0);
    }

    private void CloseDoorBoss()
    {
        animator.SetInteger(nameState, 1);
    }
    
    private void ClosedDoorBoss()
    {
        animator.SetInteger(nameState, 2);
    }

}
