using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIInCostume : BaseState
{
    [Header("코스튬 패널정보")]
    GameObject m_go3DCanvas, m_goCostumePanel, m_goSaveLoadPanel, m_goPresetPanel, m_goMyMoneyPanel;
    Button m_btnExit, m_btnSave, m_btnLoad, m_btnPurchase;
    Button[] m_arrBtnPreset;
    Text m_txtMyMoney;

    SAVELOADMODE m_curMode;

    public override void OnEnterState()
    {
        CostumeManager.cm.m_arrButton[0].onClick.AddListener(OnClickHat);
        CostumeManager.cm.m_arrButton[1].onClick.AddListener(OnClickHair);
        CostumeManager.cm.m_arrButton[2].onClick.AddListener(OnClickEyebrow);
        CostumeManager.cm.m_arrButton[3].onClick.AddListener(OnClickGlasses);
        CostumeManager.cm.m_arrButton[4].onClick.AddListener(OnClickMustache);
        CostumeManager.cm.m_arrButton[5].onClick.AddListener(OnClickBackpack);
        CostumeManager.cm.m_arrButton[6].onClick.AddListener(OnClickOutwear);
        CostumeManager.cm.m_arrButton[7].onClick.AddListener(OnClickGlove);
        CostumeManager.cm.m_arrButton[8].onClick.AddListener(OnClickPants);
        CostumeManager.cm.m_arrButton[9].onClick.AddListener(OnClickShoe);
        CostumeManager.cm.m_arrButton[10].onClick.AddListener(OnClickFullBody);
        CostumeManager.cm.m_arrButton[11].onClick.AddListener(OnClickBody);

        foreach (var item in CostumeManager.cm.m_listItems)
        {
            item.GetComponent<Button>().onClick.AddListener(() => OnClickItem(item));
            item.GetComponent<Button>().onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        }

        m_go3DCanvas = GameObject.Find("3DCanvas").gameObject;
        //처음에 활성화될것
        m_goCostumePanel    = m_go3DCanvas.transform.Find("CostumePanel").gameObject;
        m_goCostumePanel.gameObject.SetActive(true);
        m_goSaveLoadPanel   = m_go3DCanvas.transform.Find("SaveLoadPanel").gameObject;
        m_goSaveLoadPanel.gameObject.SetActive(true);
        m_goMyMoneyPanel    = m_go3DCanvas.transform.Find("MyMoneyPanel").gameObject;
        m_goMyMoneyPanel.gameObject.SetActive(true);

        m_txtMyMoney = m_goMyMoneyPanel.transform.Find("MoneyText").GetComponent<Text>();
        m_txtMyMoney.text = CostumeManager.cm.m_strMyMoney;

        UIManager.um.ShowScaleUI(m_goMyMoneyPanel);
        UIManager.um.ShowSlideUI(m_goCostumePanel, UIDIRECTION.LEFT);
        UIManager.um.ShowSlideUI(m_goSaveLoadPanel, UIDIRECTION.RIGHT);

        m_btnSave = m_goSaveLoadPanel.transform.Find("SaveButton").gameObject.GetComponent<Button>();
        m_btnSave.onClick.AddListener(OnClickSave);
        m_btnSave.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_btnLoad = m_goSaveLoadPanel.transform.Find("LoadButton").gameObject.GetComponent<Button>();
        m_btnLoad.onClick.AddListener(OnClickLoad);
        m_btnLoad.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        m_btnPurchase = m_goSaveLoadPanel.transform.Find("PurchaseButton").gameObject.GetComponent<Button>();
        m_btnPurchase.onClick.AddListener(OnClickPurchase);
        m_btnPurchase.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        //비활성화되어있는거
        m_goPresetPanel     = m_go3DCanvas.transform.Find("PresetPanel").gameObject;

        m_arrBtnPreset = m_goPresetPanel.GetComponentsInChildren<Button>();

        foreach(var btn in m_arrBtnPreset)
        {
            btn.onClick.AddListener(() => OnClickPreset(btn));
            btn.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        }

        m_btnExit = m_go3DCanvas.transform.Find("CostumeExitButton").gameObject.GetComponent<Button>();
        m_btnExit.gameObject.SetActive(true);

        UIManager.um.ShowScaleUI(m_btnExit.gameObject);

        m_btnExit.onClick.AddListener(OnClickCostumeExit);
        m_btnExit.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

        m_curMode = SAVELOADMODE.NONE;

        CostumeManager.cm.ActiveItem(COSTUMETYPE.Hat);
    }

    public override void OnUpdateState()
    {

    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        foreach(var btn in CostumeManager.cm.m_arrButton)
        {
            btn.onClick.RemoveAllListeners();
        }

        m_btnExit.onClick.RemoveAllListeners();
        m_btnSave.onClick.RemoveAllListeners();
        m_btnLoad.onClick.RemoveAllListeners();
        m_btnPurchase.onClick.RemoveAllListeners();

        foreach (var btn in m_arrBtnPreset)
        {
            btn.onClick.RemoveAllListeners();
        }

        m_goCostumePanel.SetActive(false);
        m_goPresetPanel.SetActive(false);
        m_goSaveLoadPanel.SetActive(false);
        m_btnExit.gameObject.SetActive(false);
    }

    #   region 코스튬 관련 클릭 함수들
    public void OnClickHat()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Hat);
    }

    public void OnClickHair()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Hair);
    }

    public void OnClickEyebrow()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Eyebrow);
    }

    public void OnClickGlasses()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Glasses);
    }

    public void OnClickMustache()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Mustache);
    }

    public void OnClickBackpack()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Backpack);
    }

    public void OnClickOutwear()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Outerwear);
    }

    public void OnClickGlove()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Glove);
    }

    public void OnClickPants()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Pants);
    }

    public void OnClickShoe()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Shoe);
    }

    public void OnClickFullBody()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.FullBody);
    }

    public void OnClickBody()
    {
        CostumeManager.cm.ActiveItem(COSTUMETYPE.Body);
    }

    public void OnClickItem(GameObject item)
    {
        CostumeManager.cm.ChangeCostume(item);
    }
    #   endregion

    public void OnClickCostumeExit()
    {
        //코스튬 X버튼임
        UIManager.um.m_stateMachine.ChangeState(UISTATE.READY);
    }

    void OnClickSave()
    {
        bool isCheck = false;

        for (int i = 0; i < CostumeManager.cm.m_dicChangeCostume.Count; i++)
        {
            if (CostumeManager.cm.m_dicChangeCostume[COSTUMETYPE.Hat + i] !=
                CostumeManager.cm.m_dicCurCostume[COSTUMETYPE.Hat + i])
            {
                isCheck = true;
                break;
            }
        }

        if(!isCheck)
        {
            UIManager.um.SystemMessage("변경된 사항이 없습니다.");
            return;
        }

        m_curMode = SAVELOADMODE.SAVE;

        UIManager.um.HideSlideUI(m_goSaveLoadPanel, UIDIRECTION.LEFT);

        StartCoroutine(ShowPanel(m_goPresetPanel, UIDIRECTION.RIGHT));
    }

    void OnClickLoad()
    {
        m_curMode = SAVELOADMODE.LOAD;

        UIManager.um.HideSlideUI(m_goSaveLoadPanel, UIDIRECTION.LEFT);

        StartCoroutine(ShowPanel(m_goPresetPanel, UIDIRECTION.RIGHT));
    }

    void OnClickPurchase()
    {
        if(CostumeManager.cm.m_goCurSelectedItem == null)
        {
            UIManager.um.SystemMessage("선택한 아이템이 없습니다.");
        }

        string strKey = "Costume_" + CostumeManager.cm.m_eSelectedCosType.ToString() + "_" + CostumeManager.cm.m_iCurSelectedItemIndex.ToString();

        if (!CostumeManager.cm.m_dicInven.ContainsKey(strKey))
        {
            CostumeManager.cm.m_goPurchasePanel.SetActive(true);
            UIManager.um.ShowScaleUI(CostumeManager.cm.m_goPurchasePanel);
            CostumeManager.cm.m_txtItemPrice.text = CostumeManager.cm.m_dicCatalogItem[strKey].VirtualCurrencyPrices["PG"].ToString();
            CostumeManager.cm.m_strCurItemKey = strKey;
        }
    }

    IEnumerator ShowPanel(GameObject panel, UIDIRECTION eDir)
    {
        yield return new WaitForSeconds(0.3f);

        panel.SetActive(true);
        UIManager.um.ShowSlideUI(panel, eDir);
    }

    #region 프리셋 버튼 함수들
    void OnClickPreset(Button i_curBtn)
    {
        UIManager.um.HideSlideUI(m_goPresetPanel, UIDIRECTION.LEFT);

        StartCoroutine(ShowPanel(m_goSaveLoadPanel, UIDIRECTION.RIGHT));

        List<Dictionary<string, object>> costumeInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.COSTUMEINFO].recordDataList;

        for (int i = 0; i < m_arrBtnPreset.Length; i++)
        {
            Button btn = m_arrBtnPreset[i];
            if(btn == i_curBtn)
            {
                //코스튬을 최신으로 변형시키고 서버 테이블데이터에도 저장한다.
                //또한 로컬 데이터에도 저장시킨다.
                if(m_curMode == SAVELOADMODE.LOAD)
                {
                    for(int j = 0; j < (int)COSTUMETYPE._MAX_; j++)
                    {
                        CostumeManager.cm.m_dicChangeCostume[(COSTUMETYPE.Hat + j)] = int.Parse(costumeInfo[i][(COSTUMETYPE.Hat + j).ToString()].ToString());

                        if (CostumeManager.cm.m_dicChangeCostume[COSTUMETYPE.Hat + j] !=
                            CostumeManager.cm.m_dicCurCostume[COSTUMETYPE.Hat + j])
                        {
                            string strPath = "Costume/" + (COSTUMETYPE.Hat + j).ToString() + "/" + CostumeManager.cm.m_dicChangeCostume[COSTUMETYPE.Hat + j].ToString();

                            GameObject item = Resources.Load<GameObject>(strPath);

                            CostumeManager.cm.LoadMyCostume(item, COSTUMETYPE.Hat + j);
                        }
                    }
                }

                else if (m_curMode == SAVELOADMODE.SAVE)
                {
                    //먼저 로컬 프리셋 저장
                    for (int j = 0; j < (int)COSTUMETYPE._MAX_; j++)
                    {
                         costumeInfo[i][(COSTUMETYPE.Hat + j).ToString()] = CostumeManager.cm.m_dicChangeCostume[(COSTUMETYPE.Hat + j)].ToString();
                    }

                    CSVManager.instance.SaveFile(LOCALDATALOADTYPE.COSTUMEINFO, costumeInfo);

                    //서버 데이터테이블도 그에 맞게 변경
                    NetworkManager.nm.SaveCostumeTitleTable();
                }

                break;
            }
        }

        m_curMode = SAVELOADMODE.NONE;
    }

    #endregion
}
