using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public void OnFinishedAttack()
    {
        GameManager.gm.m_pcLocal.m_stateMachine.ChangeState(PLAYERSTATE.IDLE);
    }
}
