using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAniController : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.keepAnimatorControllerStateOnDisable = true;
    }

}
