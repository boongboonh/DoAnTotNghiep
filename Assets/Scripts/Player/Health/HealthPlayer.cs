using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPlayer : BinhBehaviour
{

    [SerializeField] private int healthPlayerMax = 3;

    [SerializeField] private int nowHeal;
    [SerializeField] private GameObject soul;
    [SerializeField] private GameObject ringWavePlayerDie; // hieu ung chet
    [SerializeField] private GameObject ringRevivalPlayer; // hieuj ung hoi sinh

    private static HealthPlayer instance;
    public static HealthPlayer Instance { get => instance; }
    public int HealthPlayerInfor { get => healthPlayerMax;}
    public int NowHeal { get => nowHeal; }


    protected override void OnEnable()
    {
        fullHP();
        //chay hieu ung
        GameObject EffectPlayerRevival = Instantiate(ringRevivalPlayer, transform.position, Quaternion.identity);
        Destroy(EffectPlayerRevival, 1f);
    }

    protected override void Awake()
    {
        base.Awake();
        if (HealthPlayer.instance != null) Debug.LogError("Erro only Health Player run");
        HealthPlayer.instance = this;
    }


    protected override void Start()
    {
        base.Start();
        nowHeal = healthPlayerMax;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Bullet"))
        {
            Debug.Log("player take dame trigger");
            takeDame(1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("player take dame coli");
            takeDame(1);
        }
    }

    public bool checkHealth(int dameTake)
    {
        if (NowHeal > dameTake)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void healing()
    {
        if (NowHeal < healthPlayerMax)
        {
            nowHeal++;
        }
    }

    public void takeDame(int dameTake)
    {

        if (checkHealth(dameTake))
        {
            nowHeal-= dameTake;

            /* //chay hieu ung stop hit
             gameObject.GetComponent<HitStop>().StopTimeCall();*/
        }
        else
        {
            nowHeal = 0;
            diePlayer();
        }
    }

    public void addHealth()
    {
        //max 9 mau
        if(healthPlayerMax < 9)
        {
            healthPlayerMax++;
            nowHeal++;
        }
    }

    private void diePlayer()
    {
        soul.SetActive(false);
        gameObject.SetActive(false);
        GameObject EffectPlayerDieClone = Instantiate(ringWavePlayerDie, transform.position, Quaternion.identity);
        Destroy(EffectPlayerDieClone, 1f);


        //chuyeenr hamf ddee cammera goi ho
        MonoBehaviour camMono = Camera.main.GetComponent<MonoBehaviour>();
        camMono.StartCoroutine(playerDieCamCall());

    }

    //delay chuyen man
    IEnumerator playerDieCamCall()
    {
        yield return new WaitForSeconds(2f);
        gameObject.transform.position = new Vector2(PlayerPrefs.GetFloat("FirstPlayPosX"), PlayerPrefs.GetFloat("FirstPlayPosY"));
        soul.transform.position = gameObject.transform.position;
        gameObject.SetActive(true);
        soul.SetActive(true);
    }


    //dung cho cacs ddoi tuwong tieu diet player ngay lap tuc
    public void killPlayer()
    {
        takeDame(nowHeal);
    }

    //hoi toan bo hp khi o tru
    public void fullHP()
    {
        nowHeal = healthPlayerMax; // cai lai mau
    }
}
