using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoolManager : MonoBehaviourPunCallbacks
{
    public static SpawnPoolManager spm;

    public GameObject spawnPrefab;

    Queue<EnemySpawn> poolingSpawnQueue = new Queue<EnemySpawn>();

    private void Awake()
    {
        spm = this;
        DontDestroyOnLoad(spm);
    }

    private void Init(int InitCnt)
    {
        for(int i = 0; i< InitCnt; i++)
        {
            poolingSpawnQueue.Enqueue(CreateNewSpawn());
        }
    }

    private EnemySpawn CreateNewSpawn()
    {
        var newSpawn = Instantiate(spawnPrefab).GetComponent<EnemySpawn>();
        newSpawn.transform.SetParent(GameObject.Find("SpawnPool").transform);
        newSpawn.gameObject.SetActive(false);
        return newSpawn;
    }

    public static EnemySpawn GetSpawn()
    {
        if(spm.poolingSpawnQueue.Count > 0)
        {
            var obj = spm.poolingSpawnQueue.Dequeue();
            obj.transform.SetParent(GameObject.Find("SpawnPool").transform);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = spm.CreateNewSpawn();
            newObj.transform.SetParent(GameObject.Find("SpawnPool").transform);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static void ReturnSpawn(EnemySpawn obj)
    {
        obj.gameObject.SetActive(false);
        spm.poolingSpawnQueue.Enqueue(obj);
    }
}
