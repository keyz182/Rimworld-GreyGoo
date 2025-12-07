using System.Collections.Generic;

namespace Grey_Goo.State;

/// <summary>
/// Transition bucket keyed by destination for a given source state.
/// Used internally by <see cref="StateMachine{E,T}"/> to quickly lookup edges from a source state.
/// </summary>
/// <typeparam name="E">Enum that identifies states.</typeparam>
/// <typeparam name="T">Concrete state type.</typeparam>
public sealed class StateTransitions<E, T>
    where E : struct, System.Enum
    where T : State<E, T>, IIdentifiableState<E>
{
    private readonly Dictionary<T, Transition<E, T>> _transitionsByTarget = new();

    public void Add(Transition<E, T> transition) { _transitionsByTarget[transition.To] = transition; }

    public bool TryGet(T to, out Transition<E, T> transition) => _transitionsByTarget.TryGetValue(to, out transition);

    public bool TryTransitionTo(StateMachine<E, T> stateMachine, T to, out string reason)
    {
        reason = string.Empty;
        return _transitionsByTarget.TryGetValue(to, out Transition<E, T> transition) && transition.CanApply(stateMachine, out reason);
    }

    public void Apply(T to, StateMachine<E, T> stateMachine) => _transitionsByTarget[to].Apply(stateMachine);
}
