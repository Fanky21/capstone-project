using UnityEngine;

public class TestLog : MonoBehaviour
{
    void Start()
    {
        Debug.Log("### TEST LOG: Ini adalah pesan percobaan dari TestLog.cs!", this);
        Debug.LogError("### TEST ERROR: Ini adalah pesan ERROR percobaan dari TestLog.cs!", this);
    }
}
