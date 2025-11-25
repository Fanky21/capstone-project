using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FurnitureManager))]
public class FurnitureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        FurnitureManager furnitureManager = (FurnitureManager)target;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("=== FURNITURE EDITOR TOOLS ===", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Reset Individual Furniture
        EditorGUILayout.LabelField("Reset Furniture Tertentu", EditorStyles.boldLabel);
        
        if (furnitureManager.furnitureList.Count > 0)
        {
            for (int i = 0; i < furnitureManager.furnitureList.Count; i++)
            {
                FurnitureBeli furniture = furnitureManager.furnitureList[i];
                
                EditorGUILayout.BeginHorizontal();
                
                // Tampilkan nama furniture
                EditorGUILayout.LabelField(furniture.namaFurniture, GUILayout.Width(150));
                
                // Status
                bool isPurchased = furnitureManager.IsFurniturePurchased(furniture.namaFurniture);
                string status = isPurchased ? "✓ Dibeli" : "✗ Belum Dibeli";
                EditorGUILayout.LabelField(status, isPurchased ? EditorStyles.helpBox : EditorStyles.label, GUILayout.Width(100));
                
                // Button Reset
                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f); // Merah muda
                if (GUILayout.Button("Reset", GUILayout.Width(80)))
                {
                    furnitureManager.ResetFurniture(furniture.namaFurniture);
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Tidak ada furniture dalam list!", MessageType.Warning);
        }
        
        EditorGUILayout.Space(15);
        
        // Reset All Furniture
        EditorGUILayout.LabelField("Reset Semua Furniture", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Merah lebih terang
        
        if (GUILayout.Button("RESET ALL FURNITURE", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Konfirmasi Reset",
                "Apakah Anda yakin ingin mereset SEMUA furniture?\n\nIni akan menghapus data pembelian semua furniture.",
                "Ya, Reset Semua",
                "Batal"))
            {
                furnitureManager.ResetAllFurniture();
                EditorUtility.DisplayDialog("Berhasil", "Semua furniture telah direset!", "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
    }
}
