using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualCamSetting : MonoBehaviour
{
    CinemachineVirtualCamera virCam;

    public float scrollSpeed = 2000.0f;
    float InitFieldOfView;

    private void Awake()
    {
        CinemachineCore.GetInputAxis = clickControl;
    }

    private void Start()
    {
        virCam = this.GetComponent<CinemachineVirtualCamera>();
        InitFieldOfView = virCam.m_Lens.FieldOfView;
    }

    public float clickControl(string axis)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return 0;

        float scrollWheel = -Input.GetAxis("Mouse ScrollWheel");

        virCam.m_Lens.FieldOfView += scrollWheel * Time.deltaTime * scrollSpeed;

        if (virCam.m_Lens.FieldOfView > 50) virCam.m_Lens.FieldOfView = InitFieldOfView;
        if (virCam.m_Lens.FieldOfView < 10) virCam.m_Lens.FieldOfView = 10;

        if (Input.GetMouseButton(0))
            return UnityEngine.Input.GetAxis(axis);

        return 0;
    }
}
