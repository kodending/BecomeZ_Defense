using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntryRoomManager : MonoBehaviourPunCallbacks
{
    static public EntryRoomManager erm;

    public GameObject m_goRoomPanel;
    public GameObject m_goPublic;
    public GameObject m_goPrivate;
    public Text m_txtRoomName;
    public Text m_txtRoomNum;
    public InputField m_fieldPassword;

    RoomInfo m_curRoomInfo;
    int m_curRoomIdx;

    private void Awake()
    {
        erm = this;
    }

    static public void OpenEntryRoom(int roomIdx)
    {
        erm.m_curRoomIdx = roomIdx;
        erm.m_curRoomInfo = NetworkManager.nm.m_curRoomInfo[roomIdx];

        if(erm.m_curRoomInfo.PlayerCount >= erm.m_curRoomInfo.MaxPlayers)
        {
            UIManager.um.SystemMessage("방 인원이 꽉 찼습니다.");
            return;
        }

        erm.m_txtRoomName.text = erm.m_curRoomInfo.Name;
        string number = (roomIdx + 1).ToString("D3");
        erm.m_txtRoomNum.text = number;

        if(erm.m_curRoomInfo.CustomProperties.Count == 0)
        {
            erm.m_goPublic.SetActive(true);
            erm.m_goPrivate.SetActive(false);
            erm.m_fieldPassword.gameObject.SetActive(false);
            erm.m_fieldPassword.text = "";
        }

        else
        {
            erm.m_goPrivate.SetActive(true);
            erm.m_goPublic.SetActive(false);
            erm.m_fieldPassword.text = "";
            erm.m_fieldPassword.gameObject.SetActive(true);
        }

        erm.m_goRoomPanel.SetActive(true);
    }

    public void OnClickExitButton()
    {
        erm.m_goRoomPanel.SetActive(false);    
    }

    public void OnClickEntryButton()
    {
        if((string)erm.m_curRoomInfo.CustomProperties["password"] != null && 
            erm.m_fieldPassword.text != (string)erm.m_curRoomInfo.CustomProperties["password"])
        {
            UIManager.um.SystemMessage("패스워드가 일치하지 않습니다.");
            return;
        }

        if(NetworkManager.nm.m_curRoomInfo[erm.m_curRoomIdx].PlayerCount >= NetworkManager.nm.m_curRoomInfo[erm.m_curRoomIdx].MaxPlayers)
        {
            UIManager.um.SystemMessage("인원이 가득 찼습니다.");
            return;
        }

        if(!NetworkManager.nm.m_curRoomInfo[erm.m_curRoomIdx].IsOpen)
        {
            UIManager.um.SystemMessage("이미 게임이 시작된 방입니다.");
            return;
        }

        NetworkManager.nm.JoinRoom(erm.m_curRoomInfo.Name);
        GameManager.gm.ChangeScene("InGameScene", GMSTATE.FIELD);
    }
}
