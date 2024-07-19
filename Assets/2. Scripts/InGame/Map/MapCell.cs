using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class MapCell : MonoBehaviourPunCallbacks
{
    public GameObject m_goPointFloor;

    public bool m_isUnit;

    public bool m_isUser;

    private void OnMouseDown()
    {
        MouseDownCraftMode();
        MouseDownGambleMode();
    }

    private void OnMouseOver()
    {
        if (!GameManager.gm.m_bCraftMode && !GameManager.gm.m_bGambleMode) return;
        if (EventSystem.current.IsPointerOverGameObject())
            m_goPointFloor.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (!GameManager.gm.m_bCraftMode && !GameManager.gm.m_bGambleMode) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (gameObject.layer != LayerMask.NameToLayer("Floor")) return;

        m_goPointFloor.SetActive(true);
    }

    private void OnMouseExit()
    {
        m_goPointFloor.SetActive(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((collision.collider.gameObject.layer == LayerMask.NameToLayer("Unit") &&
            !m_isUnit) ||
            (collision.collider.gameObject.layer == LayerMask.NameToLayer("GambleBox") &&
            !m_isUnit))
        {
            m_isUnit = true;
        }

        if ((collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") &&
            !m_isUser) ||
            (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") &&
            !m_isUser))
        {
            m_isUser = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Unit") ||
            collision.collider.gameObject.layer == LayerMask.NameToLayer("GambleBox"))
        {
            m_isUnit = false;
        }

        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") ||
            collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            m_isUser = false;
        }
    }

    void MouseDownCraftMode()
    {
        if (!GameManager.gm.m_bCraftMode) return;

        GameManager.gm.m_bCraftMode = false;

        if (GameManager.gm.m_iUnitCount >= GameManager.gm.m_iLimitUnit)
        {
            UIManager.um.SystemMessage("유닛은 유저당 3개까지 가능");
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (gameObject.layer != LayerMask.NameToLayer("Floor"))
        {
            UIManager.um.SystemMessage("설치할 수 없는 곳입니다.");
            return;
        }

        if (m_isUnit || m_isUser)
        {
            UIManager.um.SystemMessage("이미 무언가 설치되어 있습니다.");
            return;
        }

        if (!GameManager.gm.CostCheckToCal("UNITCRAFT"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            return;
        }

        GameManager.gm.GenerateUnits(this.transform);
    }

    void MouseDownGambleMode()
    {
        if (!GameManager.gm.m_bGambleMode) return;

        GameManager.gm.m_bGambleMode = false;
        UnitPanelManager.upm.m_goReturnPanel.SetActive(false);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (gameObject.layer != LayerMask.NameToLayer("Floor"))
        {
            UIManager.um.SystemMessage("설치할 수 없는 곳입니다.");
            return;
        }

        if (m_isUnit || m_isUser)
        {
            UIManager.um.SystemMessage("이미 무언가 설치되어 있습니다.");
            return;
        }

        if (!GameManager.gm.CostCheckToCal("GAMBLE"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            return;
        }

        GameManager.gm.GenerateGambleBox(this.transform);
    }
}
