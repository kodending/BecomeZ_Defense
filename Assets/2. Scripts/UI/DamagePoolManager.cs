using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class DamagePoolManager : MonoBehaviourPunCallbacks
{
    public static DamagePoolManager dpm;

    public GameObject damagePrefab;

    [SerializeField] Transform m_trCam;

    Queue<GameObject> poolingDamageQueue = new Queue<GameObject>();

    private void Awake()
    {
        dpm = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitCreate(5);
    }

    void InitCreate(int cnt)
    {
        for (int i = 0; i < cnt; i++)
        {
            poolingDamageQueue.Enqueue(CreateNewDamage());
        }
    }

    private void Update()
    {
        transform.LookAt(m_trCam);
    }

    GameObject CreateNewDamage()
    {
        var newDamage = Instantiate(damagePrefab);
        newDamage.transform.SetParent(transform);
        newDamage.transform.localEulerAngles = new Vector3(0, 180, 0);
        newDamage.gameObject.SetActive(false);
        return newDamage;
    }

    public static GameObject GetDamage(string strDmg)
    {
        if (dpm.poolingDamageQueue.Count > 0)
        {
            var obj = dpm.poolingDamageQueue.Dequeue();
            obj.GetComponent<TextMeshPro>().text = strDmg;
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = dpm.CreateNewDamage();
            newObj.GetComponent<TextMeshPro>().text = strDmg;
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static IEnumerator ReturnDamage(GameObject obj, float fTimer = 0.4f)
    {
        yield return new WaitForSeconds(fTimer);

        obj.gameObject.SetActive(false);
        dpm.poolingDamageQueue.Enqueue(obj);
    }
}
