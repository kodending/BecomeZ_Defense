using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSettingInfo : MonoBehaviourPunCallbacks
{
    public Image outlineImg;
    public Text roomNumTxt;
    public Text roomName;
    public Text curPeopleTxt;
    public GameObject privateObj;
    public GameObject publicObj;

    public int roomIdx;

    public void OnClickRoom()
    {
        EntryRoomManager.OpenEntryRoom(roomIdx);
    }
}
