/// <summary>
/// Mot trang thai cua state machine. Moi state la mot class rieng, giu logic cua trang thai do.
/// TOwner la class chu (vd EnemyDashAI) de state truy cap component/du lieu cua doi tuong.
/// </summary>
public abstract class State<TOwner>
{
    /// <summary>Doi tuong so huu state machine nay.</summary>
    public TOwner Owner { get; private set; }

    /// <summary>State machine dang chay state nay.</summary>
    public StateMachine<TOwner> Machine { get; private set; }

    /// <summary>Thoi gian (giay) ke tu luc vao state nay.</summary>
    public float TimeInState => Machine != null ? Machine.TimeInState : 0f;

    internal void Bind(TOwner owner, StateMachine<TOwner> machine)
    {
        Owner = owner;
        Machine = machine;
    }

    /// <summary>Goi mot lan khi vao state.</summary>
    public virtual void Enter() { }

    /// <summary>Goi moi Update khi dang o state nay (sau khi da kiem tra transition).</summary>
    public virtual void Tick() { }

    /// <summary>Goi moi FixedUpdate khi dang o state nay.</summary>
    public virtual void FixedTick() { }

    /// <summary>Goi mot lan khi roi state.</summary>
    public virtual void Exit() { }

    /// <summary>Chuyen sang state khac ngay lap tuc (state phai da duoc dang ky).</summary>
    protected void ChangeState<T>() where T : State<TOwner>
    {
        Machine.ChangeState<T>();
    }
}
