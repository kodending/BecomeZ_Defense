using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoving : MonoBehaviour
{
    public static CameraMoving cm;

    private GameObject m_goTarget;

    [SerializeField]
    private float m_fSpeed;

    [SerializeField]
    private Vector3 m_vDifValue;

    private void Awake()
    {
        cm = this;
    }
    
    private void Update()
    {
        if(m_goTarget != null)
            this.transform.position = Vector3.Lerp(this.transform.position, m_goTarget.transform.position + m_vDifValue, m_fSpeed);
    }

    public void SetTarget(GameObject i_goTarget)
    {
        m_goTarget = i_goTarget;

        this.transform.position = m_goTarget.transform.position + m_vDifValue;
    }
}
