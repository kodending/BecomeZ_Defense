using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPoolManager : MonoBehaviourPunCallbacks
{
    public static RoomPoolManager rpm;

    public GameObject m_goRoomPrefab;

    public Transform m_parent;

    Queue<GameObject> poolingRoomQueue = new Queue<GameObject>();

    private void Awake()
    {
        rpm = this;
    }
    GameObject CreateNewHpBar()
    {
        var newRoom = Instantiate(m_goRoomPrefab);
        newRoom.transform.SetParent(m_parent);
        newRoom.GetComponent<RectTransform>().localPosition = new Vector3(newRoom.GetComponent<RectTransform>().localPosition.x, newRoom.GetComponent<RectTransform>().localPosition.y, 0); ;
        newRoom.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        newRoom.gameObject.SetActive(false);
        return newRoom;
    }

    public static GameObject GetRoom()
    {
        if (rpm.poolingRoomQueue.Count > 0)
        {
            var obj = rpm.poolingRoomQueue.Dequeue();
            obj.gameObject.SetActive(false);
            return obj;
        }
        else
        {
            var newObj = rpm.CreateNewHpBar();
            return newObj;
        }
    }

    public static void ReturnRoom(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        rpm.poolingRoomQueue.Enqueue(obj);
    }
}
