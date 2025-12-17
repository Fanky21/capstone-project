using UnityEngine;
using System;

[System.Serializable]
public class DepositoCoreObjectHistory
{
    public int depositoCode;
    public float depositoAmount;
    public float depositoDurationInHours; // durasi dalam jam (1 hari = 24 jam, 2 hari = 48 jam)
    public float depositoInterestRate; // rate bunga dalam persen
    public float startTimeInHours; // waktu mulai dalam jam (dari WorldTime / 60)
    public float timeRemainingInHours; // waktu tersisa dalam jam
    public bool isActive; // status apakah deposito masih aktif
    public bool isCompleted; // status apakah sudah selesai dan uang dikembalikan
}
