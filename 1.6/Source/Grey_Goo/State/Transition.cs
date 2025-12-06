using System.Collections.Generic;
using JetBrains.Annotations;

namespace Grey_Goo.State;

public sealed class Transition<T>(T from, T to, TransitionValidator<T> validator = null)
    where T : State
{
    public T From = from;
    public T To = to;
    [CanBeNull] public TransitionValidator<T> validator = validator;

    public bool CanApply(StateMachine<T> stateMachine, out string reason)
    {
        reason = string.Empty;
        if (!EqualityComparer<T>.Default.Equals(stateMachine.State, From)) return false;
        return validator?.Invoke(stateMachine, From, To, out reason) ?? true;
    }

    public void Apply(StateMachine<T> stateMachine)
    {
        T from = stateMachine.State;
        // Exit current state
        from.OnExit(stateMachine, To);
        // Enter new state
        stateMachine.State = To;
        To.OnEnter(stateMachine, from);
    }
}
