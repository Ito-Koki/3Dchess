using TMPro;
using UnityEngine;

public class DispCode : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.GetComponent<TextMeshProUGUI>().text = "Code:" + RelayUI.JoinCode;
    }

}
