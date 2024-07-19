using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Falling : PlayerBaseState
{
    [Tooltip("이동 방향 설정")]
    Vector3 m_vecMove;
    float m_fMoveX = 0.0f;
    float m_fMoveY = 0.0f;

    public Player_Falling(PlayerController pc) : base(pc) { }

    public override void OnEnterState()
    {
        if (playerController.m_pv.IsMine)
        {
            if (playerController.m_isRun)
            {
                playerController.ActiveTriggerAnim("RunFalling", PLAYERSTATE.FALLING);
                playerController.ActiveBoolAnim("isRunFalling", PLAYERSTATE.FALLING, true);
            }
            else
            {
                playerController.ActiveTriggerAnim("Falling", PLAYERSTATE.FALLING);
                playerController.ActiveBoolAnim("isFalling", PLAYERSTATE.FALLING, true);
            }
        }
    }

    public override void OnUpdateState()
    {

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

        if (m_vecMove.sqrMagnitude == 0) return;

        Vector3 conDirAngle = Quaternion.LookRotation(m_vecMove).eulerAngles;
        Vector3 camPivotAngle = playerController.m_trCam.eulerAngles;

        Vector3 moveAngle = Vector3.up * (conDirAngle.y + camPivotAngle.y);

        playerController.transform.rotation = Quaternion.Euler(moveAngle);


        playerController.m_curRigid.MovePosition(playerController.transform.position + playerController.transform.forward
                                                    * 1.5f * Time.deltaTime);
    }

    void ControlKeyboard()
    {
        m_fMoveX = Input.GetAxisRaw("Horizontal");
        m_fMoveY = Input.GetAxisRaw("Vertical");

        //대각선 이동시 속도 정규화 작업
        m_vecMove = new Vector3(m_fMoveX, 0, m_fMoveY).normalized;

        if (m_vecMove.sqrMagnitude == 0) return;

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
                                                * 1.5f * Time.deltaTime);
    }
}
