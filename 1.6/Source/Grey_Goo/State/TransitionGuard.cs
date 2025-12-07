namespace Grey_Goo.State;

/// <summary>
/// Delegate that guards a transition between two states.
/// </summary>
/// <typeparam name="E">Enum that identifies states.</typeparam>
/// <typeparam name="T">Concrete state type.</typeparam>
/// <param name="stateMachine">The hosting state machine.</param>
/// <param name="from">Source state.</param>
/// <param name="to">Destination state.</param>
/// <param name="reason">Output reason when the transition is rejected; empty when accepted.</param>
/// <returns><c>true</c> to allow the transition; <c>false</c> to block it.</returns>
public delegate bool TransitionGuard<E, T>(StateMachine<E, T> stateMachine, T from, T to, out string reason)
    where E : struct, System.Enum
    where T : State<E, T>, IIdentifiableState<E>;
