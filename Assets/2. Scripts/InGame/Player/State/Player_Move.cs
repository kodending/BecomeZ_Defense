using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class Player_Move : PlayerBaseState
{
    [Tooltip("이동 방향 설정")]
    Vector3 m_vecMove;
    float m_fMoveX = 0.0f;
    float m_fMoveY = 0.0f;

    public Player_Move(PlayerController pc) : base(pc) { }

    public override void OnEnterState()
    {
        if (playerController.m_pv.IsMine)
        {
            playerController.ActiveTriggerAnim("Run", PLAYERSTATE.MOVE);
        }

        playerController.m_isRun = true;
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
                ControlJoyStick();
                break;
            case PLAYERCONTROL.KEYBOARD:
                ControlKeyboard();
                break;
        }
    }

    public override void OnExitState()
    {

    }

    void ControlJoyStick()
    {
        m_fMoveX = playerController.m_fixedJoy.Horizontal;
        m_fMoveY = playerController.m_fixedJoy.Vertical;

        m_vecMove = new Vector3(m_fMoveX, 0, m_fMoveY);

        if (m_vecMove.sqrMagnitude == 0)
        {
            playerController.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
            return;    //움직임이 없다면 idle 상태로 바꿔줘라
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat")
            {
                playerController.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
                return;
            }

        //if (!playerController.m_anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        //    playerController.ActiveTriggerAnim("ForceRun", PLAYERSTATE.MOVE);

        Vector3 conDirAngle = Quaternion.LookRotation(m_vecMove).eulerAngles;
        Vector3 camPivotAngle = playerController.m_trCam.eulerAngles;

        Vector3 moveAngle = Vector3.up * (conDirAngle.y + camPivotAngle.y);

        playerController.transform.rotation = Quaternion.Euler(moveAngle);

        playerController.m_curRigid.MovePosition(playerController.transform.position + playerController.transform.forward
                                                    * playerController.m_fMoveSpeed * Time.deltaTime);
    }

    void ControlKeyboard()
    {
        m_fMoveX = Input.GetAxisRaw("Horizontal");
        m_fMoveY = Input.GetAxisRaw("Vertical");

        //대각선 이동시 속도 정규화 작업
        m_vecMove = new Vector3(m_fMoveX, 0, m_fMoveY).normalized;

        if (m_vecMove == Vector3.zero)
        {
            playerController.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
            return;
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            if (EventSystem.current.currentSelectedGameObject.name == "InputChat")
            {
                playerController.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
                return;
            }

        //if (!playerController.m_anim.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        //    playerController.ActiveTriggerAnim("ForceRun", PLAYERSTATE.MOVE);

        Vector3 conDirAngle = Quaternion.LookRotation(m_vecMove).eulerAngles;
        Vector3 camPivotAngle = playerController.m_trCam.eulerAngles;

        Vector3 moveAngle = Vector3.up * (conDirAngle.y + camPivotAngle.y);

        playerController.transform.rotation = Quaternion.Euler(moveAngle);

        if (Mathf.Sign(playerController.transform.forward.x) != Mathf.Sign(moveAngle.x) ||
            Mathf.Sign(playerController.transform.forward.z) != Mathf.Sign(moveAngle.z))
        {
            playerController.transform.Rotate(0, 1, 0);
        }

        playerController.m_curRigid.MovePosition(playerController.transform.position + playerController.transform.forward
                                                * playerController.m_fMoveSpeed * Time.deltaTime);
    }
}
