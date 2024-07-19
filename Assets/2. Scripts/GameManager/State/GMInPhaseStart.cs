using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMInPhaseStart : BaseState
{
    public override void OnEnterState()
    {
        GameManager.gm.m_curState = GMSTATE.PHASE_START;
        GameManager.gm.m_iCurRound++;

        if (GameManager.gm.m_iCurRound > GameManager.gm.m_iMaxRound) return;

        GameManager.gm.SetRoundInfo(GameManager.gm.m_iCurRound);
        StartTimer();

        if (PhotonNetwork.IsMasterClient)
            GameManager.gm.StartEnemyWave();
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

        NetworkManager.nm.m_iGameTimer = 61;

        GameManager.gm.StartCoroutine(NetworkManager.nm.InGameTimer());
    }
}
