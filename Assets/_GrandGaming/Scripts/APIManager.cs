using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Networking;


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

    private string base_url = "https://vnwp9menq5.execute-api.us-east-1.amazonaws.com/Prod/games/updateGameScore";
    //private string base_url = "https://vxwuq445k5.execute-api.ap-south-1.amazonaws.com/dev/games/updateGameScore";

    public void UpdateGameScore(int score, string winOrLoss, int level)
    {
        UpdatePoints form = new UpdatePoints();
        form.game_id = userData.Data.game_id.ToString();
        form.game_score = score.ToString();
        form.game_outcome = winOrLoss;
        form.bot_player = "No";
        form.points = score;
        form.level = level;
        print("updating score in api");
        print("Game ID : " + userData.Data.game_id);

        CallPostAPI<UpdatePoints>("update_points", null, form);
    }

    #region Get API
    public void CallGetAPI(string endPoint, Action<string> callback)
    {
        StartCoroutine(IECallGetAPI(base_url, callback));
    }

    IEnumerator IECallGetAPI(string uri, Action<string> callback)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get(uri);
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
        StartCoroutine(IECallPostAPI<T>(base_url, callback, form));
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

    public
    APIResponse<T> SerializeJson<T>(string json)
    {
        return JsonUtility.FromJson<APIResponse<T>>(json);
    }
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
}
