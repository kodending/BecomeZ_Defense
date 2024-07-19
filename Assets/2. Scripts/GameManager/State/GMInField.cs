using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GMInField : BaseState
{
    public override void OnEnterState()
    {
        //MapGenerator.mg.GenerateMap();
        UIManager.um.m_stateMachine.ChangeState(UISTATE.ENTERING_FIELD);
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
