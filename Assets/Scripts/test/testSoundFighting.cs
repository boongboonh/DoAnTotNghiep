using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSoundFighting : MonoBehaviour
{
    public void fighting()
    {
        EventManager.Instance.onMusicFighting();
    }
    public void stopFighting()
    {
        EventManager.Instance.offMusicFighting();
    }
}
