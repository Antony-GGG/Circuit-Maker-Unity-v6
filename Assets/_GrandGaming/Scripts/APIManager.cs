using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using GameAnalyticsSDK;

public class APIManager : MonoBehaviour
{
    #region Instance
    public static APIManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    [SerializeField] UserDataObject userData;

    private int iv;
    private int coins;
    private int scorebase;
    private int levelbase;

    private int user_id;

    private string roomCode;

    private string base_url = "https://vnwp9menq5.execute-api.us-east-1.amazonaws.com/Prod/games";
    //private string base_url = "https://vxwuq445k5.execute-api.ap-south-1.amazonaws.com/dev/games";

    private void Start()
    {
        DecyrptToken(userData.Data.token);
        GameAnalytics.SetCustomId(user_id.ToString());
        GameAnalytics.Initialize();
    }

    public void UpdateGameScore(int score, string winOrLoss, int level)
    {
        UpdatePoints form = new UpdatePoints();
        form.game_id = userData.Data.game_id.ToString();
        form.game_score = score.ToString();
        form.game_outcome = winOrLoss;
        form.bot_player = "No";
        form.points = score;
        form.level = level;
        form.room_code = roomCode;
        form.coins = coins;
        print("updating score in api");
        print("Game ID : " + userData.Data.game_id);

        CallPostAPI<UpdatePoints>("/updateGameScore", null, form);
    }

    #region Get API
    public void CallGetAPI(string endPoint, Action<string> callback)
    {
        StartCoroutine(IECallGetAPI(base_url + endPoint, callback));
    }

    IEnumerator IECallGetAPI(string uri, Action<string> callback)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get(uri);
        getRequest.SetRequestHeader("token", userData.Data.token);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.ConnectionError || getRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + getRequest.error);
            callback?.Invoke(null);
        }
        else
        {
            Debug.Log("Response: " + getRequest.downloadHandler.text);
            callback?.Invoke(getRequest.downloadHandler.text);
        }
    }
    #endregion

    #region Post API
    public void CallPostAPI<T>(string endPoint, Action<string> callback, T form)
    {
        Debug.Log("Calling Post API : " + userData.Data.token);
        StartCoroutine(IECallPostAPI<T>(base_url + endPoint, callback, form));
    }

    IEnumerator IECallPostAPI<T>(string uri, Action<string> callback, T form)
    {
        string json = JsonUtility.ToJson(form);
        Debug.Log("Calling Post API : " + uri + " : " + json);
        var postRequest = new UnityWebRequest(uri, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        postRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        postRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("token", userData.Data.token);
        postRequest.SetRequestHeader("Content-Type", "application/json");


        yield return postRequest.SendWebRequest();

        Debug.Log("Response : " + postRequest.downloadHandler.text);
        if (postRequest.result == UnityWebRequest.Result.ConnectionError || postRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + postRequest.error);
            callback?.Invoke(null);
        }
        else
        {
            Debug.Log("Response: " + postRequest.downloadHandler.text);
            callback?.Invoke(postRequest.downloadHandler.text);
        }
    }

    #endregion

    public APIResponse<T> SerializeJson<T>(string json)
    {
        return JsonUtility.FromJson<APIResponse<T>>(json);
    }
    public void StartGame()
    {
        CallGetAPI("/startgame", (val) =>
        {
            if (val == null)
                return;

            if (!string.IsNullOrEmpty(SerializeGetJson<string>(val).message))
            {
                roomCode = SerializeGetJson<string>(val).message;
                Debug.Log("RoomCode" + roomCode);
            }
            else
            {
                Debug.LogWarning("RoomCode not found");
            }
        });
    }

    public GetResponse<T> SerializeGetJson<T>(string json)
    {
        return JsonUtility.FromJson<GetResponse<T>>(json);
    }

    public void DecyrptToken(string token)
    {
        //token will be received from index page to unity
        try
        {
            string payload = token.Split('.')[1];
            payload = payload.Replace('-', '+').Replace('_', '/');

            //Fix padding
            int padding = 4 - (payload.Length % 4);
            if (padding < 4)
            {
                payload = payload.PadRight(payload.Length + padding, '=');
            }
            byte[] bytes = Convert.FromBase64String(payload);
            string plainjson = Encoding.UTF8.GetString(bytes);
            Debug.Log("TOKEN DATA" + plainjson);
            TokenRoot var1 = JsonUtility.FromJson<TokenRoot>(plainjson);

            iv = int.Parse(var1.data.score_setting.ivalue.ToString());
            scorebase = int.Parse(var1.data.score_setting.scorebase.ToString());
            levelbase = int.Parse(var1.data.score_setting.levelbase.ToString());
            coins = int.Parse(var1.data.score_setting.coins.ToString());
            user_id = var1.data.user_id;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void coinsEarningLevelBased(double userlevel)
    {
        //token will be received from index page to unity
        try
        {
            /*TextMeshProUGUI _GGCoinText = GameObject.FindGameObjectWithTag("GGCoinText").GetComponent<TextMeshProUGUI>();
            if (_GGCoinText != null)
            {
                _GGCoinText.text = "You've earned 1 GG Coin";
                Debug.Log("Worked");
            }
            else
            {
                Debug.Log("Fucked");
            }*/

            int coinsearned = 0;  //variable to store coins earned 

            if (coins > 0)  //if any coins to be given
            {
                if (levelbase > 0)
                {

                    if ((userlevel % levelbase) == 0)
                    {
                        TextMeshProUGUI _GGCoinText = GameObject.FindGameObjectWithTag("GGCoinText").GetComponent<TextMeshProUGUI>();

                        _GGCoinText.text = "You've earned " + coins.ToString() + " GG Coin";

                        //coinsearned = (int)((userlevel / levelbase) * coins);
                        /*if (coinsearned > 0)
                        {
                            //display coins on game UI using below variables


                            //Response.Write("Coins earned " + coins);
                            //Response.Write("Total coins " + coinsearned);
                        }*/
                    }

                }
                else
                {
                    //coins earning is not level based for this game
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Hello " + ex);
            //exception block
            //Response.Write(ex.ToString());
        }
    }
}

[Serializable]
public class GetResponse<T>
{
    public bool error;
    public int code;
    public string message;
    public T data;
    public string state_json;
    public int points;
}

[Serializable]
public struct APIResponse<T>
{
    public bool error;
    public int code;
    public string message;
    public List<T> data;
}

[Serializable]
public class UpdatePoints
{
    public string game_id;
    public string game_score;
    public string game_outcome;
    public string bot_player;
    public int points;
    public int level;
    public string room_code;
    public int coins;
}

[System.Serializable]
public class ScoreSetting
{
    public string ivalue;
    public string coins;
    public string scorebase;
    public string levelbase;
}

[System.Serializable]
public class Data
{
    public int user_id;
    public ScoreSetting score_setting;
}

[System.Serializable]
public class TokenRoot
{
    public Data data;
}