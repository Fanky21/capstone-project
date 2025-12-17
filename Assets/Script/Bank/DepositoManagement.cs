using TMPro;
using UnityEngine;

public class DepositoManagement : MonoBehaviour
{
    public CoreDeposito coreDeposito;
    public TMP_InputField jumlahUangInput;
    public TMP_InputField durasiHariInput;

    public void CreateNewDeposito(float jumlahUangInput, int durasiHariInput, float customInterestRate = 6f)
    {
        if (coreDeposito != null)
        {
            bool success = coreDeposito.StartDeposito(jumlahUangInput, durasiHariInput, customInterestRate);
            if (success)
            {
                Debug.Log("Deposito baru berhasil dibuat.");
            }
            else
            {
                Debug.LogWarning("Gagal membuat deposito baru.");
            }
        }
        else
        {
            Debug.LogError("CoreDeposito tidak ditemukan.");
        }
    }
}
