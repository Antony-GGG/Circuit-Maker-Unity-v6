using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LvlData_", menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    public int row;
    public int col;
    public int[] propType;
}
