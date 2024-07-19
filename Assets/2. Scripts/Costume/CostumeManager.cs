using ExitGames.Client.Photon;
using JetBrains.Annotations;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CostumeManager : MonoBehaviour
{
    public static CostumeManager cm;

    [SerializeField]
    GameObject m_goCosCharacter;

    [HideInInspector]
    public Animator m_anim;

    [HideInInspector]
    public COSTUMETYPE m_eCurCosType;

    public GameObject[] m_listItems;

    public Button[] m_arrButton;

    [SerializeField]
    RectTransform m_rtrContent;

    [HideInInspector]
    public Dictionary<COSTUMETYPE, int> m_dicCurCostume;

    [HideInInspector]
    public Dictionary<COSTUMETYPE, int> m_dicChangeCostume;

    [HideInInspector]
    public Dictionary<string, ItemInstance> m_dicInven;

    [HideInInspector]
    public Dictionary<string, CatalogItem> m_dicCatalogItem;

    [HideInInspector]
    public string m_strMyMoney;

    [HideInInspector]
    public GameObject m_goPurchasePanel;
    [HideInInspector]
    public Button m_btnYes, m_btnNo;
    [HideInInspector]
    public Text m_txtItemPrice;
    [HideInInspector]
    public string m_strCurItemKey;
    [HideInInspector]
    public Text m_txtCurMoney;
    [HideInInspector]
    public GameObject   m_goCurSelectedItem;
    [HideInInspector]
    public int          m_iCurSelectedItemIndex;
    [HideInInspector]
    public COSTUMETYPE  m_eSelectedCosType;

    private void Awake()
    {
        cm = this;
        DontDestroyOnLoad(cm);
    }

    private void Start()
    {
        m_anim = m_goCosCharacter.GetComponent<Animator>();

        m_eCurCosType = COSTUMETYPE._MAX_;

        m_rtrContent.anchoredPosition = new Vector2(0, 0);

        m_dicCurCostume = new Dictionary<COSTUMETYPE, int>();
        m_dicChangeCostume = new Dictionary<COSTUMETYPE, int>();

        m_dicCatalogItem = new Dictionary<string, CatalogItem>();

        m_dicInven = new Dictionary<string, ItemInstance>();

        m_goPurchasePanel = GameObject.Find("3DCanvas").transform.Find("PurchasePanel").gameObject;

        m_btnYes = m_goPurchasePanel.transform.Find("YesButton").GetComponent<Button>();
        m_btnYes.onClick.AddListener(OnClickPurchaseYes);
        m_btnNo = m_goPurchasePanel.transform.Find("NoButton").GetComponent<Button>();
        m_btnNo.onClick.AddListener(OnClickPurchaseNo);

        m_txtItemPrice = m_goPurchasePanel.transform.Find("MoneyText").GetComponent<Text>();

        m_txtCurMoney = GameObject.Find("3DCanvas").transform.Find("MyMoneyPanel").transform.Find("MoneyText").GetComponent<Text>();
    }

    private void DisabledChildRecursively(GameObject go)
    {
        go.SetActive(false);

        foreach (Transform child in go.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(m_rtrContent != null && m_rtrContent.anchoredPosition.y < 0)
        {
            m_rtrContent.anchoredPosition = new Vector2(0, 0);
            m_rtrContent.parent.parent.GetComponent<ScrollRect>().StopMovement();
        }
    }

    public void ChangeAction(string i_strAction)
    {
        m_anim.SetTrigger(i_strAction);
    }

    public void ActiveItem(COSTUMETYPE eType, bool onClick = true)
    {
        if (m_eCurCosType == eType && onClick) return;

        m_eCurCosType = eType;

        foreach(var btn in m_arrButton)
        {
            btn.image.color = Color.white;
        }

        foreach(var item in m_listItems)
        {
            if(item.activeSelf)
            {
                DisabledChildRecursively(item);
            }
        }

        for(int i = 0; i < m_listItems.Length; i++)
        {
            var item = m_listItems[i];

            if (item.name == "InitItem")
            {
                item.SetActive(true);
                Image moneyImg = item.transform.Find("CashImg").GetComponent<Image>();
                moneyImg.gameObject.SetActive(false);
                Text moneyText = item.transform.Find("MoneyText").GetComponent<Text>();
                moneyText.gameObject.SetActive(false);
                Image img = item.transform.Find("Image").GetComponent<Image>();
                img.gameObject.SetActive(true);
            }

            if (item.transform.Find(eType.ToString()) != null)
            {
                item.SetActive(true);
                item.transform.Find(eType.ToString()).gameObject.SetActive(true);
                item.transform.Find("LockImg").gameObject.SetActive(true);
                m_arrButton[(int)eType].image.color = Color.green;

                string strKey = "Costume_" + eType.ToString() + "_" + i.ToString();

                Image moneyImg = item.transform.Find("CashImg").GetComponent<Image>();
                moneyImg.gameObject.SetActive(true);

                //가격표시
                Text moneyTxt = item.transform.Find("MoneyText").GetComponent<Text>();
                moneyTxt.gameObject.SetActive(true);
                moneyTxt.text = m_dicCatalogItem[strKey].VirtualCurrencyPrices["PG"].ToString();


                //여기서 아이템있는지 검사하고 있으면 잠금 비활성화
                if (m_dicInven.Count > 0)
                {
                    if(m_dicInven.ContainsKey(strKey))
                    {
                        item.transform.Find("LockImg").gameObject.SetActive(false);
                    }
                }
            }
        }

        if(m_eCurCosType == COSTUMETYPE.Body)
        {
            m_listItems[0].SetActive(false);
            m_listItems[0].transform.Find("Image").gameObject.SetActive(false);
        }

        m_rtrContent.anchoredPosition = new Vector2(0, 0);
        m_rtrContent.parent.parent.GetComponent<ScrollRect>().StopMovement();
    }

    public void ChangeCostume(GameObject item)
    {
        GameObject charItem = m_goCosCharacter.transform.Find(m_eCurCosType.ToString()).gameObject;

        if (item.name == "InitItem")
        {
            charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;

            m_dicChangeCostume[m_eCurCosType] = 0;

            return;
        }

        GameObject childItem = item.transform.Find(m_eCurCosType.ToString()).gameObject;

        //검증하기
        //클릭한 아이템이 있고 가지고있는게 아니라면 구매창 띄우기
        int itemIdx = 0;
        for (; itemIdx < m_listItems.Length; itemIdx++)
        {
            if (m_listItems[itemIdx] == item)
            {
                break;
            }
        }
        
        string strKey = "Costume_" + m_eCurCosType.ToString() + "_" + itemIdx.ToString();

        if (m_dicInven.Count > 0)
        {
            //아이템을 가지고 있지 않다면
            if (!m_dicInven.ContainsKey(strKey))
            {
                //원래 착용했었던 아이템을 원래대로 돌려놔야함
                if(m_goCurSelectedItem != null)
                {
                    RestoreCostume();
                }
                
                m_goCurSelectedItem = item;
                m_iCurSelectedItemIndex = itemIdx;
                m_eSelectedCosType = m_eCurCosType;

                GameObject changeItem = null;

                foreach (Transform child in childItem.transform)
                {
                    if (child.gameObject.name != "rig")
                    {
                        changeItem = child.gameObject;
                    }
                }

                if (changeItem != null)
                    charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                        changeItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;

                m_dicChangeCostume[m_eCurCosType] = itemIdx;
            }

            //아이템을 가지고 있다면
            else
            {
                if(m_goCurSelectedItem != null)
                {
                    //원래 착용했었던 아이템은 원래대로 돌려놔야함
                    RestoreCostume();

                    m_goCurSelectedItem = null;
                    m_iCurSelectedItemIndex = 0;
                    m_eSelectedCosType = COSTUMETYPE._MAX_;
                }

                GameObject changeItem = null;

                foreach (Transform child in childItem.transform)
                {
                    if (child.gameObject.name != "rig")
                    {
                        changeItem = child.gameObject;
                    }
                }

                if (changeItem != null)
                    charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                        changeItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;

                m_dicChangeCostume[m_eCurCosType] = itemIdx;
            }
        }
    }

    public void InitLoadMyCostume()
    {
        if (m_dicCurCostume.Count == 0)
        {
            //오류 메세지 띄운다.
            return;
        }

        //코스튬 정보에 따른 리소스를 가져온다.
        for (int i = 0; i < m_dicCurCostume.Count; i++)
        {
            if (m_dicCurCostume[(COSTUMETYPE.Hat + i)] == 0)
            {
                GameObject charItem = m_goCosCharacter.transform.Find((COSTUMETYPE.Hat + i).ToString()).gameObject;
                charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
                continue;
            }

            string strDir = "Costume" + "/" + (COSTUMETYPE.Hat + i).ToString() + "/" + m_dicCurCostume[COSTUMETYPE.Hat + i].ToString();
            GameObject costume = Resources.Load<GameObject>(strDir);

            //해당 리소스로 코스튬을 변경시킨다.
            LoadMyCostume(costume, COSTUMETYPE.Hat + i);
        }

    }

    public void LoadMyCostume(GameObject item, COSTUMETYPE i_curType)
    {
        GameObject charItem = m_goCosCharacter.transform.Find(i_curType.ToString()).gameObject;

        GameObject changeItem = item.transform.Find(i_curType.ToString()).gameObject;

        if (changeItem != null)
            charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                changeItem.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void RestoreCostume()
    {
        if (m_dicCurCostume[m_eSelectedCosType] == 0)
        {
            GameObject charItem = m_goCosCharacter.transform.Find(m_eSelectedCosType.ToString()).gameObject;
            charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
            return;
        }

        string strPath = "Costume/" + m_eSelectedCosType.ToString() + "/" + m_dicCurCostume[m_eSelectedCosType].ToString();

        GameObject costume = Resources.Load<GameObject>(strPath);

        LoadMyCostume(costume, m_eSelectedCosType);
    }

    public void ChangeMyCostume()
    {
        if (m_dicCurCostume.Count == 0)
        {
            //오류 메세지 띄운다.
            return;
        }

        //코스튬 정보에 따른 리소스를 가져온다.
        for (int i = 0; i < m_dicCurCostume.Count; i++)
        {
            if (m_dicCurCostume[(COSTUMETYPE.Hat + i)] == 0)
            {
                GameObject charItem = m_goCosCharacter.transform.Find((COSTUMETYPE.Hat + i).ToString()).gameObject;
                charItem.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
                continue;
            }

            string strDir = "Costume" + "/" + (COSTUMETYPE.Hat + i).ToString() + "/" + m_dicCurCostume[COSTUMETYPE.Hat + i].ToString();
            GameObject costume = Resources.Load<GameObject>(strDir);

            //해당 리소스로 코스튬을 변경시킨다.
            LoadMyCostume(costume, COSTUMETYPE.Hat + i);
        }
    }

    public void RefreshInven()
    {
        NetworkManager.nm.GetInventory();
    }

    void OnClickPurchaseYes()
    {
        UIManager.um.HideScaleUI(m_goPurchasePanel);

        if (int.Parse(m_strMyMoney) < int.Parse(m_txtItemPrice.text))
        {
            UIManager.um.SystemMessage("돈이 부족혀;;");
            m_strCurItemKey = "";
            return;
        }

        //돈이 부족한게 아니니께
        //돈을 차감하고 인벤토리에 해당 아이템 들어오게한다.
        NetworkManager.nm.PurchaseItem(m_strCurItemKey, int.Parse(m_txtItemPrice.text));

        m_strCurItemKey = "";
    }

    void OnClickPurchaseNo()
    {
        UIManager.um.HideScaleUI(m_goPurchasePanel);
        m_strCurItemKey = "";
    }

    public void RefreshMyCostumeInfo()
    {
        m_txtCurMoney.text = m_strMyMoney;
        ActiveItem(m_eCurCosType, false);
        if (m_goCurSelectedItem != null) ChangeCostume(m_goCurSelectedItem);
    }
}
