using UnityEngine;

public class NPCWalkSound : MonoBehaviour
{
    [SerializeField] private float walkSfxInterval = 0.5f;
    [SerializeField] private string soundName = "Walk";
    private float walkSfxTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        walkSfxTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            walkSfxTimer += Time.deltaTime;
            if (walkSfxTimer >= walkSfxInterval)
            {
                walkSfxTimer = 0f;
                // Gunakan PlaySound3D dengan posisi GameObject untuk efek 3D sound
                SoundManager.Instance.PlaySound3D(soundName, transform.position);
            }
        }
    }
}
