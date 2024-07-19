using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;

public class ObjectPoolManager : MonoBehaviourPunCallbacks, IPunPrefabPool
{
    static public ObjectPoolManager opm;

    [HideInInspector] 
    public Dictionary<POOLTYPE, List<GameObject>> m_dicPrefabs;

    [SerializeField] private List<GameObject> m_listEnemies;
    [SerializeField] private List<GameObject> m_listPlayers;
    [SerializeField] private List<GameObject> m_listUnits;
    [SerializeField] private List<GameObject> m_listUnitBullets;
    [SerializeField] private List<GameObject> m_listGamebleBox;


    Dictionary<int, Queue<GameObject>> m_dicPoolingEnemy = new Dictionary<int, Queue<GameObject>>();
    Dictionary<int, Queue<GameObject>> m_dicPoolingUnit = new Dictionary<int, Queue<GameObject>>();
    Dictionary<int, Queue<GameObject>> m_dicPoolingUnitBullet = new Dictionary<int, Queue<GameObject>>();
    Queue<GameObject> m_poolingGambleBox = new Queue<GameObject>();

    private void Awake()
    {
        opm = this;
        PhotonNetwork.PrefabPool = this;
        DontDestroyOnLoad(this);
        m_dicPrefabs = new Dictionary<POOLTYPE, List<GameObject>>();
    }

    private void Start()
    {
        InitAddPrefabs();

        for (int i = 0; i < m_listEnemies.Count; i++)
        {
            Queue<GameObject> goQueue = new Queue<GameObject>();
            m_dicPoolingEnemy.Add(i + 1, goQueue);
        }

        for (int i = 0; i < m_listUnits.Count; i++)
        {
            Queue<GameObject> goQueue = new Queue<GameObject>();
            m_dicPoolingUnit.Add(i + 1, goQueue);
        }

        for (int i = 0; i < m_listUnitBullets.Count; i++)
        {
            Queue<GameObject> goQueue = new Queue<GameObject>();
            m_dicPoolingUnitBullet.Add(i + 1, goQueue);
        }
    }
    void InitAddPrefabs()
    {
        m_dicPrefabs.Add(POOLTYPE.ENEMY, m_listEnemies);
        m_dicPrefabs.Add(POOLTYPE.PLAYER, m_listPlayers);
        m_dicPrefabs.Add(POOLTYPE.UNIT, m_listUnits);
        m_dicPrefabs.Add(POOLTYPE.UNITBULLET, m_listUnitBullets);
        m_dicPrefabs.Add(POOLTYPE.GAMBLEBOX, m_listGamebleBox);
    }

    public void Destroy(GameObject gameObject)
    {
        if(gameObject.GetComponent<EnemyFSM>() != null)
        {
            gameObject.SetActive(false);
            int idx = gameObject.GetComponent<EnemyFSM>().m_sInfo.idx;

            m_dicPoolingEnemy[idx].Enqueue(gameObject);
        }

        else if (gameObject.GetComponent<UnitFSM>() != null)
        {
            gameObject.SetActive(false);
            int idx = (int)gameObject.GetComponent<UnitFSM>().m_sInfo.eType;

            m_dicPoolingUnit[idx].Enqueue(gameObject);
        }

        else if (gameObject.GetComponent<UnitBullet>() != null)
        {
            gameObject.SetActive(false);
            int idx = gameObject.GetComponent<UnitBullet>().m_iIndex;

            m_dicPoolingUnitBullet[idx].Enqueue(gameObject);
        }

        else if (gameObject.GetComponent<GambleBox>() != null)
        {
            gameObject.SetActive(false);
            m_poolingGambleBox.Enqueue(gameObject);
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        string[] strInfo = prefabId.Split('_');

        POOLTYPE eCurType = POOLTYPE._MAX_;

        for (int i = 0; i < (int)POOLTYPE._MAX_; i++)
        {
            if((POOLTYPE.PLAYER + i).ToString() == strInfo[0])
            {
                eCurType = POOLTYPE.PLAYER + i;
                break;
            }
        }

        if (eCurType == POOLTYPE._MAX_) return null;

        if (eCurType == POOLTYPE.ENEMY)
        {
            if (m_dicPoolingEnemy[int.Parse(strInfo[1])].Count > 0)
            {
                Queue<GameObject> goQueue = m_dicPoolingEnemy[int.Parse(strInfo[1])];
                var go = goQueue.Dequeue();
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
            else
            {
                foreach (var fab in m_dicPrefabs[eCurType])
                {
                    if (fab.name == strInfo[1])
                    {
                        GameObject go = Instantiate(fab, position, rotation);
                        go.SetActive(false);
                        return go;
                    }
                }
            }
        }

        else if (eCurType == POOLTYPE.UNIT)
        {
            if (m_dicPoolingUnit[int.Parse(strInfo[1])].Count > 0)
            {
                Queue<GameObject> goQueue = m_dicPoolingUnit[int.Parse(strInfo[1])];
                var go = goQueue.Dequeue();
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
            else
            {
                foreach (var fab in m_dicPrefabs[eCurType])
                {
                    if (fab.name == strInfo[1])
                    {
                        GameObject go = Instantiate(fab, position, rotation);
                        go.SetActive(false);
                        return go;
                    }
                }
            }
        }

        else if (eCurType == POOLTYPE.UNITBULLET)
        {
            if (m_dicPoolingUnitBullet[int.Parse(strInfo[1])].Count > 0)
            {
                Queue<GameObject> goQueue = m_dicPoolingUnitBullet[int.Parse(strInfo[1])];
                var go = goQueue.Dequeue();
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
            else
            {
                foreach (var fab in m_dicPrefabs[eCurType])
                {
                    if (fab.name == strInfo[1])
                    {
                        GameObject go = Instantiate(fab, position, rotation);
                        go.SetActive(false);
                        return go;
                    }
                }
            }
        }

        else if (eCurType == POOLTYPE.GAMBLEBOX)
        {
            if (m_poolingGambleBox.Count > 0)
            {
                var go = m_poolingGambleBox.Dequeue();
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
            else
            {
                foreach (var fab in m_dicPrefabs[eCurType])
                {
                    if (fab.name == strInfo[1])
                    {
                        GameObject go = Instantiate(fab, position, rotation);
                        go.SetActive(false);
                        return go;
                    }
                }
            }
        }

        else
        {
            foreach (var fab in m_dicPrefabs[eCurType])
            {
                if (fab.name == strInfo[1])
                {
                    GameObject go = Instantiate(fab, position, rotation);
                    go.SetActive(false);
                    return go;
                }
            }
        }


        return null;
    }
}

