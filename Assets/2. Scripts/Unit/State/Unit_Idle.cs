using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;


public class Unit_Idle : UnitBaseState
{
    public Unit_Idle(UnitFSM unit) : base(unit) { }

    public override void OnEnterState()
    {
        //unitFSM.m_anim.SetTrigger("Idle");
        unitFSM.m_curState = UNITSTATE.IDLE;
        
        if (!unitFSM.m_pv.IsMine) return;

        //unitFSM.m_pv.RPC("AnimTriggerRPC", RpcTarget.All, "Idle", (int)UNITSTATE.IDLE);
        //unitFSM.m_pv.RPC("AnimBoolRPC", RpcTarget.All, "isAttack", false , (int)UNITSTATE.IDLE);
    }

    public override void OnUpdateState()
    {
        if (!unitFSM.m_pv.IsMine) return;

        if (unitFSM.m_enemyScan.checkedTargetEnemies.Count > 0)
            if (unitFSM.m_enemyScan.nearestTargetEnemy != null)
                    ScanAttackEnemy();
    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {

    }

    void ScanAttackEnemy()
    {
        if (unitFSM.m_sInfo.eType == UNITTYPE.BUFFER || unitFSM.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER) return;

        if (unitFSM.m_curState != UNITSTATE.ATTACK && unitFSM.m_curState != UNITSTATE.ENTRY)
            //unitFSM.m_stateMachine.ChangeState(UNITSTATE.ATTACK);
            unitFSM.m_pv.RPC("ChangeStateRPC", RpcTarget.All, (int)UNITSTATE.ATTACK);
    }
}
