using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.Rendering.DebugUI.Table;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;


public class Unit_Attack : UnitBaseState
{
    float m_fMaxTimer;
    float m_fAnimTimer;
    float m_fAnimSpeed;
    int   m_icurAtk;
    List <RaycastHit> m_listHitEnemies = new List<RaycastHit>();

    EnemyFSM m_curTargetEnemy;

    public Unit_Attack(UnitFSM unit) : base(unit) { }

    public override void OnEnterState()
    {
        m_fAnimTimer = 0;
        m_fMaxTimer = (unitFSM.m_fDefaultAtkSpd / (unitFSM.m_sInfo.atkSpeed /* //버프캐릭의 공격속도 더하기 */ + unitFSM.m_sUnitBuff.buffAtkSpd + unitFSM.m_sPlayerBuff.buffAtkSpd)) * unitFSM.m_sInfo.animLength;
        m_fAnimSpeed = unitFSM.m_fDefaultAtkSpd * (unitFSM.m_sInfo.atkSpeed /* //버프캐릭의 공격속도 더하기 */ + unitFSM.m_sUnitBuff.buffAtkSpd + unitFSM.m_sPlayerBuff.buffAtkSpd);

        m_icurAtk = unitFSM.m_sInfo.atk + (unitFSM.m_sInfo.curLV * unitFSM.m_sInfo.LvAtk) /* + 버프캐릭의 버프 */ + unitFSM.m_sUnitBuff.buffAtk;

        unitFSM.m_anim.SetTrigger("Attack");
        unitFSM.m_curState = UNITSTATE.ATTACK;
        unitFSM.m_anim.SetFloat("AttackSpeed", m_fAnimSpeed);

        if (!unitFSM.m_pv.IsMine) return;

        //unitFSM.m_pv.RPC("AnimTriggerRPC", RpcTarget.All, "Attack", (int)UNITSTATE.ATTACK);
        //unitFSM.m_pv.RPC("AnimBoolRPC", RpcTarget.All, "isAttack", true, (int)UNITSTATE.ATTACK);

        m_curTargetEnemy = unitFSM.m_enemyScan.nearestTargetEnemy.GetComponent<EnemyFSM>();

        Vector3 rot = m_curTargetEnemy.transform.position - unitFSM.transform.position; rot.y = 0;

        ////공격방향
        unitFSM.transform.rotation = Quaternion.LookRotation(rot.normalized);

        //unitFSM.m_pv.RPC("AnimFloatRPC", RpcTarget.All, "AttackSpeed", m_fAnimSpeed);

        //히트된 에너미를 딱 고른다.
        m_listHitEnemies.Clear();
        foreach (var target in unitFSM.m_enemyScan.checkedTargetEnemies)
        {
            m_listHitEnemies.Add(target);
        }
    }

    public override void OnUpdateState()
    {
        if (!unitFSM.m_pv.IsMine) return;

        OnTypeAttack();
    }

    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState() 
    { 
    }

    void OnTypeAttack()
    {
        Vector3 rot = m_curTargetEnemy.transform.position - unitFSM.transform.position; rot.y = 0;

        switch (unitFSM.m_sInfo.eType)
        {
                ////공격방향
            case UNITTYPE.MELEE_SINGLE:
                m_fAnimTimer += Time.deltaTime;
                unitFSM.transform.rotation = Quaternion.LookRotation(rot.normalized);

                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;
                    if (!m_curTargetEnemy.m_isDead)
                    {
                        foreach (var target in unitFSM.m_enemyScan.checkedTargetEnemies)
                        {
                            if (target.transform.GetComponent<EnemyFSM>() == m_curTargetEnemy)
                            {
                                int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                                NetworkManager.nm.AtkSignal(idx, m_icurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                                string fxName = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? "UNITATTACK_MeleeSingleEpic" : "UNITATTACK_MeleeSingle";
                                unitFSM.m_pv.RPC("OnFXRPC", RpcTarget.All, fxName, target.transform.position);
                                break;
                            }
                        }
                    }

                    unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                }

                break;

            case UNITTYPE.MELEE_MULTI:
                m_fAnimTimer += Time.deltaTime;

                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;

                    //내려 찍었을 때 해당 범위에 있었던 놈들을 공격한다.
                    foreach (var target in unitFSM.m_enemyScan.checkedTargetEnemies)
                    {
                        if (target.transform.GetComponent<EnemyFSM>().m_isDead) continue;

                        int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                        NetworkManager.nm.AtkSignal(idx, m_icurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    }

                    Vector3 pos = new Vector3(unitFSM.transform.position.x, 0.01f, unitFSM.transform.position.z);
                    string fxName = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? "UNITATTACK_MeleeMultiEpic" : "UNITATTACK_MeleeMulti";
                    unitFSM.m_pv.RPC("OnFXRPC", RpcTarget.All, fxName, pos);
                    //unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                    unitFSM.m_pv.RPC("ChangeStateRPC", RpcTarget.All, (int)UNITSTATE.IDLE);
                }
                break;

            case UNITTYPE.SPECIAL_MELEE:
                m_fAnimTimer += Time.deltaTime;

                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;

                    //내려 찍었을 때 해당 범위에 있었던 놈들을 공격한다.
                    foreach (var target in unitFSM.m_enemyScan.checkedTargetEnemies)
                    {
                        if (target.transform.GetComponent<EnemyFSM>().m_isDead) continue;

                        int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                        NetworkManager.nm.AtkSignal(idx, m_icurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    }

                    Vector3 pos = new Vector3(unitFSM.transform.position.x, 0.01f, unitFSM.transform.position.z);
                    unitFSM.m_pv.RPC("OnFXRPC", RpcTarget.All, "UNITATTACK_SpecialMelee1", pos);
                    unitFSM.m_pv.RPC("OnFXRPC", RpcTarget.All, "UNITATTACK_SpecialMelee2", pos);
                    unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                }
                break;

            case UNITTYPE.RANGED_SINGLE:
                m_fAnimTimer += Time.deltaTime;
                unitFSM.transform.rotation = Quaternion.LookRotation(rot.normalized);

                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;
                    //총알 발사 해야됨
                    //총알 생성하자
                    if (m_curTargetEnemy.m_isDead)
                    {
                        unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                        break;
                    }

                    UNITBULLETTYPE bulletType = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? UNITBULLETTYPE.SINGLE_EPIC : UNITBULLETTYPE.SINGLE_NORMAL;

                    float spd = 8f;

                    bool isPiercing = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? true : false;

                    GenerateBullets(bulletType, unitFSM.transform.rotation, m_icurAtk, spd, isPiercing);

                    unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                }
                break;

            case UNITTYPE.RANGED_MULTI:
                m_fAnimTimer += Time.deltaTime;
                unitFSM.transform.rotation = Quaternion.LookRotation(rot.normalized);

                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;

                    if (m_curTargetEnemy.m_isDead)
                    {
                        unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                        break;
                    }
                    //총알 발사 해야됨
                    //총알 생성하자
                    UNITBULLETTYPE bulletType = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? UNITBULLETTYPE.MULTI_EPIC : UNITBULLETTYPE.MULTI_NORMAL;

                    float spd = 4.5f;

                    float range = unitFSM.m_sInfo.eRank >= UNITRANK.EPIC ? 4f : 2.5f;

                    GenerateBullets(bulletType, unitFSM.transform.rotation, m_icurAtk, spd, false, range);

                    unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                }
                break;
            case UNITTYPE.SPECIAL_RANGED:
                m_fAnimTimer += Time.deltaTime;
                if (m_fAnimTimer >= m_fMaxTimer)
                {
                    m_fAnimTimer = 0;

                    //마법진을 초반에 그리고 애니메이션이 당시 들어왔던 애들을 원격 타격한다.
                    foreach (var target in m_listHitEnemies)
                    {
                        if (target.transform.GetComponent<EnemyFSM>().m_isDead) continue;

                        int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                        Vector3 pos = target.transform.position + new Vector3(0, 2, 0);
                        unitFSM.m_pv.RPC("OnFXRPC", RpcTarget.All, "UNITATTACK_SpecialRanged", pos);
                        NetworkManager.nm.AtkSignal(idx, m_icurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    }

                    unitFSM.m_stateMachine.ChangeState(UNITSTATE.IDLE);
                }
                break;
        }
    }

    void GenerateBullets(UNITBULLETTYPE eType, Quaternion rot, int atk, float spd, bool isPiercing = false, float scanRange = 0)
    {
        string strFolderName = "UNITBULLET_";

        int bulletIdx = (int)eType;
    
        string strBullet = strFolderName + bulletIdx.ToString();

        GameObject bullet = PhotonNetwork.Instantiate(strBullet, unitFSM.m_trBulletPos.position, rot);
        bullet.GetComponent<PhotonView>().RPC("RotRPC", RpcTarget.All, rot, bulletIdx, atk, spd, isPiercing, scanRange);
        bullet.transform.SetParent(GameObject.Find("BulletPool").transform);
    }
}
