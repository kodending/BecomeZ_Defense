using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator mg;

    [SerializeField]
    private int m_iMapWidth;

    [SerializeField]
    MapCell m_PrefabCell;

    [SerializeField]
    private int m_iMapHeight;

    private MapCell[,] m_arrMapGrid;

    private float m_fPosLBX;
    private float m_fPosLBZ;

    private void Awake()
    {
        mg = this;
        DontDestroyOnLoad(mg);
    }

    public void GenerateMap()
    {
        m_arrMapGrid = new MapCell[m_iMapWidth, m_iMapHeight];

        for (int idxX = 0; idxX < m_iMapWidth; idxX++)
        {
            for (int idxZ = 0; idxZ < m_iMapHeight; idxZ++)
            {
                m_arrMapGrid[idxX, idxZ] = Instantiate(m_PrefabCell, new Vector3(m_fPosLBX + (idxX * 2), 0, m_fPosLBZ + (idxZ * 2)), Quaternion.identity);
                m_arrMapGrid[idxX, idxZ].transform.SetParent(this.transform);
            }
        }
    }

    public void InitCheck()
    {
        for (int idxX = 0; idxX < m_iMapWidth; idxX++)
        {
            for (int idxZ = 0; idxZ < m_iMapHeight; idxZ++)
            {
                m_arrMapGrid[idxX, idxZ].m_isUnit = false;
                m_arrMapGrid[idxX, idxZ].m_isUser = false;
            }
        }
    }
}
