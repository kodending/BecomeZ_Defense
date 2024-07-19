using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class UnitBullet : MonoBehaviourPunCallbacks
{
    public PhotonView m_pv;

    public int m_iIndex;

    public int m_iAtkPower;

    public float m_fSpeed;

    public bool m_isPiercing;

    public float m_fScanRange;
    public LayerMask targetEnemyLayers;
    public List<RaycastHit> checkedTargetEnemies = new List<RaycastHit>();
    public RaycastHit[] TargetEnemies;

    private void Update() => transform.Translate(Vector3.forward * m_fSpeed * Time.deltaTime);

    private void FixedUpdate()
    {
        TargetEnemies = Physics.SphereCastAll(transform.position, m_fScanRange, Vector3.up, 0, targetEnemyLayers);

        checkedTargetEnemies.Clear();
        foreach (RaycastHit hit in TargetEnemies)
        {
            if (hit.transform.GetComponent<EnemyFSM>().m_isDead) continue;

            checkedTargetEnemies.Add(hit);
        }
    }


    [PunRPC]
    void RotRPC(Quaternion q, int idx, int atk, float speed, bool isPiecing = false, float scanRange = 0)
    {
        transform.rotation = q;
        m_iIndex = idx;
        m_iAtkPower = atk;
        m_fSpeed = speed;
        m_isPiercing = isPiecing;
        m_fScanRange = scanRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_pv.IsMine) return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Wall") ||
            other.gameObject.layer == LayerMask.NameToLayer("PlayerWall"))
        {
            PhotonNetwork.Destroy(this.gameObject);
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            RangedAttack(other.gameObject.GetComponent<EnemyFSM>());
        }
    }

    void RangedAttack(EnemyFSM enemy)
    {
        switch((UNITBULLETTYPE)m_iIndex)
        {
            case UNITBULLETTYPE.SINGLE_NORMAL:

                //비관통
                if(!enemy.m_isDead)
                {
                    int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(enemy);
                    NetworkManager.nm.AtkSignal(idx, m_iAtkPower, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    m_pv.RPC("OnFXRPC", RpcTarget.All, "UNITATTACK_RangedSingle", enemy.transform.position);
                    AudioManager.PlaySfx(SFX.UNIT_RANGED_HIT);
                }

                PhotonNetwork.Destroy(this.gameObject);

                break;
            case UNITBULLETTYPE.SINGLE_EPIC:

                //관통
                if (!enemy.m_isDead)
                {
                    int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(enemy);
                    NetworkManager.nm.AtkSignal(idx, m_iAtkPower, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    m_pv.RPC("OnFXRPC", RpcTarget.All, "UNITATTACK_RangedSingleEpic", enemy.transform.position);
                    AudioManager.PlaySfx(SFX.UNIT_RANGED_HIT);
                }

                break;
            case UNITBULLETTYPE.MULTI_NORMAL:
            case UNITBULLETTYPE.MULTI_EPIC:

                if (!enemy.m_isDead)
                {
                    int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(enemy);
                    NetworkManager.nm.AtkSignal(idx, m_iAtkPower, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    string fxName = (UNITBULLETTYPE)m_iIndex >= UNITBULLETTYPE.MULTI_EPIC ? "UNITATTACK_RangedMultiEpic" : "UNITATTACK_RangedMulti";
                    m_pv.RPC("OnFXRPC", RpcTarget.All, fxName, enemy.transform.position);
                    AudioManager.PlaySfx(SFX.UNIT_RANGED_HIT);
                }

                foreach (var target in checkedTargetEnemies)
                {
                    if (target.transform.GetComponent<EnemyFSM>() == enemy) continue;
                    if (target.transform.GetComponent<EnemyFSM>().m_isDead) continue;

                    int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                    string fxName = (UNITBULLETTYPE)m_iIndex >= UNITBULLETTYPE.MULTI_EPIC ? "UNITATTACK_RangedMultiOtherEpic" : "UNITATTACK_RangedMultiOther";
                    m_pv.RPC("OnFXRPC", RpcTarget.All, fxName, target.transform.position);
                    AudioManager.PlaySfx(SFX.UNIT_RANGED_HIT);
                    NetworkManager.nm.AtkSignal(idx, (int)(m_iAtkPower * 0.5f), NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                }

                PhotonNetwork.Destroy(this.gameObject);
                break;
        }
    }

    [PunRPC]
    void OnFXRPC(string fxName, Vector3 pos)
    {
        GameObject ef = EffectPoolManager.GetEffect(fxName);
        ef.transform.position = pos;
        ef.SetActive(true);
        ef.GetComponent<EffectSystem>().OnStartEffect();
    }
}
