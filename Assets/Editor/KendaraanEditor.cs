using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;

[CustomEditor(typeof(Kendaraan))]
public class KendaraanEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        Kendaraan kendaraan = (Kendaraan)target;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("=== KENDARAAN EDITOR TOOLS ===", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // List grup kendaraan dengan kontrol
        EditorGUILayout.LabelField("Grup Kendaraan", EditorStyles.boldLabel);
        
        if (kendaraan.grupKendaraanList.Count > 0)
        {
            for (int g = 0; g < kendaraan.grupKendaraanList.Count; g++)
            {
                GrupKendaraan grup = kendaraan.grupKendaraanList[g];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Header grup
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(grup.namaGrup, EditorStyles.boldLabel);
                string statusGrup = grup.isGrupActive ? "🟢 Aktif" : "⚪ Nonaktif";
                EditorGUILayout.LabelField(statusGrup, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("Progress: " + grup.currentKendaraanIndex + " / " + grup.kendaraanList.Count);
                
                // Spawn Interval untuk grup
                grup.spawnInterval = EditorGUILayout.FloatField("Spawn Interval (Delay Awal)", grup.spawnInterval);
                
                // Progress timer (read-only)
                EditorGUILayout.LabelField("Spawn Timer: " + grup.grupSpawnTimer.ToString("F2") + "s");
                
                // List kendaraan dalam grup
                if (grup.kendaraanList.Count > 0)
                {
                    for (int i = 0; i < grup.kendaraanList.Count; i++)
                    {
                        KendaraanData vehicleData = grup.kendaraanList[i];
                        
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        // Header dengan nama kendaraan
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Kendaraan #" + i, EditorStyles.boldLabel);
                        
                        // Status
                        string status = vehicleData.isActive ? "🟢 Active" : "⚪ Idle";
                        string isCurrentMarker = (i == grup.currentKendaraanIndex) ? " [SEKARANG]" : "";
                        EditorGUILayout.LabelField(status + isCurrentMarker, GUILayout.Width(150));
                        
                        EditorGUILayout.EndHorizontal();
                        
                        // Tampilkan informasi
                        EditorGUILayout.ObjectField("GameObject", vehicleData.gambarKendaraan, typeof(GameObject), true);
                        EditorGUILayout.ObjectField("Jalur (Spline)", vehicleData.jalurKendaraan, typeof(SplineContainer), true);
                        
                        vehicleData.speed = EditorGUILayout.FloatField("Speed", vehicleData.speed);
                        vehicleData.flipX = EditorGUILayout.Toggle("Flip X", vehicleData.flipX);
                        
                        // Progress bar
                        EditorGUILayout.LabelField("Progress: " + vehicleData.currentProgress.ToString("F2"));
                        EditorGUILayout.Slider(vehicleData.currentProgress, 0f, 1f);
                        
                        // Control buttons
                        EditorGUILayout.BeginHorizontal();
                        
                        GUI.backgroundColor = new Color(0.3f, 1f, 0.3f); // Hijau
                        if (GUILayout.Button("Spawn", GUILayout.Height(30)))
                        {
                            kendaraan.TestSpawnVehicleInGrup(g, i);
                        }
                        GUI.backgroundColor = Color.white;
                        
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.3f); // Orange
                        if (GUILayout.Button("Reset", GUILayout.Height(30)))
                        {
                            vehicleData.isActive = false;
                            if (vehicleData.gambarKendaraan != null)
                                vehicleData.gambarKendaraan.SetActive(false);
                            vehicleData.currentProgress = 0f;
                            vehicleData.spawnTimer = 0f;
                        }
                        GUI.backgroundColor = Color.white;
                        
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Tidak ada kendaraan dalam grup ini!", MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Tidak ada grup kendaraan!", MessageType.Warning);
        }
        
        EditorGUILayout.Space(15);
        
        // Reset semua
        EditorGUILayout.LabelField("Kontrol Umum", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Merah
        
        if (GUILayout.Button("RESET ALL VEHICLES", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Konfirmasi Reset",
                "Apakah Anda yakin ingin mereset SEMUA kendaraan?",
                "Ya, Reset Semua",
                "Batal"))
            {
                kendaraan.ResetAllVehicles();
                EditorUtility.DisplayDialog("Berhasil", "Semua kendaraan telah direset!", "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
        
        // Tips
        EditorGUILayout.Space(15);
    }
    
    // Visualisasi di Scene view
    private void OnSceneGUI()
    {
        Kendaraan kendaraan = (Kendaraan)target;
        
        // Gambar spline path untuk setiap grup
        foreach (GrupKendaraan grup in kendaraan.grupKendaraanList)
        {
            foreach (KendaraanData vehicleData in grup.kendaraanList)
            {
                if (vehicleData.jalurKendaraan == null)
                    continue;
                
                Spline spline = vehicleData.jalurKendaraan.Spline;
                
                // Gambar jalur dengan garis (2D)
                Handles.color = Color.cyan;
                int segmentCount = 50;
                Vector3 lastPos = (Vector3)spline.EvaluatePosition(0f);
                lastPos.z = 0f;
                
                for (int i = 1; i <= segmentCount; i++)
                {
                    float t = i / (float)segmentCount;
                    Vector3 pos = (Vector3)spline.EvaluatePosition(t);
                    pos.z = 0f;
                    Handles.DrawLine(lastPos, pos, 2f);
                    lastPos = pos;
                }
                
                // Gambar titik awal (A) dan akhir (B) dengan circle (2D)
                Handles.color = Color.green;
                Vector3 startPos = (Vector3)spline.EvaluatePosition(0f);
                startPos.z = 0f;
                Handles.CircleHandleCap(0, startPos, Quaternion.identity, 0.5f, EventType.Repaint);
                
                Handles.color = Color.red;
                Vector3 endPos = (Vector3)spline.EvaluatePosition(1f);
                endPos.z = 0f;
                Handles.CircleHandleCap(0, endPos, Quaternion.identity, 0.5f, EventType.Repaint);
                
                // Label
                Handles.color = Color.green;
                Handles.Label(startPos + Vector3.up * 0.5f, "Titik A (Spawn)");
                
                Handles.color = Color.red;
                Handles.Label(endPos + Vector3.up * 0.5f, "Titik B (Lenyap)");
            }
        }
    }
}
