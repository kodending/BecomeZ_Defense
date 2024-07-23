using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#region µÓ±ﬁ∫∞ ø¿∫Í¡ß∆Æ µÒº≈≥ ∏Æ
[System.Serializable]
public class DicRankPrefabs : SerializableDictionary<UNITRANK, GameObject> { }
#endregion

public class UnitFSM : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public UnitStateMachine m_stateMachine { get; private set; }

    public PhotonView m_pv;

    public Animator m_anim;

    public UNITINFO m_sInfo;

    public Projector m_pjAttackRange;

    public UNITSTATE m_curState;

    public EnemyScanner m_enemyScan;

    [SerializeField] public float m_fDefaultAtkSpd = 1;

    public DicRankPrefabs m_dicPrefabs;

    public UNITRANK m_eCurRank;

    public GameObject m_goSelectEffect;

    public BUFFINFO m_sUnitBuff;

    public BUFFINFO m_sPlayerBuff;

    public Transform m_trBulletPos;

    public ParticleSystem[] m_arrEffects;

    private void Start()
    {
        InitStateMachine();

        if (m_pv.IsMine)
        {
            m_enemyScan = GetComponent<EnemyScanner>();
            m_sUnitBuff = new BUFFINFO();
            m_sPlayerBuff = new BUFFINFO();
        }
    }

    private void Update()
    {
        m_stateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        m_stateMachine.FixedUpdateState();
    }

    void InitStateMachine()
    {
        m_stateMachine = new UnitStateMachine(UNITSTATE.ENTRY, new Unit_Entry(this));
        m_stateMachine.AddState(UNITSTATE.IDLE, new Unit_Idle(this));
        m_stateMachine.AddState(UNITSTATE.ATTACK, new Unit_Attack(this));
    }

    public void InitParam(Dictionary<string, object> dicInfo, float fDelayTime = 0, int curLV = 0)
    {
        if (!m_pv.IsMine) return;

        m_pv.RPC("InitParamRPC", RpcTarget.All, dicInfo, fDelayTime, curLV);

        if (m_stateMachine != null)
            m_stateMachine.ChangeState(UNITSTATE.IDLE);
    }

    [PunRPC]
    void InitParamRPC(Dictionary<string, object> dicInfo, float fDelayTime = 0, int curLV = 0)
    {
        gameObject.SetActive(true);

        m_sInfo.eType = (UNITTYPE)int.Parse(dicInfo["TYPE"].ToString());
        m_sInfo.eRank = (UNITRANK)int.Parse(dicInfo["RANK"].ToString());
        m_sInfo.atk = int.Parse(dicInfo["ATK"].ToString());
        m_sInfo.atkRange = float.Parse(dicInfo["ATKRANGE"].ToString());
        m_sInfo.atkSpeed = float.Parse(dicInfo["ATKSPD"].ToString());
        m_sInfo.animLength = float.Parse(dicInfo["ANIMLENGTH"].ToString());
        if(curLV == 0 && GameManager.gm.m_listMyUnits.Count > 0)
        {
            foreach(var unit in GameManager.gm.m_listMyUnits)
            {
                if (unit == this) continue;

                if (unit.m_sInfo.eType == m_sInfo.eType)
                {
                    m_sInfo.curLV = unit.m_sInfo.curLV;
                    break;
                }
            }
        }
        else m_sInfo.curLV = curLV;
        m_sInfo.LvAtk = int.Parse(dicInfo["LVATK"].ToString());

        m_anim = m_dicPrefabs[m_sInfo.eRank].GetComponent<Animator>();
        transform.SetParent(GameObject.Find("BakeNavi").transform.Find("Units"));

        ActiveRankUnit(m_sInfo.eRank, fDelayTime);

        m_pjAttackRange.orthographicSize = m_sInfo.atkRange;
        m_enemyScan.scanRange = m_pjAttackRange.orthographicSize * 0.75f;
    }

    [PunRPC]
    public void AnimTriggerRPC(string strName, int stateIdx) { m_anim.SetTrigger(strName); m_curState = (UNITSTATE)stateIdx; }

    [PunRPC]
    public void AnimFloatRPC(string strName, float fFloat) => m_anim.SetFloat(strName, fFloat);

    [PunRPC]
    public void AnimIntRPC(string strName, int iInt) => m_anim.SetInteger(strName, iInt);

    [PunRPC]
    public void AnimBoolRPC(string strName, bool bBool, int stateIdx) { m_anim.SetBool(strName, bBool); m_curState = (UNITSTATE)stateIdx; Debug.Log("≥ª ªÛ≈¬ : " + m_curState.ToString()); }

    [PunRPC]
    public void ChangeStateRPC(int stateIdx)
    {
        m_stateMachine.ChangeState((UNITSTATE)stateIdx);
    }

    public void SetUnitBuff(int buffAtk = 0, float buffAtkSpd = 0, int buffIdx = 0)
    {
        m_pv.RPC("SetUnitBuffRPC", RpcTarget.All, buffAtk, buffAtkSpd, buffIdx);
    }

    [PunRPC]
    void SetUnitBuffRPC(int buffAtk = 0, float buffAtkSpd = 0, int buffIdx = 0)
    {
        if(buffAtk > m_sUnitBuff.buffAtk) m_sUnitBuff.buffAtk = buffAtk;
        if(buffAtkSpd > m_sUnitBuff.buffAtkSpd) m_sUnitBuff.buffAtkSpd = buffAtkSpd;

        if (buffAtk == 0 && buffAtkSpd == 0)
        {
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
            m_pjAttackRange.orthographicSize = m_sInfo.atkRange + m_sPlayerBuff.buffAtkRange;
            m_enemyScan.OnRefreshScanRange();
        }

        if (buffAtkSpd == 0 && buffAtkRng == 0)
        {
            m_sPlayerBuff = new BUFFINFO();

            m_pjAttackRange.orthographicSize = m_sInfo.atkRange + buffAtkRng;
            m_enemyScan.OnRefreshScanRange();

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
    void OnFXRPC(string fxName, Vector3 pos)
    {
        //Debug.Log("≥ª ¿Ã∆Â∆Æ ΩË¥Ÿ : " +  fxName);

        GameObject ef = EffectPoolManager.GetEffect(fxName);
        ef.transform.position = pos;
        ef.SetActive(true);
        ef.GetComponent<EffectSystem>().OnStartEffect();

        if (m_sInfo.eType == UNITTYPE.MELEE_SINGLE) AudioManager.PlaySfx(SFX.UNIT_MELEE);
        if (m_sInfo.eType == UNITTYPE.MELEE_MULTI || m_sInfo.eType == UNITTYPE.SPECIAL_MELEE) AudioManager.PlaySfx(SFX.UNIT_MELEE_SPECIAL);
        if (m_sInfo.eType == UNITTYPE.RANGED_SINGLE || m_sInfo.eType == UNITTYPE.RANGED_MULTI) AudioManager.PlaySfx(SFX.UNIT_RANGED);
        if (m_sInfo.eType == UNITTYPE.SPECIAL_RANGED) AudioManager.PlaySfx(SFX.UNIT_RANGED_SPECIAL_HIT);
    }

    [PunRPC]
    void LevelUpRPC()
    {
        m_sInfo.curLV += 1;
        AudioManager.PlaySfx(SFX.UPGRADE);
    }

    void ActiveRankUnit(UNITRANK eRank, float fDelayTime = 0f)
    {
        if (m_curState != 0)
            UIManager.um.HideScaleUI(m_dicPrefabs[m_eCurRank]);

        m_eCurRank = eRank;
        StartCoroutine(ShowNextRank(eRank, fDelayTime));
    }

    IEnumerator ShowNextRank(UNITRANK eRank, float fDelayTime = 0f)
    {
        yield return new WaitForSeconds(fDelayTime);

        m_dicPrefabs[eRank].SetActive(true);
        UIManager.um.ShowScaleUI(m_dicPrefabs[eRank], 1f);

        m_anim = m_dicPrefabs[eRank].GetComponent<Animator>();
        m_anim.SetFloat("AttackSpeed", m_fDefaultAtkSpd * m_sInfo.atkSpeed);

        m_stateMachine.currentState?.OnEnterState();
    }
}
