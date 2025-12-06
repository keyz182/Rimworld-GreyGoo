namespace Grey_Goo.State;

public abstract class State
{
    public abstract void OnEnter<T>(StateMachine<T> sm, T from) where T : State;
    public abstract void OnExit<T>(StateMachine<T> sm, T to) where T : State;

    public abstract override string ToString();
}
