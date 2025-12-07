using System.Collections.Generic;
using Verse;

namespace Grey_Goo.State;

using System;

/// <summary>
/// This interface defines the requirements for a state to be used by the State Machine.
/// It ensures that every state has a unique identifier (ID) based on an Enum.
/// </summary>
/// <typeparam name="TStateId">
/// The specific Enum type that lists all your possible states (e.g., 'TrafficLightColors').
/// </typeparam>
public interface IIdentifiableState<TStateId> where TStateId : struct, Enum
{
    /// <summary>
    /// Gets the unique ID for this state.
    /// The State Machine uses this ID to look up and switch between states.
    /// </summary>
    TStateId Id { get; }
}

/// <summary>
/// Strongly-typed finite state machine keyed by an enum <typeparamref name="E"/>.
/// </summary>
/// <typeparam name="E">Enum that uniquely identifies each state.</typeparam>
/// <typeparam name="T">Concrete state class that derives from <see cref="State{E,T}"/> and implements <see cref="IHasId{E}"/>.</typeparam>
/// <remarks>
/// Typical usage:
/// <code>
/// var sm = new MyStateMachine();
/// sm.Initialise(MyStates.Offline);
/// if (sm.TryTransitionTo(MyStates.Online, out var reason)) { /* ... */ }
/// </code>
/// States self-register during construction in <c>BuildTransitions()</c>, and transitions are grouped by their source
/// state for fast lookup. The machine persists its current/previous state id and last transition tick via RimWorld's Scribe.
/// </remarks>
public class StateMachine<E, T> : IExposable
    where E : struct, Enum
    where T : State<E, T>, IIdentifiableState<E>
{
    /// <summary>
    /// True after <see cref="Initialise(E)"/> has been called. Prevents double-initialisation after load.
    /// </summary>
    public bool Initialised = false;

    private readonly Dictionary<E, T> _states = new();
    private E _stateId;
    private E _prevStateId;
    private int _lastTransitionTick;

    // Transitions are grouped by source state: From -> (To -> Transition)
    protected readonly Dictionary<T, StateTransitions<E, T>> _transitions = new();

    /// <summary>
    /// Gets or sets the current state instance. Setting this assigns <see cref="CurrentId"/> to the state's <see cref="IHasId{E}.Id"/>.
    /// </summary>
    public T State
    {
        get => _states[_stateId];
        set => _stateId = value.Id;
    }

    /// <summary>
    /// The previous state instance (before the last successful transition).
    /// </summary>
    public T PrevState => _states[_prevStateId];
    /// <summary>
    /// Id of the current state without resolving the instance.
    /// Useful for cheap comparisons and logging.
    /// </summary>
    public E CurrentId => _stateId;
    /// <summary>
    /// Game tick when the last transition completed.
    /// </summary>
    public int LastTransitionTick { get => _lastTransitionTick; private set => _lastTransitionTick = value; }
    /// <summary>
    /// Number of game ticks spent in the current state.
    /// </summary>
    public int TicksInState => Math.Max(0, Find.TickManager.TicksGame - _lastTransitionTick);

    /// <summary>
    /// Saves/loads the state machine using RimWorld's Scribe system.
    /// Rebuilds transitions on <see cref="LoadSaveMode.PostLoadInit"/>.
    /// </summary>
    public void ExposeData()
    {
        Scribe_Values.Look(ref _stateId, "state");
        Scribe_Values.Look(ref _prevStateId, "prevState");
        Scribe_Values.Look(ref _lastTransitionTick, "lastTransitionTick");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            BuildTransitions();
    }

    /// <summary>
    /// Registers a state instance with the machine.
    /// </summary>
    /// <param name="state">The state instance to register.</param>
    /// <param name="addSelfTransition">When true, also registers a self-transition for this state.</param>
    public virtual void RegisterState(T state, bool addSelfTransition = false)
    {
        _states[state.Id] = state;
        if (addSelfTransition) AddTransition(new Transition<E, T>(state, state));
    }

    /// <summary>
    /// Override to construct states and add transitions between them.
    /// States typically self-register in their constructors by calling <see cref="RegisterState(T,bool)"/>.
    /// </summary>
    public virtual void BuildTransitions() { }

    /// <summary>
    /// Builds transitions (if not already built) and sets the initial state.
    /// </summary>
    /// <param name="initial">The starting state id.</param>
    public virtual void Initialise(E initial)
    {
        BuildTransitions();
        _stateId = initial;
        _prevStateId = initial;
        LastTransitionTick = Find.TickManager.TicksGame;
        Initialised = true;
    }

    /// <summary>
    /// Removes all registered transitions. Does not remove states.
    /// </summary>
    protected void ClearTransitions() => _transitions.Clear();

    /// <summary>
    /// Adds a transition object to the machine.
    /// </summary>
    /// <param name="t">The transition to add.</param>
    protected void AddTransition(Transition<E, T> t)
    {
        if (!_transitions.TryGetValue(t.From, out StateTransitions<E, T> bucket))
        {
            bucket = new StateTransitions<E, T>();
            _transitions[t.From] = bucket;
        }
        bucket.Add(t);
    }

    /// <summary>
    /// Convenience overload to add a transition without explicitly constructing <see cref="Transition{E,T}"/>.
    /// </summary>
    /// <param name="from">Source state.</param>
    /// <param name="to">Destination state.</param>
    /// <param name="guard">Optional guard that determines whether the transition can be taken.</param>
    protected void AddTransition(T from, T to, TransitionGuard<E, T> guard = null)
    {
        AddTransition(new Transition<E, T>(from, to, guard));
    }

    /// <summary>
    /// Checks whether the machine is currently in the specified state.
    /// </summary>
    /// <param name="id">State id to compare against.</param>
    /// <returns><c>true</c> if the current state equals <paramref name="id"/>; otherwise <c>false</c>.</returns>
    public bool IsInState(E id) => EqualityComparer<E>.Default.Equals(_stateId, id);

    /// <summary>
    /// Attempts to transition to the specified state id if a valid transition exists from the current state and it passes validation.
    /// </summary>
    /// <param name="newStateId">Target state id.</param>
    /// <param name="reason">Output parameter describing why the transition failed; empty on success.</param>
    /// <returns><c>true</c> if the transition was applied; otherwise <c>false</c>.</returns>
    public bool TryTransitionTo(E newStateId, out string reason)
    {
        reason = string.Empty;
        if (!_states.TryGetValue(newStateId, out T newState))
        {
            reason = "state '" + newStateId + "' not found. Valid states:" + string.Join(",", _states.Keys);
            return false;
        }

        if (!_transitions.TryGetValue(State, out StateTransitions<E, T> tf) || !tf.TryGet(newState, out Transition<E, T> tr) || !tr.CanApply(this, out reason))
            return false;

        _prevStateId = _stateId;
        tr.Apply(this);
        LastTransitionTick = Find.TickManager.TicksGame;
        return true;
    }

    /// <summary>
    /// Checks whether a transition to the specified state could be performed right now, without mutating the machine.
    /// </summary>
    /// <param name="newStateId">Target state id.</param>
    /// <param name="reason">Output parameter describing why the transition is not allowed; empty when allowed.</param>
    /// <returns><c>true</c> if such a transition exists and all validators pass; otherwise <c>false</c>.</returns>
    public bool CanTransitionTo(E newStateId, out string reason)
    {
        reason = string.Empty;
        if (!_states.TryGetValue(newStateId, out var newState)) return false;
        return _transitions.TryGetValue(State, out StateTransitions<E, T> tf) && tf.TryGet(newState, out Transition<E, T> tr) && tr.CanApply(this, out reason);
    }
}
