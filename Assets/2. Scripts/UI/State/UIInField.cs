using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class UIInField : BaseState
{
    Button m_btnJoy;

    Button m_btnAttack;

    Button m_btnJump;

    Button m_btnCraft;

    Text m_txtReady;

    Button m_btnStart;

    bool m_isCheck;

    #region 채팅관련 함수

    InputField m_inputChat;

    Button m_btnSend;

    Button m_btnChat;

    GameObject m_goChatView;
    #endregion

    #region 세팅관련 변수
    Button m_btnSetting;
    #endregion

    public override void OnEnterState()
    {
        UIManager.um.m_eCurState = UISTATE.FIELD;
        JoinLoading();
    }

    public override void OnUpdateState()
    {
        //if (m_isPossibleAttack && Input.GetKeyDown(KeyCode.F))
        //{
        //    OnClickAttackBtn();
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    OnClickCraftBtn();
        //}

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickSendBtn();
        }

        if (m_btnCraft != null)
        {
            if(!GameManager.gm.m_bCraftMode &&
                m_btnCraft.image.color != Color.white)
            {
                m_btnCraft.image.color = Color.white;
            }
        }
    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {

    }

    void JoinLoading()
    {
        StartCoroutine(InActivePanel());

        GameObject gameUIPanel = GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject;

        m_btnAttack = gameUIPanel.transform.Find("AttackButton").GetComponent<Button>();
        m_btnAttack.onClick.RemoveAllListeners();
        m_btnAttack.onClick.AddListener(OnClickAttackBtn);
        m_btnAttack.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnJump = gameUIPanel.transform.Find("JumpButton").GetComponent<Button>();
        m_btnJump.onClick.RemoveAllListeners();
        m_btnJump.onClick.AddListener(OnClickJumpBtn);
        m_btnJump.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnCraft = gameUIPanel.transform.Find("CraftButton").GetComponent<Button>();
        m_btnCraft.onClick.RemoveAllListeners();
        m_btnCraft.onClick.AddListener(OnClickCraftBtn);
        m_btnCraft.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnJoy = gameUIPanel.transform.Find("JoyButton").GetComponent<Button>();
        m_btnJoy.onClick.RemoveAllListeners();
        m_btnJoy.onClick.AddListener(OnClickJoyBtn);
        m_btnJoy.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_btnSend = gameUIPanel.transform.Find("SendButton").GetComponent<Button>();
        m_btnSend.onClick.RemoveAllListeners();
        m_btnSend.onClick.AddListener(OnClickSendBtn);
        m_btnSend.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        UIManager.um.m_btnReady = gameUIPanel.transform.Find("ReadyButton").GetComponent<Button>();
        UIManager.um.m_btnReady.onClick.RemoveAllListeners();
        UIManager.um.m_btnReady.onClick.AddListener(OnClickReady);
        UIManager.um.m_btnReady.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        UIManager.um.m_btnReady.gameObject.SetActive(true);
        m_txtReady = UIManager.um.m_btnReady.gameObject.transform.Find("ReadyText").GetComponent<Text>();

        m_btnChat = gameUIPanel.transform.Find("ActiveChatButton").GetComponent<Button>();
        m_btnChat.onClick.RemoveAllListeners();
        m_btnChat.onClick.AddListener(OnClickChat);
        m_btnChat.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_goChatView = gameUIPanel.transform.Find("ChatView").gameObject;

        m_btnStart = gameUIPanel.transform.Find("StartButton").GetComponent<Button>();
        m_btnStart.onClick.RemoveAllListeners();
        m_btnStart.onClick.AddListener(OnClickStart);
        m_btnStart.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        if (PhotonNetwork.IsMasterClient)m_btnStart.gameObject.SetActive(true);

        UIManager.um.m_txtCount = gameUIPanel.transform.Find("CountText").GetComponent<Text>();

        m_inputChat = gameUIPanel.transform.Find("InputChat").GetComponent<InputField>();

        m_isCheck = GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? false : true;

        m_btnJoy.image.sprite = UIManager.um.m_listCheckSprite[Convert.ToInt32(m_isCheck)];

        GameManager.gm.m_pcLocal.m_fixedJoy.gameObject.SetActive(!m_isCheck);

        NetworkManager.nm.m_iStartTimer = 4;

        #region 인게임 리더보드
        UIManager.um.m_goLeaderboardPanel = gameUIPanel.transform.Find("LeaderboardPanel").gameObject;
        UIManager.um.m_arrTxtBoard = UIManager.um.m_goLeaderboardPanel.GetComponentsInChildren<Text>();

        if(PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < UIManager.um.m_arrTxtBoard.Length; i++)
            {
                var txt = UIManager.um.m_arrTxtBoard[i];

                if (i == 0)
                {
                    var player = PhotonNetwork.PlayerList[i];
                    txt.text = player.NickName + "   :   " + "WAIT";
                    GameManager.gm.m_dicUserRoomState.Add(player.NickName, (int)USERROOMSTATE.WAIT);
                }

                else
                {
                    txt.text = "";
                }
            }
        }    

        UIManager.um.RefreshReadyLeaderboard();
        UIManager.um.m_goLeaderboardPanel.SetActive(true);
        #endregion

        #region 인게임 타이머 관련 변수
        UIManager.um.m_goTimerPanel = gameUIPanel.transform.Find("TimerPanel").gameObject;
        UIManager.um.m_txtGameTimer = UIManager.um.m_goTimerPanel.transform.Find("TimeText").GetComponent<Text>();
        #endregion

        #region 세팅관련 변수
        m_btnSetting = gameUIPanel.transform.Find("SettingPanel").transform.Find("SettingButton").GetComponent<Button>();
        m_btnSetting.onClick.RemoveAllListeners();
        m_btnSetting.onClick.AddListener(OnClickSetting);
        m_btnSetting.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        #endregion

        OnJoinedChat();
    }

    void OnClickJoyBtn()
    {
        GameManager.gm.m_pcLocal.m_eCurControl =
        GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? PLAYERCONTROL.KEYBOARD : PLAYERCONTROL.JOYSTCK;

        m_isCheck = GameManager.gm.m_pcLocal.m_eCurControl == PLAYERCONTROL.JOYSTCK ? false : true;

        m_btnJoy.image.sprite = UIManager.um.m_listCheckSprite[Convert.ToInt32(m_isCheck)];

        GameManager.gm.m_pcLocal.m_fixedJoy.gameObject.SetActive(!m_isCheck);
    }

    void OnClickAttackBtn()
    {
        if (GameManager.gm.m_bCraftMode) return;

        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

        if (GameManager.gm.m_pcLocal.m_curState != PLAYERSTATE.ATTACK)
            GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.ATTACK);
    }
    
    void OnClickJumpBtn()
    {
        if (GameManager.gm.m_bCraftMode) return;

        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

        if (GameManager.gm.m_pcLocal.m_curState == PLAYERSTATE.IDLE || GameManager.gm.m_pcLocal.m_curState == PLAYERSTATE.MOVE)
            GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.JUMP);
    }

    void OnClickCraftBtn()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

        if (GameManager.gm.m_pcLocal.m_curState != PLAYERSTATE.IDLE &&
            GameManager.gm.m_pcLocal.m_curState != PLAYERSTATE.MOVE) return;

        GameManager.gm.m_bCraftMode = !GameManager.gm.m_bCraftMode;

        //if (GameManager.gm.m_bCraftMode) m_btnCraft.image.color = Color.red;
    }

    void OnClickReady()
    {
        if(GameManager.gm.m_curRoomState == USERROOMSTATE.WAIT)
        {
            GameManager.gm.m_curRoomState = USERROOMSTATE.READY;
            GameManager.gm.ChangeReadyState(USERROOMSTATE.READY);
            m_txtReady.text = "<color=blue>" + "준비완료" + "</color>";
        }

        else if(GameManager.gm.m_curRoomState == USERROOMSTATE.READY)
        {
            GameManager.gm.m_curRoomState = USERROOMSTATE.WAIT;
            GameManager.gm.ChangeReadyState(USERROOMSTATE.WAIT);
            m_txtReady.text ="준비";
        }
    }

    void OnClickStart()
    {
        //인원 전원 레디 되었는지 확인
        //if (PhotonNetwork.PlayerList.Length < 2)
        //{
        //    UIManager.um.SystemMessage("2명 이상부터 시작할 수 있습니다.");
        //    return;
        //}

        bool bReady = true;

        foreach(var player in PhotonNetwork.PlayerList)
        {
            if (GameManager.gm.m_dicUserRoomState[player.NickName] == (int)USERROOMSTATE.WAIT)
            {
                bReady = false;
                break;
            }
        }

        if (!bReady)
        {
            UIManager.um.SystemMessage("준비완료가 되지 않았어요");
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;

        NetworkManager.nm.InActiveReadyButton();

        UIManager.um.HideScaleUI(m_btnStart.gameObject);

        StartCoroutine(NetworkManager.nm.GameStartCount());
    }

    void OnClickSendBtn()
    {
        if (!m_goChatView.activeSelf) return;
        
        SendChat();
    }

    void OnClickChat()
    {
        if(!m_goChatView.activeSelf)
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

    IEnumerator InActivePanel()
    {
        yield return new WaitForSeconds(1f);

        GameObject canvas = UIManager.um.m_LoadCanvas;
        canvas.gameObject.SetActive(false);
        canvas.transform.Find("ConnectingPanel").gameObject.SetActive(false);
    }

    void OnClickSetting()
    {
        UIManager.um.OnClickOption();
    }
}
