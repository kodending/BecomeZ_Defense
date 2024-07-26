using Photon.Pun;
using PlayFab.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UIInReady : BaseState
{
    [Header("준비단계 버튼 관련 변수")]
    GameObject m_goCanvas, m_go3DCanvas, m_goReadyPanel, m_goChangePanel, m_goCheckPanel, m_goConnectingPanel;
    Button m_btnCostume, m_btnEntry, m_btnChangeName, m_btnLogout;
    Text m_txtNickName;

    [Header("닉네임 변경 패널 변수")]
    InputField m_inputNickName;
    Button m_btnConfirm, m_btnCancel;

    [Header("재확인용 메세지")]
    Button m_btnYes, m_btnNo;

    public override void OnEnterState()
    {
        UIManager.um.m_eCurState = UISTATE.READY;
        if(!AudioManager.IsPlayBGM(BGM.LOBBY))
            AudioManager.PlayBGM(BGM.LOBBY, true);
        StartSet();
    }

    public override void OnUpdateState()
    {

    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        m_btnCostume.onClick.RemoveAllListeners();
        m_btnEntry.onClick.RemoveAllListeners();
        m_btnChangeName.onClick.RemoveAllListeners();
        m_btnConfirm.onClick.RemoveAllListeners();
        m_btnCancel.onClick.RemoveAllListeners();
        m_btnYes.onClick.RemoveAllListeners();
        m_btnNo.onClick.RemoveAllListeners();
        m_btnLogout.onClick.RemoveAllListeners();

        //m_goReadyPanel.SetActive(false);
    }

    void StartSet()
    {
        #region 준비상태패널
        m_go3DCanvas = GameObject.Find("3DCanvas").gameObject;
        m_goCanvas = GameObject.Find("Canvas").gameObject;

        m_goReadyPanel = m_go3DCanvas.transform.Find("ReadyPanel").gameObject;

        m_btnEntry = m_goReadyPanel.transform.Find("EntryButton").GetComponent<Button>();
        m_btnEntry.onClick.AddListener(OnClickEntry);
        m_btnEntry.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnChangeName = m_goReadyPanel.transform.Find("ChangeButton").GetComponent<Button>();
        m_btnChangeName.onClick.AddListener(OnClickChangeName);
        m_btnChangeName.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_txtNickName = m_goReadyPanel.transform.Find("TagNickName").transform.Find("NickNameText").GetComponent<Text>();
        m_txtNickName.text = NetworkManager.nm.m_myPlayFabInfo.DisplayName;

        m_btnCostume = m_goReadyPanel.transform.Find("CostumeButton").GetComponent<Button>();
        m_btnCostume.onClick.AddListener(OnClickCostume);
        m_btnCostume.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnLogout = m_goReadyPanel.transform.Find("LogoutButton").GetComponent<Button>();
        m_btnLogout.onClick.AddListener(OnClickLogout);
        m_btnLogout.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        #endregion

        #region 닉네임변경패널
        m_goChangePanel = m_go3DCanvas.transform.Find("ChangePanel").gameObject;

        m_inputNickName = m_goChangePanel.transform.Find("InputNickName").GetComponent<InputField>();

        if (NetworkManager.nm.m_myPlayFabInfo.DisplayName != "")
        {
            m_inputNickName.text = NetworkManager.nm.m_myPlayFabInfo.DisplayName;
        }
        else Debug.Log("디스플레이 이름이 없어용");
        if (PhotonNetwork.LocalPlayer.NickName != "")
        {
            m_inputNickName.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else Debug.Log("로컬플레이어 이름이 없어용");

        m_btnConfirm = m_goChangePanel.transform.Find("ConfirmButton").GetComponent<Button>();
        m_btnConfirm.onClick.AddListener(OnClickConfirm);
        m_btnConfirm.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnCancel = m_goChangePanel.transform.Find("CancelButton").GetComponent<Button>();
        m_btnCancel.onClick.AddListener(OnClickCancel);
        m_btnCancel.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        #endregion

        #region 최종체크 패널
        m_goCheckPanel = m_goChangePanel.transform.Find("CheckPanel").gameObject;

        m_btnYes = m_goCheckPanel.transform.Find("YesButton").GetComponent<Button>();
        m_btnYes.onClick.AddListener(OnClickYes);
        m_btnYes.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnNo = m_goCheckPanel.transform.Find("NoButton").GetComponent<Button>();
        m_btnNo.onClick.AddListener(OnClickNo);
        m_btnNo.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        #endregion

        m_goCanvas.transform.Find("MainPanel").gameObject.SetActive(false);

        m_goConnectingPanel = UIManager.um.m_LoadingPanel;

        NetworkManager.nm.SetCostumeData();

        UIManager.um.StartCoroutine(DelayShowHideUI());
    }

    IEnumerator DelayShowHideUI()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject canvas = UIManager.um.m_LoadCanvas;
        canvas.gameObject.SetActive(false);
        m_goConnectingPanel.SetActive(false);

        m_goReadyPanel.SetActive(true);
        UIManager.um.ShowScaleUI(m_goReadyPanel);
    }

    void OnClickCostume()
    {
        m_goReadyPanel.SetActive(false);
        UIManager.um.m_stateMachine.ChangeState(UISTATE.COSTUME);
    }

    void OnClickLogout()
    {
        NetworkManager.nm.LogoutPlayfab();
        UIManager.um.m_stateMachine.ChangeState(UISTATE.MAIN);
    }

    void OnClickEntry()
    {
        //서버 들어가고
        //게임씬으로 들어가기
        //if(!JoinPossibleCheck())
        //{
        //    UIManager.um.SystemMessage("게임이 시작하여 방에 입장할 수 없습니다.");
        //    return;
        //}

        //NetworkManager.nm.JoinRoom();
        //GameManager.gm.ChangeScene("InGameScene", GMSTATE.FIELD);

        GameManager.gm.m_stateMachine.ChangeState(GMSTATE.LOBBY);
        UIManager.um.m_stateMachine.ChangeState(UISTATE.LOBBY);
    }

    bool JoinPossibleCheck()
    {
        if (NetworkManager.nm.m_curRoomInfo.Count == 0) return true;

        if (!NetworkManager.nm.m_curRoomInfo[0].IsOpen) return false;

        return true;
    }


    void OnClickChangeName()
    {
        m_goChangePanel.SetActive(true);

        UIManager.um.ShowScaleUI(m_goChangePanel);
    }

    void OnClickConfirm()
    {
        if (m_inputNickName.text == "") return;
        if (m_inputNickName.text == NetworkManager.nm.m_myPlayFabInfo.DisplayName)
        {
            UIManager.um.SystemMessage("이전 닉네임과 동일합니다.");
            m_inputNickName.text = "";
            return;
        }

        m_goCheckPanel.SetActive(true);
        UIManager.um.ShowScaleUI(m_goCheckPanel);
    }

    void OnClickCancel()
    {
        UIManager.um.HideScaleUI(m_goChangePanel);
        m_inputNickName.text = "";
    }

    void OnClickYes()
    {
        //디스플레이 네임 저장시키기
        NetworkManager.nm.UpdateNickName(m_inputNickName.text);

        UIManager.um.HideScaleUI(m_goCheckPanel);
        UIManager.um.HideScaleUI(m_goChangePanel);

        m_txtNickName.text = m_inputNickName.text;
        PhotonNetwork.LocalPlayer.NickName = m_txtNickName.text;
    }

    void OnClickNo()
    {
        UIManager.um.HideScaleUI(m_goCheckPanel);
    }
}
