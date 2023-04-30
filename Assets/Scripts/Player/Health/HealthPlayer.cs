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

    private bool canTakeDamage = true;
    [Header("sprite player")]
    [SerializeField] SpriteRenderer sprite;


    private static HealthPlayer instance;
    public static HealthPlayer Instance { get => instance; }
    public int HealthPlayerMax { get => healthPlayerMax;}
    public int NowHeal { get => nowHeal; }



    [Header("Name PlayerPrefs")]
    [SerializeField] private string NumberPlay = "NumberPlay";
    [SerializeField] string nameHPDataPlayerNow = "PlayerHPNow";
    [SerializeField] string nameHPDataPlayerMax = "PlayerHPMax";

    protected override void OnEnable()
    {
        //tat trinh nghe cammera
        Camera.main.GetComponent<AudioListener>().enabled = false;

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
        setHPPlayeAgain();
        Debug.Log("Player hP" + nowHeal);
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
        if (!canTakeDamage) return;

        if (checkHealth(dameTake))
        {
            //chay am thanh
            PlayerSounds.instance.HurtPlayerAudio();
          
            nowHeal-= dameTake;
            StartCoroutine(DamageFlicker());
        }
        else
        {


            //bat trinh nghe cammera
            Camera.main.GetComponent<AudioListener>().enabled = true;

            //chay am thanh
            PlayerSounds.instance.DeathPlayerAudio();

            nowHeal = 0;
            diePlayer();
        }
    }

    IEnumerator DamageFlicker()
    {
        canTakeDamage = false;
        for (int i = 0; i < 4; i++)
        {
            sprite.color = new Color(1f, 1f, 1f, .5f);
            yield return new WaitForSeconds(.1f);
            sprite.color = Color.white;
            yield return new WaitForSeconds(.1f);
        }
        canTakeDamage = true;
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

    public void diePlayer()
    {
        soul.SetActive(false);
        gameObject.SetActive(false);
        GameObject EffectPlayerDieClone = Instantiate(ringWavePlayerDie, transform.position, Quaternion.identity);
        Destroy(EffectPlayerDieClone, 1f);


        Invoke("playerDieIvokeCall", 2);        //goi ham hoi sinh sau 2 giay

    }

    //delay chuyen man
    private void playerDieIvokeCall()
    {
        gameObject.transform.position = new Vector2(PlayerPrefs.GetFloat("FirstPlayPosX"), PlayerPrefs.GetFloat("FirstPlayPosY"));        //dich chuyen den vi tri da luu
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


    //kiem tra lan choi va cai hp cu
    private void setHPPlayeAgain()
    {
        if (PlayerPrefs.GetInt(NumberPlay) != 1) //neu lan choi thu nhat
        {
            nowHeal = healthPlayerMax;
        }
        else
        {
            nowHeal = PlayerPrefs.GetInt(nameHPDataPlayerNow);
            healthPlayerMax = PlayerPrefs.GetInt(nameHPDataPlayerMax);
        }
    }
}
