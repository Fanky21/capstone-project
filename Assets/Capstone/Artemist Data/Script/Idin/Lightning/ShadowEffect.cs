using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public class GenerateShadowCasters : MonoBehaviour
{
    public Tilemap tilemap;

    [System.Obsolete]

    void Start()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                GameObject shadowObj = new GameObject("ShadowCaster");
                shadowObj.transform.parent = transform;
                shadowObj.transform.localPosition = tilemap.CellToLocal(pos) + new Vector3(0.5f, 0.5f, 0);
                var sc = shadowObj.AddComponent<ShadowCaster2D>();
                sc.useRendererSilhouette = false; // Gunakan shape manual

                // Unity's built-in ShadowCaster2D does not expose a public API to set the shape at runtime.
                // As a workaround, you can add a PolygonCollider2D to define the shadow shape.
                var polygon = shadowObj.AddComponent<PolygonCollider2D>();
                polygon.points = new Vector2[]
                {
                    new Vector2(-0.5f, -0.5f),
                    new Vector2(-0.5f,  0.5f),
                    new Vector2( 0.5f,  0.5f),
                    new Vector2( 0.5f, -0.5f)
                };
                // Note: ShadowCaster2D will use the shape from the PolygonCollider2D if useRendererSilhouette is false.
            }
        }
    }
}