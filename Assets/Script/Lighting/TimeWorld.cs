using UnityEngine;
using TMPro;

public class TimeWorld : MonoBehaviour
{
    [Header("UI Jam Dunia")]
    public TextMeshProUGUI jamDunia;

    private void Update()
    {
        if (jamDunia == null) return;

        if (DayNightCycle.Instance == null)
        {
            jamDunia.text = "00 : 00";
            return;
        }

        float worldTime = DayNightCycle.Instance.WorldTime;

        int hours = Mathf.FloorToInt(worldTime / 60f);
        int minutes = Mathf.FloorToInt(worldTime % 60f);

        jamDunia.text = $"{hours:00} : {minutes:00}";
    }
}
