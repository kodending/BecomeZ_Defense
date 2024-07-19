using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMInPhaseReady : BaseState
{
    public override void OnEnterState()
    {
        GameManager.gm.m_curState = GMSTATE.PHASE_READY;

        StartTimer();
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

    void StartTimer()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        //NetworkManager.nm.m_iGameTimer = 31;
        NetworkManager.nm.m_iGameTimer = 16;

        GameManager.gm.StartCoroutine(NetworkManager.nm.InGameTimer());
    }
}
