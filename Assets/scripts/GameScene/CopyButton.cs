
using TMPro;
using UnityEngine;

public class CopyButton : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;

    public void CopyCode()
    {
        GUIUtility.systemCopyBuffer = codeText.text;
        Debug.Log($"Copied: {codeText.text}");
    }
}