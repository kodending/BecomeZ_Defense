using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMInMain : BaseState
{

    public override void OnEnterState()
    {
        GameManager.gm.m_curState = GMSTATE.MAIN;
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
