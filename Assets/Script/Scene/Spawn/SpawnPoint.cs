using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("A unique identifier for this spawn point. E.g., 'From_Rumah_MC' or 'Spawn_Point_A'.")]
    public string spawnId;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 0.1f)); 
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 1.5f);
    }
}
