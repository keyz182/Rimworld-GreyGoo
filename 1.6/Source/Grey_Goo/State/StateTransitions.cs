using System.Collections.Generic;

namespace Grey_Goo.State;

public sealed class StateTransitions<T> where T : State
{
    private readonly Dictionary<T, Transition<T>> _byTarget = new();
    public void Add(Transition<T> t) { _byTarget[t.To] = t; }
    public bool TryGet(T to, out Transition<T> tr) => _byTarget.TryGetValue(to, out tr);
    public bool TryTransitionTo(StateMachine<T> sm, T to, out string reason)
    {
        reason = string.Empty;
        return _byTarget.TryGetValue(to, out Transition<T> tr) && tr.CanApply(sm, out reason);
    }
    public void Apply(T to, StateMachine<T> sm) => _byTarget[to].Apply(sm);
}
