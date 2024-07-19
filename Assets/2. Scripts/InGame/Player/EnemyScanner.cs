using Photon.Pun;
using PlayFab.ServerModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyScanner : MonoBehaviourPunCallbacks
{
    public float scanRange;
    public LayerMask targetEnemyLayers;
    public List<RaycastHit> checkedTargetEnemies = new List<RaycastHit>();
    public RaycastHit[] TargetEnemies;
    public Transform nearestTargetEnemy;

    public LayerMask targetAllyLayers;
    public RaycastHit[] targetAlly;
    List<RaycastHit> listCheckAlly = new List<RaycastHit>();

    UnitFSM unit;

    private void Start()
    {
        if (GetComponent<PlayerController>() != null)
            scanRange = GetComponent<PlayerController>().m_pjAttackRange.orthographicSize * 0.75f;

        if (GetComponent<UnitFSM>() != null)
        {
            scanRange = GetComponent<UnitFSM>().m_pjAttackRange.orthographicSize * 0.75f;
            unit = GetComponent<UnitFSM>();
        }
    }

    private void FixedUpdate()
    {
        TargetEnemies = Physics.SphereCastAll(transform.position, scanRange, Vector3.up, 0, targetEnemyLayers);

        checkedTargetEnemies.Clear();
        foreach (RaycastHit hit in TargetEnemies)
        {
            if (hit.transform.GetComponent<EnemyFSM>().m_isDead) continue;

            checkedTargetEnemies.Add(hit);
        }

        nearestTargetEnemy = GetNearestEnemy();

        if (unit != null)
        {
            if (unit.m_sInfo.eType == UNITTYPE.BUFFER ||
                unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER)
            {
                targetAlly = Physics.SphereCastAll(transform.position, scanRange, Vector3.up, 0, targetAllyLayers);

                if(listCheckAlly.Count == 0)
                {
                    foreach(var ally in targetAlly)
                    {
                        if (ally.transform.gameObject == unit.transform.gameObject) continue;

                        listCheckAlly.Add(ally);
                        //아군들 버프 시키기 위한 정보를 넘긴다.
                        if(ally.transform.GetComponent<PlayerController>() != null)
                        {
                            int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                            ally.transform.GetComponent<PlayerController>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                        }

                        else if (ally.transform.GetComponent<UnitFSM>() != null)
                        {
                            int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                            ally.transform.GetComponent<UnitFSM>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                        }
                    }
                }

                else
                {
                    //새로들어온 놈 확인 해야되는데..
                    foreach(var ally in targetAlly)
                    {
                        if (ally.transform.gameObject == unit.transform.gameObject) continue;

                        //기존에 없던 새로 들어온놈이므로 정보를 보낸다.
                        if (!listCheckAlly.Contains(ally))
                        {
                            if (ally.transform.GetComponent<PlayerController>() != null)
                            {
                                int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                                ally.transform.GetComponent<PlayerController>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                            }

                            else if (ally.transform.GetComponent<UnitFSM>() != null)
                            {
                                int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                                ally.transform.GetComponent<UnitFSM>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                            }
                        }
                    }

                    //아군이 벗어났는지 확인용
                    foreach(var ally in listCheckAlly)
                    {
                        //버프 초기화
                        if(!targetAlly.Contains(ally))
                        {
                            if (ally.transform.GetComponent<PlayerController>() != null)
                            {
                                ally.transform.GetComponent<PlayerController>().SetUnitBuff();
                            }

                            else if (ally.transform.GetComponent<UnitFSM>() != null)
                            {
                                ally.transform.GetComponent<UnitFSM>().SetUnitBuff();
                            }
                        }
                    }

                    //검열이 끝나면 싹 비우고
                    listCheckAlly.Clear();

                    foreach(var ally in targetAlly)
                    {
                        if (ally.transform.gameObject == unit.transform.gameObject) continue;

                        //채우기만한다.
                        listCheckAlly.Add(ally);
                    }


                    //혹시나 버프 꺼진놈이 있으면 버프 다시 켜준다.
                    foreach(var ally in listCheckAlly)
                    {
                        if (ally.transform.GetComponent<PlayerController>() != null)
                        {
                            if (!ally.transform.GetComponent<PlayerController>().m_arrEffects[0].transform.gameObject.activeSelf &&
                                !ally.transform.GetComponent<PlayerController>().m_arrEffects[1].transform.gameObject.activeSelf)
                            {
                                int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                                ally.transform.GetComponent<PlayerController>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                            }
                        }

                        else if (ally.transform.GetComponent<UnitFSM>() != null)
                        {
                            if (!ally.transform.GetComponent<UnitFSM>().m_arrEffects[0].transform.gameObject.activeSelf &&
                                !ally.transform.GetComponent<UnitFSM>().m_arrEffects[1].transform.gameObject.activeSelf)
                            {
                                int buffIdx = unit.m_sInfo.eType == UNITTYPE.SPECIAL_BUFFER ? 1 : 0;
                                ally.transform.GetComponent<UnitFSM>().SetUnitBuff(unit.m_sInfo.atk, unit.m_sInfo.atkSpeed, buffIdx);
                            }
                        }
                    }
                }
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, scanRange);
    //}

    Transform GetNearestEnemy()
    {
        Transform result = null;
        float diff = 100;

        foreach (var target in checkedTargetEnemies)
        {
            if (target.transform.GetComponent<EnemyFSM>().m_isDead) continue;

            Vector3 myPos = transform.position;     
            Vector3 targetPos = target.transform.position;

            float curDiff = Vector3.Distance(myPos, targetPos);

            if(curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }

    public void OnRefreshScanRange()
    {
        if (GetComponent<PlayerController>() != null)
            scanRange = GetComponent<PlayerController>().m_pjAttackRange.orthographicSize * 0.75f;

        if (GetComponent<UnitFSM>() != null)
            scanRange = GetComponent<UnitFSM>().m_pjAttackRange.orthographicSize * 0.75f;
    }
}
