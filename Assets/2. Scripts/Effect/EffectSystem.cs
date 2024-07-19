using Photon.Chat.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    [SerializeField]
    ParticleSystem m_ps;

    public void OnStartEffect() => m_ps.Play();

    private void Update()
    {
        if (m_ps.isStopped)
        {
            EffectPoolManager.ReturnEffect(this.gameObject);
            gameObject.SetActive(false);
        }
    }

}
