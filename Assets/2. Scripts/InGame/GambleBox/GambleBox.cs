using Photon.Pun;
using Photon.Realtime;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GambleBox : MonoBehaviourPunCallbacks
{
    public PhotonView m_pv;

    public Animator m_anim;

    public ParticleSystem[] m_arrEffects;

    public List<Dictionary<string, object>> m_listGambleInfo;

    public bool m_isOpen;

    private void Start()
    {
        if (!m_pv.IsMine) return;

        m_listGambleInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.GAMBLEINFO].recordDataList;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") && !m_isOpen)
        {
            if(m_pv.IsMine)
            {
                string name = "";

                if (collision.collider.GetComponent<PlayerController>() != null)
                    name = collision.collider.GetComponent<PlayerController>().m_pv.Owner.NickName;

                Dictionary<string, object> GambleInfo = RandomWeight.RandomItem(m_listGambleInfo);

                int money = int.Parse(GambleInfo["GOLD"].ToString());
                int efIdx = int.Parse(GambleInfo["RANK"].ToString());

                m_pv.RPC("OpenRPC", RpcTarget.All, efIdx);
                //Debug.Log("상자 연 놈 : " + name + " 획득 금액 : " + money.ToString());
            }
        }
    }

    [PunRPC]
    void OpenRPC(int idx)
    {
        m_isOpen = true;
        m_anim.SetTrigger("Open");
        StartCoroutine(OpenBox(idx));
        AudioManager.PlaySfx(SFX.GAMBLE_OPEN);
    }

    IEnumerator OpenBox(int idx)
    {
        yield return new WaitForSeconds(1f);
        m_arrEffects[idx].gameObject.SetActive(true);
        m_arrEffects[idx].Play();
        if (idx != 0) AudioManager.PlaySfx(SFX.GAMBLE_SUCCESS);
        //else 실패했을 때 터지는 효과 넣어야됨

        yield return new WaitForSeconds(2f);

        m_arrEffects[idx].gameObject.SetActive(false);
        m_anim.SetTrigger("Start");
        transform.position = transform.position + new Vector3(0, 3, 0);
        UIManager.um.HideScaleUI(this.gameObject);

        yield return new WaitForSeconds(0.5f);
        m_isOpen = false;
        if (m_pv.IsMine) PhotonNetwork.Destroy(this.gameObject);
    }
}
