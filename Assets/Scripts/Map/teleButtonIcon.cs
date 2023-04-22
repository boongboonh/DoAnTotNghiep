using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleButtonIcon : MonoBehaviour
{
    [SerializeField] GameObject teleportButton;
    [SerializeField] int numbertree;

    public void OpenTelebutton()
    {
        teleportButton.SetActive(true);
        teleportButton.GetComponent<TeleButton>().treeNumber = numbertree;//truyen vao chi so so cay
    }

}
