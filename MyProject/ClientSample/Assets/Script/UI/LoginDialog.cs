using GameServer;
using UnityEngine;
using UnityEngine.UI;

public class LoginDialog : MonoBehaviour
{
    public GameObject LoginPanel;
    public InputField InputAccount;
    public InputField InputPassword;
    
    public GameObject CreateAccountPanel;
    public InputField InputNewAccount;
    public InputField InputNewPassword;
    public InputField InputName;
    
    public Text TextStateLog;
    private float _selfDestroyTime = 2f;

    void Awake()
    {
        LoginPanel.SetActive(true);
        CreateAccountPanel.SetActive(false);
    }
    
    public void OnClickLogin()
    {
        if (CNetworkManager.Inst.user_state == USER_STATE.NOT_CONNECTED)
        {
            TextStateLog.text = "서버 점검중 입니다.";
            return;
        }

        CNetworkManager.Inst.RequestEnterGameServer(InputAccount.text, InputPassword.text, (res, error) =>
        {
            if (error != ERROR.NONE)
            {
                TextStateLog.text = error.ToString();
                return;
            }
            
            var data = (PlayerIdData) res;
            GameManager.Inst.UserId = data.playerId;
            GameManager.Inst.PrintSystemLog($"{data.playerId}님이 서버에 접속하였습니다.");
            StartGame();
        });
    }
    
    public void OnClickCreateAccount()
    {
        LoginPanel.SetActive(false);
        CreateAccountPanel.SetActive(true);
    }

    public void OnClickRequestNewAccount()
    {
        var account = InputNewAccount.text;
        var password = InputNewPassword.text;
        var name = InputName.text;
        
        CNetworkManager.Inst.RequestCreateAccount(account, password, name, (res, error) =>
        {
            if (error != ERROR.NONE)
            {
                TextStateLog.text = error.ToString();
                return;
            }
            
            var data = (StringResponseData) res;

            TextStateLog.text = "생성완료";
            
            LoginPanel.SetActive(true);
            CreateAccountPanel.SetActive(false);
        });
    }

    private void StartGame()
    {
        gameObject.SetActive(false);
        
        GameManager.Inst.StartGame();
    }
}
