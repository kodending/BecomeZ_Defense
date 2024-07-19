using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HpBarPoolManager : MonoBehaviourPunCallbacks
{
    public static HpBarPoolManager hppm;

    public GameObject m_goHpPrefab;

    Queue<GameObject> poolingHpQueue = new Queue<GameObject>();

    private void Awake()
    {
        hppm = this;
    }

    GameObject CreateNewHpBar()
    {
        var newHpBar = Instantiate(m_goHpPrefab);
        newHpBar.transform.SetParent(transform);
        newHpBar.gameObject.SetActive(false);
        return newHpBar;
    }

    public static GameObject GetHpBar(Transform trTarget)
    {
        if (hppm.poolingHpQueue.Count > 0)
        {
            var obj = hppm.poolingHpQueue.Dequeue();
            obj.GetComponent<HpBarControl>().m_trTarget = trTarget.GetComponent<EnemyFSM>().m_sdHpBarPos;
            obj.GetComponent<HpBarControl>().transform.position = trTarget.transform.position;
            obj.gameObject.SetActive(false);
            return obj;
        }
        else
        {
            var newObj = hppm.CreateNewHpBar();
            newObj.GetComponent<HpBarControl>().m_trTarget = trTarget.GetComponent<EnemyFSM>().m_sdHpBarPos;
            newObj.GetComponent<HpBarControl>().transform.position = trTarget.transform.position;
            return newObj;
        }
    }

    public static void ReturnHpBar(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        hppm.poolingHpQueue.Enqueue(obj);
    }
}
