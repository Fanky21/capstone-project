using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Splines;
using System.Collections;

[System.Serializable]
public class KendaraanData
{
    public GameObject gambarKendaraan;
    public SplineContainer jalurKendaraan; // Spline Container untuk jalur
    public float speed = 5f; // Kecepatan bergerak dari A ke B
    public bool flipX = false; // Flip sprite menghadap arah gerakan
    
    [HideInInspector]
    public float currentProgress = 0f; // Progress saat ini di spline (0-1)
    [HideInInspector]
    public bool isActive = false; // Apakah kendaraan sedang aktif
    [HideInInspector]
    public float spawnTimer = 0f; // Timer untuk respawn
}

[System.Serializable]
public class GrupKendaraan
{
    public string namaGrup = "Grup Kendaraan 1"; // Nama untuk identifikasi grup
    public List<KendaraanData> kendaraanList = new List<KendaraanData>();
    public float spawnInterval = 3f; // Delay sebelum grup pertama kali spawn
    
    [HideInInspector]
    public int currentKendaraanIndex = 0; // Index kendaraan yang sedang berjalan
    [HideInInspector]
    public bool isGrupActive = false; // Apakah grup sedang aktif
    [HideInInspector]
    public float grupSpawnTimer = 0f; // Timer untuk delay awal grup
}

public class Kendaraan : MonoBehaviour
{
    public List<GrupKendaraan> grupKendaraanList = new List<GrupKendaraan>();
    
    private void Start()
    {
        // Initialize semua grup kendaraan
        foreach (GrupKendaraan grup in grupKendaraanList)
        {
            grup.currentKendaraanIndex = 0;
            grup.isGrupActive = true;
            grup.grupSpawnTimer = 0f;
            
            // Initialize semua kendaraan dalam grup
            foreach (KendaraanData kendaraan in grup.kendaraanList)
            {
                if (kendaraan.gambarKendaraan != null)
                {
                    kendaraan.gambarKendaraan.SetActive(false);
                    kendaraan.spawnTimer = 0f;
                    kendaraan.isActive = false;
                    kendaraan.currentProgress = 0f;
                }
            }
        }
    }
    
    private void Update()
    {
        foreach (GrupKendaraan grup in grupKendaraanList)
        {
            if (!grup.isGrupActive || grup.kendaraanList.Count == 0)
                continue;
            
            // Cek apakah grup sudah bisa spawn (tunggu spawnInterval)
            if (grup.grupSpawnTimer < grup.spawnInterval)
            {
                grup.grupSpawnTimer += Time.deltaTime;
                
                if (grup.grupSpawnTimer >= grup.spawnInterval)
                {
                    // Waktu spawn, mulai spawn kendaraan pertama
                    SpawnKendaraan(grup.kendaraanList[grup.currentKendaraanIndex]);
                }
                continue;
            }
            
            // Proses kendaraan saat ini dalam grup
            KendaraanData kendaraanAktif = grup.kendaraanList[grup.currentKendaraanIndex];
            
            if (kendaraanAktif.jalurKendaraan == null || kendaraanAktif.gambarKendaraan == null)
                continue;
            
            // Jika kendaraan aktif, gerakan melalui spline
            if (kendaraanAktif.isActive)
            {
                MoveKendaraan(kendaraanAktif);
                
                // Cek apakah kendaraan sudah selesai
                if (kendaraanAktif.currentProgress >= 1f)
                {
                    // Kendaraan selesai, despawn
                    DespawnKendaraan(kendaraanAktif);
                    
                    // Lanjut ke kendaraan berikutnya (tanpa delay)
                    MoveToNextKendaraan(grup);
                }
            }
        }
    }
    
    private void SpawnKendaraanDalamGrup(GrupKendaraan grup, int index)
    {
        if (index >= 0 && index < grup.kendaraanList.Count)
        {
            grup.currentKendaraanIndex = index;
            KendaraanData kendaraan = grup.kendaraanList[index];
            SpawnKendaraan(kendaraan);
            Debug.Log("Grup '" + grup.namaGrup + "' spawn kendaraan #" + index);
        }
    }
    
    private void MoveToNextKendaraan(GrupKendaraan grup)
    {
        // Pindah ke kendaraan berikutnya
        grup.currentKendaraanIndex++;
        
        if (grup.currentKendaraanIndex < grup.kendaraanList.Count)
        {
            // Ada kendaraan berikutnya, spawn langsung (tanpa delay)
            KendaraanData nextKendaraan = grup.kendaraanList[grup.currentKendaraanIndex];
            SpawnKendaraan(nextKendaraan);
            Debug.Log("Grup '" + grup.namaGrup + "' lanjut ke kendaraan #" + grup.currentKendaraanIndex);
        }
        else
        {
            // Semua kendaraan sudah selesai, reset untuk loop berikutnya
            Debug.Log("Grup '" + grup.namaGrup + "' semua kendaraan selesai, menunggu spawn interval berikutnya");
            grup.currentKendaraanIndex = 0;
            grup.grupSpawnTimer = 0f; // Reset timer untuk spawn ulang
        }
    }
    
    private void SpawnKendaraan(KendaraanData kendaraan)
    {
        kendaraan.isActive = true;
        kendaraan.currentProgress = 0f;
        kendaraan.spawnTimer = 0f;
        kendaraan.gambarKendaraan.SetActive(true);
        
        // Set posisi awal di titik A (progress 0)
        UpdateKendaraanPosition(kendaraan);
    }
    
    private void MoveKendaraan(KendaraanData kendaraan)
    {
        // Hitung jarak yang harus ditempuh
        Spline spline = kendaraan.jalurKendaraan.Spline;
        float splineLength = spline.GetLength();
        
        // Update progress berdasarkan speed
        float distancePerFrame = kendaraan.speed * Time.deltaTime;
        kendaraan.currentProgress += distancePerFrame / splineLength;
        
        // Update posisi kendaraan
        UpdateKendaraanPosition(kendaraan);
    }
    
    private void UpdateKendaraanPosition(KendaraanData kendaraan)
    {
        Spline spline = kendaraan.jalurKendaraan.Spline;
        
        // Clamp progress antara 0-1
        float progress = Mathf.Clamp01(kendaraan.currentProgress);
        
        // Dapatkan posisi di spline berdasarkan progress
        Vector3 position = (Vector3)spline.EvaluatePosition(progress);
        
        // Dapatkan tangent untuk rotasi
        Vector3 tangent = (Vector3)spline.EvaluateTangent(progress);
        
        // Aplikasikan transform dari SplineContainer ke world space
        Transform splineTransform = kendaraan.jalurKendaraan.transform;
        Vector3 worldPosition = splineTransform.TransformPoint(position);
        
        // Untuk 2D: set Z ke kendaraan Z asli atau 0
        worldPosition.z = kendaraan.gambarKendaraan.transform.position.z;
        kendaraan.gambarKendaraan.transform.position = worldPosition;
        
        // Update flip menghadap arah pergerakan (untuk 2D sprite)
        if (tangent != Vector3.zero)
        {
            // Flip sprite berdasarkan arah X dari tangent
            SpriteRenderer spriteRenderer = kendaraan.gambarKendaraan.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = tangent.x < 0;
            }
        }
    }
    
    private void DespawnKendaraan(KendaraanData kendaraan)
    {
        kendaraan.isActive = false;
        kendaraan.gambarKendaraan.SetActive(false);
        kendaraan.currentProgress = 0f;
        kendaraan.spawnTimer = 0f;
    }
    
    #region Editor Helper Functions
    /// <summary>
    /// Reset semua kendaraan dalam semua grup ke state awal
    /// </summary>
    public void ResetAllVehicles()
    {
        foreach (GrupKendaraan grup in grupKendaraanList)
        {
            grup.currentKendaraanIndex = 0;
            grup.isGrupActive = true;
            grup.grupSpawnTimer = 0f;
            
            foreach (KendaraanData kendaraan in grup.kendaraanList)
            {
                if (kendaraan.gambarKendaraan != null)
                {
                    kendaraan.gambarKendaraan.SetActive(false);
                    kendaraan.isActive = false;
                    kendaraan.currentProgress = 0f;
                    kendaraan.spawnTimer = 0f;
                }
            }
        }
        Debug.Log("Reset semua kendaraan dari semua grup!");
    }
    
    /// <summary>
    /// Spawn kendaraan tertentu dalam grup untuk testing
    /// </summary>
    public void TestSpawnVehicleInGrup(int grupIndex, int kendaraanIndex)
    {
        if (grupIndex >= 0 && grupIndex < grupKendaraanList.Count)
        {
            GrupKendaraan grup = grupKendaraanList[grupIndex];
            SpawnKendaraanDalamGrup(grup, kendaraanIndex);
        }
    }
    #endregion
}
