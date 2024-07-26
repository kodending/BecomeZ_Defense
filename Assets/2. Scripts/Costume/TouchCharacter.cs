using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TouchCharacter : MonoBehaviour
{
    [SerializeField]
    Camera m_3dCam;

    RaycastHit hit;

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = m_3dCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if(hit.collider.name == "Character")
                {
                    transform.Rotate(0f, -Input.GetAxis("Mouse X") * 30f, 0f);
                }
            }
        }
    }
}
