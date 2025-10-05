using System;
using System.Collections.Generic;


public class StateMachine
{
    private readonly Dictionary<Type, IState> _states = new();
    public IState CurrentState { get; private set; }

    public void RegisterState(IState state)
    {
        _states[state.GetType()] = state;
    }

    public void ChangeState<T>() where T : IState
    {
        if (CurrentState != null)
            CurrentState.OnExit();

        var type = typeof(T);
        if (_states.TryGetValue(type, out var newState))
        {
            CurrentState = newState;
            CurrentState.OnEnter();
        }
        else
        {
            UnityEngine.Debug.LogError($"Estado {type.Name} no registrado en la FSM.");
        }
    }

    public void Tick()
    {
        CurrentState?.OnUpdate();
    }

    public void FixedTick()
    {
        CurrentState?.OnFixedUpdate();
    }
}



public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
