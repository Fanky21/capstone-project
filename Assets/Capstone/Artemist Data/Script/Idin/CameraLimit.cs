// using UnityEngine;
// using Cinemachine; // Pastikan menggunakan namespace Cinemachine
// using UnityEngine.Tilemaps;

// public class StrictCameraBoundary : MonoBehaviour  
// {  
//     [SerializeField] private Tilemap boundaryTilemap;  
//     private CinemachineVirtualCamera vcam; // Gunakan tipe yang benar
//     private float halfWidth, halfHeight;

//     void Start()  
//     {  
//         // Dapatkan referensi ke CinemachineVirtualCamera
//         vcam = GetComponent<CinemachineVirtualCamera>();  
//         halfHeight = vcam.m_Lens.OrthographicSize;  
//         halfWidth = halfHeight * Screen.width / Screen.height;  

//         // Pastikan boundaryTilemap sudah ter-compress
//         if (boundaryTilemap != null)
//             boundaryTilemap.CompressBounds();
//     }  

//     void LateUpdate()  
//     {  
//         if (boundaryTilemap == null) return;  

//         // Hitung batas tilemap
//         Bounds bounds = boundaryTilemap.localBounds;
//         float minX = bounds.min.x + halfWidth;
//         float maxX = bounds.max.x - halfWidth;
//         float minY = bounds.min.y + halfHeight;
//         float maxY = bounds.max.y - halfHeight;

//         // Kunci posisi kamera
//         Vector3 pos = transform.position; // Gunakan transform kamera langsung
//         pos.x = Mathf.Clamp(pos.x, minX, maxX);
//         pos.y = Mathf.Clamp(pos.y, minY, maxY);
//         transform.position = pos;

//         // Debug visual
//         Debug.DrawLine(new Vector3(minX, minY), new Vector3(maxX, maxY), Color.green, 0.1f);
//     }  
// }