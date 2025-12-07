namespace Grey_Goo.State;

/// <summary>
/// Base class for a state in a strongly-typed finite state machine.
/// </summary>
/// <typeparam name="E">Enum that identifies states.</typeparam>
/// <typeparam name="T">Concrete state type.</typeparam>
public abstract class State<E, T>(E id)
    where E : struct, System.Enum
    where T : State<E, T>, IIdentifiableState<E>
{
    public E Id { get; } = id;

    /// <summary>
    /// Called after transitioning into this state.
    /// </summary>
    /// <param name="sm">The hosting state machine.</param>
    /// <param name="from">The state we transitioned from.</param>
    public abstract void OnEnter(StateMachine<E, T> sm, T from);

    /// <summary>
    /// Called before transitioning out of this state.
    /// </summary>
    /// <param name="sm">The hosting state machine.</param>
    /// <param name="to">The state we are transitioning to.</param>
    public abstract void OnExit(StateMachine<E, T> sm, T to);

    /// <summary>
    /// A friendly string representation of this state, typically the enum id.
    /// </summary>
    public override string ToString() => Id.ToString();
}
