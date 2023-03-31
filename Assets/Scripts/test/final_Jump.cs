using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class final_Jump : MonoBehaviour
{
    public AnimationCurve curve;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float heightY = 3.0f;
    GameObject player;
    public IEnumerator Curve(Vector2 start, Vector2 target)
    {
        float timePassed = 0f;
        Vector2 end = target;
        while(timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            float heightT = curve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);
            transform.position = Vector2.Lerp(start, end, linearT) + new Vector2(0f, height);
            yield return null;
        }

    }

    void Start()
    {
        player = GameObject.Find("Player");
      
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(Curve(Vector2.zero,player.transform.position));
        }
    }
}
