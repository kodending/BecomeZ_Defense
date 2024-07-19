using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPassEnemy : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (PhotonNetwork.IsMasterClient)
                NetworkManager.nm.PV.RPC("PassEnemyRPC", RpcTarget.All);

            other.GetComponent<EnemyFSM>().m_pv.RPC("PassRPC", RpcTarget.All);
        }
    }
}
