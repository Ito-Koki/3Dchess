using UnityEngine;
using TMPro;
using System.ComponentModel;
using UnityEngine.UI;
using System.Collections;


public class LogText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DisplayText; // t/mが表示されているテキスト
    [SerializeField] private ScrollRect scrollRect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static LogText Instance;


    private int Count = 1;
    private int TurnText = 1;

    private void Awake()
    {
        Instance = this;
    }

    public void AddRecord(string pieceName, string toTile, bool take, bool check, bool checkmate)
    {
        string InputText = "";
        if (pieceName.Contains("King")) InputText = "K"; 
        if (pieceName.Contains("Queen")) InputText = "Q";
        if (pieceName.Contains("Rook")) InputText = "R";
        if (pieceName.Contains("Bishop")) InputText = "B";
        if (pieceName.Contains("Knight")) InputText = "N";
        if (pieceName.Contains("Pawn")) InputText = "P";


        //本来のマス表記への変換
        char level = toTile[0];
        char row = toTile[1];
        char col = toTile[2];
        int x = col - '1';     // 0～7
        int y = row - 'a';     // 0～7
        char file = (char)('a' + x);
        int rank = 8 - y;
        toTile = $"{level}{file}{rank}";

        if (Count % 2 != 0) DisplayText.text += $"{TurnText}";
        else DisplayText.text += "  ";

        DisplayText.text += $". {InputText}";
        if (take) DisplayText.text += $"x";
        DisplayText.text += $"{toTile}";
        if (check && !checkmate) DisplayText.text += $"+";
        if (checkmate) DisplayText.text += $"#";

        if (Count % 2 == 0)
        {
            DisplayText.text += $"\n";
            TurnText++;
        }
        Count++;



        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
