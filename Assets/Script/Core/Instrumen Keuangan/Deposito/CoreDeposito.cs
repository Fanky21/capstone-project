using UnityEngine;
using System;
using System.Collections.Generic;

public class CoreDeposito : MonoBehaviour
{
    public MainCoreGame mainCoreGame;
    public DepositoCoreObject depositoCoreObject;
    
    void startDeposito(float ammount, int duration)
    {
        // Cek apakah uang cukup
        if (mainCoreGame.uang < ammount)
        {
            Debug.Log("Uang tidak cukup untuk deposito!");
            return;
        }

        Debug.Log("Deposito dimulai dengan jumlah: " + ammount);
        mainCoreGame.uang -= ammount;
        Debug.Log("Sisa uang setelah deposito: " + mainCoreGame.uang);

        // Buat data deposito baru
        DepositoCoreObjectHistory newDeposito = new DepositoCoreObjectHistory();
        newDeposito.depositoCode = generateDepositoCode();
        newDeposito.depositoAmmount = ammount;
        newDeposito.depositoDuration = duration;
        newDeposito.depositoStartDate = mainCoreGame.tanggal;
        newDeposito.depositoEndDate = mainCoreGame.tanggal.AddMonths(duration);

        // Tambahkan ke array deposito
        addDepositoToHistory(newDeposito);
        
        Debug.Log("Deposito berhasil dibuat dengan kode: " + newDeposito.depositoCode + 
                  " | Berakhir pada: " + newDeposito.depositoEndDate.ToString("dd/MM/yyyy"));
    }

    float intrestRateCalculator(float ammount, int duration)
    {
        float interestRate = 0.06f; // Bunga deposito default annualy 6% per tahun.
        float interestEarned = ammount * interestRate * (duration / 12.0f); // Menghitung bunga berdasarkan durasi dalam bulan.

        Debug.Log("Bunga yang diperoleh setelah " + duration + " bulan: " + interestEarned);
        float totalDepositoEarning = ammount + interestEarned;

        Debug.Log("Total pendapatan deposito: " + totalDepositoEarning);

        return totalDepositoEarning;
    }

    int generateDepositoCode()
    {
        // Generate kode deposito unik berdasarkan timestamp
        return (int)(DateTime.Now.Ticks % int.MaxValue);
    }

    void addDepositoToHistory(DepositoCoreObjectHistory newDeposito)
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null)
        {
            depositoCoreObject.depositoCoreObjectHistory = new DepositoCoreObjectHistory[1];
            depositoCoreObject.depositoCoreObjectHistory[0] = newDeposito;
        }
        else
        {
            // Expand array dan tambahkan deposito baru
            DepositoCoreObjectHistory[] tempArray = new DepositoCoreObjectHistory[depositoCoreObject.depositoCoreObjectHistory.Length + 1];
            for (int i = 0; i < depositoCoreObject.depositoCoreObjectHistory.Length; i++)
            {
                tempArray[i] = depositoCoreObject.depositoCoreObjectHistory[i];
            }
            tempArray[tempArray.Length - 1] = newDeposito;
            depositoCoreObject.depositoCoreObjectHistory = tempArray;
        }
    }

    void checkDepositoStatus()
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null) return;

        List<int> expiredIndices = new List<int>();

        // Cek setiap deposito apakah sudah expired
        for (int i = 0; i < depositoCoreObject.depositoCoreObjectHistory.Length; i++)
        {
            DepositoCoreObjectHistory deposito = depositoCoreObject.depositoCoreObjectHistory[i];
            
            // Bandingkan tanggal saat ini dengan tanggal berakhir deposito
            if (mainCoreGame.tanggal.Date >= deposito.depositoEndDate.Date)
            {
                // Deposito sudah expired, hitung total pendapatan
                float totalEarning = intrestRateCalculator(deposito.depositoAmmount, deposito.depositoDuration);
                
                // Tambahkan ke uang player
                mainCoreGame.uang += totalEarning;
                
                Debug.Log("Deposito dengan kode " + deposito.depositoCode + " telah berakhir!");
                Debug.Log("Total pendapatan: " + totalEarning);
                Debug.Log("Uang player sekarang: " + mainCoreGame.uang);
                
                // Tandai untuk dihapus
                expiredIndices.Add(i);
            }
        }

        // Hapus deposito yang sudah expired (dari index tertinggi ke terendah)
        for (int i = expiredIndices.Count - 1; i >= 0; i--)
        {
            removeDepositoFromHistory(expiredIndices[i]);
        }
    }

    void removeDepositoFromHistory(int index)
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null || 
            index < 0 || index >= depositoCoreObject.depositoCoreObjectHistory.Length) return;

        // Buat array baru tanpa elemen yang dihapus
        DepositoCoreObjectHistory[] newArray = new DepositoCoreObjectHistory[depositoCoreObject.depositoCoreObjectHistory.Length - 1];
        
        int newIndex = 0;
        for (int i = 0; i < depositoCoreObject.depositoCoreObjectHistory.Length; i++)
        {
            if (i != index)
            {
                newArray[newIndex] = depositoCoreObject.depositoCoreObjectHistory[i];
                newIndex++;
            }
        }
        
        depositoCoreObject.depositoCoreObjectHistory = newArray;
        Debug.Log("Deposito pada index " + index + " telah dihapus dari history.");
    }

    void Update()
    {
        // Lakukan pengecekan deposito secara berkala
        checkDepositoStatus();
    }

    // Method public untuk memulai deposito dari UI atau script lain
    public void StartNewDeposito(float amount, int durationInMonths)
    {
        startDeposito(amount, durationInMonths);
    }

    // Method untuk mendapatkan informasi deposito aktif
    public DepositoCoreObjectHistory[] GetActiveDepositos()
    {
        return depositoCoreObject.depositoCoreObjectHistory;
    }

    // Method untuk mendapatkan total nilai deposito yang sedang berjalan
    public float GetTotalDepositoValue()
    {
        if (depositoCoreObject.depositoCoreObjectHistory == null) return 0f;

        float total = 0f;
        foreach (var deposito in depositoCoreObject.depositoCoreObjectHistory)
        {
            total += deposito.depositoAmmount;
        }
        return total;
    }
}
