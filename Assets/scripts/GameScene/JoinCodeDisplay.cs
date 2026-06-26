using UnityEngine;
using TMPro;

public class JoinCodeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeText;

    void Start()
    {
        if (RelayManager.Instance != null)
        {
            codeText.text = $"Join Code: {RelayManager.Instance.CurrentJoinCode}";
        }
    }
}