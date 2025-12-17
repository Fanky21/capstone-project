using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class CoreDeposito : MonoBehaviour
{
    [Header("Deposito Settings")]
    public DepositoCoreObject depositoCoreObject;
    public float defaultInterestRate = 6f;

    [Header("Time Reference")]
    private float lastCheckedTime = 0f;
    private float elapsedGameHours = 0f;

    private void Update()
    {
        if (depositoCoreObject == null || DayNightCycle.Instance == null) return;

        // Hitung berapa jam yang sudah berlalu di game world
        float currentWorldTime = DayNightCycle.Instance.WorldTime; // dalam menit (0-1440)
        float currentWorldTimeInHours = currentWorldTime / 60f; // konversi ke jam (0-24)

        // Hitung delta time dalam jam game
        // Karena WorldTime reset setiap 24 jam, kita perlu handle wrap-around
        float deltaHours = 0f;
        
        if (lastCheckedTime == 0f)
        {
            lastCheckedTime = currentWorldTimeInHours;
            return;
        }

        if (currentWorldTimeInHours >= lastCheckedTime)
        {
            deltaHours = currentWorldTimeInHours - lastCheckedTime;
        }
        else
        {
            // Wrap around terjadi (midnight)
            deltaHours = (24f - lastCheckedTime) + currentWorldTimeInHours;
        }

        lastCheckedTime = currentWorldTimeInHours;
        elapsedGameHours += deltaHours;

        // Update semua deposito aktif
        UpdateActiveDepositos(deltaHours);
    }

    /// <summary>
    /// Memulai deposito baru
    /// </summary>
    /// <param name="amount">Jumlah uang yang didepositokan</param>
    /// <param name="durationInDays">Durasi dalam hari (akan dikonversi ke jam)</param>
    /// <param name="customInterestRate">Rate bunga kustom, jika -1 akan gunakan default</param>
    public bool StartDeposito(float amount, int durationInDays, float customInterestRate = -1f)
    {
        if (depositoCoreObject == null)
        {
            Debug.LogError("DepositoCoreObject tidak di-assign!");
            return false;
        }

        if (PlayerMoneyManager.Instance == null)
        {
            Debug.LogError("PlayerMoneyManager tidak ditemukan!");
            return false;
        }

        if (!PlayerMoneyManager.Instance.HasEnoughMoney((int)amount))
        {
            Debug.LogWarning($"Uang tidak cukup untuk deposito sebesar Rp{amount:N0}");
            return false;
        }

        if (DayNightCycle.Instance == null)
        {
            Debug.LogError("DayNightCycle tidak ditemukan!");
            return false;
        }

        // Ambil uang dari player
        if (!PlayerMoneyManager.Instance.RemoveMoney((int)amount))
        {
            return false;
        }

        // Hitung durasi dalam jam (1 hari = 24 jam)
        float durationInHours = durationInDays * 24f;

        // Gunakan interest rate yang dipilih
        float interestRate = customInterestRate > 0 ? customInterestRate : defaultInterestRate;

        // Generate kode deposito unik
        int newDepositoCode = GenerateDepositoCode();

        // Buat history baru
        DepositoCoreObjectHistory newDeposito = new DepositoCoreObjectHistory
        {
            depositoCode = newDepositoCode,
            depositoAmount = amount,
            depositoDurationInHours = durationInHours,
            depositoInterestRate = interestRate,
            startTimeInHours = elapsedGameHours,
            timeRemainingInHours = durationInHours,
            isActive = true,
            isCompleted = false
        };

        // Tambahkan ke array
        List<DepositoCoreObjectHistory> depositoList = new List<DepositoCoreObjectHistory>(depositoCoreObject.depositoCoreObjectHistory);
        depositoList.Add(newDeposito);
        depositoCoreObject.depositoCoreObjectHistory = depositoList.ToArray();

        Debug.Log($"Deposito dimulai! Kode: {newDepositoCode}, Jumlah: Rp{amount:N0}, Durasi: {durationInDays} hari ({durationInHours} jam), Bunga: {interestRate}%");
        
        return true;
    }

    /// <summary>
    /// Update semua deposito aktif, kurangi waktu dan cek jika ada yang jatuh tempo
    /// </summary>
    private void UpdateActiveDepositos(float deltaHours)
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null || depositoCoreObject.depositoCoreObjectHistory.Length == 0)
            return;

        for (int i = 0; i < depositoCoreObject.depositoCoreObjectHistory.Length; i++)
        {
            DepositoCoreObjectHistory deposito = depositoCoreObject.depositoCoreObjectHistory[i];

            if (!deposito.isActive || deposito.isCompleted)
                continue;

            // Kurangi waktu tersisa
            deposito.timeRemainingInHours -= deltaHours;

            // Cek jika sudah jatuh tempo
            if (deposito.timeRemainingInHours <= 0)
            {
                CompleteDeposito(deposito);
            }
        }
    }

    /// <summary>
    /// Menyelesaikan deposito dan mengembalikan uang + bunga
    /// </summary>
    private void CompleteDeposito(DepositoCoreObjectHistory deposito)
    {
        if (deposito.isCompleted)
            return;

        // Hitung total yang dikembalikan (pokok + bunga)
        float interestAmount = deposito.depositoAmount * (deposito.depositoInterestRate / 100f);
        float totalReturn = deposito.depositoAmount + interestAmount;

        // Kembalikan uang ke player
        if (PlayerMoneyManager.Instance != null)
        {
            PlayerMoneyManager.Instance.AddMoney((int)totalReturn);
            Debug.Log($"Deposito #{deposito.depositoCode} selesai! Dikembalikan: Rp{totalReturn:N0} (Pokok: Rp{deposito.depositoAmount:N0} + Bunga: Rp{interestAmount:N0})");
        }

        // Update status
        deposito.isActive = false;
        deposito.isCompleted = true;
        deposito.timeRemainingInHours = 0f;
    }

    /// <summary>
    /// Generate kode deposito unik
    /// </summary>
    private int GenerateDepositoCode()
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null || depositoCoreObject.depositoCoreObjectHistory.Length == 0)
        {
            return 1001; // Kode awal
        }

        // Ambil kode tertinggi dan tambah 1
        int maxCode = depositoCoreObject.depositoCoreObjectHistory.Max(d => d.depositoCode);
        return maxCode + 1;
    }

    /// <summary>
    /// Mendapatkan list deposito aktif
    /// </summary>
    public List<DepositoCoreObjectHistory> GetActiveDepositos()
    {
        if (depositoCoreObject == null || depositoCoreObject.depositoCoreObjectHistory == null)
            return new List<DepositoCoreObjectHistory>();

        return depositoCoreObject.depositoCoreObjectHistory
            .Where(d => d.isActive && !d.isCompleted)
            .ToList();
    }

    /// <summary>
    /// Mendapatkan total uang yang sedang didepositokan
    /// </summary>
    public float GetTotalDepositedAmount()
    {
        return GetActiveDepositos().Sum(d => d.depositoAmount);
    }

    /// <summary>
    /// Cancellation deposito sebelum jatuh tempo (opsional - dengan penalty)
    /// </summary>
    public bool CancelDeposito(int depositoCode, float penaltyPercent = 10f)
    {
        if (depositoCoreObject == null || depositoCoreObject.depositoCoreObjectHistory == null)
            return false;

        DepositoCoreObjectHistory deposito = depositoCoreObject.depositoCoreObjectHistory
            .FirstOrDefault(d => d.depositoCode == depositoCode && d.isActive && !d.isCompleted);

        if (deposito == null)
        {
            Debug.LogWarning($"Deposito dengan kode {depositoCode} tidak ditemukan atau sudah tidak aktif");
            return false;
        }

        // Hitung jumlah yang dikembalikan dengan penalty
        float penaltyAmount = deposito.depositoAmount * (penaltyPercent / 100f);
        float returnAmount = deposito.depositoAmount - penaltyAmount;

        // Kembalikan uang ke player
        if (PlayerMoneyManager.Instance != null)
        {
            PlayerMoneyManager.Instance.AddMoney((int)returnAmount);
            Debug.Log($"Deposito #{depositoCode} dibatalkan! Dikembalikan: Rp{returnAmount:N0} (Penalty: Rp{penaltyAmount:N0})");
        }

        // Update status
        deposito.isActive = false;
        deposito.isCompleted = true;

        return true;
    }
}
