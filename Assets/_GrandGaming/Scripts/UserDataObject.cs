using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Player Data", menuName = "Data/PlayerData")]
public class UserDataObject : ScriptableObject
{
    public UserData Data;
}

[Serializable]
public class UserData
{
    public string game_id;
    public string token;
}

