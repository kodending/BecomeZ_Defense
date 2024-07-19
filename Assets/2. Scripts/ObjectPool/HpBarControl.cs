using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarControl : MonoBehaviourPunCallbacks
{
    public Transform m_trTarget;

    public Slider m_sdHpBar;
    public Slider m_sdHpBarBack;

    private void Update()
    {
        if(m_trTarget != null)
            transform.position = m_trTarget.position;
    }
}
