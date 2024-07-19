using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class Player_Idle : PlayerBaseState
{
    [Tooltip("¿Ãµø πÊ«‚ º≥¡§")]
    Vector3 m_vecMove;
    float m_fMoveX = 0.0f;
    float m_fMoveY = 0.0f;

    public Player_Idle(PlayerController pc) : base(pc) { }

    public override void OnEnterState()
    {
        if (playerController.m_pv.IsMine)
        {
            playerController.ActiveTriggerAnim("Idle", PLAYERSTATE.IDLE);
        }

        playerController.m_curRigid.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        playerController.m_curRigid.velocity = Vector3.zero;
        playerController.m_isRun = false;
    }

    public override void OnUpdateState()
    {
        if (!playerController.m_pv.IsMine) return;

        if (playerController.m_bJDown && !playerController.m_isJumping)
        {
            playerController.m_isJumping = true;
            playerController.m_stateMachine.ChangeState(PLAYERSTATE.JUMP);
        }
    }

    public override void OnFixedUpdateState()
    {
        if (!playerController.m_pv.IsMine) return;

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

            return;    //øÚ¡˜¿”¿Ã æ¯¥Ÿ∏È ∏Æ≈œ«ÿ¡‡∂Û
        }

        //±◊∞‘ æ∆¥œ∂Û∏È øÚ¡˜¿”¿Ã ¿÷¿∏¥œ±Ó run¿∏∑Œ πŸ≤„¡ÿ¥Ÿ
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
            //¿ÃªÛ«œ∞‘ ∏ÿ≠ü¥Ÿ∞° ¥ŸΩ√ øÚ¡˜¿Ã¥¬ «ˆªÛ ºˆ¡§«ÿæﬂµ 
            return;    //øÚ¡˜¿”¿Ã æ¯¥Ÿ∏È ∏Æ≈œ«ÿ¡‡∂Û
        }

        playerController.m_stateMachine.ChangeState(PLAYERSTATE.MOVE);
    }
}
