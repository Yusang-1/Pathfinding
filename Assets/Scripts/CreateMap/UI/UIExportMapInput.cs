using UnityEngine;
using TMPro;

public class UIExportMapInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    public string GetValue()
    {
        return inputField.text;
    }
}
