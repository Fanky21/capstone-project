using UnityEngine;
using UnityEditor;
using TMPro;
using System.Reflection; // Tambahkan ini untuk reflection

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    private int desiredUang; // Uang yang ingin disimpan
    private float desiredHealth; // Health yang ingin disimpan
    private float desiredStamina; 
    private FieldInfo currentHealthField;
    private FieldInfo currentStaminaField;

    void OnEnable()
    {
        currentHealthField = typeof(Player).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);
        currentStaminaField = typeof(Player).GetField("currentStamina", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Editor Uang Player", EditorStyles.boldLabel);

        desiredUang = EditorGUILayout.IntField("Uang yang Diinginkan", desiredUang);

        if (GUILayout.Button("Simpan Uang"))
        {
            SaveDesiredUang();
        }

        if (GUILayout.Button("Muat Uang dari Save"))
        {
            LoadUangFromSave();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Health Player", EditorStyles.boldLabel);
        desiredHealth = EditorGUILayout.FloatField("Health yang Diinginkan", desiredHealth);

        if (GUILayout.Button("Simpan Health"))
        {
            SaveDesiredHealth();
        }
        if (GUILayout.Button("Reset Health ke 100"))
        {
            ResetHealthToMax();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Stamina Player", EditorStyles.boldLabel);
        desiredStamina = EditorGUILayout.FloatField("Stamina yang Diinginkan", desiredStamina);

        if (GUILayout.Button("Simpan Stamina"))
        {
            SaveDesiredStamina();
        }
        if (GUILayout.Button("Reset Stamina ke 100"))
        {
            ResetStaminaToMax();
        }
    }

    private void SaveDesiredUang()
    {
        Player player = (Player)target;
        player.uang = desiredUang;

        if (player.uangPlayer != null)
        {
            player.uangPlayer.text = player.uang.ToString();
        }

        PlayerPrefs.SetInt("Uang", player.uang);
        PlayerPrefs.Save();

        Debug.Log("Uang pemain disimpan ke: " + player.uang);
    }

    private void LoadUangFromSave()
    {
        Player player = (Player)target;
        player.uang = PlayerPrefs.GetInt("Uang", 1000); // Default 1000 jika tidak ada

        // Update UI uang
        if (player.uangPlayer != null)
        {
            player.uangPlayer.text = player.uang.ToString();
        }

        desiredUang = player.uang;
        Debug.Log("Uang pemain dimuat dari save: " + player.uang);
    }

    private void SaveDesiredHealth()
    {
        Player player = (Player)target;
        
        float clampedHealth = Mathf.Clamp(desiredHealth, 0f, player.maxHealth);
        currentHealthField.SetValue(player, clampedHealth);

        if (player.healthSlider != null)
        {
            player.healthSlider.value = clampedHealth;
        }

        PlayerPrefs.SetFloat("CurrentHealth", clampedHealth);
        PlayerPrefs.Save();

        Debug.Log("Health pemain disimpan ke: " + clampedHealth);
    }

    private void ResetHealthToMax()
    {
        Player player = (Player)target;

        currentHealthField.SetValue(player, player.maxHealth);

        if (player.healthSlider != null)
        {
            player.healthSlider.value = player.maxHealth;
        }

        PlayerPrefs.SetFloat("CurrentHealth", player.maxHealth);
        PlayerPrefs.Save();

        desiredHealth = player.maxHealth;

        Debug.Log("Health pemain direset ke max: " + player.maxHealth);
    }

    private void SaveDesiredStamina()
    {
        Player player = (Player)target;

        float clampedStamina = Mathf.Clamp(desiredStamina, 0f, player.maxStamina);
        currentStaminaField.SetValue(player, clampedStamina);

        if (player.staminaSlider != null)
        {
            player.staminaSlider.value = clampedStamina;
        }

        PlayerPrefs.SetFloat("CurrentStamina", clampedStamina);
        PlayerPrefs.Save();

        Debug.Log("Stamina pemain disimpan ke: " + clampedStamina);
    }

    private void ResetStaminaToMax()
    {
        Player player = (Player)target;

        currentStaminaField.SetValue(player, player.maxStamina);

        if (player.staminaSlider != null)
        {
            player.staminaSlider.value = player.maxStamina;
        }

        PlayerPrefs.SetFloat("CurrentStamina", player.maxStamina);
        PlayerPrefs.Save();

        desiredStamina = player.maxStamina;

        Debug.Log("Stamina pemain direset ke max: " + player.maxStamina);
    }
}
