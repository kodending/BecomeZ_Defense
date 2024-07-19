using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Unit_Idle : UnitBaseState
{
    public Unit_Idle(UnitFSM unit) : base(unit) { }

    public override void OnEnterState()
    {
        if (!unitFSM.m_pv.IsMine) return;

        unitFSM.m_pv.RPC("AnimTriggerRPC", RpcTarget.All, "Idle", (int)UNITSTATE.IDLE);
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

}
