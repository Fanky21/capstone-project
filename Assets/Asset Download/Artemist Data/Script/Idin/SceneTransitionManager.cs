using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    private string sceneAsal;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void goToScene(string namaScene)
    {
        SceneManager.LoadScene(namaScene);
    }

    public void SetSceneAsal(string namaScene)
    {
        sceneAsal = namaScene;
    }

    public string GetSceneAsal()
    {
        return sceneAsal;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string spawnObjectName = "";

        // Tentukan nama spawn point berdasarkan scene asal
        if (sceneAsal == "City1")
        {
            spawnObjectName = "SpawnDariKota";
        }
        else if (sceneAsal == "MiniBoss_Room")
        {
            spawnObjectName = "SpawnDariMiniboss";
        }
        // else if (sceneAsal == "KantorErik")
        // {
        //     spawnObjectName = "SpawnDariKantorErik";
        // }
        else if (sceneAsal == "Dungeon")
        {
            spawnObjectName = "SpawnDariDungeon";
        }
        else if (sceneAsal == "RuangTamu")
        {
            spawnObjectName = "SpawnDariRuangTamu";
        }
        else if (sceneAsal == "Dapur")
        {
            spawnObjectName = "SpawnDariDapur";
        }
        else if (sceneAsal == "Kantor")
        {
            spawnObjectName = "SpawnDariKantor";
        }
        else if (sceneAsal == "Kantor-Quest")
        {
            spawnObjectName = "SpawnDariKantor";
        }
        else if (sceneAsal == "Dapur-Quest")
        {
            spawnObjectName = "SpawnDariDapur";
        }
        else if (sceneAsal == "RuangTamu-Quest")
        {
            spawnObjectName = "SpawnDariRuangTamu";
        }

        Transform spawnPoint = GameObject.Find(spawnObjectName)?.transform;

        if (spawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = spawnPoint.position;
                Debug.Log($"Player dipindah ke {spawnObjectName} dari {sceneAsal}");
            }
        }
        else
        {
            Debug.LogWarning($"Spawn point tidak ditemukan untuk sceneAsal: {sceneAsal}");
        }
    }
}
