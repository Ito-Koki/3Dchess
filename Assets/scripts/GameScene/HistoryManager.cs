using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager Instance;

    private string histfile;
    public List<GameObject> Pieces = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        Instance = this;
    }

    public void LateStart()
    {
        string outputDir = Application.isEditor
                    ? Path.Combine(Application.dataPath, "Output") // エディタ中はAssets/Output
                    : Application.persistentDataPath;               // ビルド後は実行端末の保存領域

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        histfile = Path.Combine(outputDir, "hist.csv");
        File.WriteAllText(histfile, "Turn,Piece,Tile\n");
        

        //Piecesにオブジェクトを入れる
        for(int i = 1; i<= 8; i++)
        {
            Pieces.Add(GameObject.Find("ma" + i).transform.GetChild(0).gameObject);
            Pieces.Add(GameObject.Find("mb" + i).transform.GetChild(0).gameObject);
            Pieces.Add(GameObject.Find("mg" + i).transform.GetChild(0).gameObject);
            Pieces.Add(GameObject.Find("mh" + i).transform.GetChild(0).gameObject);
        }

        Record();
    }

    public void Record() // csv に (turn,  tag + piece, tile)でpieceの数だけ保存
    {
        int turn = SelectionManager.Instance.turn.Value;
        foreach (GameObject piece in Pieces)
        {
            if(piece == null) continue;
            string tile = piece.transform.parent.gameObject.name;
            string pieceInfo = piece.tag + "_" + piece.name;

            string line = $"{turn},{pieceInfo},{tile}\n";

            File.AppendAllText(histfile, line);
        }
    }

    public bool CheckLastMove(GameObject targetPawn, GameObject targetTile)
    {
        List<string> prememory = new List<string>();
        List<string> nowmemory = new List<string>();
        if (!File.Exists(histfile)) return false;
        int currentTurn = SelectionManager.Instance.turn.Value;
        string[] lines = File.ReadAllLines(histfile);
        if (lines.Length < 2) return false; // ヘッダしかない


        for(int i = 0;i < lines.Length; i++)//2つ前のターンのポーンの場所一覧を記録する
        {
            string currentLine = lines[i];
            string[] parts = currentLine.Split(',');

            string lastTurn = parts[0];
            string lastPiece = parts[1]; // 例: "black_Pawn(Clone)"
            string lastTile = parts[2];  // 例: "md5"
            string targetInfo = targetPawn.tag + "_" + targetPawn.name;
            if ((currentTurn - 1).ToString() == parts[0])
            //if (currentTurn - 1 == int.Parse(parts[0]))
            {
                if (parts[1] == targetInfo)
                {
                    prememory.Add(lastTile);
                }
            }
        }

        for(int i = 0;i < lines.Length; i++)//1つ前のターンのポーンの場所一覧を記録する
        {
            string currentLine = lines[i];
            string[] parts = currentLine.Split(',');

            string lastTurn = parts[0];
            string lastPiece = parts[1]; // 例: "black_Pawn(Clone)"
            string lastTile = parts[2];  // 例: "md5"
            string targetInfo = targetPawn.tag + "_" + targetPawn.name;
            if (currentTurn.ToString() == parts[0])//
            //if (currentTurn == int.Parse(parts[0]))
            {
                if (parts[1] == targetInfo)
                {
                    nowmemory.Add(lastTile);
                }
            }
        }

        foreach(string pretileName in prememory)//movecnt.value == 1 + pretがnowtの2前だったら〇
        {
            foreach (string nowtileName in nowmemory)
            {
                if (GameObject.Find(nowtileName).transform.childCount == 0) continue;//チェック状態にて必要
                GameObject nowtmppawn = GameObject.Find(nowtileName).transform.GetChild(0).gameObject;
                if (nowtmppawn.GetComponent<PieceTouch>().movecnt.Value == 1)
                {
                    //Debug.Log(pretileName);
                    //Debug.Log(nowtileName);
                    if (pretileName != nowtileName)
                    {
                        char[] pret = pretileName.ToCharArray();
                        char[] nowt = nowtileName.ToCharArray();
                        if (nowtmppawn.tag == "black" && nowt[1] == 'd' && pret[1] == 'b' && nowt[0] == pret[0] && nowt[2] == pret[2]) return true;
                        if (nowtmppawn.tag == "white" && nowt[1] == 'e' && pret[1] == 'g' && nowt[0] == pret[0] && nowt[2] == pret[2]) return true;
                    }
                }
            }
        }


        return false;
    }

}
