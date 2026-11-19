using UnityEngine;

/// <summary>
/// MonoBehaviour co san mot StateMachine. Ke thua theo mau:
///
///   public class EnemyDashAI : StateMachineOwner&lt;EnemyDashAI&gt;
///   {
///       public Transform LimitLeft, LimitRight;           // du lieu dung chung cho cac state
///
///       protected override void SetupStates(StateMachine&lt;EnemyDashAI&gt; m)
///       {
///           m.AddState&lt;PatrolState&gt;();
///           m.AddState&lt;AttackState&gt;();
///           m.SetInitialState&lt;PatrolState&gt;();
///           m.AddTransition&lt;PatrolState, AttackState&gt;(() =&gt; inRange);
///           m.AddTransition&lt;AttackState, PatrolState&gt;(() =&gt; !inRange);
///       }
///
///       private class PatrolState : State&lt;EnemyDashAI&gt;
///       {
///           public override void Tick() { /* Owner.transform... */ }
///       }
///   }
///
/// Neu override Awake/OnEnable/OnDisable/Update/FixedUpdate thi phai goi base.Xxx().
/// </summary>
public abstract class StateMachineOwner<TSelf> : MonoBehaviour where TSelf : StateMachineOwner<TSelf>
{
    [Header("State machine (debug)")]
    [SerializeField] private bool logTransitions;
    [SerializeField] private string currentStateName;

    private StateMachine<TSelf> machine;

    /// <summary>Tam dung / tiep tuc viec Tick (vd khi pause, chet, cutscene).</summary>
    public bool Paused { get; set; }

    public StateMachine<TSelf> Machine
    {
        get
        {
            if (machine == null) BuildMachine();
            return machine;
        }
    }

    /// <summary>Dang ky state, transition va state khoi dau (SetInitialState).</summary>
    protected abstract void SetupStates(StateMachine<TSelf> machine);

    private void BuildMachine()
    {
        machine = new StateMachine<TSelf>((TSelf)this);
        SetupStates(machine);
        machine.StateChanged += OnStateChanged;
    }

    protected virtual void Awake()
    {
        if (machine == null) BuildMachine();
    }

    protected virtual void OnEnable()
    {
        Paused = false;
        Machine.Start();
    }

    protected virtual void OnDisable()
    {
        machine?.Stop();
    }

    protected virtual void Update()
    {
        if (!Paused) machine?.Tick();
    }

    protected virtual void FixedUpdate()
    {
        if (!Paused) machine?.FixedTick();
    }

    private void OnStateChanged(State<TSelf> from, State<TSelf> to)
    {
        currentStateName = to != null ? to.GetType().Name : "(none)";
        if (logTransitions)
            Debug.Log($"[{name}] {(from != null ? from.GetType().Name : "(none)")} -> {currentStateName}", this);
    }
}
