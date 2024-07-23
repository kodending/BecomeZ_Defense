using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.XR;
using UnityEditor;
using System.Linq;
using UnityEngine.EventSystems;

public class Player_Attack : PlayerBaseState
{
    [Tooltip("ÀÌµ¿ ¹æÇâ ¼³Á¤")]
    Vector3 m_vecMove;
    float m_fMoveX = 0.0f;
    float m_fMoveY = 0.0f;

    float m_fMaxTimer;
    float m_fAnimTimer;
    float m_fAnimSpeed;
    int m_iCurAtk;
    int m_iTargetCnt;

    EnemyFSM m_curTargetEnemy;

    public Player_Attack(PlayerController pc) : base(pc) { }

    public override void OnEnterState()
    {
        if (playerController.m_pv.IsMine)
        {
            playerController.m_curRigid.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            switch (playerController.m_curType)
            {
                case PLAYERTYPE.BOXER:
                    m_curTargetEnemy = playerController.m_enemyScan.nearestTargetEnemy.GetComponent<EnemyFSM>();

                    Vector3 rot = m_curTargetEnemy.transform.position - playerController.transform.position; rot.y = 0;

                    ////°ø°Ý¹æÇâ
                    playerController.transform.rotation = Quaternion.LookRotation(rot.normalized);

                    OnEnterBoxer();
                    break;

                case PLAYERTYPE.CHEERLEADER:
                    OnEnterCheer();
                    break;
            }
        }

        playerController.m_curRigid.velocity = Vector3.zero;
    }

    public override void OnUpdateState()
    {
        if (!playerController.m_pv.IsMine) return;


        switch (playerController.m_curType)
        {
            case PLAYERTYPE.BOXER:
                Vector3 rot = m_curTargetEnemy.transform.position - playerController.transform.position; rot.y = 0;

                ////°ø°Ý¹æÇâ
                playerController.transform.rotation = Quaternion.LookRotation(rot.normalized);
                OnUpdateBoxer();
                break;

            case PLAYERTYPE.CHEERLEADER:
                OnUpdateCheer();
                break;
        }


    }

    public override void OnFixedUpdateState()
    {
        if (!playerController.m_pv.IsMine) return;
        if (playerController.m_curType != PLAYERTYPE.CHEERLEADER) return;

        switch (playerController.m_eCurControl)
        {
            case PLAYERCONTROL.JOYSTCK:
                if (EventSystem.current.currentSelectedGameObject != null)
                    if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

                ControlJoyStick();
                break;
            case PLAYERCONTROL.KEYBOARD:
                if (EventSystem.current.currentSelectedGameObject != null)
                    if (EventSystem.current.currentSelectedGameObject.name == "InputChat") return;

                ControlKeyboard();
                break;
        }
    }

    public override void OnExitState()
    {
        playerController.m_curRigid.constraints = RigidbodyConstraints.None;
        playerController.m_curRigid.constraints = RigidbodyConstraints.FreezeRotation;

        if (!playerController.m_pv.IsMine) return;

        if(playerController.m_curType == PLAYERTYPE.CHEERLEADER)
        {
            foreach (var ally in playerController.m_listCheckAlly)
            {
                NotCheerAlly(ally);
            }

            playerController.m_pv.RPC("CheerFXRPC", RpcTarget.All, false, 0);
        }
    }

    void OnEnterBoxer()
    {
        playerController.m_fAtkComboTimer = 0;
        playerController.m_iAtkCombo++;
        m_fAnimTimer = 0;
        m_iTargetCnt = 0;
        m_fAnimSpeed = playerController.m_sInfo.atkSpeed + playerController.m_sUnitBuff.buffAtkSpd + playerController.m_sPlayerBuff.buffAtkSpd;
        if (playerController.m_iAtkCombo >= 4) playerController.m_iAtkCombo = 1;
        if (playerController.m_iAtkCombo != 3) m_fMaxTimer = 1f / m_fAnimSpeed;
        else m_fMaxTimer = 1.5f / m_fAnimSpeed;
        playerController.ActiveTriggerAnim("Attack", PLAYERSTATE.ATTACK, "AttackSpeed", m_fAnimSpeed, "AttackCombo", playerController.m_iAtkCombo);

        m_iCurAtk = playerController.m_sInfo.atk + playerController.m_sUnitBuff.buffAtk;
        //3Å¸´Â °ø°Ý·Â 1.5¹è
        m_iCurAtk = playerController.m_iAtkCombo == 3 ? (int)(m_iCurAtk * 1.5f) : m_iCurAtk;
    }

    void OnUpdateBoxer()
    {
        m_fAnimTimer += Time.deltaTime;

        if (m_fAnimTimer >= m_fMaxTimer)
        {
            //°ø°ÝÈ÷Æ® ÆÇÁ¤
            m_fAnimTimer = 0;
            if (!m_curTargetEnemy.m_isDead)
            {
                foreach (var target in playerController.m_enemyScan.checkedTargetEnemies)
                {
                    if (target.transform.GetComponent<EnemyFSM>() == m_curTargetEnemy)
                    {
                        m_iTargetCnt++;
                        int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                        NetworkManager.nm.AtkSignal(idx, m_iCurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                        playerController.m_pv.RPC("OnFXRPC", RpcTarget.All, "PLAYERATTACK_Boxer", target.transform.position, playerController.m_iAtkCombo);
                        break;
                    }
                }

                foreach (var target in playerController.m_enemyScan.checkedTargetEnemies)
                {
                    if (m_iTargetCnt >= playerController.m_sInfo.targetCnt) break;
                    if (target.transform.GetComponent<EnemyFSM>() == m_curTargetEnemy) continue;

                    int idx = NetworkManager.nm.m_listEnemyInfo.IndexOf(target.transform.GetComponent<EnemyFSM>());
                    NetworkManager.nm.AtkSignal(idx, m_iCurAtk, NetworkManager.nm.m_myPlayFabInfo.DisplayName);
                    playerController.m_pv.RPC("OnFXRPC", RpcTarget.All, "PLAYERATTACK_Boxer", target.transform.position, playerController.m_iAtkCombo);
                    m_iTargetCnt++;
                }
            }

            playerController.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
        }
    }

    void OnEnterCheer()
    {
        m_iTargetCnt = 0;
        playerController.m_listCheckAlly.Clear();
        playerController.ActiveTriggerAnim("Cheer", PLAYERSTATE.ATTACK);

        //·£´ý À½¾Ç Á¤ÇÏ°í
        int randCheerNum = Random.Range(0, (int)CHEERBGM._MAX_);
        playerController.m_pv.RPC("CheerFXRPC", RpcTarget.All, true, randCheerNum);
    }

    void OnUpdateCheer()
    {
        //ÁÖº¯ ¾Æ±º °Ë»öÇÏ°í ¾Æ±ºÇÑÅ× ¹öÇÁÁÖ±â
        playerController.m_targetAlly = Physics.SphereCastAll(playerController.transform.position, playerController.m_sInfo.atkRange * 0.75f, Vector3.up, 0, playerController.m_targetAllyLayers);

        if (playerController.m_listCheckAlly.Count == 0)
        {
            foreach (var ally in playerController.m_targetAlly)
            {
                //º»ÀÎÀº Á¦¿Ü½ÃÅ²´Ù.
                if (ally.transform.gameObject == playerController.transform.gameObject) continue;
                if (m_iTargetCnt >= playerController.m_sInfo.targetCnt) break;

                CheerAlly(ally);
                m_iTargetCnt++;
                playerController.m_listCheckAlly.Add(ally);
            }
        }

        else
        {
            //»õ·Îµé¾î¿Â ³ð È®ÀÎ ÇØ¾ßµÇ´Âµ¥..
            foreach (var ally in playerController.m_targetAlly)
            {
                if (ally.transform.gameObject == playerController.transform.gameObject) continue;
                if (m_iTargetCnt >= playerController.m_sInfo.targetCnt) break;

                //±âÁ¸¿¡ ¾ø´ø »õ·Î µé¾î¿Â³ðÀÌ¹Ç·Î Á¤º¸¸¦ º¸³½´Ù.
                if (!playerController.m_listCheckAlly.Contains(ally))
                {
                    CheerAlly(ally);
                    m_iTargetCnt++;
                    playerController.m_listCheckAlly.Add(ally);
                }
            }

            List<RaycastHit> listRemoveally = new List<RaycastHit>();

            //¾Æ±ºÀÌ ¹þ¾î³µ´ÂÁö È®ÀÎ¿ë
            foreach (var ally in playerController.m_listCheckAlly)
            {
                //¹öÇÁ ÃÊ±âÈ­
                if (!playerController.m_targetAlly.Contains(ally))
                {
                    NotCheerAlly(ally);
                    m_iTargetCnt--;
                    //playerController.m_listCheckAlly.Remove(ally);
                    listRemoveally.Add(ally);
                }
            }

            foreach (var ally in listRemoveally)
            {
                playerController.m_listCheckAlly.Remove(ally);
            }

            //È¤½Ã³ª ¹öÇÁ ²¨Áø³ðÀÌ ÀÖÀ¸¸é ¹öÇÁ ´Ù½Ã ÄÑÁØ´Ù.
            foreach (var ally in playerController.m_listCheckAlly)
            {
                if (ally.transform.GetComponent<PlayerController>() != null)
                {
                    if (!ally.transform.GetComponent<PlayerController>().m_arrEffects[2].transform.gameObject.activeSelf)
                    {
                        ally.transform.GetComponent<PlayerController>().SetPlayerBuff(playerController.m_sInfo.atkSpeed, playerController.m_sInfo.atkRange / 2f, 2);
                    }
                }

                else if (ally.transform.GetComponent<UnitFSM>() != null)
                {
                    if (!ally.transform.GetComponent<UnitFSM>().m_arrEffects[2].transform.gameObject.activeSelf)
                    {
                        ally.transform.GetComponent<UnitFSM>().SetPlayerBuff(playerController.m_sInfo.atkSpeed, playerController.m_sInfo.atkRange / 2f, 2);
                    }
                }
            }
        }
    }

    void CheerAlly(RaycastHit ally)
    {
        if (ally.transform.GetComponent<PlayerController>() != null)
        {
            ally.transform.GetComponent<PlayerController>().SetPlayerBuff(playerController.m_sInfo.atkSpeed, playerController.m_sInfo.atkRange / 2f, 2);
        }

        else if (ally.transform.GetComponent<UnitFSM>() != null)
        {
            ally.transform.GetComponent<UnitFSM>().SetPlayerBuff(playerController.m_sInfo.atkSpeed, playerController.m_sInfo.atkRange / 2f, 2);
        }

    }

    void NotCheerAlly(RaycastHit ally)
    {
        if (ally.transform.GetComponent<PlayerController>() != null)
        {
            ally.transform.GetComponent<PlayerController>().SetPlayerBuff();
        }

        else if (ally.transform.GetComponent<UnitFSM>() != null)
        {
            ally.transform.GetComponent<UnitFSM>().SetPlayerBuff();
        }
    }

    void ControlJoyStick()
    {
        m_fMoveX = playerController.m_fixedJoy.Horizontal;
        m_fMoveY = playerController.m_fixedJoy.Vertical;

        m_vecMove = new Vector3(m_fMoveX, 0, m_fMoveY);

        if (m_vecMove.sqrMagnitude == 0)
        {
            //if (!playerController.m_anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            //    playerController.ActiveTriggerAnim("ForceIdle", PLAYERSTATE.IDLE);

            return;    //¿òÁ÷ÀÓÀÌ ¾ø´Ù¸é ¸®ÅÏÇØÁà¶ó
        }

        //±×°Ô ¾Æ´Ï¶ó¸é ¿òÁ÷ÀÓÀÌ ÀÖÀ¸´Ï±î runÀ¸·Î ¹Ù²ãÁØ´Ù
        playerController.m_stateMachine.ChangeState(PLAYERSTATE.MOVE);
    }

    void ControlKeyboard()
    {
        m_fMoveX = Input.GetAxisRaw("Horizontal");
        m_fMoveY = Input.GetAxisRaw("Vertical");

        m_vecMove = new Vector3(m_fMoveX, 0, m_fMoveY);

        if (m_vecMove == Vector3.zero)
        {
            //if(!playerController.m_anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            //    playerController.ActiveTriggerAnim("ForceIdle", PLAYERSTATE.IDLE);
            //ÀÌ»óÇÏ°Ô ¸Ø­Ÿ´Ù°¡ ´Ù½Ã ¿òÁ÷ÀÌ´Â Çö»ó ¼öÁ¤ÇØ¾ßµÊ
            return;    //¿òÁ÷ÀÓÀÌ ¾ø´Ù¸é ¸®ÅÏÇØÁà¶ó
        }

        playerController.m_stateMachine.ChangeState(PLAYERSTATE.MOVE);
    }
}
