using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFactory : MonoBehaviourPunCallbacks
{
    public static EnemyFactory ef;

    public List<Dictionary<string, object>> m_enemyInfo;

    private void Awake()
    {
        ef = this;
        DontDestroyOnLoad(ef);
    }

    private void Start()
    { 
        m_enemyInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.ENEMYINFO].recordDataList;
    }

    public void SpawnEnemies(int i_iIdx, int i_iCount, float i_fInterval, ENEMYSPAWNSPOT i_eSpot, bool i_bSpawnRandom = false)
    {
        //클래스를 새로 할당하고
        var enemySpawn = SpawnPoolManager.GetSpawn();

        enemySpawn.SpawnEnemy(i_iIdx, i_iCount, i_fInterval, i_eSpot, i_bSpawnRandom);
    }
}

