using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Unit_Entry : UnitBaseState
{
    public Unit_Entry(UnitFSM unit) : base(unit) { }

    public override void OnEnterState()
    {
        unitFSM.m_anim.SetTrigger("Entry");
        unitFSM.m_curState = UNITSTATE.ENTRY;

        //if (!unitFSM.m_pv.IsMine) return;
        //unitFSM.m_pv.RPC("AnimTriggerRPC", RpcTarget.All, "Entry", (int)UNITSTATE.ENTRY);

        unitFSM.StartCoroutine(ChangeIdleState());
    }

    public override void OnUpdateState()
    {

    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {

    }

    IEnumerator ChangeIdleState()
    {
        yield return new WaitForSeconds(1f);

        unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
    }

}
