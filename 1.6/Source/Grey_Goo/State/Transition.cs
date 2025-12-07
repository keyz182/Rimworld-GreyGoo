using System.Collections.Generic;
using Grey_Goo;

namespace Grey_Goo.State;

/// <summary>
/// Represents a directed transition between two concrete states of the state machine.
/// </summary>
/// <typeparam name="E">Enum that identifies states.</typeparam>
/// <typeparam name="T">Concrete state type.</typeparam>
public sealed class Transition<E, T>
    where E : struct, System.Enum
    where T : State<E, T>, IIdentifiableState<E>
{
    public readonly T From;
    public readonly T To;
    public TransitionGuard<E, T> Guard { get; }

    public Transition(T from, T to, TransitionGuard<E, T> guard = null)
    {
        From = from;
        To = to;
        Guard = guard;
    }

    public bool CanApply(StateMachine<E, T> stateMachine, out string reason)
    {
        reason = string.Empty;
        if (!EqualityComparer<T>.Default.Equals(stateMachine.State, From)) return false;
        return Guard?.Invoke(stateMachine, From, To, out reason) ?? true;
    }

    public void Apply(StateMachine<E, T> stateMachine)
    {
        T from = stateMachine.State;
        // Log self-transition for debugging purposes
        if (EqualityComparer<T>.Default.Equals(from, To))
            ModLog.Debug($"[FSM] Self-transition detected: {typeof(E).Name}:{from} -> {To} at tick {Verse.Find.TickManager.TicksGame}");
        // Exit current state
        from.OnExit(stateMachine, To);
        // Enter new state
        stateMachine.State = To;
        To.OnEnter(stateMachine, from);
    }
}
