using UnityEngine;

[CreateAssetMenu(fileName = "MainCoreGame", menuName = "Scriptable Objects/MainCoreGame")]
public class MainCoreGame : ScriptableObject
{
    public int hari;
    public float uang;
    public float hunger;
    public float fatigue;
    public MainCoreGameAsset[] playerAsset;
}
