using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIInLoad : BaseState
{
    GameObject m_goLoadPanel;

    public override void OnEnterState()
    {
        m_goLoadPanel = GameObject.Find("Canvas").transform.Find("LoadPanel").gameObject;

        StartCoroutine(NextState());
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

    IEnumerator NextState()
    {
        yield return new WaitForSeconds(5f);

        GameManager.gm.ChangeScene("MainScene", GMSTATE.MAIN);

        UIManager.um.m_LoadCanvas.SetActive(true);
        UIManager.um.m_LoadingPanel.SetActive(true);
        UIManager.um.m_txtLoading.text = "데이터를 로드 중입니다. 잠시만 기다려주세요";

        StartCoroutine(ChangeMainLoad());
    }

    IEnumerator ChangeMainLoad()
    {
        yield return new WaitForSeconds(1f);

        UIManager.um.m_LoadCanvas.SetActive(false);
        UIManager.um.m_LoadingPanel.SetActive(false);
        UIManager.um.m_stateMachine.ChangeState(UISTATE.MAIN);
    }
}
