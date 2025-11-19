using System;
using UnityEngine;

[System.Serializable]
public class MainCoreGameAssetHistory
{
    [SerializeField]
    private string dateTimeString;

    public DateTime Date
    {
        get => DateTime.TryParse(dateTimeString, out var dt) ? dt : default;
        set => dateTimeString = value.ToString("o"); // ISO 8601 format
    }

    public float price;
}