using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class FreeLookSetting : MonoBehaviour
{
    CinemachineFreeLook freeLook;

    public float scrollSpeed = 2000.0f;
    float InitFieldOfView;

    bool isMouseOverUI;

    [SerializeField]
    Transform m_trNormalTarget, m_trCraftTarget;

    private void Awake()
    {
        //CinemachineCore.GetInputAxis = clickControl;
    }

    private void Start()
    {
        freeLook = this.GetComponent<CinemachineFreeLook>();
        InitFieldOfView = freeLook.m_Lens.FieldOfView;
    }

    private void Update()
    {
        if(GameManager.gm.m_bCraftMode || GameManager.gm.m_bUpUnitMode || GameManager.gm.m_bEvolUnitMode || GameManager.gm.m_bGambleMode)
        {
            if(freeLook.LookAt != m_trCraftTarget)
            {
                freeLook.LookAt = m_trCraftTarget;
                freeLook.Follow = m_trCraftTarget;
            }
        }
        else
        {
            if (freeLook.LookAt != m_trNormalTarget)
            {
                freeLook.LookAt = m_trNormalTarget;
                freeLook.Follow = m_trNormalTarget;
            }
        }
    }

    public float clickControl(string axis)
    {
        float scrollWheel = -Input.GetAxis("Mouse ScrollWheel");

        freeLook.m_Lens.FieldOfView += scrollWheel * Time.deltaTime * scrollSpeed;

        if (freeLook.m_Lens.FieldOfView > 150) freeLook.m_Lens.FieldOfView = 150;
        if (freeLook.m_Lens.FieldOfView < 10) freeLook.m_Lens.FieldOfView = 10;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                isMouseOverUI = true;
            else isMouseOverUI = false;
        }

        if (isMouseOverUI) return 0;

        if (Input.GetMouseButton(0))
            return UnityEngine.Input.GetAxis(axis);

        return 0;
    }
}
