using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using System.Net.Http.Headers;
using UnityEngine.UIElements;

public class EffectPoolManager : MonoBehaviourPunCallbacks
{
    static EffectPoolManager epm;

    [HideInInspector]
    public Dictionary<EFFECTTYPE, List<GameObject>> m_dicPrefabs = new Dictionary<EFFECTTYPE, List<GameObject>>();

    [SerializeField] private List<GameObject> m_listUnitAttacks;
    Dictionary<string, Queue<GameObject>> m_dicPoolingUnitAttack = new Dictionary<string, Queue<GameObject>>();

    [SerializeField] private List<GameObject> m_listPlayerAttacks;
    Dictionary<string, Queue<GameObject>> m_dicPoolingPlayerAttack = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        epm = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        InitAddPrefabs();

        for (int i = 0; i < m_listUnitAttacks.Count; i++)
        {
            var ef = m_listUnitAttacks[i];
            Queue<GameObject> goQueue = new Queue<GameObject> ();
            m_dicPoolingUnitAttack.Add(ef.name, goQueue);
        }

        for (int i = 0; i < m_listPlayerAttacks.Count; i++)
        {
            var ef = m_listPlayerAttacks[i];
            Queue<GameObject> goQueue = new Queue<GameObject>();
            m_dicPoolingPlayerAttack.Add(ef.name, goQueue);
        }
    }

    void InitAddPrefabs()
    {
        m_dicPrefabs.Add(EFFECTTYPE.UNITATTACK, m_listUnitAttacks);
        m_dicPrefabs.Add(EFFECTTYPE.PLAYERATTACK, m_listPlayerAttacks);
    }

    private GameObject CreateNewEffect(GameObject prefab)
    {
        var newGameObject = Instantiate(prefab).gameObject;
        newGameObject.gameObject.SetActive(false);
        newGameObject.transform.SetParent(transform);
        return newGameObject;
    }

    public static void ReturnEffect(GameObject go)
    {
        string strName = go.name.Substring(0, go.name.LastIndexOf('('));
        string[] strInfo = strName.Split('_');

        EFFECTTYPE eCurType = EFFECTTYPE._MAX_;

        for (int i = 0; i < (int)EFFECTTYPE._MAX_; i++)
        {
            if ((EFFECTTYPE.UNITATTACK + i).ToString() == strInfo[0])
            {
                eCurType = EFFECTTYPE.UNITATTACK + i;
                break;
            }
        }

        switch (eCurType)
        {
            case EFFECTTYPE.UNITATTACK:
                go.gameObject.SetActive(false);
                epm.m_dicPoolingUnitAttack[strName].Enqueue(go);
                break;
        }
    }

    public static GameObject GetEffect(string effectName)
    {
        string[] strInfo = effectName.Split('_');

        EFFECTTYPE eCurType = EFFECTTYPE._MAX_;

        for (int i = 0; i < (int)EFFECTTYPE._MAX_; i++)
        {
            if ((EFFECTTYPE.UNITATTACK + i).ToString() == strInfo[0])
            {
                eCurType = EFFECTTYPE.UNITATTACK + i;
                break;
            }
        }

        switch(eCurType)
        {
            case EFFECTTYPE.UNITATTACK:
                if (epm.m_dicPoolingUnitAttack[effectName].Count > 0)
                {
                    Queue<GameObject> goQueue = epm.m_dicPoolingUnitAttack[effectName];
                    var go = goQueue.Dequeue();
                    return go;
                }
                else
                {
                    foreach (var fab in epm.m_dicPrefabs[eCurType])
                    {
                        if (fab.name == effectName)
                        {
                            GameObject go = epm.CreateNewEffect(fab);
                            go.SetActive(false);
                            return go;
                        }
                    }
                }
                break;

            case EFFECTTYPE.PLAYERATTACK:
                if (epm.m_dicPoolingPlayerAttack[effectName].Count > 0)
                {
                    Queue<GameObject> goQueue = epm.m_dicPoolingPlayerAttack[effectName];
                    var go = goQueue.Dequeue();
                    return go;
                }
                else
                {
                    foreach (var fab in epm.m_dicPrefabs[eCurType])
                    {
                        if (fab.name == effectName)
                        {
                            GameObject go = epm.CreateNewEffect(fab);
                            go.SetActive(false);
                            return go;
                        }
                    }
                }
                break;
        }

        return null;
    }
}
