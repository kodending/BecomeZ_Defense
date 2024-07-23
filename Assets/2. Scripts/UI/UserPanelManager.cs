using DG.Tweening;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.UI;

public class UserPanelManager : MonoBehaviourPunCallbacks
{
    static public UserPanelManager upm;

    public GameObject m_goUserPanel;

    public GameObject m_goFirstPanel;
    public GameObject m_goJobPanel;
    public GameObject m_goUpGamblePanel;

    public GameObject[] m_arrCardObj;
    bool isSelectedCard;

    public Sprite[] m_arrSprite;

    List<Dictionary<string, object>> m_cardInfo;

    List<Dictionary<string, object>> m_costInfo;

    public Text m_txtJobCost;
    public Text m_txtUpCost;
    public Text m_txtBoxerCost;
    public Text m_txtCheerCost;

    private void Awake()
    {
        upm = this;
    }

    private void Start()
    {
        m_cardInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.CARDINFO].recordDataList;
        m_costInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.COSTINFO].recordDataList;

        m_txtJobCost.text = m_costInfo[0]["USERJOB"].ToString();
        m_txtBoxerCost.text = m_costInfo[0]["USERJOB"].ToString();
        m_txtCheerCost.text = m_costInfo[0]["USERJOB"].ToString();
        m_txtUpCost.text = m_costInfo[0]["USERUPGRADE"].ToString();
    }

    public void OnInit()
    {
        m_goFirstPanel.SetActive(true);
        m_goJobPanel.SetActive(false);
        m_goUpGamblePanel.SetActive(false);
        isSelectedCard = false;
        GameManager.gm.m_bUpUserMode = false;
        GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject.SetActive(true);
    }

    public void OnClickExitButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        OnInit();
        m_goUserPanel.SetActive(false);
    }

    public void OnClickJobButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (GameManager.gm.m_pcLocal.m_curType != PLAYERTYPE.COMMONS)
        {
            UIManager.um.SystemMessage("이미 직업을 고르셨어요. 현재 직업 : " + GameManager.gm.m_pcLocal.m_curType.ToString());
            return;
        }

        OnActviePanel(m_goJobPanel);
    }

    public void OnClickUpUserButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (GameManager.gm.m_pcLocal.m_curType == PLAYERTYPE.COMMONS)
        {
            UIManager.um.SystemMessage("평민은 업그레이드 할 수 없습니다.");
            return;
        }

        OnActviePanel(m_goUpGamblePanel);
        m_goUserPanel.SetActive(false);
        GameManager.gm.m_bUpUserMode = true;
        GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject.SetActive(false);
    }

    public void OnClickBoxerButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (!GameManager.gm.CostCheckToCal("USERJOB"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            OnClickExitButton();
            return;
        }

        GameManager.gm.m_pcLocal.m_pv.RPC("SelectJobRPC", RpcTarget.All, (int)PLAYERTYPE.BOXER);
        OnClickExitButton();
    }

    public void OnClickCheerButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (!GameManager.gm.CostCheckToCal("USERJOB"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            OnClickExitButton();
            return;
        }

        GameManager.gm.m_pcLocal.m_pv.RPC("SelectJobRPC", RpcTarget.All, (int)PLAYERTYPE.CHEERLEADER);
        OnClickExitButton();
    }

    public void OnClickReturnFirstButton()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (!m_goUserPanel.activeSelf) m_goUserPanel.SetActive(true);
        GameObject.Find("Canvas").transform.Find("GameUIPanel").gameObject.SetActive(true);
        OnActviePanel(m_goFirstPanel);
    }

    public void OnClickFirstCard()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (isSelectedCard) return;

        OnClickCard(0);
    }

    public void OnClickSecondCard()
    {
        AudioManager.PlaySfx(SFX.BUTTON);
        if (isSelectedCard) return;

        OnClickCard(1);
    }

    public void OnClickThirdCard()
    {
        AudioManager.PlaySfx(SFX.BUTTON);

        if (isSelectedCard) return;

        OnClickCard(2);
    }

    void OnActviePanel(GameObject panel, bool isActive = true)
    {
        m_goFirstPanel.SetActive(false);
        m_goJobPanel.SetActive(false);
        m_goUpGamblePanel.SetActive(false);

        panel.SetActive(isActive);
    }

    void OnClickCard(int idx)
    {
        if (!GameManager.gm.CostCheckToCal("USERUPGRADE"))
        {
            UIManager.um.SystemMessage("금액이 모자랍니다.");
            OnClickExitButton();
            return;
        }

        Dictionary<string, object> cardInfo = RandomWeight.RandomItem(m_cardInfo);

        Image img = m_arrCardObj[idx].transform.Find("SelectImage").GetComponent<Image>();

        img.sprite = GetSprite(cardInfo);

        GameObject ef = GetEffect(cardInfo, idx);

        m_arrCardObj[idx].transform.DORotate(new Vector3(0, 0, 0), 0.5f);

        isSelectedCard = true;

        AudioManager.PlaySfx(SFX.CARD_SELECT);

        StartCoroutine(ExitPanel(ef, cardInfo, idx));
    }

    IEnumerator ExitPanel(GameObject effect, Dictionary<string, object> cardInfo, int idx)
    {
        yield return new WaitForSeconds(0.1f);

        AudioManager.PlaySfx(SFX.CARD_ROTATION);

        yield return new WaitForSeconds(0.4f);

        effect.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        int typeIdx = int.Parse(cardInfo["TYPE"].ToString());
        string nickName = GameManager.gm.m_pcLocal.m_pv.Owner.NickName;
        float plus = float.Parse(cardInfo["PLUS"].ToString());

        string msg = "꽝";
        if(typeIdx != 0)
            msg = ((CARDTYPE)typeIdx).ToString() + " : " + plus.ToString() + " 증가";

        UIManager.um.SystemMessage(msg);

        GameManager.gm.m_pcLocal.m_pv.RPC("UpStatRPC", RpcTarget.All, typeIdx, nickName, plus);

        m_arrCardObj[idx].transform.DORotate(new Vector3(0, 180, 0), 0.1f);
        //능력치부여 한다
        effect.SetActive(false);
        OnClickExitButton();
    }

    Sprite GetSprite(Dictionary<string, object> cardInfo)
    {
        Sprite sp = null;

        sp = m_arrSprite[int.Parse(cardInfo["TYPE"].ToString())];

        return sp;
    }

    GameObject GetEffect(Dictionary<string, object> cardInfo, int idx)
    {
        GameObject ef = null;

        switch ((CARDRANK)int.Parse(cardInfo["RANK"].ToString()))
        {
            case CARDRANK.NORMAL:
                if((CARDTYPE)int.Parse(cardInfo["TYPE"].ToString()) == CARDTYPE.BOOM)
                    ef = m_arrCardObj[idx].transform.Find("BoomEffect").gameObject;
                else ef = m_arrCardObj[idx].transform.Find("NormalEffect").gameObject;
                break;

            case CARDRANK.RARE:
                ef = m_arrCardObj[idx].transform.Find("RareEffect").gameObject;
                break;

            case CARDRANK.EPIC:
                ef = m_arrCardObj[idx].transform.Find("EpicEffect").gameObject;
                break;
        }


        return ef;
    }
}
