using System;
using UnityEngine;
using UnityEngine.UI;


public class ToggleButton : MonoBehaviour
{
    private bool isOn = false;   // 現在の状態（オフから開始）
    private Button button;
    public OrbitCamera rotateScript;
    public Image[] speakerImages; 

    void Start()
    {
        button = GetComponent<Button>();

        // ボタンが押されたときに Toggle メソッドを実行
        if(rotateScript != null)button.onClick.AddListener(Toggle);
    }

    void Toggle()
    {
        isOn = !isOn;  // 状態を反転

        rotateScript.enabled = isOn;
        /*色相反転
        if (isOn)
        {
            transform.GetChild(0).GetComponent<Image>().color = new Color(0f, 1f, 1f, 0f);
        }
        else
        {
            transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        }*/
    }


}
