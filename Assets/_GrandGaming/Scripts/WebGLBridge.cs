using TMPro;
using UnityEngine;

public class WebGLBridge : BridgeBase
{
    /*private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }*/

    //public TextMeshProUGUI tokenText;
    public override void GetUserData()
    {
        try
        {
            var json = JsonUtility.ToJson(UserData.Data);
            // GameManager_.Instance.PlayerData = JsonUtility.FromJson<RuntimePlayer>(json);
            //GameManager_.Instance.RoomCode = UserData.Data.room_code;
        }
        catch
        {
            Debug.Log("Error in Bridge Data");
        }
    }

    /*public override void GetGameData()
    {
        try
        {
            var json = JsonUtility.ToJson(GameData.Data);
            //GameManager_.Instance.GameConfig = JsonUtility.FromJson<RuntimeConfig>(json);
            //FillGameConfig();
        }
        catch
        {
            Debug.Log("Error in Bridge Data");
        }
    }
*/
    // This method will be called by the JavaScript when data is sent.
    public void GetBridgeUserData(string data)
    {
        Debug.Log("User data received from the web page: " + data);
        // Do something with the data
        // Do something with the data
        UserData.Data = JsonUtility.FromJson<UserData>(data);
        Debug.Log("User data received from the web page: " + UserData.Data.token);
        //tokenText.text = UserData.Data.token;
        Debug.Log("Bridge Data : " + UserData.Data.token);
    }

}

