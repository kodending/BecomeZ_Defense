using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using Unity.VisualScripting;
using ExitGames.Client.Photon;
using PlayFab.GroupsModels;
using System.Net.Http;
using PlayFab.CloudScriptModels;
using PlayFab.EconomyModels;
using Photon.Pun.UtilityScripts;
using System.ComponentModel;
using DG.Tweening;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Tooltip("네트워크 매니저 싱글톤화")]
    public static NetworkManager nm;

    public PhotonView PV;

    [Header("Disconnect")]
    public PlayerLeaderboardEntry m_myPlayFabInfo;
    public List<PlayerLeaderboardEntry> m_listPlayFabUser = new List<PlayerLeaderboardEntry>();
    public Text m_txtMail, m_txtPW, m_txtName;

    int costumeIndex = 0;  //현재 저장된 값 인덱스 확인

    [HideInInspector]
    public Dictionary<string, Dictionary<int, int>> m_dicPlayFabCostume = new Dictionary<string, Dictionary<int, int>>();

    [HideInInspector]
    public int m_curUserNumInRoom;

    [HideInInspector]
    public int m_iStartTimer;

    [HideInInspector]
    public int m_iGameTimer;

    [HideInInspector]
    public List<RoomInfo> m_curRoomInfo = new List<RoomInfo>();

    [HideInInspector]
    public Queue<ATTACKINFO> m_qAttackOrder = new Queue<ATTACKINFO>();

    [HideInInspector]
    public List<EnemyFSM> m_listEnemyInfo = new List<EnemyFSM>();

    private void Awake()
    {
        nm = this;

        DontDestroyOnLoad(nm);

        //동기화를 좀더 빠르게 하기 위함
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Update()
    {
        if (m_curUserNumInRoom != m_dicPlayFabCostume.Count && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                m_curUserNumInRoom = m_dicPlayFabCostume.Count;
                PV.RPC("InformNewPlayer", RpcTarget.Others, m_dicPlayFabCostume, m_curUserNumInRoom);
            }
        }
    }

    #region 공격 신호 직렬 처리 관련 함수
    void UpdateAttackSignalProcess()
    {
        if (!PhotonNetwork.IsMasterClient && m_qAttackOrder.Count == 0) return;

        ATTACKINFO info = m_qAttackOrder.Dequeue();
        info.enemyFSM.m_sInfo.curHp -= info.iDamage;

        if (info.enemyFSM.m_sInfo.curHp <= 0)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var player = PhotonNetwork.PlayerList[i];
                if(player.NickName == info.strAttackerName)
                {
                    //이걸 아마 다른곳에서 저장시키고 해야할듯...
                    GameManager.gm.m_dicUserKillCount[player.NickName]++;
                    RefreshKillLeaderboard(GameManager.gm.m_dicUserKillCount);
                    PV.RPC("GetGoldRPC", RpcTarget.All, player.NickName, 10);
                    break;
                }
            }
        }

        FloatDmg(m_listEnemyInfo.IndexOf(info.enemyFSM), info.iDamage, info.enemyFSM.m_sInfo.curHp);

        //그거완 상관없이 공격 데미지 띄워
        PV.RPC("FloatDmg", RpcTarget.Others, m_listEnemyInfo.IndexOf(info.enemyFSM), info.iDamage, info.enemyFSM.m_sInfo.curHp);
    }

    [PunRPC]
    void GetGoldRPC(string name, int gold)
    {
        if(m_myPlayFabInfo.DisplayName == name)
        {
            GameManager.gm.m_pcLocal.m_iMyGold += gold;
            UIManager.um.RefreshMyGoldText();
            if (GameManager.gm.m_pcLocal.m_pv.IsMine) AudioManager.PlaySfx(SFX.ENEMY_DIE);
        }
    }

    [PunRPC]
    void FloatDmg(int idx, int dmg, int curHp)
    {
        GameObject go = DamagePoolManager.GetDamage(dmg.ToString());
        m_listEnemyInfo[idx].RefreshHp(curHp);
        go.transform.position = m_listEnemyInfo[idx].transform.position + new Vector3(Random.Range(0f, 1f), Random.Range(0f, 3f), Random.Range(0f, 1f));
        UIManager.um.ShowHidleScaleUI(go);
        StartCoroutine(DamagePoolManager.ReturnDamage(go));
    }

    public void AtkSignal(int idx, int dmg, string atkName)
    {
        PV.RPC("AtkRPC", RpcTarget.MasterClient, idx, dmg, atkName);
    }

    [PunRPC]
    void AtkRPC(int idx, int dmg, string atkName)
    {
        ATTACKINFO info = new ATTACKINFO();
        info.enemyFSM = m_listEnemyInfo[idx];
        info.iDamage = dmg;
        info.strAttackerName = atkName;
        m_qAttackOrder.Enqueue(info);

        UpdateAttackSignalProcess();
    }

    #endregion

    [PunRPC]
    void PassEnemyRPC()
    {
        UIManager.um.LoseLife();

        //사망한 것으로 판단
        if(UIManager.um.m_oldLife <= 0)
        {
           if(PhotonNetwork.IsMasterClient)
            {
                PV.RPC("GameSetRPC", RpcTarget.All, (int)GAMERESULT.GAMEOVER);
            }

            GameManager.gm.m_stateMachine.ChangeState(GMSTATE.RESULT);
        }
    }

    [PunRPC]
    void GameSetRPC(int result)
    {
        UIManager.um.OnResult(result);
    }

    //playfab 로그인
    public void Login(string i_strMail, string i_strPW)
    {
        var request = new LoginWithEmailAddressRequest { Email = i_strMail, Password = i_strPW };
        PlayFabClientAPI.LoginWithEmailAddress(request,
            (result) => { GetLeaderboard(result.PlayFabId); UIManager.um.SaveLoginInfo(i_strMail); GetCatalogItem(); GetInventory(); Connet(); },
            (error) => { UIManager.um.m_stateMachine.ChangeState(UISTATE.MAIN); UIManager.um.SystemMessage("로그인 실패"); });
    }

    public void Register(string i_strMail, string i_strPW, string i_strID)
    {
        var request = new RegisterPlayFabUserRequest { Email = i_strMail, Password = i_strPW, Username = i_strID, DisplayName = i_strID };
        PlayFabClientAPI.RegisterPlayFabUser(request,
            (result) => { print("회원가입 성공!"); SetStat(); SetData("default", i_strMail); SetCharacterDefaultStats(); UIManager.um.SystemMessage("회원가입 성공"); },
            (error) => { print("회원가입 실패"); UIManager.um.SystemMessage("회원가입 실패"); });
    }

    void GetLeaderboard(string myID)
    {
        m_listPlayFabUser.Clear();

        for (int i = 0; i < 100; i++)
        {
            var request = new GetLeaderboardRequest
            {
                StartPosition = i * 100,
                StatisticName = "IDInfo",
                MaxResultsCount = 100,
                ProfileConstraints = new PlayerProfileViewConstraints() { ShowDisplayName = true }
            };

            PlayFabClientAPI.GetLeaderboard(request, (result) =>
            {
                if (result.Leaderboard.Count == 0) return;
                for (int j = 0; j < result.Leaderboard.Count; j++)
                {
                    m_listPlayFabUser.Add(result.Leaderboard[j]);
                    if (result.Leaderboard[j].PlayFabId == myID) m_myPlayFabInfo = result.Leaderboard[j];
                }

                PhotonNetwork.LocalPlayer.NickName = m_myPlayFabInfo.DisplayName;
            },
            (error) => { });
        }
    }

    void SetStat()
    {
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "IDInfo", Value = 0 } } };
        PlayFabClientAPI.UpdatePlayerStatistics(request, (result) => { }, (error) => print("값 저장실패"));
    }

    void SetData(string curData, string i_strMail)
    {
        UIManager.um.m_strSavedMail = i_strMail;
    }

    public void UpdateNickName(string i_strName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = i_strName
        },
        (result) =>
        {
            print("닉네임 바꾸기 성공성공");
        },
        (error) => print("이건 아니야")
        );
    }

    public void LogoutPlayfab()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        DisConnect();
    }

    //서버에 연결하는 함수
    public void Connet() => PhotonNetwork.ConnectUsingSettings();

    //로비에 바로 접속 되도록 함수 구현
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();


    public override void OnJoinedLobby()
    {

        StartCoroutine(JoinLobby());
    }

    IEnumerator JoinLobby()
    {
        yield return new WaitForSeconds(2f);

        UIManager.um.m_stateMachine.ChangeState(UISTATE.READY);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;

        for (int i = 0; i< roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if(!m_curRoomInfo.Contains(roomList[i])) m_curRoomInfo.Add(roomList[i]);
                else m_curRoomInfo[m_curRoomInfo.IndexOf(roomList[i])] = roomList[i];
            }

            else if (m_curRoomInfo.IndexOf(roomList[i]) != -1)
                m_curRoomInfo.RemoveAt(m_curRoomInfo.IndexOf(roomList[i]));
        }
    }

    //public void JoinRoom() => PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, null);
    public void CreateRoom(string name, string password)
    { 
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        if(password == "")
        {
            PhotonNetwork.CreateRoom(name, roomOptions, null);
        }

        else
        {
            Hashtable hash = new Hashtable();
            hash["password"] = password;

            roomOptions.CustomRoomProperties = hash;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };

            PhotonNetwork.CreateRoom(name, roomOptions, null);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    //인게임에 접속 했을 때 필요한 패널들 활성화시키기
    public override void OnJoinedRoom()
    {
        UIManager.um.m_listChatTexts = GameObject.Find("Canvas").transform.Find("GameUIPanel").transform.Find("ChatView").transform.Find("Viewport").
                                       transform.Find("Content").GetComponentsInChildren<Text>();

        for (int idx = 0; idx < UIManager.um.m_listChatTexts.Length; idx++)
            UIManager.um.m_listChatTexts[idx].text = "";

        Dictionary<int, int> dic = new Dictionary<int, int>();
        for (int i = 0; i < (int)COSTUMETYPE._MAX_; i++)
        {
            dic[i] = CostumeManager.cm.m_dicCurCostume[COSTUMETYPE.Hat + i];
        }

        PV.RPC("InformEnterRoom", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.NickName, dic);

        GameObject user = PhotonNetwork.Instantiate("PLAYER_Player", new Vector3(0, 10, 0), Quaternion.identity);
        user.SetActive(true);

        StartCoroutine(PlayerSetInfo());
    }

    [PunRPC]
    void InformEnterRoom(string key, Dictionary<int, int> dic)
    {
        m_dicPlayFabCostume.Add(key, dic);
    }

    [PunRPC]
    void InformNewPlayer(Dictionary<string, Dictionary<int, int>> dic, int userNum)
    {
        m_dicPlayFabCostume = dic;
        m_curUserNumInRoom = userNum;
    }

    IEnumerator PlayerSetInfo()
    {
        yield return new WaitForSeconds(0.5f);

        UIManager.um.m_stateMachine.ChangeState(UISTATE.FIELD);
    }

    public void DisConnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + newPlayer.NickName + "님이 참가하였습니다</color>");
            GameManager.gm.m_dicUserRoomState.Add(newPlayer.NickName, (int)USERROOMSTATE.WAIT);
            GameManager.gm.m_dicUserKillCount.Add(newPlayer.NickName, 0);
            RefreshRoomPlayerState(GameManager.gm.m_dicUserRoomState);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + otherPlayer.NickName + "님이 퇴장하였습니다</color>");
            m_dicPlayFabCostume.Remove(otherPlayer.NickName);
            GameManager.gm.m_dicUserKillCount.Remove(otherPlayer.NickName);
        }

        GameManager.gm.m_dicUserRoomState.Remove(otherPlayer.NickName);

        if(UIManager.um.m_eCurState != UISTATE.INGAME)
            UIManager.um.RefreshReadyLeaderboard();
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다.
    void ChatRPC(string msg)
    {
        if (UIManager.um.m_listChatTexts.Length == 0) return;

        bool isInput = false;
        for (int idx = 0; idx < UIManager.um.m_listChatTexts.Length; idx++)
            if (UIManager.um.m_listChatTexts[idx].text == "")
            {
                isInput = true;
                UIManager.um.m_listChatTexts[idx].text = msg;
                break;
            }
        if (!isInput)
        {
            for (int idx = 1; idx < UIManager.um.m_listChatTexts.Length; idx++)
                UIManager.um.m_listChatTexts[idx - 1].text = UIManager.um.m_listChatTexts[idx].text;
            UIManager.um.m_listChatTexts[UIManager.um.m_listChatTexts.Length - 1].text = msg;
        }
    }

    public void InActiveReadyButton()
    {
        PV.RPC("InActiveReadyButtonRPC", RpcTarget.All);
    }

    [PunRPC]
    void InActiveReadyButtonRPC()
    {
        UIManager.um.HideScaleUI(UIManager.um.m_btnReady.gameObject);
    }

    public void RefreshRoomPlayerState(Dictionary<string, int> dic)
    {
        PV.RPC("RefreshRoomPlayerStateRPC", RpcTarget.All, dic);
    }

    [PunRPC]
    void RefreshRoomPlayerStateRPC(Dictionary<string, int> dic)
    {
        GameManager.gm.m_dicUserRoomState = dic;
        UIManager.um.RefreshReadyLeaderboard();
    }

    #region 게임 시작 타이머
    public IEnumerator GameStartCount()
    {
        if (m_iStartTimer > 0) m_iStartTimer -= 1;
        else
        {
            PV.RPC("StartGameRPC", RpcTarget.All);

            yield break;
        }

        PV.RPC("UpdateStartCountRPC", RpcTarget.All, m_iStartTimer);

        yield return new WaitForSeconds(1);
        StartCoroutine(GameStartCount());
    }

    [PunRPC]
    void UpdateStartCountRPC(int time)
    {
        m_iStartTimer = time;
        UIManager.um.m_txtCount.gameObject.SetActive(true);
        UIManager.um.ShowScaleUI(UIManager.um.m_txtCount.gameObject);

        if (m_iStartTimer != 0) UIManager.um.m_txtCount.text = time.ToString();
        else UIManager.um.m_txtCount.text = "START";
    }

    [PunRPC]
    void StartGameRPC()
    {
        m_iStartTimer = 4;
        UIManager.um.HideScaleUI(UIManager.um.m_txtCount.gameObject);
        GameManager.gm.m_stateMachine.ChangeState(GMSTATE.PHASE_READY);
        UIManager.um.m_stateMachine.ChangeState(UISTATE.INGAME);
    }
    #endregion

    #region 게임 중 동작하는 타이머
    public IEnumerator InGameTimer()
    {
        if (m_iGameTimer > 1) m_iGameTimer -= 1;
        else
        {
            PV.RPC("GameTimeSetRPC", RpcTarget.All);

            yield break;
        }

        PV.RPC("UpdateGameTimerCountRPC", RpcTarget.All, m_iGameTimer);

        yield return new WaitForSeconds(1);
        StartCoroutine(InGameTimer());
    }

    [PunRPC]
    void UpdateGameTimerCountRPC(int time)
    {
        m_iGameTimer = time;

        if (GameManager.gm.m_curState != GMSTATE.RESULT)
        {
            GameManager.gm.m_pcLocal.m_iMyGold += 2;
            UIManager.um.RefreshMyGoldText();
        }

        int min = m_iGameTimer / 60;
        int sec = m_iGameTimer % 60;

        UIManager.um.m_txtGameTimer.text = string.Format("{0:D2}:{1:D2}", min, sec);
    }

    [PunRPC]
    void GameTimeSetRPC()
    {
        if(GameManager.gm.m_curState == GMSTATE.PHASE_READY)
            GameManager.gm.m_stateMachine.ChangeState(GMSTATE.PHASE_START);
        else if (GameManager.gm.m_curState == GMSTATE.PHASE_START)
        {
            if(GameManager.gm.m_iCurRound == GameManager.gm.m_iMaxRound)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PV.RPC("GameSetRPC", RpcTarget.All, (int)GAMERESULT.GAMEOVER);
                }

                GameManager.gm.m_stateMachine.ChangeState(GMSTATE.RESULT);
                return;
            }    

            GameManager.gm.m_stateMachine.ChangeState(GMSTATE.PHASE_READY);
        }
        else if(GameManager.gm.m_curState == GMSTATE.RESULT)
        {
            //게임을 끝내고 방을 폭파 시킨다.

            Debug.Log("방 폭파 됨요");
            m_dicPlayFabCostume.Clear();
            GameManager.gm.m_dicUserKillCount.Clear();
            GameManager.gm.m_dicUserRoomState.Clear();
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        GameManager.gm.ChangeScene("MainScene", GMSTATE.MAIN);
        StartCoroutine(ChangeLobbyUI());
    }

    IEnumerator ChangeLobbyUI()
    {
        GameObject canvas = UIManager.um.m_LoadCanvas;
        canvas.gameObject.SetActive(true);
        UIManager.um.m_LoadingPanel.gameObject.SetActive(true);
        UIManager.um.m_txtLoading.text = "로비화면으로 이동중입니다....";

        yield return new WaitForSeconds(0.5f);

        UIManager.um.m_stateMachine.ChangeState(UISTATE.ENTERING_READY);
    }

    #endregion

    #region 게임 중 킬 리더보드 업데이트
    public void RefreshKillLeaderboard(Dictionary<string, int> dic)
    {
        PV.RPC("RefreshKillLeaderboardRPC", RpcTarget.All, dic);
    }

    [PunRPC]
    void RefreshKillLeaderboardRPC(Dictionary<string, int> dic)
    {
        StartCoroutine(UIManager.um.RefreshKillLeaderboard(dic));
    }
    #endregion

    #region PlayFab 실행 관련 함수
    [ContextMenu("클라우드 스크립트 실행")]
    void SetCharacterDefaultStats() //캐릭터 디폴트 정보를 클라우드 스크립트를 통해서 입력하도록 한다
    {
        Dictionary<string, string> dicData = new Dictionary<string, string>();

        for(; costumeIndex < (int)COSTUMETYPE._MAX_; costumeIndex++)
        {
            if(costumeIndex == (int)COSTUMETYPE.Glove)
            {
                dicData.Add((COSTUMETYPE.Hat + costumeIndex).ToString(), "0");
                costumeIndex++;
                Invoke("SetCharacterDefaultStats", 1f);
                break;
            }

            if(costumeIndex == (int)COSTUMETYPE.Body)
            {
                dicData.Add((COSTUMETYPE.Hat + costumeIndex).ToString(), "1");
                //이떄 아이템 지급하자 1번 Body 줘야됨

                SetInitCostume();
            }

            else
                dicData.Add((COSTUMETYPE.Hat + costumeIndex).ToString(), "0");
        }

        if (costumeIndex == (int)COSTUMETYPE._MAX_) costumeIndex = 0;

        //타이틀데이터는 한번에 10개까지만 저장됨.. 그럼 그 이상은 어떻게 해야되나요?..
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = dicData
        },
        (result) => print("저장 성공!!"), (error) => print("저장실패!!"));
    }

    public void SaveCostumeTitleTable()
    {
        Dictionary<string, string> dicData = new Dictionary<string, string>();

        for (; costumeIndex < (int)COSTUMETYPE._MAX_; costumeIndex++)
        {
            if (costumeIndex == (int)COSTUMETYPE.Glove)
            {
                dicData.Add((COSTUMETYPE.Hat + costumeIndex).ToString(), CostumeManager.cm.m_dicChangeCostume[(COSTUMETYPE.Hat + costumeIndex)].ToString());
                costumeIndex++;
                Invoke("SaveCostumeTitleTable", 1f);
                break;
            }

            else
                dicData.Add((COSTUMETYPE.Hat + costumeIndex).ToString(), CostumeManager.cm.m_dicChangeCostume[(COSTUMETYPE.Hat + costumeIndex)].ToString());
        }

        if (costumeIndex == (int)COSTUMETYPE._MAX_) costumeIndex = 0;

        //타이틀데이터는 한번에 10개까지만 저장됨.. 그럼 그 이상은 어떻게 해야되나요?..
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = dicData
        },
        (result) => 
        { 
            if(costumeIndex == 0)
            {
                if (CostumeManager.cm.m_dicCurCostume.Count > 0) CostumeManager.cm.m_dicCurCostume.Clear();

                for (int i = 0; i < CostumeManager.cm.m_dicChangeCostume.Count; i++)
                {
                    CostumeManager.cm.m_dicCurCostume.Add(COSTUMETYPE.Hat + i, CostumeManager.cm.m_dicChangeCostume[COSTUMETYPE.Hat + i]);
                }

                CostumeManager.cm.ChangeMyCostume();
            }

            print("데이터 저장 성공");

        }, (error) => print("저장실패!!"));
    }

    void SetInitCostume()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "hello"
        };
        PlayFabClientAPI.ExecuteCloudScript(request, (result) => print("아이템 지급 성공"), (error) => print("아이템 지급 안됨"));
    }


    //플레이어 데이터테이블에 있는 커스텀 데이터 불러오기
    public void SetCostumeData()
    {       
        Dictionary<string, string> dicCostume = new Dictionary<string, string>();
        //초기는 무조건 현재 데이터테이블에 저장된 코스튬 정보를 가져온다.
        var request = new GetUserDataRequest() { PlayFabId = m_myPlayFabInfo.PlayFabId };
        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            for (int i = 0; i < result.Data.Count; i++)
            {
                if (result.Data[(COSTUMETYPE.Hat + i).ToString()] != null)
                {
                    var value = result.Data[(COSTUMETYPE.Hat + i).ToString()].Value;
                    dicCostume.Add((COSTUMETYPE.Hat + i).ToString(), value);
                }
            }

            AddCosutmeData(dicCostume);
        },
        (error) => print("데이터 불러오기 실패욤"));
    }

    public void GetCostumeData(string i_strPlayFabId, GameObject i_goPC)
    {
        var request = new GetUserDataRequest() { PlayFabId = i_strPlayFabId };
        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            for (int i = 0; i < result.Data.Count; i++)
            {
                if (result.Data[(COSTUMETYPE.Hat + i).ToString()] != null)
                {
                    var value = result.Data[(COSTUMETYPE.Hat + i).ToString()].Value;

                    GameObject charItem = i_goPC.transform.Find((COSTUMETYPE.Hat + i).ToString()).gameObject;

                    if (int.Parse(value) == 0)
                    {
                        charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
                        continue;
                    }

                    string strPath = "Costume/" + (COSTUMETYPE.Hat + i).ToString() + "/" + value;

                    GameObject item = Resources.Load<GameObject>(strPath);

                    GameObject changeItem = item.transform.Find((COSTUMETYPE.Hat + i).ToString()).gameObject;

                    if (changeItem != null)
                        charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                            changeItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                }
            }
        },
        (error) => Debug.Log("데이터 불러오기 실패욤"));
    }

    void AddCosutmeData(Dictionary<string, string> i_dicCostume)
    {
        //인벤토리에 해당 코스튬이 있는지 확인한다. (말그대로 확인검증)
        bool isHave = true;
        string strCostume = "Costume";
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), (result) =>
        {
            foreach (var invenItem in result.Inventory)
            {
                if (i_dicCostume[(COSTUMETYPE.Hat).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Hat).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Hat).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Hair).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Hair).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Hair).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Eyebrow).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Eyebrow).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Eyebrow).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Glasses).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Glasses).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Glasses).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Mustache).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Mustache).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Mustache).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Backpack).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Backpack).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Backpack).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Outerwear).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Outerwear).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Outerwear).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Glove).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Glove).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Glove).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Pants).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Pants).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Pants).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Shoe).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Shoe).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Shoe).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.FullBody).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.FullBody).ToString() + "_" + i_dicCostume[(COSTUMETYPE.FullBody).ToString()]) isHave = false;

                if (i_dicCostume[(COSTUMETYPE.Body).ToString()] != "0" &&
                    invenItem.ItemId != strCostume + "_" + (COSTUMETYPE.Body).ToString() + "_" + i_dicCostume[(COSTUMETYPE.Body).ToString()]) isHave = false;

                if (!isHave) break;
            }
        }, (error) => print("인벤 데이터 불러오기 실패"));

        if (CostumeManager.cm.m_dicCurCostume.Count != 0) CostumeManager.cm.m_dicCurCostume.Clear();

        for (int i = 0; i < i_dicCostume.Count; i++)
        {
            CostumeManager.cm.m_dicCurCostume.Add(COSTUMETYPE.Hat + i, int.Parse(i_dicCostume[(COSTUMETYPE.Hat + i).ToString()]));
        }

        if (CostumeManager.cm.m_dicChangeCostume.Count > 0) CostumeManager.cm.m_dicChangeCostume.Clear();

        for (int i = 0; i < CostumeManager.cm.m_dicCurCostume.Count; i++)
        {
            CostumeManager.cm.m_dicChangeCostume.Add(COSTUMETYPE.Hat + i, CostumeManager.cm.m_dicCurCostume[COSTUMETYPE.Hat + i]);
        }

        CostumeManager.cm.InitLoadMyCostume();
    }

    //인벤 정보 가져오기
    public void GetInventory()
    {
        if (CostumeManager.cm.m_dicInven.Count != 0) CostumeManager.cm.m_dicInven.Clear();

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), (result) =>
        {
            foreach(var item in result.Inventory)
            {
                CostumeManager.cm.m_dicInven.Add(item.ItemId, item);
            }

            CostumeManager.cm.m_strMyMoney = result.VirtualCurrency["PG"].ToString();
            CostumeManager.cm.RefreshMyCostumeInfo();
        },
        (error) => print("인벤토리 정보 가져오기 실패"));
    }

    public void GetCatalogItem()
    {
        if (CostumeManager.cm.m_dicCatalogItem.Count != 0) CostumeManager.cm.m_dicCatalogItem.Clear();

        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest() { CatalogVersion = "Main" }, (result) =>
        {
            foreach (var catalogItem in result.Catalog)
            {
                CostumeManager.cm.m_dicCatalogItem.Add(catalogItem.ItemId, catalogItem);
            }
        },
        (error) => print("카탈로그 불러오기 실패"));
    }

    public void PurchaseItem(string itemId, int price)
    {
        var request = new PurchaseItemRequest { CatalogVersion = "Main", ItemId = itemId, VirtualCurrency = "PG", Price = price };
        PlayFabClientAPI.PurchaseItem(request, (result) => { GetInventory(); }, (error) => print("아이템 구입 실패"));
    }

    #endregion
}
