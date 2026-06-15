using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class TyleNumbering : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GameObject boardcore = this.gameObject;
        GameObject[,] tyles = new GameObject[8, 8];


        for (int i = 0; i < boardcore.transform.childCount; i++)//ƒ}ƒX–Ú‚Ì”‚¾‚¯loop
        {
            Transform child = transform.Find("Cube (" + i + ")");
            //Debug.Log(child.gameObject.name);

            char colnum = (char)('a' + i / 8);
            int rownum = (i % 8) + 1;
            if (boardcore.name == "board_top")
            {
                child.gameObject.name = "t" + colnum.ToString() + rownum.ToString();
            }
            else if(boardcore.name == "board_mid")
            {
                child.gameObject.name = "m" + colnum.ToString() + rownum.ToString();
            }
            else
            {
                child.gameObject.name = "b" + colnum.ToString() + rownum.ToString();
            }
            child.gameObject.tag = "tile";
        }
    }
}
