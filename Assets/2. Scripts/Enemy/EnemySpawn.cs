using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviourPunCallbacks
{
    int m_iTotalCount;
    int m_iCurCount;
    int m_iIdx;
    ENEMYSPAWNSPOT m_eSpot;
    bool m_bRandomSpawn;

    float m_fTimer;

    public void SpawnEnemy(int i_iIdx, int i_iCount, float i_fInterval, ENEMYSPAWNSPOT i_eSpot, bool i_bSpawnRandom = false)
    {
        m_iTotalCount = i_iCount;
        m_iCurCount = 0;
        m_eSpot = i_eSpot;
        m_bRandomSpawn = i_bSpawnRandom;
        m_iIdx = i_iIdx;

        InvokeRepeating("Spawn", 0f, i_fInterval);
    }

    void Spawn()
    {
        if (m_iCurCount >= m_iTotalCount)
        {
            SpawnPoolManager.ReturnSpawn(this);
            return;
        }

        string strEnemy = "ENEMY_" + m_iIdx.ToString();
        int rand = Random.Range(0, (int)m_eSpot + 1);

        Vector3 vStartPos = new Vector3();

        if (!m_bRandomSpawn) vStartPos = GameObject.Find(m_eSpot.ToString()).transform.position;
        else vStartPos = GameObject.Find((ENEMYSPAWNSPOT.LeftFlags + rand).ToString()).transform.position;

        GameObject enemy = PhotonNetwork.Instantiate(strEnemy, vStartPos, Quaternion.identity);
        EnemyFSM enemyFSM = enemy.GetComponent<EnemyFSM>();

        enemyFSM.InitParam(m_iIdx - 1, m_bRandomSpawn, rand, (int)m_eSpot);

        m_iCurCount++;
    }
}
