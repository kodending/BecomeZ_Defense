using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanelManager : MonoBehaviourPunCallbacks
{
    static public UnitPanelManager upm;

    public GameObject m_goUnitPanel;

    public GameObject m_goReturnPanel;

    public Text m_txtCraftCost;
    public Text m_txtUpCost;
    public Text m_txtEvoCost;

    List<Dictionary<string, object>> m_costInfo;

    private void Awake()
    {
        upm = this;
    }

    private void Start()
    {
        m_costInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.COSTINFO].recordDataList;

        m_txtCraftCost.text = m_costInfo[0]["UNITCRAFT"].ToString();
        m_txtUpCost.text = m_costInfo[0]["UNITUPGRADE"].ToString();
        m_txtEvoCost.text = m_costInfo[0]["UNITEVOLUTION"].ToString();
    }

    public void OnClickExitButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        m_goUnitPanel.SetActive(false);
    }

    public void OnClickCraftButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        m_goUnitPanel.SetActive(false);
        GameManager.gm.m_bCraftMode = true;
    }

    public void OnClickUpUnitButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if(GameManager.gm.m_listMyUnits.Count == 0)
        {
            UIManager.um.SystemMessage("설치된 유닛이 없습니다.");
            return;
        }

        m_goUnitPanel.SetActive(false);
        m_goReturnPanel.SetActive(true);
        GameManager.gm.m_bUpUnitMode = true;
    }

    public void OnClickEvolutionButton()
    {
        if (GameManager.gm.m_listMyUnits.Count == 0)
        {
            UIManager.um.SystemMessage("설치된 유닛이 없습니다.");
            return;
        }

        m_goUnitPanel.SetActive(false);
        m_goReturnPanel.SetActive(true);
        GameManager.gm.m_bEvolUnitMode = true;
        AudioManager.PlaySfx(SFX.BUTTON);
    }

    public void OnClickReturnButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        foreach (var unit in GameManager.gm.m_listMyUnits)
        {
            unit.m_goSelectEffect.SetActive(false);
        }

        m_goReturnPanel.SetActive(false);
        GameManager.gm.m_bUpUnitMode = false;
        GameManager.gm.m_bEvolUnitMode = false;
        GameManager.gm.m_bGambleMode = false;
    }
}
