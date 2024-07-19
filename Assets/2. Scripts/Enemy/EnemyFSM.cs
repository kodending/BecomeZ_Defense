using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation.Samples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR;

public class EnemyFSM : MonoBehaviourPunCallbacks, IPunObservable
{
    public NavMeshAgent m_agent;

    [SerializeField]
    Animator m_anim;

    public PhotonView m_pv;

    [SerializeField] Rigidbody m_oldRigid;

    Transform m_trTarget;

    public ENEMYSPAWNSPOT m_eStartType;

    Transform[] m_arrFlags;

    int curFlagIdx;

    [Tooltip("적 정보 저장")]
    public ENEMYINFO m_sInfo;

    public bool m_isDead;

    Vector3 m_vNetworkPos;
    Quaternion m_qNetworkRot;
    [SerializeField] Rigidbody m_curRigid;

    HpBarControl m_hpBar;

    public Transform m_sdHpBarPos;

    bool m_bJump;

    [SerializeField] AgentLinkMover m_linkMover;

    void Start()
    {
        //m_trCam = GameObject.Find("Main Camera").transform;
        transform.SetParent(GameObject.Find("Enemies").transform);
    }

    private void FixedUpdate()
    {
        if (!m_pv.IsMine)
        {
            m_curRigid.position = Vector3.MoveTowards(m_curRigid.position, m_vNetworkPos, Time.fixedDeltaTime);
            m_curRigid.rotation = Quaternion.RotateTowards(m_curRigid.rotation, m_qNetworkRot, Time.fixedDeltaTime * 100.0f);
        }
    }

    void LateUpdate()
    {
        if (!m_pv.IsMine) return;

        m_anim.SetBool("IsJump", m_agent.isOnOffMeshLink);

        //if (!m_bJump && m_agent.isOnOffMeshLink)
        //{
        //    m_anim.SetBool("IsJump", m_agent.isOnOffMeshLink);
        //    m_bJump = true;
        //    Debug.Log("나 점프 했다");
        //}

        //else if (m_bJump && !m_agent.isOnOffMeshLink)
        //{
        //    m_anim.SetBool("IsJump", m_agent.isOnOffMeshLink);
        //    m_bJump = false;
        //    Debug.Log("나 착지 했다");
        //}
    }

    private void Update()
    {
        if (m_agent.pathStatus == NavMeshPathStatus.PathComplete
            && m_agent.remainingDistance - m_agent.stoppingDistance < 0.1f
            && curFlagIdx < m_arrFlags.Length)
        {
            curFlagIdx++;

            if (curFlagIdx >= m_arrFlags.Length) return;

            NextFlag(curFlagIdx);
        }
    }

    void NextFlag(int flag)
    {
        m_agent.SetDestination(m_arrFlags[flag].position);
    }

    public void DeathMotion()
    {
        m_pv.RPC("DeathParamRPC", RpcTarget.All);
    }

    IEnumerator DestroyObject()
    {
        if (!PhotonNetwork.IsMasterClient) yield break;

        yield return new WaitForSeconds(2f);

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    void DeathParamRPC()
    {
        m_isDead = true;
        m_agent.enabled = false;
        m_hpBar.m_sdHpBar.value = 100;
        m_hpBar.m_sdHpBarBack.value = 100;
        HpBarPoolManager.ReturnHpBar(m_hpBar.gameObject);
        m_anim.SetTrigger("Death");
        StartCoroutine(DestroyObject());
    }

    [PunRPC]
    void PassRPC()
    {
        m_agent.enabled = false;
        m_hpBar.m_sdHpBar.value = 100;
        m_hpBar.m_sdHpBarBack.value = 100;
        HpBarPoolManager.ReturnHpBar(m_hpBar.gameObject);
        AudioManager.PlaySfx(SFX.LIFE_HIT);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    public void InitParam(int idx, bool bRand, int iRand, int eStartType)
    {
        m_pv.RPC("ResetParamRPC", RpcTarget.All, idx, bRand, iRand, eStartType);

        m_agent.acceleration = m_sInfo.accel;
        m_agent.speed = m_sInfo.spd;
    }

    [PunRPC]
    void ResetParamRPC(int idx, bool bRand, int iRand, int eStartType)
    {
        gameObject.SetActive(true);
        m_isDead = false;
        m_agent.enabled = true;
        m_agent.velocity = Vector3.zero;
        StartCoroutine(m_linkMover.Start());

        m_hpBar = HpBarPoolManager.GetHpBar(this.transform).GetComponent<HpBarControl>();

        if (!bRand) m_eStartType = (ENEMYSPAWNSPOT)eStartType;
        else m_eStartType = ENEMYSPAWNSPOT.LeftFlags + iRand;
        
        m_arrFlags = GameObject.Find(m_eStartType.ToString()).GetComponentsInChildren<Transform>();
        curFlagIdx = 1;
        m_agent.SetDestination(m_arrFlags[curFlagIdx].position);

        m_sInfo.idx = 1;

        var dicInfo = EnemyFactory.ef.m_enemyInfo[idx];

        m_sInfo = new ENEMYINFO();
        //정보 입력
        m_sInfo.idx = int.Parse(dicInfo["INDEX"].ToString());
        m_sInfo.hp = int.Parse(dicInfo["HP"].ToString());
        m_sInfo.curHp = m_sInfo.hp;
        m_sInfo.def = int.Parse(dicInfo["DEF"].ToString());
        m_sInfo.atk = int.Parse(dicInfo["ATK"].ToString());
        m_sInfo.accel = float.Parse(dicInfo["ACCEL"].ToString());
        m_sInfo.spd = float.Parse(dicInfo["SPD"].ToString());
        m_sInfo.gold = int.Parse(dicInfo["GOLD"].ToString());

        m_anim.SetTrigger("Run");
    }

    public void RefreshHp(int curHp)
    {

        if (m_pv.IsMine && !PhotonNetwork.IsMasterClient) return;
        if (!m_pv.IsMine && PhotonNetwork.IsMasterClient) return;
        if (curHp <= 0)
        {
            DeathMotion();
            return;
        }

        m_sInfo.curHp = curHp;

        if (m_sInfo.hp != m_sInfo.curHp)
        {
            m_hpBar.gameObject.SetActive(true);
        }

        float value = ((float)m_sInfo.curHp / (float)m_sInfo.hp) * 110;
        m_hpBar.m_sdHpBar.value = value;
        StartCoroutine(RefreshHpMotion(value));
    }

    IEnumerator RefreshHpMotion(float value)
    {
        float delta = 0;
        float duration = 0.5f;

        while (delta <= duration)
        {
            float t = delta / duration;
            //t = Mathf.Sin((t * Mathf.PI) / 2);
            t = t * t * t * t * t;

            m_hpBar.m_sdHpBarBack.value = Mathf.Lerp(m_hpBar.m_sdHpBarBack.value, value, t);

            delta += Time.deltaTime;
            yield return null;
        }

        m_hpBar.m_sdHpBarBack.value = value;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.m_oldRigid.position);
            stream.SendNext(this.m_oldRigid.rotation);
            stream.SendNext(this.m_oldRigid.velocity);
            stream.SendNext(this.m_bJump);
        }
        else
        {
            m_vNetworkPos = (Vector3)stream.ReceiveNext();
            m_qNetworkRot = (Quaternion)stream.ReceiveNext();
            this.m_curRigid.velocity = (Vector3)stream.ReceiveNext();

            //소유자로부터 정보를 받은 값의 시간 차이를 계산
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
            m_vNetworkPos += (this.m_oldRigid.velocity * lag);
            m_bJump = (bool)stream.ReceiveNext();
        }
    }
}
