using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultPanel : MonoBehaviourPunCallbacks
{
    static public GameResultPanel grp;

    public GameObject m_goResultPanel;

    public Text m_txtResult;

    public Text[] m_arrTxtName;
    public Text[] m_arrTxtKill;
    public Text[] m_arrTxtReward;

    private void Awake()
    {
        grp = this;
    }

    static public void RefreshInfo(GAMERESULT eResult)
    {
        Dictionary<string, int> killInfo = GameManager.gm.m_dicUserKillCount;

        List<PLAYERKILLINFO> listKillInfo = new List<PLAYERKILLINFO>();

        UIManager.um.SystemMessage("5초 후 자동으로 방에서 나가집니다.");

        grp.m_txtResult.text = eResult == GAMERESULT.VICTORY ? "승리" : "패배";

        grp.m_goResultPanel.SetActive(true);

        for (int i = 0; i < grp.m_arrTxtKill.Length; i++)
        {
            grp.m_arrTxtKill[i].text = "";
            grp.m_arrTxtName[i].text = "";
            grp.m_arrTxtReward[i].text = "";
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            PLAYERKILLINFO info = new PLAYERKILLINFO();
            info.strName = player.NickName;
            info.iKillCount = killInfo[player.NickName];
            listKillInfo.Add(info);
        }

        listKillInfo.Sort((PLAYERKILLINFO a, PLAYERKILLINFO b) => { return b.iKillCount.CompareTo(a.iKillCount); });

        for (int i = 0; i < listKillInfo.Count; i++)
        {
            var info = listKillInfo[i];

            grp.m_arrTxtName[i].text = info.strName;
            grp.m_arrTxtKill[i].text = info.iKillCount.ToString();

            int rewardMoney = info.iKillCount * 100;

            grp.m_arrTxtReward[i].text = rewardMoney.ToString();
        }
    }
}
