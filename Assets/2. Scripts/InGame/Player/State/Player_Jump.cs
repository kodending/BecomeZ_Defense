using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Jump : PlayerBaseState
{
    [Tooltip("이동 방향 설정")]
    Vector3 m_vecMove;
    float m_fMoveX = 0.0f;
    float m_fMoveY = 0.0f;

    public Player_Jump(PlayerController pc) : base(pc) { }

    public override void OnEnterState()
    {
        if (playerController.m_pv.IsMine)
        {
            if (playerController.m_isRun)
            {
                playerController.ActiveTriggerAnim("RunJump", PLAYERSTATE.JUMP);
            }
            else
            {
                playerController.ActiveTriggerAnim("Jump", PLAYERSTATE.JUMP);
            }

            Vector3 vRunJump = playerController.transform.forward + Vector3.up;

            if (playerController.m_isRun)
                playerController.m_curRigid.AddForce(vRunJump * 5, ForceMode.Impulse);
            else
                playerController.StartCoroutine(LateJump());
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

    IEnumerator LateJump()
    {
        yield return new WaitForSeconds(0.5f);

        playerController.m_curRigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
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

        if(!playerController.m_isRun)
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

        if (!playerController.m_isRun)
            playerController.m_curRigid.MovePosition(playerController.transform.position + playerController.transform.forward
                                                * 1.5f * Time.deltaTime);
    }
}
