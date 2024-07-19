using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Cinemachine;
using Photon.Pun.Demo.Cockpit;

public enum PLAYERCONTROL
{
    JOYSTCK,
    KEYBOARD
}

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public PlayerStateMachine m_stateMachine { get; private set; }

    public PhotonView m_pv;

    public FixedJoystick m_fixedJoy;

    public Animator m_curAnim;

    public Transform m_trCam;

    public Rigidbody m_curRigid;

    public float m_fMoveSpeed;

    public PLAYERCONTROL m_eCurControl;

    public PLAYERSTATE m_curState;

    public PLAYERTYPE m_curType;

    public bool m_bJDown;

    public bool m_isRun;

    public bool m_bRespawnDown;

    public bool m_isJumping;

    bool m_isCheckCostume;

    [SerializeField]
    private GameObject m_goCircle;

    [HideInInspector]
    public float m_fAtkComboTimer = 0;
    public int m_iAtkCombo = 0;

    public Projector m_pjAttackRange;

    public EnemyScanner m_enemyScan;

    [SerializeField]
    Transform m_trNavObstacle;

    public BUFFINFO m_sUnitBuff;

    public BUFFINFO m_sPlayerBuff;

    public ParticleSystem[] m_arrEffects;

    public PLAYERINFO m_sInfo = new PLAYERINFO();

    [SerializeField]
    public RuntimeAnimatorController[] m_arrAnimCtrlType;

    public LayerMask m_targetAllyLayers;
    public RaycastHit[] m_targetAlly;
    public List<RaycastHit> m_listCheckAlly = new List<RaycastHit>();

    public int m_iMyGold;

    private void Start()
    {
        InitStateMachine();

        if (m_pv.IsMine)
        {
            CameraMoving.cm.SetTarget(this.gameObject);
            m_fixedJoy = GameObject.Find("Canvas").transform.Find("GameUIPanel").
                         transform.Find("Fixed Joystick").GetComponent<FixedJoystick>();
            m_trCam = CameraMoving.cm.transform;
            m_eCurControl = PLAYERCONTROL.JOYSTCK;
            GameManager.gm.m_pcLocal = this;

            m_curType = PLAYERTYPE.COMMONS;

            m_goCircle.SetActive(true);
            m_pjAttackRange.gameObject.SetActive(true);
            m_enemyScan = GetComponent<EnemyScanner>();

            m_sUnitBuff = new BUFFINFO();
            m_sPlayerBuff = new BUFFINFO();

            NetworkManager.nm.GetCostumeData(NetworkManager.nm.m_myPlayFabInfo.PlayFabId, gameObject);

            if (PhotonNetwork.IsMasterClient)
                GameManager.gm.m_dicUserKillCount.Add(m_pv.Owner.NickName, 0);

            m_sInfo.atk = 0;
            m_sInfo.atkRange = 0f;
            m_sInfo.atkSpeed = 2f;
            m_sInfo.runSpeed = 2f;

            m_iMyGold = 0;

            m_pjAttackRange.orthographicSize = m_sInfo.atkRange;
            m_fMoveSpeed = m_sInfo.runSpeed;
        }
    }

    private void Update()
    {
        if (!m_pv.IsMine)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                var player = PhotonNetwork.PlayerList[i];

                if (m_pv.Owner.NickName == player.NickName && !m_isCheckCostume &&
                   NetworkManager.nm.m_dicPlayFabCostume.ContainsKey(m_pv.Owner.NickName))
                {
                    m_isCheckCostume = true;
                    OtherChangeCostume();
                    break;
                }
            }

            return;
        }

        if (m_iAtkCombo != 0 && m_curState != PLAYERSTATE.ATTACK)
        {
            m_fAtkComboTimer += Time.deltaTime;

            if (m_fAtkComboTimer >= 0.5f)
            {
                m_iAtkCombo = 0;
                m_fAtkComboTimer = 0;
            }
        }

        m_goCircle.transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        GetInput();
        CheckFalling();
        Respawn();

        switch (m_curType)
        {
            case PLAYERTYPE.BOXER:
                if (m_enemyScan.checkedTargetEnemies.Count > 0)
                    if (m_enemyScan.nearestTargetEnemy != null) ScanAttackEnemy();
                break;

            case PLAYERTYPE.CHEERLEADER:
                if (m_curState == PLAYERSTATE.IDLE)
                    ScanAttackEnemy();
                break;
        }



        m_stateMachine?.UpdateState();
    }

    private void FixedUpdate()
    {
        m_trNavObstacle.eulerAngles = Vector3.zero;

        if (!m_pv.IsMine)
        {
            return;
        }

        m_stateMachine.FixedUpdateState();
    }

    void GetInput()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

        if (GameManager.gm.m_bCraftMode) return;

        //m_bJDown = Input.GetButtonDown("Jump");
        m_bRespawnDown = Input.GetKeyDown(KeyCode.R);
    }

    void ScanAttackEnemy()
    {
        //평민은 공격할 수 없음
        if (m_curType == PLAYERTYPE.COMMONS) return;

        if (GameManager.gm.m_pcLocal.m_curState != PLAYERSTATE.ATTACK &&
            GameManager.gm.m_pcLocal.m_curState != PLAYERSTATE.MOVE)
            GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.ATTACK);
    }

    void InitStateMachine()
    {
        m_stateMachine = new PlayerStateMachine(PLAYERSTATE.IDLE, new Player_Idle(this));
        m_stateMachine.AddState(PLAYERSTATE.MOVE, new Player_Move(this));
        m_stateMachine.AddState(PLAYERSTATE.ATTACK, new Player_Attack(this));
        m_stateMachine.AddState(PLAYERSTATE.JUMP, new Player_Jump(this));
        m_stateMachine.AddState(PLAYERSTATE.FALLING, new Player_Falling(this));

        m_curState = PLAYERSTATE.IDLE;
        m_stateMachine.currentState?.OnEnterState();
    }

    public void SetUnitBuff(int buffAtk = 0, float buffAtkSpd = 0, int buffIdx = 0)
    {
        m_pv.RPC("SetUnitBuffRPC", RpcTarget.All, buffAtk, buffAtkSpd, buffIdx);
    }

    [PunRPC]
    void SetUnitBuffRPC(int buffAtk = 0, float buffAtkSpd = 0, int buffIdx = 0)
    {
        if (buffAtk > m_sUnitBuff.buffAtk) m_sUnitBuff.buffAtk = buffAtk;
        if (buffAtkSpd > m_sUnitBuff.buffAtkSpd) m_sUnitBuff.buffAtkSpd = buffAtkSpd;

        if (buffAtk == 0 && buffAtkSpd == 0)
        {
            m_sUnitBuff = new BUFFINFO();

            foreach (var eff in m_arrEffects)
            {
                eff.transform.gameObject.SetActive(false);
                eff.Stop();
            }
        }

        else
        {
            foreach (var eff in m_arrEffects)
            {
                eff.transform.gameObject.SetActive(false);
                eff.Stop();
            }

            m_arrEffects[buffIdx].transform.gameObject.SetActive(true);
            m_arrEffects[buffIdx].Play();
            AudioManager.PlaySfx(SFX.UNIT_BUFFER);
        }
    }

    public void SetPlayerBuff(float buffAtkSpd = 0, float buffAtkRng = 0, int buffIdx = 0)
    {
        m_pv.RPC("SetPlayerBuffRPC", RpcTarget.All, buffAtkSpd, buffAtkRng, buffIdx);
    }

    [PunRPC]
    void SetPlayerBuffRPC(float buffAtkSpd = 0, float buffAtkRng = 0, int buffIdx = 0)
    {
        if (buffAtkSpd > m_sPlayerBuff.buffAtkSpd) m_sPlayerBuff.buffAtkSpd = buffAtkSpd;
        if (buffAtkRng > m_sPlayerBuff.buffAtkRange)
        {
            m_sPlayerBuff.buffAtkRange = buffAtkRng;
            if (m_pv.IsMine)
            {
                m_pjAttackRange.orthographicSize = m_sInfo.atkRange + m_sPlayerBuff.buffAtkRange;
                m_enemyScan.OnRefreshScanRange();
            }
        }

        if (buffAtkSpd == 0 && buffAtkRng == 0)
        {
            m_sPlayerBuff = new BUFFINFO();

            if(m_pv.IsMine)
            {
                m_pjAttackRange.orthographicSize = m_sInfo.atkRange + buffAtkRng;
                m_enemyScan.OnRefreshScanRange();
            }

            foreach (var eff in m_arrEffects)
            {
                m_arrEffects[2].transform.gameObject.SetActive(false);
                m_arrEffects[2].Stop();
            }
        }

        else
        {
            m_arrEffects[2].transform.gameObject.SetActive(true);
            m_arrEffects[2].Play();
            AudioManager.PlaySfx(SFX.UNIT_BUFFER);
        }
    }

    [PunRPC]
    void CheerFXRPC(bool isActive, int BgmNum = 0)
    {
        m_arrEffects[3].transform.gameObject.SetActive(isActive);
        AudioManager.PlayCheerSfx((CHEERBGM)BgmNum, this, isActive);
    }

    public void ActiveTriggerAnim(string i_strAnim, PLAYERSTATE ePlayerState, string floatName = "", float animSpeed = 1.0f, string intName = "", int combo = 0)
    {
        m_pv.RPC("ActiveTriggerAnimRPC", RpcTarget.All, i_strAnim, ePlayerState, floatName, animSpeed, intName, combo);
    }

    public void ActiveBoolAnim(string i_strAnim, PLAYERSTATE ePlayerState, bool isTrue, string floatName = "", float animSpeed = 1.0f, string intName = "", int combo = 0)
    {
        m_pv.RPC("ActiveBoolAnimRPC", RpcTarget.All, i_strAnim, ePlayerState, isTrue, floatName, animSpeed, intName, combo);
    }

    [PunRPC]
    void ActiveTriggerAnimRPC(string i_strAnim, PLAYERSTATE ePlayerState, string floatName = "", float animSpeed = 1.0f, string intName = "", int combo = 0)
    {
        if (floatName != "") m_curAnim.SetFloat(floatName, animSpeed);
        if (intName != "") m_curAnim.SetInteger(intName, combo);
        m_curAnim.SetTrigger(i_strAnim);
        m_curState = ePlayerState;
    }

    [PunRPC]
    void ActiveBoolAnimRPC(string i_strAnim, PLAYERSTATE ePlayerState, bool isTrue, string floatName = "", float animSpeed = 1.0f, string intName = "", int combo = 0)
    {
        if (floatName != "") m_curAnim.SetFloat(floatName, animSpeed);
        if (intName != "") m_curAnim.SetInteger(intName, combo);
        m_curAnim.SetBool(i_strAnim, isTrue);
        m_curState = ePlayerState;
    }

    [PunRPC]
    void OnFXRPC(string fxName, Vector3 pos, int atkCombo)
    {
        GameObject ef = EffectPoolManager.GetEffect(fxName);
        ef.transform.position = pos;
        ef.SetActive(true);
        ef.GetComponent<EffectSystem>().OnStartEffect();
        if (atkCombo == 3) AudioManager.PlaySfx(SFX.BOXER_HIT_RIGHT);
        else AudioManager.PlaySfx(SFX.BOXER_HIT_JAB);
    }

    [PunRPC]
    void SelectJobRPC(int jobIdx)
    {
        switch ((PLAYERTYPE)jobIdx)
        {
            case PLAYERTYPE.BOXER:
                m_curAnim.runtimeAnimatorController = m_arrAnimCtrlType[jobIdx];

                if(m_pv.IsMine)
                {
                    m_curType = PLAYERTYPE.BOXER;
                    m_sInfo.atk = 30;
                    m_sInfo.atkRange = 2f;
                    m_sInfo.atkSpeed = 1f;
                    m_sInfo.runSpeed = 4f;
                    m_sInfo.targetCnt = 1;

                    m_fMoveSpeed = m_sInfo.runSpeed;
                    m_pjAttackRange.orthographicSize = m_sInfo.atkRange;
                    m_enemyScan.OnRefreshScanRange();

                    UIManager.um.SystemMessage("전직완료 : 복서");
                }

                GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
                break;

            case PLAYERTYPE.CHEERLEADER:
                m_curAnim.runtimeAnimatorController = m_arrAnimCtrlType[jobIdx];

                if (m_pv.IsMine)
                {
                    m_curType = PLAYERTYPE.CHEERLEADER;
                    m_sInfo.atk = 0;
                    m_sInfo.atkRange = 3f;
                    m_sInfo.atkSpeed = 0.3f;
                    m_sInfo.runSpeed = 5f;
                    m_sInfo.targetCnt = 1;

                    m_fMoveSpeed = m_sInfo.runSpeed;
                    m_pjAttackRange.orthographicSize = m_sInfo.atkRange;
                    m_enemyScan.OnRefreshScanRange();

                    UIManager.um.SystemMessage("전직완료 : 응원단장");
                }
  
                GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
                break;
        }
    }

    [PunRPC]
    void UpStatRPC(int typeIdx, string nickName, float plus)
    {
        if ((CARDTYPE)typeIdx == CARDTYPE.BOOM)
        {
            UIManager.um.SystemMessage(nickName + "님이 꽝을 선택하셨습니다ㅋㅋ");
            //망했을때 사운드 찾아서 넣어놓기
        }

        if (!m_pv.IsMine) return;

        switch ((CARDTYPE)typeIdx)
        {
            case CARDTYPE.ATTACKPOWER:
                m_sInfo.atk += (int)plus;
                break;
            case CARDTYPE.ATTACKSPEED:
                m_sInfo.atkSpeed += plus;
                break;
            case CARDTYPE.ATTACKRANGE:
                m_sInfo.atkRange += plus;
                m_pjAttackRange.orthographicSize = m_sInfo.atkRange + m_sPlayerBuff.buffAtkRange;
                m_enemyScan.OnRefreshScanRange();
                break;
            case CARDTYPE.MOVESPEED:
                m_sInfo.runSpeed += plus;
                m_fMoveSpeed = m_sInfo.runSpeed;
                break;
            case CARDTYPE.TARGETCOUNT:
                m_sInfo.targetCnt += (int)plus;
                break;
        }

        AudioManager.PlaySfx(SFX.UPGRADE);
    }

    public IEnumerator ChangeIdle()
    {
        yield return new WaitForSeconds(0.3f);

        m_isJumping = false;
        m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
    }

    void CheckFalling()
    {
        //떨어지고 있다고 판단
        if (m_curRigid.velocity.y < -1f && m_pv.IsMine && m_curState != PLAYERSTATE.FALLING)
        {
            m_stateMachine.ChangeState(PLAYERSTATE.FALLING);
        }
    }

    //debug용
    void Respawn()
    {
        if (m_bRespawnDown)
        {
            transform.position = new Vector3(0, 3, 0);
            transform.rotation = Quaternion.identity;
            m_curRigid.velocity = Vector3.zero;
        }
    }

    void OtherChangeCostume()
    {
        var dic = NetworkManager.nm.m_dicPlayFabCostume[m_pv.Owner.NickName];

        for (int j = 0; j < dic.Count; j++)
        {
            GameObject charItem = gameObject.transform.Find((COSTUMETYPE.Hat + j).ToString()).gameObject;

            if (dic[(int)(COSTUMETYPE.Hat + j)] == 0)
            {
                charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
                continue;
            }

            string strDir = "Costume" + "/" + (COSTUMETYPE.Hat + j).ToString() + "/" + dic[(int)COSTUMETYPE.Hat + j].ToString();
            GameObject costume = Resources.Load<GameObject>(strDir);

            GameObject changeItem = costume.transform.Find((COSTUMETYPE.Hat + j).ToString()).gameObject;

            if (changeItem != null)
                charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                    changeItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(m_iMyGold);
        }

        else
        {
            m_iMyGold = (int)stream.ReceiveNext();
        }
    }
}
