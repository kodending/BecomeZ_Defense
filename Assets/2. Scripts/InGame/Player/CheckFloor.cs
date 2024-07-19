using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFloor : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private PlayerController m_pc;

    private void OnTriggerEnter(Collider other)
    {
        if (!m_pc.m_pv.IsMine) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Floor")
            && m_pc.m_curState == PLAYERSTATE.FALLING)
        {
            if (m_pc.m_isRun) m_pc.ActiveBoolAnim("isRunFalling", PLAYERSTATE.LANDING, false);

            else m_pc.ActiveBoolAnim("isFalling", PLAYERSTATE.LANDING, false);
            
            m_pc.StartCoroutine(m_pc.ChangeIdle());
        }
    }
}
