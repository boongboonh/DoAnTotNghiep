using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] string NameParaActiveAnimator = "ActiveSpike";
    [SerializeField] string NameCloseAnimator = "SpikeClose";
    bool isPush = false;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isPush) return;     //neu chua cham thi khong chay
        if (collision.CompareTag("Player"))
        {
            closeSpike();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPush) return;
        if (collision.CompareTag("Player"))
        {
            isPush = true;
            transform.parent.GetComponent<Animator>().SetTrigger(NameParaActiveAnimator);
        }
    }


    public void closeSpike()
    {
        StartCoroutine(delayClose());
    }

    IEnumerator delayClose()
    {
        yield return new WaitForSeconds(2f);
        transform.parent.GetComponent<Animator>().Play(NameCloseAnimator);
        yield return new WaitForSeconds(2f);
        isPush = false;
    }

}
