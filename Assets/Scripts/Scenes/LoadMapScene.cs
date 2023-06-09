using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMapScene : MonoBehaviour
{
    //public string mapName = "";
    [SerializeField] string mapNameToMiniMap = "Map1";

    public float timeOut = 4f;      //thoi gian gioi han ra khoi map se pha huy map

    private float timer;
    private bool playerInRange;     //kiem tra player trong vung map do khong
    private bool isLoaded;          // bieens kiem tra map do da duoc load chua
    private void Start()
    {
        playerInRange = true;
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if(scene.name == mapNameToMiniMap)
                {
                    isLoaded = true;
                }
            }
        }    
    }


    //tai map khi nhan vatj vao khu vuwc
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            timer = 0f;
            LoadMap();
            openMiniMap();
        }
    }




    //thoat map khi nhan vaatj ra khoi khu vuc
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange) return;
        timer += Time.deltaTime;
        destroyMap();
    }

    //huy map khi du thoi gian
    private void destroyMap()
    {
        if (timer >= timeOut)
        {
            playerInRange = true;
            UnLoadMap();
        }
    }

    //tai map 
    private void LoadMap()
    {
        if (!isLoaded)
        {
            SceneManager.LoadSceneAsync(mapNameToMiniMap, LoadSceneMode.Additive);
            isLoaded = true;
        }
    }

    //huy map
    private void UnLoadMap()
    {
        if (isLoaded)
        {
            SceneManager.UnloadSceneAsync(mapNameToMiniMap);
            isLoaded = false;
        }
    }


    private void openMiniMap()
    {
        if (PlayerPrefs.GetInt(mapNameToMiniMap) == 1) return;  //neu bang 1 da kich hoat return
        PlayerPrefs.SetInt(mapNameToMiniMap, 1);                //luu bang 1 kich hoat
    }

}
