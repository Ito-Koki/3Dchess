using UnityEngine;

public class EndManager : MonoBehaviour
{
    GameObject UI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UI = GameObject.Find("fin");
    }

}
