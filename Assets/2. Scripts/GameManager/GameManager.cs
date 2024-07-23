using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon;
using PlayFab.ClientModels;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Tooltip("게임 매니저 싱글톤화")]
    public static GameManager gm;

    [Tooltip("로컬 플레이어가 누군지 확인용"), HideInInspector]
    public PlayerController m_pcLocal;

    [HideInInspector]
    public GMStateMachine m_stateMachine { get; private set; }

    public bool m_bCraftMode;
    public bool m_bUpUnitMode;
    public bool m_bEvolUnitMode;
    public bool m_bUpUserMode;
    public bool m_bGambleMode;

    public List<Dictionary<string, object>> m_unitInfo;

    public List<UnitFSM> m_listMyUnits = new List<UnitFSM>();

    public USERROOMSTATE m_curRoomState;

    public Dictionary<string, int> m_dicUserRoomState = new Dictionary<string, int>();

    public Dictionary<string, int> m_dicUserKillCount = new Dictionary<string, int>();

    public GMSTATE m_curState;

    public int m_iLimitUnit;

    [HideInInspector]public int m_iUnitCount;

    [SerializeField] LayerMask m_UnitLayer;

    public List<Dictionary<string, object>> m_costInfo;

    public int m_iCurRound;
    public int m_iMaxRound;

    public GAMEROUNDINFO m_sCurRoundInfo;

    public List<Dictionary<string, object>> m_roundInfo;

    public GameObject scriptMachine;
    
    [HideInInspector]
    public Dictionary<string, CatalogItem> m_dicCatalogItem = new Dictionary<string, CatalogItem>();

    private void Awake()
    {
        gm = this;
        DontDestroyOnLoad(gm);

        //debug용
        Screen.SetResolution(960, 540, false);
        InitStateMachine();
    }

    private void Start()
    {
        m_unitInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.UNITINFO].recordDataList;
        m_costInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.COSTINFO].recordDataList;
        m_roundInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.GAMEROUNDINFO].recordDataList;
        m_curRoomState = USERROOMSTATE.WAIT;
        m_iMaxRound = m_roundInfo.Count;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "InGameScene" && m_curState != GMSTATE.RESULT)
        {
            GameObject go = GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject;

            if ((m_bCraftMode || m_bGambleMode) && go.activeSelf) go.SetActive(false);

            else if ((!m_bCraftMode && !m_bUpUserMode && !m_bGambleMode) && !go.activeSelf) go.SetActive(true);

            if ((m_bUpUnitMode || m_bEvolUnitMode) && go.activeSelf)
            {
                //유닛 이펙트 활성화
                foreach(var unit in m_listMyUnits)
                {
                    unit.m_goSelectEffect.SetActive(true);
                }

                go.SetActive(false);
            }

            else if ((!m_bUpUnitMode && !m_bEvolUnitMode && !m_bUpUserMode && !m_bGambleMode) && !go.activeSelf)
            {
                //유닛 이펙트 비활성화
                foreach (var unit in m_listMyUnits)
                {
                    unit.m_goSelectEffect.SetActive(false);
                }
                go.SetActive(true);
            }

            if (m_bEvolUnitMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    EvolutionUnitCheck();
                    UnitPanelManager.upm.OnClickReturnButton();
                }
            }

            if (m_bUpUnitMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    UpgradeUnitCheck();
                    UnitPanelManager.upm.OnClickReturnButton();
                }
            }
        }

            m_stateMachine?.UpdateState();
    }

    private void FixedUpdate()
    {
        m_stateMachine.FixedUpdateState();
    }

    void InitStateMachine()
    {
        m_stateMachine = new GMStateMachine(GMSTATE.MAIN, gameObject.AddComponent<GMInMain>());
        m_stateMachine.AddState(GMSTATE.FIELD, gameObject.AddComponent<GMInField>());
        m_stateMachine.AddState(GMSTATE.PHASE_READY, gameObject.AddComponent<GMInPhaseReady>());
        m_stateMachine.AddState(GMSTATE.PHASE_START, gameObject.AddComponent<GMInPhaseStart>());
        m_stateMachine.AddState(GMSTATE.RESULT, gameObject.AddComponent<GMInResult>());
        m_stateMachine.AddState(GMSTATE.LOBBY, gameObject.AddComponent<GMInLobby>());

        m_curState = GMSTATE.MAIN;
        m_stateMachine.currentState?.OnEnterState();
    }

    public void ChangeScene(string i_strScene, GMSTATE gState)
    {
        SceneManager.LoadScene(i_strScene);
        m_stateMachine.ChangeState(gState);
    }

    public void GenerateUnits(Transform i_trPos)
    {
        if (!m_pcLocal.m_pv.IsMine) return;

        Dictionary<string, object> unitInfo = RandomWeight.RandomItem(m_unitInfo);

        //test
        //Dictionary<string, object> unitInfo = m_unitInfo[6];

        string strFolderName = "UNIT_";
        int iRandType = int.Parse(unitInfo["TYPE"].ToString());

        string strUnits = strFolderName + iRandType.ToString();

        int iStartRank = int.Parse(unitInfo["RANK"].ToString());

        GameObject unit = PhotonNetwork.Instantiate(strUnits, i_trPos.position, Quaternion.Euler(0, 180, 0));

        unit.GetComponent<UnitFSM>().InitParam(unitInfo);

        m_listMyUnits.Add(unit.GetComponent<UnitFSM>());
        m_iUnitCount++;
    }

    public void GenerateGambleBox(Transform i_trPos)
    {
        if (!m_pcLocal.m_pv.IsMine) return;

        string boxName = "GAMBLEBOX_GambleBox";

        GameObject box = PhotonNetwork.Instantiate(boxName, i_trPos.position, Quaternion.Euler(0, 180, 0));
        box.transform.SetParent(GameObject.Find("GambleBoxes").transform);
    }

    public void ChangeReadyState(USERROOMSTATE eState)
    {
        m_dicUserRoomState[m_pcLocal.m_pv.Owner.NickName] = (int)eState;
        NetworkManager.nm.RefreshRoomPlayerState(m_dicUserRoomState);
    }

    void EvolutionUnitCheck()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!CostCheckToCal("UNITEVOLUTION"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, m_UnitLayer))
        {
            if (!m_listMyUnits.Contains(hit.transform.GetComponent<UnitFSM>()))
            {
                UIManager.um.SystemMessage("내 유닛이 아닙니다.");
                return;
            }

            else if(m_listMyUnits.Contains(hit.transform.GetComponent<UnitFSM>()))
            {
                var unit = hit.transform.GetComponent<UnitFSM>();

                UNITRANK eRank = unit.m_sInfo.eRank;
                UNITTYPE eType = unit.m_sInfo.eType;

                //확률 적용해야됨
                eRank += 1;

                if (eRank >= UNITRANK.LEGEND)
                {
                    UIManager.um.SystemMessage("더 이상 진화 할 수 없는 유닛입니다.");
                    return;
                }

                Dictionary<string, object> unitInfo = new Dictionary<string, object>();

                float p = Random.value * 100;
                float prob = 0;

                foreach (var info in m_unitInfo)
                {
                    if (int.Parse(info["TYPE"].ToString()) == (int)eType &&
                        int.Parse(info["RANK"].ToString()) == (int)eRank)
                    {
                        prob = float.Parse(info["EVOPROB"].ToString());
                    }
                }


                if (p > prob) //당첨 안된거임 ㅎㅎ
                {
                    UIManager.um.SystemMessage("응 진화 실패요~");
                    return;
                }

                foreach (var info in m_unitInfo)
                {
                    if (int.Parse(info["TYPE"].ToString()) == (int)eType &&
                        int.Parse(info["RANK"].ToString()) == (int)eRank)
                    {
                        unitInfo = info;
                        break;
                    }
                }

                unit.InitParam(unitInfo, 0.5f, unit.m_sInfo.curLV);
                AudioManager.PlaySfx(SFX.EVOLUTION);
                UIManager.um.SystemMessage("진화 성공!");
            }
        }
    }

    void UpgradeUnitCheck()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!CostCheckToCal("UNITUPGRADE"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, m_UnitLayer))
        {
            var target = hit.transform.GetComponent<UnitFSM>();

            if (!m_listMyUnits.Contains(target))
            {
                UIManager.um.SystemMessage("내 유닛이 아닙니다.");
                return;
            }

            else if (m_listMyUnits.Contains(target))
            {
                foreach (var unit in m_listMyUnits)
                {
                    if (unit.m_sInfo.eType == target.m_sInfo.eType)
                    {
                        unit.m_pv.RPC("LevelUpRPC", RpcTarget.All);
                    }
                }

                UIManager.um.SystemMessage("레벨 업 !");
            }
        }
    }

    public bool CostCheckToCal(string costCate)
    {
        bool isCheck = false;
        int needCost = int.Parse(m_costInfo[0][costCate].ToString());

        if (m_pcLocal.m_iMyGold >= needCost)
        {
            m_pcLocal.m_iMyGold -= needCost;
            UIManager.um.RefreshMyGoldText();
            isCheck = true;
        }

        return isCheck;
    }

    public void SetRoundInfo(int idx)
    {
        m_sCurRoundInfo.enemyIndex = int.Parse(m_roundInfo[idx - 1]["ENEMYINDEX"].ToString());
        m_sCurRoundInfo.amount = int.Parse(m_roundInfo[idx - 1]["AMOUNT"].ToString());
        m_sCurRoundInfo.interval = float.Parse(m_roundInfo[idx - 1]["INTERVAL"].ToString());
        m_sCurRoundInfo.isRandSpawn = System.Convert.ToBoolean(int.Parse(m_roundInfo[idx - 1]["RANDSPAWN"].ToString()));
        m_sCurRoundInfo.eSpot = (ENEMYSPAWNSPOT)int.Parse(m_roundInfo[idx - 1]["SPAWNPOS"].ToString());
    }

    public void StartEnemyWave()
    {
        EnemyFactory.ef.SpawnEnemies(m_sCurRoundInfo.enemyIndex, m_sCurRoundInfo.amount, m_sCurRoundInfo.interval, m_sCurRoundInfo.eSpot, m_sCurRoundInfo.isRandSpawn);
    }
}
