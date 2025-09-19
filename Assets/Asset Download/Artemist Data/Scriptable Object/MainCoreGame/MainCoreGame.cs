using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MainCoreGame", menuName = "Scriptable Objects/MainCoreGame")]
public class MainCoreGame : ScriptableObject
{
    public DateTime tanggal;
    public float uang;
    public float hunger;
    public float fatigue;
    public MainCoreGameAsset[] playerAsset;
}
