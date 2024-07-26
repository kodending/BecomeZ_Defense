using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMInLobby : BaseState
{
    public override void OnEnterState()
    {
        GameManager.gm.m_curState = GMSTATE.LOBBY;
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
