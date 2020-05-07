using System.Collections;
using System.Collections.Generic;
using GameServer;
using UnityEngine;
using UnityEngine.UI;

public class LoginDialog : MonoBehaviour
{
    public InputField InputField;
    public Text TextStateLog;
    private float _selfDestroyTime = 2f;
    
    public void OnClickLogin()
    {
        if (CNetworkManager.Inst.user_state == USER_STATE.NOT_CONNECTED)
        {
            TextStateLog.text = "서버 점검중 입니다.";
            return;
        }
        
        var id = int.Parse(InputField.text);

        CNetworkManager.Inst.RequestEnterGameServer(id, (res, error) =>
        {
            if (error != ERROR.NONE)
            {
                TextStateLog.text = error.ToString();
                return;
            }
            
            var data = (UserData) res;

            GameManager.Inst.UserId = data.userId;
            
            GameManager.Inst.PrintSystemLog($"{data.userId}님이 서버에 접속하였습니다.");
        });
        
        StartGame();
    }

    private void StartGame()
    {
        gameObject.SetActive(false);
        
        GameManager.Inst.StartGame();
    }
}
