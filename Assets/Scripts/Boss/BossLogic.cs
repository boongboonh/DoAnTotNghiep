using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss dang state machine:
///   Wait        -> vua xuat hien, cho triggerAreaBoss goi ActiveAttackBoss().
///   Idle        -> nghi mot luc (idleDelay) roi random 1 patten (khong lap lai patten vua dung).
///   ShockWay    -> (patten 0) ban song 2 ben; ket thuc bang animation event outPattenBanSong. Dang tren khong thi huy, thu lai sau 1s.
///   DropAim     -> (patten 1) dich chuyen len tren dau player, anim 1; doi animation event activeDrop.
///   DropFall    -> lao xuong den khi cham dat; lap 3 lan (DropAim <-> DropFall) roi ve Idle.
///   SwordCast   -> (patten 2) dich chuyen toi vi tri kunai, treo tren khong; muaKiem spawn kunai; changeAttackMuaKiem ket thuc.
///   SwordRecover-> doi 1s roi roi xuong (gravity) va ve Idle.
/// Ten field public va cac ham animation event giu nguyen de khong hong scene/anim.
/// </summary>
public class BossLogic : StateMachineOwner<BossLogic>
{
    [SerializeField] private Transform pointAppear;

    [SerializeField] private GameObject WayShockRight;
    [SerializeField] private GameObject WayShockLeft;
    [SerializeField] private Transform pointShockWayRight;
    [SerializeField] private Transform pointShockWayLeft;

    float[] attackWeights = new float[] { 0.4f, 0.4f, 0.2f }; // trong so random <song, dap, kiem>

    public float distance = 1.78f;
    public LayerMask layerMask;

    public float speedDrop = 5f;
    [SerializeField] Transform heightDropAttack;         //do cao tan cong

    [SerializeField] Transform heightAttackKunai;           //do cao tan cong kunai

    [SerializeField] List<Transform> pointSpawnkunai;
    [SerializeField] GameObject kunai;

    private Rigidbody2D rb;

    [SerializeField] int PattenOld = -1;

    Animator animator;

    public bool inRanger = false;                       //bien kiem tra trang thai nguoi choi co ow trong vung tan cong khong.

    [Header("audio patten")]
    [SerializeField] public AudioSource pattenSword;
    [SerializeField] public AudioSource pattenDrop;
    [SerializeField] public AudioSource pattenShockWay;

    private const float StartDelay = 2f;            // tu luc kich hoat den patten dau tien
    private const float PattenDelay = 2f;           // nghi giua cac patten
    private const float AirborneRetryDelay = 1f;    // song bi huy vi dang tren khong
    private const int DropRepeat = 3;
    private const float KunaiSpawnInterval = 0.4f;
    private const float SwordRecoverTime = 1f;
    private static readonly int StateBoss = Animator.StringToHash("StateBoss");

    private float idleDelay;
    private int dropCount;

    private bool kunaiSpawning;
    private int kunaiIndex;
    private float kunaiTimer;
    private bool swordAnimEnded;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.keepAnimatorStateOnDisable = true;
    }

    protected override void OnEnable()
    {
        rb.gravityScale = 0f;
        PattenOld = -1;
        transform.SetPositionAndRotation(pointAppear.position, Quaternion.identity);

        if (EventManager.Instance != null) EventManager.Instance.onMusicFighting();     //bat nhac chien dau

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (EventManager.Instance != null) EventManager.Instance.offMusicFighting();    //tat nhac chien dau
    }

    protected override void SetupStates(StateMachine<BossLogic> m)
    {
        m.AddState<WaitState>();
        m.AddState<IdleState>();
        m.AddState<ShockWayState>();
        m.AddState<DropAimState>();
        m.AddState<DropFallState>();
        m.AddState<SwordCastState>();
        m.AddState<SwordRecoverState>();
        m.SetInitialState<WaitState>();
        // Cac state chuyen qua nhau bang logic trong Tick / animation event (xem tung state)
    }

    // ---------------- API goi tu ben ngoai ----------------

    //ham goi trong trigger area cho phep tan cong khi vao khu vuc
    public void ActiveAttackBoss()
    {
        if (Machine.IsIn<WaitState>()) GoIdle(StartDelay);
    }

    // ---------------- States ----------------

    private class WaitState : State<BossLogic> { }

    private class IdleState : State<BossLogic>
    {
        public override void Enter() => Owner.SetAnim(0);

        public override void Tick()
        {
            if (TimeInState >= Owner.idleDelay) Owner.StartNextPatten();
        }
    }

    private class ShockWayState : State<BossLogic>
    {
        public override void Enter()
        {
            if (Owner.IsGround())
            {
                Owner.SetAnim(2);               // banSong / outPattenBanSong goi trong animation
            }
            else
            {
                Owner.GoIdle(AirborneRetryDelay);
            }
        }
    }

    private class DropAimState : State<BossLogic>
    {
        public override void Enter()
        {
            Owner.rb.gravityScale = 0;
            Owner.tele(Owner.getPlayerPos().position.x);
            Owner.SetAnim(1);
            if (Owner.pattenDrop != null) AudioManager.PlaySource(Owner.pattenDrop);      //am thanh chuan bi tan cong
            // activeDrop (animation event) -> DropFallState
        }
    }

    private class DropFallState : State<BossLogic>
    {
        public override void Enter() => Owner.SetAnim(4);

        public override void Tick()
        {
            Owner.transform.Translate(Vector2.down * Time.deltaTime * Owner.speedDrop);

            if (!Owner.IsGround()) return;

            Owner.dropCount++;
            if (Owner.dropCount < DropRepeat)
            {
                Machine.ChangeState<DropAimState>();
            }
            else
            {
                Owner.rb.gravityScale = 1;
                Owner.GoIdle(PattenDelay);
            }
        }
    }

    private class SwordCastState : State<BossLogic>
    {
        public override void Enter()
        {
            Owner.kunaiSpawning = false;
            Owner.swordAnimEnded = false;

            Owner.tele(Owner.heightAttackKunai.position.x);     //dich chuyen den vi tri spam kiem
            Owner.rb.gravityScale = 0;                          //treo o do

            if (Owner.pattenSword != null) AudioManager.PlaySource(Owner.pattenSword);
            Owner.SetAnim(3);                                   //hanh dong tan cong duoc goi trong animation
        }

        public override void Tick()
        {
            if (!Owner.kunaiSpawning) return;

            if (Owner.kunaiIndex < Owner.pointSpawnkunai.Count)
            {
                Owner.kunaiTimer += Time.deltaTime;
                if (Owner.kunaiTimer >= KunaiSpawnInterval)
                {
                    Owner.kunaiTimer -= KunaiSpawnInterval;
                    Instantiate(Owner.kunai, Owner.pointSpawnkunai[Owner.kunaiIndex].position, Owner.kunai.transform.rotation);
                    Owner.kunaiIndex++;
                }
            }

            // spawn xong het va animation da ket thuc
            if (Owner.kunaiIndex >= Owner.pointSpawnkunai.Count && Owner.swordAnimEnded)
            {
                Machine.ChangeState<SwordRecoverState>();
            }
        }
    }

    private class SwordRecoverState : State<BossLogic>
    {
        public override void Enter() => Owner.SetAnim(0);

        public override void Tick()
        {
            if (TimeInState < SwordRecoverTime) return;

            Owner.rb.gravityScale = 1;
            Owner.GoIdle(PattenDelay);
        }
    }

    // ---------------- Helpers ----------------

    private void SetAnim(int value)
    {
        animator.SetInteger(StateBoss, value);
    }

    private void GoIdle(float delay)
    {
        idleDelay = delay;
        Machine.ChangeState<IdleState>(true);
    }

    private void StartNextPatten()
    {
        switch (randomNoDuplicate())                //random 1 kieu tan cong khong trung lap
        {
            case 0:
                Machine.ChangeState<ShockWayState>();
                break;
            case 1:
                dropCount = 0;
                Machine.ChangeState<DropAimState>();
                break;
            case 2:
                Machine.ChangeState<SwordCastState>();
                break;
            default:
                Debug.LogWarning("khong co gia tri random dung");
                GoIdle(PattenDelay);
                break;
        }
    }

    //random ra 1 patten khong trung
    int randomNoDuplicate()
    {
        int attackIndex = WeightedRandom(attackWeights);
        while (attackIndex == PattenOld)
        {
            attackIndex = WeightedRandom(attackWeights);
        }

        PattenOld = attackIndex; //luu patten vua thuc hien
        return attackIndex;
    }

    //random trong so
    int WeightedRandom(float[] weights)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.value * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (randomValue < weights[i])
            {
                return i;
            }

            randomValue -= weights[i];
        }

        return weights.Length - 1;
    }

    //dich chuyen den vi tri x (o do cao tan cong)
    private void tele(float x)
    {
        transform.position = new Vector2(x, heightDropAttack.position.y);
    }

    private Transform getPlayerPos()
    {
        if (GetPosPlayer.Instance == null || GetPosPlayer.Instance.PlayerPos == null)
        {
            return transform;
        }
        return GetPosPlayer.Instance.PlayerPos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().takeDame(0);      //can sua
        }
    }

    private bool IsGround()
    {
        Debug.DrawRay(transform.position, new Vector2(0, -distance), Color.green);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, layerMask);
        return hit.collider != null;
    }

    // ---------------- Ham goi tu animation event ----------------

    //goi trong animation mua kiem: bat dau spawn kunai
    public void muaKiem()
    {
        swordAnimEnded = false;
        kunaiSpawning = true;
        kunaiIndex = 0;
        kunaiTimer = 0f;
    }

    //goi cuoi animation de thoat trang thai tan cong kiem
    public void changeAttackMuaKiem()
    {
        swordAnimEnded = true;
    }

    //spam song goi trong animation shock way patten
    public void banSong()
    {
        if (pattenShockWay != null) AudioManager.PlaySource(pattenShockWay);

        Instantiate(WayShockLeft, pointShockWayLeft.position, Quaternion.identity);
        Instantiate(WayShockRight, pointShockWayRight.position, Quaternion.identity);
    }

    //goi cuoi animation ban song: nghi roi chuyen patten
    public void outPattenBanSong()
    {
        if (Machine.IsIn<ShockWayState>()) GoIdle(PattenDelay);
    }

    //goi o cuoi hoat anh de drop
    public void activeDrop()
    {
        if (Machine.IsIn<DropAimState>()) Machine.ChangeState<DropFallState>();
    }
}
