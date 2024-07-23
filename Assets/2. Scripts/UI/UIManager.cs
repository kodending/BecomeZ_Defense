using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Net.NetworkInformation;
using UnityEngine.SearchService;
using System;
using DG.Tweening;
using UnityEngine.AI;
using Photon.Pun.UtilityScripts;
using PlayFab.EconomyModels;

public class UIManager : MonoBehaviourPunCallbacks
{
    static GameObject go;
    public static UIManager um;

    //UI 상태머신 등록
    [HideInInspector]
    public UIStateMachine m_stateMachine { get; private set; }

    public List<Sprite> m_listCheckSprite;

    [HideInInspector]
    public Text[] m_listChatTexts;

    public GameObject m_goMainButtonPanel, m_goLoginButtonPanel, m_goRegistButtonPanel;

    [HideInInspector]
    public UISTATE m_eCurState;

    [Header("시스템 메세지 관련 함수")]
    [SerializeField] private GameObject SystemMsgPrefab;
    private GameObject m_goSystemMsg;
    static Sequence sequenceScale;

    [Tooltip("로그인 아이디 관련 변수")]
    public string m_strSavedMail;

    [HideInInspector]
    public bool m_isSavedMail;

    [HideInInspector]
    public Text m_txtCount;

    [HideInInspector]
    public Button m_btnReady;

    //현재 골드 체크용
    int m_curGold;

    //현재 생명력 체크용
    public int m_curLife, m_oldLife;

    #region 인게임 체력 관련 변수
    [HideInInspector] public GameObject m_goLifePanel;
    [HideInInspector] public Text m_txtLife;
    [HideInInspector] public Slider m_sdLife;
    #endregion

    #region 인게임 타이머 관련 변수
    [HideInInspector] public GameObject m_goTimerPanel;
    [HideInInspector] public Text m_txtGameTimer;
    #endregion

    #region 인게임 리더보드
    [HideInInspector] public GameObject m_goLeaderboardPanel;
    [HideInInspector] public Text[] m_arrTxtBoard;
    #endregion

    #region 인게임 돈 확인
    [HideInInspector] public GameObject m_goGoldPanel;
    [HideInInspector] public Text m_txtGold;
    #endregion

    public GameObject m_LoadCanvas;
    public GameObject m_LoadingPanel;
    public Text m_txtLoading;

    public GameObject m_OptionPanel;
    public GameObject m_goGameExit;

    private void Awake()
    {
        um = this;
        DontDestroyOnLoad(um);

        InitStateMachine();
    }

    private void Start()
    {
        m_curLife = m_oldLife = 100;
    }

    private void Update()
    {
        m_stateMachine?.UpdateState();
    }

    private void FixedUpdate()
    {
        m_stateMachine?.FixedUpdateState();
    }

    void InitStateMachine()
    {
        m_stateMachine = new UIStateMachine(UISTATE.LOAD, gameObject.AddComponent<UIInLoad>());
        m_stateMachine.AddState(UISTATE.MAIN, gameObject.AddComponent<UIInMain>());
        m_stateMachine.AddState(UISTATE.FIELD, gameObject.AddComponent<UIInField>());
        m_stateMachine.AddState(UISTATE.COSTUME, gameObject.AddComponent<UIInCostume>());
        m_stateMachine.AddState(UISTATE.READY, gameObject.AddComponent<UIInReady>());
        m_stateMachine.AddState(UISTATE.ENTERING_FIELD, gameObject.AddComponent<UIInEnteringField>());
        m_stateMachine.AddState(UISTATE.ENTERING_LOGIN, gameObject.AddComponent<UIInEnteringLogin>());
        m_stateMachine.AddState(UISTATE.INGAME, gameObject.AddComponent<UIInGame>());
        m_stateMachine.AddState(UISTATE.ENTERING_READY, gameObject.AddComponent<UIInEnteringReady>());
        m_stateMachine.AddState(UISTATE.LOBBY, gameObject.AddComponent<UIInLobby>());

        m_eCurState = UISTATE.LOAD;
        m_stateMachine.currentState?.OnEnterState();
    }

    public void SaveLoginInfo(string i_strMail)
    {
        List<Dictionary<string, object>> optionInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.OPTIONINFO].recordDataList;

        if (m_isSavedMail)
        {
            optionInfo[0]["SAVED"] = 1;
            optionInfo[0]["MAIL"] = (string)i_strMail;
        }

        else
        {
            optionInfo[0]["SAVED"] = 0;
            optionInfo[0]["MAIL"] = 0;
        }

        CSVManager.instance.SaveFile(LOCALDATALOADTYPE.OPTIONINFO, optionInfo);
    }

    #region DOTWEEN 관련 함수
    public void SystemMessage(string msg)
    {
        if(GameObject.Find("Canvas").transform.Find("SystemMessage(Clone)") == null)
        {
            m_goSystemMsg = Instantiate(SystemMsgPrefab, GameObject.Find("Canvas").transform);
        }

        Text txt = m_goSystemMsg.transform.Find("SystemText").GetComponent<Text>();
        txt.text = msg;

        StartCoroutine(MessageShowHide());
    }

    IEnumerator MessageShowHide()
    {
        m_goSystemMsg.transform.localScale = Vector3.one * 0.1f;
        m_goSystemMsg.SetActive(true);

        sequenceScale = DOTween.Sequence()
        .SetAutoKill(true)
        .Append(m_goSystemMsg.transform.DOScale(1.1f, 0.2f))
        .Append(m_goSystemMsg.transform.DOScale(1.0f, 0.1f))
        .Play();

        yield return new WaitForSeconds(1.5f);

        m_goSystemMsg.transform.localScale = Vector3.one * 0.2f;

        sequenceScale = DOTween.Sequence()
        .SetAutoKill(true)
        .Append(m_goSystemMsg.transform.DOScale(1.1f, 0.1f))
        .Append(m_goSystemMsg.transform.DOScale(0.2f, 0.2f))
        .OnComplete(() =>
        {
            m_goSystemMsg.SetActive(false);
        });
    }

    public void ShowSlideUI(GameObject panel, UIDIRECTION eDir)
    {
        switch (eDir)
        {
            case UIDIRECTION.UP: //아래에서 위로 슬라이드하는 UI
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -500);

                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(20f, 0.3f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.1f))
                .Play();

                break;

            case UIDIRECTION.DOWN: //위에서 아래로 슬라이드하는 UI
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 500);

                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(-20f, 0.3f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.1f))
                .Play();

                break;

            case UIDIRECTION.LEFT: //왼쪽으로 슬라이드하는 UI
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(500, 0);

                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(-20f, 0.3f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.1f))
                .Play();

                break;

            case UIDIRECTION.RIGHT: //오른쪽으로 슬라이드하는 UI
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, 0);

                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(20f, 0.3f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.1f))
                .Play();


                break;
        }
    }

    public void HideSlideUI(GameObject panel, UIDIRECTION eDir)
    {
        switch (eDir)
        {
            case UIDIRECTION.UP: //아래에서 위로 슬라이드하는 UI
                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(-20f, 0.1f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(500f, 0.3f))
                .Play()
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                });

                break;

            case UIDIRECTION.DOWN: //위에서 아래로 슬라이드하는 UI
                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(20f, 0.1f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosY(-500f, 0.3f))
                .Play()
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                });

                break;

            case UIDIRECTION.LEFT: //왼쪽으로 슬라이드하는 UI
                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(20f, 0.1f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(-500f, 0.3f))
                .Play()
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                });

                break;

            case UIDIRECTION.RIGHT: //오른쪽으로 슬라이드하는 UI
                sequenceScale = DOTween.Sequence()
                .SetAutoKill(true)
                .OnRewind(() =>
                {
                    panel.SetActive(true);
                })
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(-20f, 0.1f))
                .Append(panel.GetComponent<RectTransform>().DOAnchorPosX(500f, 0.3f))
                .Play()
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                });


                break;
        }
    }

    public void ShowScaleUI(GameObject panel, float fScaleTime = 0.2f)
    {
        panel.transform.localScale = Vector3.one * 0.1f;

        sequenceScale = DOTween.Sequence()
        .SetAutoKill(true)
        .OnRewind(() =>
        {
            panel.SetActive(true);
        })
        .Append(panel.transform.DOScale(1.1f, fScaleTime))
        .Append(panel.transform.DOScale(1.0f, 0.1f))
        .Play();
    }

    public void HideScaleUI(GameObject panel)
    {
        panel.transform.localScale = Vector3.one * 0.2f;

        sequenceScale = DOTween.Sequence()
        .SetAutoKill(true)
        .OnRewind(() =>
        {
            panel.SetActive(true);
        })
        .Append(panel.transform.DOScale(1.1f, 0.1f))
        .Append(panel.transform.DOScale(0.2f, 0.2f))
        .OnComplete(() =>
        {
            panel.SetActive(false);
        });
    }

    public void ShowHidleScaleUI(GameObject panel, float fScaleTime = 0.4f)
    {
        StartCoroutine(ShowHideScale(panel, fScaleTime));
    }
    IEnumerator ShowHideScale(GameObject panel, float fScaleTime = 0.4f)
    {
        panel.transform.localScale = Vector3.one * 0.1f;
        panel.SetActive(true);

        sequenceScale = DOTween.Sequence()
        .SetAutoKill(true)
        .Append(panel.transform.DOScale(1.1f, 0.2f))
        .Append(panel.transform.DOScale(1.0f, 0.05f))
        .Play();

        yield return new WaitForSeconds(fScaleTime);

        panel.SetActive(false);
    }
    #endregion

    public void RefreshReadyLeaderboard()
    {
        if (m_arrTxtBoard == null) return;
        if (m_arrTxtBoard.Length == 0) return;

        //리더보드 초기화
        foreach (var txt in m_arrTxtBoard)
        {
            txt.text = "";
        }

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var player = PhotonNetwork.PlayerList[i];
            var txt = m_arrTxtBoard[i];

            if((USERROOMSTATE)GameManager.gm.m_dicUserRoomState[player.NickName] == USERROOMSTATE.WAIT)
                txt.text = player.NickName + "  :  " + ((USERROOMSTATE)GameManager.gm.m_dicUserRoomState[player.NickName]).ToString();
            else if((USERROOMSTATE)GameManager.gm.m_dicUserRoomState[player.NickName] == USERROOMSTATE.READY)
                txt.text = player.NickName + "  :  " + "<color=yellow>" + ((USERROOMSTATE)GameManager.gm.m_dicUserRoomState[player.NickName]).ToString() + "</color>";
        }
    }

    public IEnumerator RefreshKillLeaderboard(Dictionary<string, int> dicKill)
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var txt in m_arrTxtBoard)
        {
            txt.text = "";
        }

        List<PLAYERKILLINFO> listKillInfo = new List<PLAYERKILLINFO>();

        foreach(var player in  PhotonNetwork.PlayerList)
        {
            PLAYERKILLINFO info = new PLAYERKILLINFO();
            info.strName = player.NickName;
            info.iKillCount = dicKill[player.NickName];
            listKillInfo.Add(info);
        }

        listKillInfo.Sort((PLAYERKILLINFO a, PLAYERKILLINFO b) => { return b.iKillCount.CompareTo(a.iKillCount); });

        for (int i = 0; i < listKillInfo.Count; i++)
        {
            var info = listKillInfo[i];
            
            if(i == 0 && info.iKillCount > 0)
            {
                m_arrTxtBoard[i].text = "<color=red>" + info.strName + "  :  " + info.iKillCount + " KILL" + "</color>";
                continue;
            }

            m_arrTxtBoard[i].text = info.strName + "  :  " + info.iKillCount + " KILL";
        }
    }

    public void RefreshMyGoldText()
    {
        if (m_txtGold == null) return;

        Sequence moneySeq = DOTween.Sequence()
        .SetAutoKill(true)
        .OnStart(()=>
        {
            int.TryParse(m_txtGold.text, out m_curGold);
        })
        .Append(m_txtGold.DOCounter(m_curGold, GameManager.gm.m_pcLocal.m_iMyGold, 0.2f, true))
        .OnComplete(() =>
        {
            m_curGold = GameManager.gm.m_pcLocal.m_iMyGold;
        });
    }

    public void LoseLife()
    {
        //생명력 쉐낏쉐낏해주고
        Sequence shakeSeq = DOTween.Sequence()
        .SetAutoKill(true)
        .Append(m_goLifePanel.transform.DOShakePosition(0.2f, 2, 10, 1, false, true));

        m_oldLife -= 5;

        if(m_oldLife <= 0)
        {
            m_curLife = m_oldLife;
            m_txtCount.text = m_curLife.ToString();
            m_sdLife.value = 0;
            return;
        }

        Sequence countSeq = DOTween.Sequence()
        .SetAutoKill(true)
        .OnStart(() =>
        {
            int.TryParse(m_txtLife.text, out m_curLife);
        })
        .Append(m_txtLife.DOCounter(m_curLife, m_oldLife, 0.2f, true))
        .OnComplete(() =>
        {
            m_curLife = m_oldLife;
        });

        m_sdLife.DOValue(m_curLife, 0.2f, true);
    }

    public void OnResult(int result, Dictionary<string, int> dic)
    {
        GameObject gamePanel = GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject;
        gamePanel.SetActive(false);

        if((GAMERESULT)result == GAMERESULT.GAMEOVER)
        {
            AudioManager.PlaySfx(SFX.GAMEOVER);
            GameResultPanel.RefreshInfo(GAMERESULT.GAMEOVER, dic);
        }

        else
        {
            AudioManager.PlaySfx(SFX.GAMEVICTORY);
            GameResultPanel.RefreshInfo(GAMERESULT.VICTORY, dic);
        }
    }

    public void OnClickOption()
    {
        if (m_eCurState == UISTATE.INGAME || m_eCurState == UISTATE.FIELD)
        {
            m_goGameExit.SetActive(true);
        }

        else
        {
            m_goGameExit.SetActive(false);
        }

        if (!m_OptionPanel.gameObject.activeSelf)
        {
            List<Dictionary<string, object>> optionInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.OPTIONINFO].recordDataList;

            AudioManager.am.sdVolume.value = int.Parse(optionInfo[0]["SOUND"].ToString());

            m_LoadCanvas.SetActive(true);
            m_OptionPanel.gameObject.SetActive(true);
        }

        else
        {
            m_LoadCanvas.SetActive(false);
            m_OptionPanel.gameObject.SetActive(false);
        }
    }

    public void OnClickOptionExit()
    {
        m_OptionPanel.gameObject.SetActive(false);
        m_LoadCanvas.SetActive(false);
    }

    public void OnClickGameExit()
    {
        NetworkManager.nm.m_dicPlayFabCostume.Clear();
        GameManager.gm.m_dicUserKillCount.Clear();
        GameManager.gm.m_dicUserRoomState.Clear();
        m_arrTxtBoard = null;
        PhotonNetwork.LeaveRoom();
        NetworkManager.nm.m_listEnemyInfo.Clear();

        m_OptionPanel.gameObject.SetActive(false);
        m_LoadCanvas.SetActive(false);
    }
}
