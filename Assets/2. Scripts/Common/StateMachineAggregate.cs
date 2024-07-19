using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//상태머신 정의 종합한 곳

#region UI 상태머신 정의
[Tooltip("UI 상태머신 정의")]
public class UIStateMachine
{
    public BaseState currentState { get; private set; }
    private Dictionary<UISTATE, BaseState> dicStates
                                        = new Dictionary<UISTATE, BaseState>();
    public UIStateMachine(UISTATE eState, BaseState state)
    {
        AddState(eState, state);
        currentState = GetState(eState);
    }

    public void AddState(UISTATE eState, BaseState state)
    {
        if (!dicStates.ContainsKey(eState))
            dicStates.Add(eState, state);
    }

    public BaseState GetState(UISTATE eState)
    {
        if (dicStates.TryGetValue(eState, out BaseState state)) return state;

        return null;
    }

    public void DeleteState(UISTATE eState)
    {
        if (dicStates.ContainsKey(eState))
            dicStates.Remove(eState);
    }

    public void ChangeState(UISTATE eState)
    {
        currentState?.OnExitState();

        if (dicStates.TryGetValue(eState, out BaseState state))
            currentState = state;

        currentState?.OnEnterState();
    }

    public void UpdateState()
    {
        currentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        currentState?.OnFixedUpdateState();
    }
}
#endregion

#region GameManager 상태머신 정의
[Tooltip("GameManager 상태머신 정의")]
public class GMStateMachine
{
    public BaseState currentState { get; private set; }
    private Dictionary<GMSTATE, BaseState> dicStates
                                        = new Dictionary<GMSTATE, BaseState>();
    public GMStateMachine(GMSTATE eState, BaseState state)
    {
        AddState(eState, state);
        currentState = GetState(eState);
    }

    public void AddState(GMSTATE eState, BaseState state)
    {
        if (!dicStates.ContainsKey(eState))
            dicStates.Add(eState, state);
    }

    public BaseState GetState(GMSTATE eState)
    {
        if (dicStates.TryGetValue(eState, out BaseState state)) return state;

        return null;
    }

    public void DeleteState(GMSTATE eState)
    {
        if (dicStates.ContainsKey(eState))
            dicStates.Remove(eState);
    }

    public void ChangeState(GMSTATE eState)
    {
        currentState?.OnExitState();

        if (dicStates.TryGetValue(eState, out BaseState state))
            currentState = state;

        currentState?.OnEnterState();
    }

    public void UpdateState()
    {
        currentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        currentState?.OnFixedUpdateState();
    }
}
#endregion

#region 플레이어 상태머신 정의
[Tooltip("Player 상태머신 정의")]
public class PlayerStateMachine
{
    public PlayerBaseState currentState { get; private set; }
    private Dictionary<PLAYERSTATE, PlayerBaseState> dicStates
                                        = new Dictionary<PLAYERSTATE, PlayerBaseState>();
    public PlayerStateMachine(PLAYERSTATE eState, PlayerBaseState state)
    {
        AddState(eState, state);
        currentState = GetState(eState);
    }

    public void AddState(PLAYERSTATE eState, PlayerBaseState state)
    {
        if (!dicStates.ContainsKey(eState))
            dicStates.Add(eState, state);
    }

    public PlayerBaseState GetState(PLAYERSTATE eState)
    {
        if (dicStates.TryGetValue(eState, out PlayerBaseState state)) return state;

        return null;
    }

    public void DeleteState(PLAYERSTATE eState)
    {
        if (dicStates.ContainsKey(eState))
            dicStates.Remove(eState);
    }

    public void ChangeState(PLAYERSTATE eState)
    {
        currentState?.OnExitState();

        if (dicStates.TryGetValue(eState, out PlayerBaseState state))
            currentState = state;

        currentState?.OnEnterState();
    }

    public void UpdateState()
    {
        currentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        currentState?.OnFixedUpdateState();
    }
}
#endregion

#region 유닛 상태머신 정의
[Tooltip("Unit 상태머신 정의")]
public class UnitStateMachine
{
    public UnitBaseState currentState { get; private set; }
    private Dictionary<UNITSTATE, UnitBaseState> dicStates
                                        = new Dictionary<UNITSTATE, UnitBaseState>();
    public UnitStateMachine(UNITSTATE eState, UnitBaseState state)
    {
        AddState(eState, state);
        currentState = GetState(eState);
    }

    public void AddState(UNITSTATE eState, UnitBaseState state)
    {
        if (!dicStates.ContainsKey(eState))
            dicStates.Add(eState, state);
    }

    public UnitBaseState GetState(UNITSTATE eState)
    {
        if (dicStates.TryGetValue(eState, out UnitBaseState state)) return state;

        return null;
    }

    public void DeleteState(UNITSTATE eState)
    {
        if (dicStates.ContainsKey(eState))
            dicStates.Remove(eState);
    }

    public void ChangeState(UNITSTATE eState)
    {
        currentState?.OnExitState();

        if (dicStates.TryGetValue(eState, out UnitBaseState state))
            currentState = state;

        currentState?.OnEnterState();
    }

    public void UpdateState()
    {
        currentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        currentState?.OnFixedUpdateState();
    }
}
#endregion

#region 에너미 상태머신 정의
[Tooltip("Enemy 상태머신 정의")]
public class EnemyStateMachine
{
    public EnemyBaseState currentState { get; private set; }
    private Dictionary<ENEMYSTATE, EnemyBaseState> dicStates
                                        = new Dictionary<ENEMYSTATE, EnemyBaseState>();
    public EnemyStateMachine(ENEMYSTATE eState, EnemyBaseState state)
    {
        AddState(eState, state);
        currentState = GetState(eState);
    }

    public void AddState(ENEMYSTATE eState, EnemyBaseState state)
    {
        if (!dicStates.ContainsKey(eState))
            dicStates.Add(eState, state);
    }

    public EnemyBaseState GetState(ENEMYSTATE eState)
    {
        if (dicStates.TryGetValue(eState, out EnemyBaseState state)) return state;

        return null;
    }

    public void DeleteState(ENEMYSTATE eState)
    {
        if (dicStates.ContainsKey(eState))
            dicStates.Remove(eState);
    }

    public void ChangeState(ENEMYSTATE eState)
    {
        currentState?.OnExitState();

        if (dicStates.TryGetValue(eState, out EnemyBaseState state))
            currentState = state;

        currentState?.OnEnterState();
    }

    public void UpdateState()
    {
        currentState?.OnUpdateState();
    }

    public void FixedUpdateState()
    {
        currentState?.OnFixedUpdateState();
    }
}
#endregion