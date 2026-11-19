using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State machine tong quat (khong phu thuoc MonoBehaviour).
/// - Dang ky state bang AddState, transition bang AddTransition / AddAnyTransition.
/// - Moi Tick: kiem tra transition (AnyTransition truoc, roi transition rieng cua state hien tai,
///   theo thu tu dang ky; cai dau tien thoa dieu kien se duoc chon), sau do chay State.Tick().
/// - Co the chuyen state thu cong bang ChangeState khi can (vd goi tu animation event, trigger).
/// </summary>
public class StateMachine<TOwner>
{
    private sealed class Transition
    {
        public State<TOwner> To;
        public Func<bool> Condition;
    }

    private readonly TOwner owner;
    private readonly Dictionary<Type, State<TOwner>> states = new Dictionary<Type, State<TOwner>>();
    private readonly Dictionary<State<TOwner>, List<Transition>> transitions = new Dictionary<State<TOwner>, List<Transition>>();
    private readonly List<Transition> anyTransitions = new List<Transition>();

    private State<TOwner> initialState;
    private float enterTime;

    public State<TOwner> Current { get; private set; }
    public State<TOwner> Previous { get; private set; }

    /// <summary>Thoi gian (giay, theo Time.time) ke tu luc vao state hien tai.</summary>
    public float TimeInState => Current != null ? Time.time - enterTime : 0f;

    public bool IsRunning => Current != null;

    /// <summary>Goi sau khi doi state: (state cu, state moi). State cu co the null.</summary>
    public event Action<State<TOwner>, State<TOwner>> StateChanged;

    public StateMachine(TOwner owner)
    {
        this.owner = owner;
    }

    // ---------------- Setup ----------------

    /// <summary>Dang ky mot state. Moi kieu state chi duoc dang ky mot lan.</summary>
    public T AddState<T>(T state) where T : State<TOwner>
    {
        var type = state.GetType();
        if (states.ContainsKey(type))
            throw new InvalidOperationException($"State {type.Name} da duoc dang ky.");

        state.Bind(owner, this);
        states.Add(type, state);
        return state;
    }

    /// <summary>Dang ky state co constructor mac dinh.</summary>
    public T AddState<T>() where T : State<TOwner>, new()
    {
        return AddState(new T());
    }

    /// <summary>Chon state khoi dau (dung khi Start()).</summary>
    public void SetInitialState<T>() where T : State<TOwner>
    {
        initialState = GetState<T>();
    }

    /// <summary>Transition tu TFrom sang TTo khi condition tra ve true.</summary>
    public void AddTransition<TFrom, TTo>(Func<bool> condition)
        where TFrom : State<TOwner>
        where TTo : State<TOwner>
    {
        var from = GetState<TFrom>();
        if (!transitions.TryGetValue(from, out var list))
        {
            list = new List<Transition>();
            transitions.Add(from, list);
        }
        list.Add(new Transition { To = GetState<TTo>(), Condition = condition });
    }

    /// <summary>Transition tu bat ky state nao sang TTo khi condition tra ve true.</summary>
    public void AddAnyTransition<TTo>(Func<bool> condition) where TTo : State<TOwner>
    {
        anyTransitions.Add(new Transition { To = GetState<TTo>(), Condition = condition });
    }

    // ---------------- Lifecycle ----------------

    /// <summary>Bat dau chay, vao state khoi dau.</summary>
    public void Start()
    {
        if (initialState == null)
            throw new InvalidOperationException("Chua goi SetInitialState<T>().");
        ChangeState(initialState, true);
    }

    /// <summary>Dung machine: goi Exit cua state hien tai, Current = null.</summary>
    public void Stop()
    {
        if (Current == null) return;
        var old = Current;
        Current = null;
        old.Exit();
        StateChanged?.Invoke(old, null);
    }

    public void Tick()
    {
        if (Current == null) return;

        var triggered = FindTriggeredTransition();
        if (triggered != null) ChangeState(triggered.To);

        Current?.Tick();
    }

    public void FixedTick()
    {
        Current?.FixedTick();
    }

    // ---------------- State change ----------------

    public void ChangeState<T>(bool allowReenter = false) where T : State<TOwner>
    {
        ChangeState(GetState<T>(), allowReenter);
    }

    public void ChangeState(State<TOwner> next, bool allowReenter = false)
    {
        if (next == null) throw new ArgumentNullException(nameof(next));
        if (next == Current && !allowReenter) return;

        var old = Current;
        old?.Exit();

        Previous = old;
        Current = next;
        enterTime = Time.time;

        StateChanged?.Invoke(old, next);
        next.Enter();
    }

    /// <summary>Quay lai state truoc do (neu co).</summary>
    public void RevertToPrevious()
    {
        if (Previous != null) ChangeState(Previous);
    }

    // ---------------- Query ----------------

    public bool IsIn<T>() where T : State<TOwner>
    {
        return Current != null && Current.GetType() == typeof(T);
    }

    public T GetState<T>() where T : State<TOwner>
    {
        if (states.TryGetValue(typeof(T), out var state)) return (T)state;
        throw new InvalidOperationException($"State {typeof(T).Name} chua duoc dang ky (AddState).");
    }

    private Transition FindTriggeredTransition()
    {
        for (int i = 0; i < anyTransitions.Count; i++)
        {
            var t = anyTransitions[i];
            if (t.To != Current && t.Condition()) return t;
        }

        if (transitions.TryGetValue(Current, out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Condition()) return list[i];
            }
        }
        return null;
    }
}
