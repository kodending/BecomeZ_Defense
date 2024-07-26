using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInLobby : BaseState
{
    #region 로비 화면 구성
    GameObject m_goRoomPanel, m_goRoomList;
    Button m_btnReturn, m_btnCreate, m_btnRefresh, m_btnPrevious, m_btnNext;
    #endregion

    #region 방만들기 화면 구성
    GameObject m_goCreateRoomPanel;
    Button m_btnExit, m_btnCreateAndEntry, m_btnPublic, m_btnPrivate;
    InputField m_inputRoomName, m_inputPwd;
    #endregion

    public override void OnEnterState()
    {
        InitUI();
        RefreshRoomInfo();
    }

    public override void OnUpdateState()
    {

    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        m_btnReturn.onClick.RemoveAllListeners();
        m_btnCreate.onClick.RemoveAllListeners();
        m_btnRefresh.onClick.RemoveAllListeners();
        m_btnNext.onClick.RemoveAllListeners();
        m_btnPrevious.onClick.RemoveAllListeners();
        m_btnExit.onClick.RemoveAllListeners();
        m_btnCreateAndEntry.onClick.RemoveAllListeners();
        m_btnPublic.onClick.RemoveAllListeners();
        m_btnPrivate.onClick.RemoveAllListeners();

        m_goRoomPanel.gameObject.SetActive(false);
    }

    void InitUI()
    {
        #region 로비 화면 구성
        GameObject ReadyPanel = GameObject.Find("3DCanvas").transform.Find("ReadyPanel").gameObject;
        ReadyPanel.SetActive(false);

        m_goRoomPanel = GameObject.Find("3DCanvas").transform.Find("RoomInfoPanel").gameObject;
        m_goRoomPanel.gameObject.SetActive(true);

        m_goRoomList = m_goRoomPanel.transform.Find("RoomList").gameObject;

        m_btnReturn = m_goRoomPanel.transform.Find("ReturnButton").GetComponent<Button>();
        m_btnReturn.onClick.AddListener(OnClickReturn);

        m_btnCreate = m_goRoomPanel.transform.Find("CreateButton").GetComponent<Button>();
        m_btnCreate.onClick.AddListener(OnClickCreate);

        m_btnRefresh = m_goRoomPanel.transform.Find("RefreshButton").GetComponent<Button>();
        m_btnRefresh.onClick.AddListener(OnClickRefresh);

        m_btnNext = m_goRoomPanel.transform.Find("NextButton").GetComponent<Button>();
        m_btnNext.onClick.AddListener(OnClickNext);

        m_btnPrevious = m_goRoomPanel.transform.Find("PreviousButton").GetComponent<Button>();
        m_btnPrevious.onClick.AddListener(OnClickPrevious);
        #endregion

        #region 방 만들기 구성
        m_goCreateRoomPanel = GameObject.Find("3DCanvas").transform.Find("CreateRoomPanel").gameObject;
        m_goCreateRoomPanel.gameObject.SetActive(false);

        GameObject panel = m_goCreateRoomPanel.transform.Find("Panel").gameObject;

        m_btnExit = panel.transform.Find("ExitButton").GetComponent<Button>();
        m_btnExit.onClick.AddListener(OnClickExit);

        m_btnCreateAndEntry = panel.transform.Find("CreateButton").GetComponent<Button>();
        m_btnCreateAndEntry.onClick.AddListener(OnCreateAndEntry);

        m_btnPublic = panel.transform.Find("CheckPublic").GetComponent<Button>();
        m_btnPublic.onClick.AddListener(OnClickPublic);

        m_btnPrivate = panel.transform.Find("CheckPrivate").GetComponent<Button>();
        m_btnPrivate.onClick.AddListener(OnClickPrivate);

        m_inputPwd = panel.transform.Find("PasswordField").GetComponent<InputField>();
        m_inputRoomName = panel.transform.Find("RoomNameField").GetComponent<InputField>();
        #endregion
    }

    void RefreshRoomInfo()
    {
        if(m_goRoomList.transform.childCount != 0)
        {
            foreach (Transform child in m_goRoomList.transform)
            {
                RoomPoolManager.ReturnRoom(child.gameObject);
            }
        }

        if(NetworkManager.nm.m_curRoomInfo.Count == 0)
        {
            UIManager.um.SystemMessage("생성된 방이 없습니다.");
            return;
        }

        for(int i = 0; i < NetworkManager.nm.m_curRoomInfo.Count; i++)
        {
            var room = NetworkManager.nm.m_curRoomInfo[i];

            var prefab = RoomPoolManager.GetRoom().GetComponent<RoomSettingInfo>();

            prefab.outlineImg.color = room.IsOpen ? Color.blue : Color.red;
            string number = (i + 1).ToString("D3");
            prefab.roomNumTxt.text = number;
            prefab.roomName.text = room.Name;
            prefab.curPeopleTxt.text = room.PlayerCount.ToString() + " / " + room.MaxPlayers;
            prefab.roomIdx = i;

            if (room.CustomProperties.Count == 0)
            {
                prefab.publicObj.SetActive(true);
                prefab.privateObj.SetActive(false);
            }
            else
            {
                prefab.privateObj.SetActive(true);
                prefab.publicObj.SetActive(false);
            }

            prefab.gameObject.SetActive(true);
        }
    }

    void OnClickReturn()
    {
        GameManager.gm.m_stateMachine.ChangeState(GMSTATE.MAIN);
        UIManager.um.m_stateMachine.ChangeState(UISTATE.READY);
    }

    void OnClickCreate()
    {
        m_goCreateRoomPanel.SetActive(true);
        m_inputRoomName.text = "";
        m_inputPwd.text = "";
    }

    void OnClickRefresh()
    {
        RefreshRoomInfo();
    }

    void OnClickPrevious()
    {

    }

    void OnClickNext()
    {

    }
    
    void OnClickExit()
    {
        m_inputPwd.text = "";
        m_inputRoomName.text = "";
        m_inputPwd.gameObject.SetActive(false);
        m_goCreateRoomPanel.SetActive(false);
    }

    void OnCreateAndEntry()
    {
        if (NetworkManager.nm.m_curRoomInfo.Count >= 8)
        {
            UIManager.um.SystemMessage("방은 8개까지만 생성 가능합니다.");
            return;
        }

        if (!CheckDuplicate(m_inputRoomName.text))
        {
            UIManager.um.SystemMessage("이미 존재하는 방입니다.");
            return;
        }

        NetworkManager.nm.CreateRoom(m_inputRoomName.text, m_inputPwd.text);
        GameManager.gm.ChangeScene("InGameScene", GMSTATE.FIELD);
    }

    void OnClickPublic()
    {
        m_btnPublic.gameObject.SetActive(false);
        m_btnPrivate.gameObject.SetActive(true);

        m_inputPwd.gameObject.SetActive(true);
    }

    void OnClickPrivate()
    {
        m_btnPrivate.gameObject.SetActive(false);
        m_inputPwd.gameObject.SetActive(false);

        m_btnPublic.gameObject.SetActive(true);
    }

    bool CheckDuplicate(string name)
    {
        bool isCheck = false;

        if(NetworkManager.nm.m_curRoomInfo.Count == 0)
        {
            isCheck = true;
            return isCheck;
        }

        foreach (var room in NetworkManager.nm.m_curRoomInfo)
        {
            if(room.Name == name)
            {
                isCheck = false;
                return isCheck;
            }
        }


        return true;
    }
}
