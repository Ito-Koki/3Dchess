using UnityEngine;

public class AssistPage : MonoBehaviour
{
    public GameObject AssistUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UISwitch()
    {
        if (!AssistUI.activeSelf)
        {
            AssistUI.SetActive(true);
        }
        else
        {
            AssistUI.SetActive(false);
        }
    }
}
