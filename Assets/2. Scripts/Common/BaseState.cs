using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Runtime.CompilerServices;

//상태머신 정의한 곳

public abstract class BaseState : MonoBehaviourPunCallbacks
{
    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnFixedUpdateState();
    public abstract void OnExitState();
}

public abstract class PlayerBaseState : MonoBehaviourPunCallbacks
{
    protected PlayerController playerController { get; private set; }
    public PlayerBaseState(PlayerController pc)
    {
        this.playerController = pc;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnFixedUpdateState();
    public abstract void OnExitState();
}

public abstract class UnitBaseState : MonoBehaviourPunCallbacks
{
    protected UnitFSM unitFSM { get; private set; }
    public UnitBaseState(UnitFSM unit)
    {
        this.unitFSM = unit;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnFixedUpdateState();
    public abstract void OnExitState();
}

public abstract class EnemyBaseState : MonoBehaviourPunCallbacks
{
    protected EnemyFSM enemyFSM { get; private set; }
    public EnemyBaseState(EnemyFSM enemy)
    {
        this.enemyFSM = enemy;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnFixedUpdateState();
    public abstract void OnExitState();
}