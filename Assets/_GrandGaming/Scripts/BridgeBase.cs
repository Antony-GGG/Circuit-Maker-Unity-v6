using UnityEngine;


public abstract class BridgeBase : MonoBehaviour
{
    [SerializeField] private UserDataObject _userData;
    //[SerializeField] private GameDataObject _gameData;

    public UserDataObject UserData { get => _userData; set => _userData = value; }
    //public GameDataObject GameData { get => _gameData; set => _gameData = value; }

    public abstract void GetUserData();
    //public abstract void GetGameData();
}

