using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class UIInMain : BaseState
{
    [Header("MainButtonPanel")]
    GameObject m_goTitlePanel;
    Button m_btnLogin, m_btnCostume, m_btnOption, m_btnExit;

    [Header("EnterButtonPanel")]
    InputField m_inputMail, m_inputPW;
    Button m_btnEnter, m_btnJoin, m_btnMain, m_btnCheckOn, m_btnCheckOff;

    [Header("RegistButtonPanel")]
    InputField m_inputRegistMail, m_inputRegistPW, m_inputConfirmPW, m_inputUserID;
    Button m_btnRegist, m_btnLoginMenu;

    Text m_txtStatus;

    GameObject m_goMainPanel;

    List<InputField> m_listInput;

    public override void OnEnterState()
    {
        m_goMainPanel = GameObject.Find("Canvas").transform.Find("MainPanel").gameObject;

        UIManager.um.m_goMainButtonPanel = m_goMainPanel.transform.Find("MainButtonPanel").gameObject;
        UIManager.um.m_goLoginButtonPanel = m_goMainPanel.transform.Find("LoginButtonPanel").gameObject;
        UIManager.um.m_goRegistButtonPanel = m_goMainPanel.transform.Find("RegistButtonPanel").gameObject;

        m_goMainPanel.SetActive(true);

        m_goTitlePanel = m_goMainPanel.transform.Find("TitlePanel").gameObject;

        UIManager.um.ShowScaleUI(m_goTitlePanel, 0.5f);

        m_txtStatus = m_goMainPanel.transform.Find("StatusText").GetComponent<Text>();

        m_listInput = new List<InputField>();

        AudioManager.PlayBGM(BGM.MAIN, true);

        ActiveButton(true);
    }

    public override void OnUpdateState()
    {
        m_txtStatus.text = PhotonNetwork.NetworkClientState.ToString();

        GetTabKey();
    }

    public override void OnFixedUpdateState()
    {

    }

    public override void OnExitState()
    {
        ActiveButton(false);

        ShowPanel(UIManager.um.m_goMainButtonPanel);

        m_goMainPanel.SetActive(false);

        AudioManager.PlayBGM(BGM.MAIN, false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //방 참가 실패 했을 때 메세지하나 넣기
    }

    void ActiveButton(bool isActive)
    {
        if(isActive)
        {
            //쓸데없는거 끄기
            GameObject goConnect = UIManager.um.m_LoadingPanel;
            goConnect.SetActive(false);

            //메인 메뉴 버튼 관련
            m_btnLogin = UIManager.um.m_goMainButtonPanel.transform.Find("LoginButton").GetComponent<Button>();
            m_btnLogin.onClick.AddListener(OnClickLogin);
            m_btnLogin.onClick.AddListener(()=> AudioManager.PlaySfx(SFX.BUTTON));
            m_btnOption = UIManager.um.m_goMainButtonPanel.transform.Find("OptionButton").GetComponent<Button>();
            m_btnOption.onClick.AddListener(OnClickOption);
            m_btnOption.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnExit = UIManager.um.m_goMainButtonPanel.transform.Find("ExitButton").GetComponent<Button>();
            m_btnExit.onClick.AddListener(OnClickExit);
            m_btnExit.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

            //로그인정보 입력 버튼관련
            m_btnEnter = UIManager.um.m_goLoginButtonPanel.transform.Find("EnterButton").GetComponent<Button>();
            m_btnEnter.onClick.AddListener(OnClickEnter);
            m_btnEnter.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnJoin = UIManager.um.m_goLoginButtonPanel.transform.Find("JoinButton").GetComponent<Button>();
            m_btnJoin.onClick.AddListener(OnClickJoin);
            m_btnJoin.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnMain = UIManager.um.m_goLoginButtonPanel.transform.Find("MainButton").GetComponent<Button>();
            m_btnMain.onClick.AddListener(OnClickMain);
            m_btnMain.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnCheckOn = UIManager.um.m_goLoginButtonPanel.transform.Find("CheckOnButton").GetComponent<Button>();
            m_btnCheckOn.onClick.AddListener(OnClickCheckOn);
            m_btnCheckOn.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnCheckOff = UIManager.um.m_goLoginButtonPanel.transform.Find("CheckOffButton").GetComponent<Button>();
            m_btnCheckOff.onClick.AddListener(OnClickCheckOff);
            m_btnCheckOff.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));

            m_inputMail = UIManager.um.m_goLoginButtonPanel.transform.Find("InputMail").GetComponent<InputField>();
            m_inputPW = UIManager.um.m_goLoginButtonPanel.transform.Find("InputPassword").GetComponent<InputField>();

            //회원가입 입력 버튼관련
            m_inputRegistMail = UIManager.um.m_goRegistButtonPanel.transform.Find("InputMail").GetComponent<InputField>();
            m_inputRegistPW = UIManager.um.m_goRegistButtonPanel.transform.Find("InputPassword").GetComponent<InputField>();
            m_inputConfirmPW = UIManager.um.m_goRegistButtonPanel.transform.Find("InputConfirmPassword").GetComponent<InputField>();
            m_inputUserID = UIManager.um.m_goRegistButtonPanel.transform.Find("InputUserID").GetComponent<InputField>();

            m_btnRegist = UIManager.um.m_goRegistButtonPanel.transform.Find("RegistButton").GetComponent<Button>();
            m_btnRegist.onClick.AddListener(OnClickRegist);
            m_btnRegist.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
            m_btnLoginMenu = UIManager.um.m_goRegistButtonPanel.transform.Find("LoginMenuButton").GetComponent<Button>();
            m_btnLoginMenu.onClick.AddListener(OnClickLoginMenu);
            m_btnLoginMenu.onClick.AddListener(() => AudioManager.PlaySfx(SFX.BUTTON));
        }

        else
        {
            m_btnLogin.onClick.RemoveAllListeners();
            m_btnOption.onClick.RemoveAllListeners();
            m_btnExit.onClick.RemoveAllListeners();
            m_btnEnter.onClick.RemoveAllListeners();
            m_btnJoin.onClick.RemoveAllListeners();
            m_btnMain.onClick.RemoveAllListeners();
            m_btnCheckOn.onClick.RemoveAllListeners();
            m_btnCheckOff.onClick.RemoveAllListeners();
            m_btnRegist.onClick.RemoveAllListeners();
            m_btnLoginMenu.onClick.RemoveAllListeners();

            InitInputText();
        }
    }

    void GetTabKey()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && !UIManager.um.m_goMainButtonPanel.activeSelf)
        {
            bool isFocused = false;

            for (int i = 0; i < m_listInput.Count; i++)
            {
                if (m_listInput[i].isFocused)
                {
                    isFocused = true;

                    if (i == m_listInput.Count - 1)
                        m_listInput[0].Select();
                    else
                        m_listInput[i + 1].Select();

                    break;
                }
            }

            if (!isFocused) m_listInput[0].Select();
        }
    }

    void OnClickLogin()
    {
        ShowPanel(UIManager.um.m_goLoginButtonPanel);

        List<Dictionary<string, object>> loginInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.OPTIONINFO].recordDataList;

        if (Convert.ToBoolean(int.Parse(loginInfo[0]["SAVED"].ToString())))
        {
            m_btnCheckOff.gameObject.SetActive(false);
            m_btnCheckOn.gameObject.SetActive(true);
            UIManager.um.m_isSavedMail = true;

            m_inputMail.text = loginInfo[0]["MAIL"].ToString();
        }    

        if (m_listInput.Count > 0)
            m_listInput.Clear();

        m_listInput.Add(m_inputMail);
        m_listInput.Add(m_inputPW);
    }

    void OnClickOption()
    {
        UIManager.um.OnClickOption();
    }

    void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnClickEnter()
    {
        if (m_inputMail.text == "") return;

        NetworkManager.nm.Login(m_inputMail.text, m_inputPW.text);

        m_inputPW.text = "";

        UIManager.um.m_stateMachine.ChangeState(UISTATE.ENTERING_LOGIN);
    }

    void OnClickCheckOn()
    {
        m_btnCheckOff.gameObject.SetActive(true);
        m_btnCheckOn.gameObject.SetActive(false);

        UIManager.um.m_isSavedMail = false;
    }

    void OnClickCheckOff()
    {
        m_btnCheckOff.gameObject.SetActive(false);
        m_btnCheckOn.gameObject.SetActive(true);

        UIManager.um.m_isSavedMail = true;
    }

    void OnClickJoin()
    {
        ShowPanel(UIManager.um.m_goRegistButtonPanel);
        InitInputText();

        if (m_listInput.Count > 0)
            m_listInput.Clear();

        m_listInput.Add(m_inputRegistMail);
        m_listInput.Add(m_inputRegistPW);
        m_listInput.Add(m_inputConfirmPW);
        m_listInput.Add(m_inputUserID);
    }

    void OnClickMain()
    {
        ShowPanel(UIManager.um.m_goMainButtonPanel);
        InitInputText();

        if (m_listInput.Count != 0)
            m_listInput.Clear();
    }

    void OnClickRegist()
    {
        if (m_inputRegistPW.text != m_inputConfirmPW.text)
        {
            UIManager.um.SystemMessage("패스워드가 일치하지 않습니다.");
            return;
        }

        if (m_inputRegistPW.text.Length < 6)
        {
            UIManager.um.SystemMessage("비밀번호는 6자 이상 입력해라");
            return;
        }

        NetworkManager.nm.Register(m_inputRegistMail.text, m_inputRegistPW.text, m_inputUserID.text);

        Invoke("OnClickLoginMenu", 1f);
    }

    void OnClickLoginMenu()
    {
        ShowPanel(UIManager.um.m_goLoginButtonPanel);
        InitInputText();
        if (UIManager.um.m_strSavedMail != "") m_inputMail.text = UIManager.um.m_strSavedMail;

        if (m_listInput.Count > 0)
            m_listInput.Clear();

        m_listInput.Add(m_inputMail);
        m_listInput.Add(m_inputPW);
    }

    void ShowPanel(GameObject curPanel)
    {
        UIManager.um.m_goLoginButtonPanel.SetActive(false);
        UIManager.um.m_goMainButtonPanel.SetActive(false);
        UIManager.um.m_goRegistButtonPanel.SetActive(false);

        curPanel.SetActive(true);
    }

    void InitInputText()
    {
        if (m_inputMail.text != "" && m_inputMail.text != UIManager.um.m_strSavedMail)
        {
            m_inputMail.text = "";
        }
        if (m_inputPW.text          != "") m_inputPW.text           = "";
        if (m_inputRegistMail.text  != "") m_inputRegistMail.text   = "";
        if (m_inputRegistPW.text    != "") m_inputRegistPW.text     = "";
        if (m_inputConfirmPW.text   != "") m_inputConfirmPW.text    = "";  
        if (m_inputUserID.text    != "")   m_inputUserID.text    = "";
    }
}
