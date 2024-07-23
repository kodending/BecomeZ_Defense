using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;


public class UIInGame : BaseState
{
    #region 조이스틱 UI 관련 변수
    Button m_btnJoy;
    bool m_isJoyCheck;
    #endregion

    #region 채팅 관련 변수
    Button m_btnChat;
    Button m_btnSend;
    InputField m_inputChat;
    GameObject m_goChatView;
    #endregion

    #region 세팅관련 변수
    Button m_btnSetting;
    #endregion

    #region 업그레이드 관련 변수
    GameObject m_goUpgradePanel;
    Button m_btnUpChar;
    Button m_btnUpUnit;
    Button m_btnGambling;
    #endregion

    public override void OnEnterState()
    {
        UIManager.um.m_eCurState = UISTATE.INGAME;
        InitSettingUI();
    }

    public override void OnUpdateState()
    {
        //test
        //if(Input.GetKeyDown(KeyCode.G))
        //{
        //    GameManager.gm.m_pcLocal.m_iMyGold += 1000;
        //    UIManager.um.RefreshMyGoldText();
        //}
    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        ClearButtonSet();
    }

    void InitSettingUI()
    {
        GameObject gameUIPanel = GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject;

        UIManager.um.m_btnReady.gameObject.SetActive(false);

        //#region 조이스틱 UI 관련 변수
        //m_btnJoy            = gameUIPanel.transform.Find("JoyButton").GetComponent<Button>();
        //m_btnJoy.onClick.AddListener(OnClickJoy);
        //m_btnJoy.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        //m_isJoyCheck = GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? false : true;

        //m_btnJoy.image.sprite = UIManager.um.m_listCheckSprite[Convert.ToInt32(m_isJoyCheck)];

        //GameManager.gm.m_pcLocal.m_fixedJoy.gameObject.SetActive(!m_isJoyCheck);
        //#endregion

        #region 채팅 관련 변수
        m_btnChat           = gameUIPanel.transform.Find("ActiveChatButton").GetComponent<Button>();
        m_btnChat.onClick.AddListener(OnClickActiveChat);
        m_btnChat.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_btnSend           = gameUIPanel.transform.Find("SendButton").GetComponent<Button>();
        m_btnSend.onClick.AddListener(OnClickSend);
        m_btnSend.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_inputChat         = gameUIPanel.transform.Find("InputChat").GetComponent<InputField>();
        m_goChatView        = gameUIPanel.transform.Find("ChatView").gameObject;

        OnJoinedChat();

        //채팅관련 변수 비활성화로 시작
        UIManager.um.HideSlideUI(m_goChatView, UIDIRECTION.DOWN);
        UIManager.um.HideSlideUI(m_btnSend.gameObject, UIDIRECTION.DOWN);
        UIManager.um.HideSlideUI(m_inputChat.gameObject, UIDIRECTION.DOWN);
        #endregion

        #region 세팅관련 변수
        m_btnSetting        = gameUIPanel.transform.Find("SettingPanel").transform.Find("SettingButton").GetComponent<Button>();
        m_btnSetting.onClick.AddListener(OnClickSetting);
        m_btnSetting.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        #endregion

        #region 업그레이드 관련 변수
        m_goUpgradePanel    = gameUIPanel.transform.Find("UpgradePanel").gameObject;
        m_btnUpChar         = m_goUpgradePanel.transform.Find("CharacterButton").GetComponent<Button>();
        m_btnUpChar.onClick.AddListener(OnClickUpChar);
        m_btnUpChar.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_btnUpUnit         = m_goUpgradePanel.transform.Find("UnitButton").GetComponent<Button>();
        m_btnUpUnit.onClick.AddListener(OnClickUpUnit);
        m_btnUpUnit.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_btnGambling       = m_goUpgradePanel.transform.Find("GamblingButton").GetComponent<Button>();
        m_btnGambling.onClick.AddListener(OnClickGambling);
        m_btnGambling.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_goUpgradePanel.SetActive(true);
        UIManager.um.ShowSlideUI(m_goUpgradePanel, UIDIRECTION.LEFT);
        #endregion

        #region 인게임 체력 관련 변수
        UIManager.um.m_goLifePanel = gameUIPanel.transform.Find("LifePanel").gameObject;
        UIManager.um.m_txtLife = UIManager.um.m_goLifePanel.transform.Find("LifePercentText").GetComponent<Text>();
        UIManager.um.m_sdLife = UIManager.um.m_goLifePanel.transform.Find("Slider").GetComponent<Slider>();

        UIManager.um.m_goLifePanel.SetActive(true);
        UIManager.um.ShowSlideUI(UIManager.um.m_goLifePanel, UIDIRECTION.DOWN);
        #endregion

        #region 인게임 타이머 관련 변수
        UIManager.um.m_goTimerPanel = gameUIPanel.transform.Find("TimerPanel").gameObject;
        UIManager.um.m_txtGameTimer = UIManager.um.m_goTimerPanel.transform.Find("TimeText").GetComponent<Text>();

        UIManager.um.m_goTimerPanel.SetActive(true);
        UIManager.um.ShowScaleUI(UIManager.um.m_goTimerPanel);
        #endregion

        #region 인게임 리더보드
        UIManager.um.m_goLeaderboardPanel = gameUIPanel.transform.Find("LeaderboardPanel").gameObject;
        UIManager.um.m_arrTxtBoard = UIManager.um.m_goLeaderboardPanel.GetComponentsInChildren<Text>();

        int maxPlayer = PhotonNetwork.PlayerList.Length;

        for (int i = 0; i < UIManager.um.m_arrTxtBoard.Length; i++)
        {
            var txt = UIManager.um.m_arrTxtBoard[i];
            
            if (i < maxPlayer)
            {
                var player = PhotonNetwork.PlayerList[i];
                txt.text = player.NickName + "   :   " + "0 " + "KILL";    
            }

            else
            {
                txt.text = "";
            }
        }

        UIManager.um.m_goLeaderboardPanel.SetActive(true);
        UIManager.um.ShowSlideUI(UIManager.um.m_goLeaderboardPanel, UIDIRECTION.RIGHT);
        #endregion

        #region 인게임 돈 확인
        UIManager.um.m_goGoldPanel.SetActive(true);
        UIManager.um.ShowScaleUI(UIManager.um.m_goGoldPanel);
        #endregion
    }

    void ClearButtonSet()
    {
        //m_btnJoy.onClick.RemoveAllListeners();
        m_btnChat.onClick.RemoveAllListeners();
        m_btnSend.onClick.RemoveAllListeners();
        m_btnSetting.onClick.RemoveAllListeners();
        m_btnUpChar.onClick.RemoveAllListeners();
        m_btnUpUnit.onClick.RemoveAllListeners();
        m_btnGambling.onClick.RemoveAllListeners();
    }

    //void OnClickJoy()
    //{
    //    GameManager.gm.m_pcLocal.m_eCurControl =
    //    GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? PLAYERCONTROL.KEYBOARD : PLAYERCONTROL.JOYSTCK;

    //    m_isJoyCheck = GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? false : true;

    //    m_btnJoy.image.sprite = UIManager.um.m_listCheckSprite[Convert.ToInt32(m_isJoyCheck)];

    //    GameManager.gm.m_pcLocal.m_fixedJoy.gameObject.SetActive(!m_isJoyCheck);
    //}

    void OnClickActiveChat()
    {
        if (!m_goChatView.activeSelf)
        {
            m_goChatView.SetActive(true);
            UIManager.um.ShowSlideUI(m_goChatView, UIDIRECTION.UP);
            m_btnSend.gameObject.SetActive(true);
            UIManager.um.ShowSlideUI(m_btnSend.gameObject, UIDIRECTION.UP);
            m_inputChat.gameObject.SetActive(true);
            UIManager.um.ShowSlideUI(m_inputChat.gameObject, UIDIRECTION.UP);
        }
        else
        {
            UIManager.um.HideSlideUI(m_goChatView, UIDIRECTION.DOWN);
            UIManager.um.HideSlideUI(m_btnSend.gameObject, UIDIRECTION.DOWN);
            UIManager.um.HideSlideUI(m_inputChat.gameObject, UIDIRECTION.DOWN);
        }
    }
    void OnJoinedChat()
    {
        //챗 초기화
        m_inputChat.text = "";
    }

    void OnClickSend()
    {
        if (!m_goChatView.activeSelf) return;

        SendChat();
    }

    void SendChat()
    {
        if (m_inputChat == null) return;

        if (m_inputChat.text == "")
        {
            m_inputChat.Select();
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        string msg = PhotonNetwork.NickName + " : " + m_inputChat.text;
        NetworkManager.nm.PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + m_inputChat.text);
        m_inputChat.text = "";

        m_inputChat.ActivateInputField();
    }

    void OnClickSetting()
    {
        UIManager.um.OnClickOption();
    }

    void OnClickUpChar()
    {
        OnActviePanel(UserPanelManager.upm.m_goUserPanel);
    }

    void OnClickUpUnit()
    {
        OnActviePanel(UnitPanelManager.upm.m_goUnitPanel);
    }

    void OnClickGambling()
    {
        GameManager.gm.m_bGambleMode = true;
        UnitPanelManager.upm.m_goReturnPanel.SetActive(true);
    }

    void OnActviePanel(GameObject panel)
    {
        UnitPanelManager.upm.m_goUnitPanel.SetActive(false);
        UserPanelManager.upm.m_goUserPanel.SetActive(false);

        panel.SetActive(true);
    }
}
