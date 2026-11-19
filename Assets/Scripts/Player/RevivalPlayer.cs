using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevivalPlayer : MonoBehaviour
{

    //ham hoi hoi sinh
    [SerializeField] private GameObject soul;
    private void OnEnable()
    {
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
        Invoke("reBackGravity", 1f);
    }

    private void reBackGravity()
    {
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 3f;
    }

    private void Start()
    {
        gameObject.transform.position = new Vector2(PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosX"), PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosY") + 1f);// nang do cao len
        if (soul != null) soul.transform.position = gameObject.transform.position;
    }
}
