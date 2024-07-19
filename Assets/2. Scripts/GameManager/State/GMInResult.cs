using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMInResult : BaseState
{
    public override void OnEnterState()
    {
        GameManager.gm.m_curState = GMSTATE.RESULT;

        AudioManager.ClearCheerSfx();

        Debug.Log("나 왔었다");

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

        int gameTimer = NetworkManager.nm.m_iGameTimer;

        NetworkManager.nm.m_iGameTimer = 5;

        if(gameTimer <= 0)
            GameManager.gm.StartCoroutine(NetworkManager.nm.InGameTimer());
    }
}
