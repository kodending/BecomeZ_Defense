using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInEnteringLogin : BaseState
{
    GameObject m_goConnectingPanel;
    Text m_txtLoading;

    string m_strDot;
    float m_fTimer;

    public override void OnEnterState()
    {
        m_strDot = ".";
        GameObject canvas = UIManager.um.m_LoadCanvas;
        canvas.gameObject.SetActive(true);
        m_goConnectingPanel = UIManager.um.m_LoadingPanel;
        m_goConnectingPanel.SetActive(true);
        m_txtLoading = UIManager.um.m_txtLoading;
    }

    public override void OnUpdateState()
    {
        m_fTimer += Time.deltaTime;

        if (m_fTimer > 1.5f)
        {
            m_fTimer = 0;
            m_strDot += ".";
        }

        if (m_strDot == ".....") m_strDot = ".";

        m_txtLoading.text = "서버에 접속중이에염" + m_strDot;
    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        m_fTimer = 0;
    }
}
