using UnityEngine;

public class CorePinjaman : MonoBehaviour
{
    public MainCoreGame mainCoreGame;
    public CoreDataPinjaman coreDataPinjaman;
    bool CheckPinjamanTertunggak()
    {
        bool adaTertunggak = false;
        for (int i = 0; i < coreDataPinjaman.coreDataPinjamanList.Length; i++)
        {
            var pinjaman = coreDataPinjaman.coreDataPinjamanList[i];
            if (!pinjaman.pinjamanStatus) // Hanya cek pinjaman yang belum lunas
            {
                if (mainCoreGame.tanggal > mainCoreGame.tanggal.AddDays(pinjaman.pinjamanExpiryDate))
                {
                    pinjaman.isPinjamanTertunggak = true;
                    Debug.Log("Pinjaman dengan ID " + pinjaman.pinjamanId + " tertunggak.");
                    adaTertunggak = true;
                }
                else
                {
                    pinjaman.isPinjamanTertunggak = false;
                }
            }
        }
        return adaTertunggak;
    }

    public void Pinjaman(float jumlahPinjaman)
    {
        var statusPinjamanLama = CheckPinjamanTertunggak();
        if (statusPinjamanLama)
        {
            Debug.Log("Anda memiliki pinjaman tertunggak. Silakan lunasi terlebih dahulu sebelum mengambil pinjaman baru.");
            return;
        }

        // Insert data to pinjaman baru scriptable object
        // default tenor untuk pinjaman adalah 3 bulan pembayaran
        // tambahkan ammount pada mainCoreGame.uang
        
        // Create new loan data
        CoreDataPinjamanList newPinjaman = new CoreDataPinjamanList();
        newPinjaman.pinjamanId = System.Guid.NewGuid().GetHashCode();
        newPinjaman.pinjamanAmount = jumlahPinjaman;
        newPinjaman.pinjamanExpiryDate = 90; // 3 months = 90 days
        newPinjaman.pinjamanStatus = false; // belum lunas
        newPinjaman.isPinjamanTertunggak = false;
    
        mainCoreGame.uang += jumlahPinjaman;
        
        Debug.Log("Pinjaman baru sebesar " + jumlahPinjaman + " telah disetujui.");
        Debug.Log("Total uang sekarang: " + mainCoreGame.uang);
    }

    public void PelunasanPinjaman(int idPinjaman)
    {
        for (int i = 0; i < coreDataPinjaman.coreDataPinjamanList.Length; i++)
        {
            var pinjaman = coreDataPinjaman.coreDataPinjamanList[i];
            if (pinjaman.pinjamanId == idPinjaman)
            {
                if (pinjaman.pinjamanStatus)
                {
                    Debug.Log("Pinjaman dengan ID " + idPinjaman + " sudah lunas.");
                    return;
                }

                if (mainCoreGame.uang >= pinjaman.pinjamanAmount)
                {
                    mainCoreGame.uang -= pinjaman.pinjamanAmount;
                    pinjaman.pinjamanStatus = true; // tandai sebagai lunas
                    pinjaman.isPinjamanTertunggak = false; // reset status tertunggak
                    Debug.Log("Pinjaman dengan ID " + idPinjaman + " telah dilunasi.");
                    Debug.Log("Sisa uang sekarang: " + mainCoreGame.uang);
                }
                else
                {
                    Debug.Log("Uang tidak cukup untuk melunasi pinjaman dengan ID " + idPinjaman);
                }
            }
        }
    }
}