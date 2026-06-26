using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;


public class RoleManager : MonoBehaviour
{

    private static Dictionary<string, GameObject> tileCache = new Dictionary<string, GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static List<string> PreAct(string tag, string name, char[] tile, bool fromcheck, GameObject piece)
    {
        if (!piece.activeSelf) return null;
        List<string> movable_tiles = new List<string>();
        //ここに役ごとに移動可能マス計算する
        if (tag == "black" && name == "Pawn(Clone)") movable_tiles.AddRange(BlackPawnRule(tag, tile));
        if (tag == "white" && name == "Pawn(Clone)") movable_tiles.AddRange(WhitePawnRule(tag, tile));
        if (name == "King(Clone)") movable_tiles.AddRange(KingRule(tag, tile));
        if (name == "Queen(Clone)") movable_tiles.AddRange(QueenRule(tag, tile));
        if (name == "Bishop(Clone)") movable_tiles.AddRange(BishopRule(tag, tile));
        if (name == "Rook(Clone)") movable_tiles.AddRange(RookRule(tag, tile));
        if (name == "Knight(Clone)") movable_tiles.AddRange(KnightRule(tag, tile));

        GameObject currentTile = piece.transform.parent.gameObject;
        NetworkObject _piece = piece.GetComponent<NetworkObject>();


        if (fromcheck)//チェックがかかっていると動けるタイル候補を減らす
        {
            List<string> toRemove = new List<string>();

            foreach (string pretile in movable_tiles)
            {
                GameObject premoveTile = GetTile(pretile);
                if (premoveTile == null) continue;

                // 移動先の駒もタグで無効化（SetActiveを使わない）
                GameObject capturedPiece = null;
                string capturedOriginalTag = null;
                if (premoveTile.transform.childCount > 0)
                {
                    capturedPiece = premoveTile.transform.GetChild(0).gameObject;
                    capturedOriginalTag = capturedPiece.tag;
                    capturedPiece.tag = "Untagged";
                }

                string realTileName = piece.transform.parent.name;
                bool inCheck = IsKingInCheckVirtual(tag, piece, realTileName, pretile);

                // タグを元に戻す
                if (capturedPiece != null)
                    capturedPiece.tag = capturedOriginalTag;

                if (inCheck)
                    toRemove.Add(pretile);
            }

            foreach (string r in toRemove)
                movable_tiles.Remove(r);
        }



        /*
        foreach (string pretile in movable_tiles)
        {
            Debug.Log(pretile);
        }*/

        return movable_tiles;
    }

    public static void BuildTileCache()
    {
        tileCache.Clear();
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("tile")) // タイルに"tile"タグが必要
        {
            tileCache[tile.name] = tile;
        }
    }

    public static GameObject GetTile(string name)
    {
        if (tileCache.TryGetValue(name, out GameObject tile))
            return tile;
        // フォールバック(キャッシュ漏れ対策)
        tile = GameObject.Find(name);
        if (tile != null) tileCache[name] = tile;
        return tile;
    }



    private static bool IsKingInCheckVirtual(string movedTag, GameObject movingPiece, string fromTile, string toTile)
    {
        string enemyTag = movedTag == "black" ? "white" : "black";

        string kingTile;
        if (movingPiece.name == "King(Clone)")
        {
            kingTile = toTile;
        }
        else
        {
            GameObject king = GameObject.FindGameObjectsWithTag(movedTag)
                .FirstOrDefault(o => o.name == "King(Clone)");
            if (king == null) return false;
            kingTile = king.transform.parent.name;
        }

        // SetActive の代わりにタグで「仮想的に除外」
        string originalTag = movingPiece.tag;
        movingPiece.tag = "Untagged"; // ← NetworkTransformに検知されない

        bool inCheck = false;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            if (!enemy.activeSelf) continue;

            char[] enemyTile = enemy.transform.parent.name.ToCharArray();
            List<string> enemyMoves = PreAct(enemyTag, enemy.name, enemyTile, false, enemy);
            if (enemyMoves == null) continue;

            if (enemyMoves.Contains(kingTile))
            {
                inCheck = true;
                break;
            }
        }

        // タグを元に戻す
        movingPiece.tag = originalTag;
        return inCheck;
    }
    /*
    private static bool IsKingInCheckAfterMove(string movedPieceTag)
    {
        string enemyTag = movedPieceTag == "black" ? "white" : "black";
        GameObject king = GetTileGameObjectsWithTag(movedPieceTag).FirstOrDefault(obj => obj.name == "King(Clone)");
        if (king == null) return false;
        string kingTile = king.transform.parent.name;

        GameObject[] enemyPieces = GetTileGameObjectsWithTag(enemyTag);
        foreach (GameObject piece in enemyPieces)
        {
            if (!piece.activeSelf) continue;
            char[] tile = piece.transform.parent.name.ToCharArray();
            List<string> moves = PreAct(enemyTag, piece.name, tile, false, piece);
            if (moves.Contains(kingTile)) return true;
        }
        return false;
    }*/

    static bool PieceCheck(char layer, char row, char col, string tag, bool emptyCheck)
    {
        if (row < 'a' || row > 'h') return false;
        if (col < '1' || col > '8') return false;
        GameObject tile = GetTile("" + layer + row + col);
        if (tile == null) return false;

        Transform t = tile.transform;
        if (t.childCount == 1)
        {
            Transform child = t.GetChild(0);
            if (child.gameObject.tag != "Untagged")
            {
                if (emptyCheck) return false;
                return child.tag != tag;
            }
        }
        return true;
    }

    static bool PawnPieceCheck(char layer, char row, char col, string tag, bool iskill)
    {
        if (row < 'a' || row > 'h') return false;
        if (col < '1' || col > '8') return false;

        GameObject tile = GetTile("" + layer + row + col);
        if (tile == null) return false;

        Transform t = tile.transform;
        if (t.childCount == 1)
        {
            Transform child = t.GetChild(0);
            if(child.gameObject.tag != "Untagged")
            {
                if (tile.transform.GetChild(0).tag != tag && iskill) return true;
                else return false;
            }
        }
        else
        {
            if (!iskill) return true;
        }

        return false;
    }

    static List<string> BlackPawnRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();

        char layer = _tile[0];
        char row   = _tile[1];
        char col   = _tile[2];

        if(layer == 'm')
        {
            if (PawnPieceCheck('t', row, col, _tag, false)) results.Add("t" + row + col);
            if (PawnPieceCheck('b', row, col, _tag, false)) results.Add("b" + row + col);

            if (PawnPieceCheck('t', (char)(row + 1), col, _tag, true)) results.Add("t" + (char)(row + 1) + col);//取る動き
            if (PawnPieceCheck('b', (char)(row + 1), col, _tag, true)) results.Add("b" + (char)(row + 1) + col);//取る動き

            GameObject currentTileObj = GetTile("" + layer + row + col);
            if (currentTileObj.transform.childCount > 0 && currentTileObj.transform.GetChild(0).GetComponent<PieceTouch>().movecnt.Value == 0)
            {
                if (PawnPieceCheck(layer, (char)(row + 1), col, _tag, false) && PawnPieceCheck(layer, (char)(row + 2), col, _tag, false)) results.Add("" + layer + (char)(row + 2) + col); //初期状態での2マス移動
            }
        }
        else if (layer == 'b' || layer == 't')
        {
            if (PawnPieceCheck('m', row, col, _tag, false)) results.Add("m" + row + col);

            if (PawnPieceCheck('m', (char)(row + 1), col, _tag, true)) results.Add("m" + (char)(row + 1) + col);//取る動き

            //アンパッサン状態
            if ((char)(row + 1) >= 'a' && (char)(row + 1) <= 'h')
            {
                if (GetTile("m" + (char)(row + 1) + col).transform.childCount == 0 && GetTile("m" + row + col).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
                {
                    if (GetTile("m" + row + col).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
                    {
                        GameObject enemy = GetTile("m" + row + col).transform.GetChild(0).gameObject;
                        if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない = 自駒ではない
                        {
                            ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                            NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                            if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                            {
                                results.Add("m" + (char)(row + 1) + col);
                            }
                        }
                    }
                }
            }
            //
        }

        if (PawnPieceCheck(layer, (char)(row + 1), col, _tag, false)) results.Add("" + layer + (char)(row + 1) + col);

        if (PawnPieceCheck(layer, (char)(row + 1), (char)(col + 1), _tag, true)) results.Add("" + layer + (char)(row + 1) + (char)(col + 1));//取る動き
        if (PawnPieceCheck(layer, (char)(row + 1), (char)(col - 1), _tag, true)) results.Add("" + layer + (char)(row + 1) + (char)(col - 1));//取る動き

        //アンパッサン状態
        if ((row + 1 <= 'h' && col + 1 <= '8' && GetTile("" + layer + (char)(row + 1) + (char)(col + 1)).transform.childCount == 0) && GetTile("" + layer + row + (char)(col + 1)).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
        {
            if (GetTile("" + layer + row + (char)(col + 1)).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
            {
                GameObject enemy = GetTile("" + layer + row + (char)(col + 1)).transform.GetChild(0).gameObject;
                if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない
                {
                    ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                    NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                    if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                    {
                        results.Add("" + layer + (char)(row + 1) + (char)(col + 1));
                    }
                }
            }
        }
        //

        //アンパッサン状態
        if ((row + 1 <= 'h' && col - 1 >= '1' && GetTile("" + layer + (char)(row + 1) + (char)(col - 1)).transform.childCount == 0) && GetTile("" + layer + row + (char)(col - 1)).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
        {
            if (GetTile("" + layer + row + (char)(col - 1)).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
            {
                GameObject enemy = GetTile("" + layer + row + (char)(col - 1)).transform.GetChild(0).gameObject;
                if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない
                {
                    ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                    NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                    if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                    {
                        results.Add("" + layer + (char)(row + 1) + (char)(col - 1));
                    }
                }
            }
        }
        //


        return results;

    }

    static List<string> WhitePawnRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();

        char layer = _tile[0];
        char row = _tile[1];
        char col = _tile[2];

        if (layer == 'm')
        {
            if (PawnPieceCheck('t', row, col, _tag, false)) results.Add("t" + row + col);
            if (PawnPieceCheck('b', row, col, _tag, false)) results.Add("b" + row + col);

            if (PawnPieceCheck('t', (char)(row - 1), col, _tag, true)) results.Add("t" + (char)(row - 1) + col);//取る動き
            if (PawnPieceCheck('b', (char)(row - 1), col, _tag, true)) results.Add("b" + (char)(row - 1) + col);//取る動き

            GameObject currentTileObj = GetTile("" + layer + row + col);
            if (currentTileObj.transform.childCount > 0 && currentTileObj.transform.GetChild(0).GetComponent<PieceTouch>().movecnt.Value == 0)
            {
                if (PawnPieceCheck(layer, (char)(row - 1), col, _tag, false) && PawnPieceCheck(layer, (char)(row - 2), col, _tag, false)) results.Add("" + layer + (char)(row - 2) + col); //初期状態での2マス移動
            }

        }
        else if (layer == 'b' || layer == 't')
        {
            if (PawnPieceCheck('m', row, col, _tag, false)) results.Add("m" + row + col);

            if (PawnPieceCheck('m', (char)(row - 1), col, _tag, true)) results.Add("m" + (char)(row - 1) + col);//取る動き

            //アンパッサン状態
            if ((char)(row - 1) >= 'a' && (char)(row - 1) <= 'h')
            {
                if (GetTile("m" + (char)(row - 1) + col).transform.childCount == 0 && GetTile("m" + row + col).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
                {
                    if (GetTile("m" + row + col).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
                    {
                        GameObject enemy = GetTile("m" + row + col).transform.GetChild(0).gameObject;
                        if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない = 自駒ではない
                        {
                            ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                            NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                            if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                            {
                                results.Add("m" + (char)(row - 1) + col);
                            }
                        }
                    }
                }
            }
            //


        }

        if (PawnPieceCheck(layer, (char)(row - 1), col, _tag, false)) results.Add("" + layer + (char)(row - 1) + col);

        if (PawnPieceCheck(layer, (char)(row - 1), (char)(col + 1), _tag, true)) results.Add("" + layer + (char)(row - 1) + (char)(col + 1));//取る動き
        if (PawnPieceCheck(layer, (char)(row - 1), (char)(col - 1), _tag, true)) results.Add("" + layer + (char)(row - 1) + (char)(col - 1));//取る動き

        //アンパッサン状態
        if ((row - 1 >= 'a' && col + 1 <= '8' && GetTile("" + layer + (char)(row - 1) + (char)(col + 1)).transform.childCount == 0) && GetTile("" + layer + row + (char)(col + 1)).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
        {
            if (GetTile("" + layer + row + (char)(col + 1)).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
            {
                GameObject enemy = GetTile("" + layer + row + (char)(col + 1)).transform.GetChild(0).gameObject;
                if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない
                {
                    ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                    NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                    if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                    {
                        results.Add("" + layer + (char)(row - 1) + (char)(col + 1));
                    }
                }
            }
        }
        //

        //アンパッサン状態
        if ((row - 1 >= 'a' && col - 1 >= '1' && GetTile("" + layer + (char)(row - 1)  + (char)(col - 1)).transform.childCount == 0) && GetTile("" + layer + row + (char)(col - 1)).transform.childCount == 1)//斜め先に敵駒がおらず,かつその移動先の一マス手前に駒がある
        {
            if (GetTile("" + layer + row + (char)(col - 1)).transform.GetChild(0).name == "Pawn(Clone)")//駒がポーンである
            {
                GameObject enemy = GetTile("" + layer + row + (char)(col - 1)).transform.GetChild(0).gameObject;
                if (enemy.transform.GetComponent<PieceTouch>().movecnt.Value == 1)//駒が一回しか動いていない
                {
                    ulong lastId = SelectionManager.Instance.lastMovedPieceId.Value;
                            NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                            if (enemyNetObj.NetworkObjectId == lastId)//直前に動いている,
                    {
                        results.Add("" + layer + (char)(row - 1) + (char)(col - 1));
                    }
                }
            }
        }
        //


        return results;

    }

    static List<string> KnightRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();

        char layer = _tile[0];
        char row = _tile[1];
        char col = _tile[2];
        
        //3D機能部分

        if (layer == 't' || layer == 'b')
        {
            if (PieceCheck('m', row, (char)(col - 2), _tag, false)) results.Add("m" + row + (char)(col - 2));
            if (PieceCheck('m', row, (char)(col + 2), _tag, false)) results.Add("m" + row + (char)(col + 2));
            if (PieceCheck('m', (char)(row + 2), col, _tag, false)) results.Add("m" + (char)(row + 2) + col);
            if (PieceCheck('m', (char)(row - 2), col, _tag, false)) results.Add("m" + (char)(row - 2) + col);
            if(layer == 't')
            {
                if (PieceCheck('b', (char)(row + 1), col, _tag, false)) results.Add("b" + (char)(row + 1) + col);
                if (PieceCheck('b', (char)(row - 1), col, _tag, false)) results.Add("b" + (char)(row - 1) + col);
                if (PieceCheck('b', row, (char)(col + 1), _tag, false)) results.Add("b" + row + (char)(col + 1));
                if (PieceCheck('b', row, (char)(col - 1), _tag, false)) results.Add("b" + row + (char)(col - 1));
            }
            if (layer == 'b')
            {
                if (PieceCheck('t', (char)(row + 1), col, _tag, false)) results.Add("t" + (char)(row + 1) + col);
                if (PieceCheck('t', (char)(row - 1), col, _tag, false)) results.Add("t" + (char)(row - 1) + col);
                if (PieceCheck('t', row, (char)(col + 1), _tag, false)) results.Add("t" + row + (char)(col + 1));
                if (PieceCheck('t', row, (char)(col - 1), _tag, false)) results.Add("t" + row + (char)(col - 1));
            }
        }
        else
        {
            if (PieceCheck('t', row, (char)(col - 2), _tag, false)) results.Add("t" + row + (char)(col - 2));
            if (PieceCheck('b', row, (char)(col - 2), _tag, false)) results.Add("b" + row + (char)(col - 2));
            if (PieceCheck('t', row, (char)(col + 2), _tag, false)) results.Add("t" + row + (char)(col + 2));
            if (PieceCheck('b', row, (char)(col + 2), _tag, false)) results.Add("b" + row + (char)(col + 2));
            if (PieceCheck('t', (char)(row + 2), col, _tag, false)) results.Add("t" + (char)(row + 2) + col);
            if (PieceCheck('b', (char)(row + 2), col, _tag, false)) results.Add("b" + (char)(row + 2) + col);
            if (PieceCheck('t', (char)(row - 2), col, _tag, false)) results.Add("t" + (char)(row - 2) + col);
            if (PieceCheck('b', (char)(row - 2), col, _tag, false)) results.Add("b" + (char)(row - 2) + col);
        }


        //既存2D部分
        if (PieceCheck(layer, (char)(row - 2), (char)(col + 1), _tag, false)) results.Add("" + layer + (char)(row - 2) + (char)(col + 1));
        if (PieceCheck(layer, (char)(row - 2), (char)(col - 1), _tag, false)) results.Add("" + layer + (char)(row - 2) + (char)(col - 1));
        if (PieceCheck(layer, (char)(row + 2), (char)(col + 1), _tag, false)) results.Add("" + layer + (char)(row + 2) + (char)(col + 1));
        if (PieceCheck(layer, (char)(row + 2), (char)(col - 1), _tag, false)) results.Add("" + layer + (char)(row + 2) + (char)(col - 1));
        if (PieceCheck(layer, (char)(row - 1), (char)(col + 2), _tag, false)) results.Add("" + layer + (char)(row - 1) + (char)(col + 2));
        if (PieceCheck(layer, (char)(row - 1), (char)(col - 2), _tag, false)) results.Add("" + layer + (char)(row - 1) + (char)(col - 2));
        if (PieceCheck(layer, (char)(row + 1), (char)(col + 2), _tag, false)) results.Add("" + layer + (char)(row + 1) + (char)(col + 2));
        if (PieceCheck(layer, (char)(row + 1), (char)(col - 2), _tag, false)) results.Add("" + layer + (char)(row + 1) + (char)(col - 2));

        return results;
    }

    static List<string> RookRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();
        char layer = _tile[0];
        char row = _tile[1];
        char col = _tile[2];

        if(layer == 't')
        {
            if (PieceCheck('m', row, col, _tag, false)) results.Add("m" + row + col);
            if (PieceCheck('m', row, col, _tag, true) && PieceCheck('b', row, col, _tag, false)) results.Add("b" + row + col);
        }
        else if(layer == 'm')
        {
            if (PieceCheck('t', row, col, _tag, false)) results.Add("t" + row + col);
            if (PieceCheck('b', row, col, _tag, false)) results.Add("b" + row + col);
        }
        else if(layer == 'b')
        {
            if (PieceCheck('m', row, col, _tag, false)) results.Add("m" + row + col);
            if (PieceCheck('m', row, col, _tag, true) && PieceCheck('t', row, col, _tag, false)) results.Add("t" + row + col);
        }

        //進行先タイルが空or敵駒
        //進行先タイルの手前タイルが空
        for(int i = 1; i < 8; i++)
        {
            if (i == 1 && PieceCheck(layer, (char)(row + i), col, _tag, false)) results.Add("" + layer + (char)(row + i) + col);
            else if (PieceCheck(layer, (char)(row + i - 1), col, _tag, true) && PieceCheck(layer, (char)(row + i), col, _tag, false)) results.Add("" + layer + (char)(row + i) + col);
            else break;
        }

        for (int i = 1; i < 8; i++)
        {
            if (i == 1 && PieceCheck(layer, (char)(row - i), col, _tag, false)) results.Add("" + layer + (char)(row - i) + col);
            else if (PieceCheck(layer, (char)(row - i + 1), col, _tag, true) && PieceCheck(layer, (char)(row - i), col, _tag, false)) results.Add("" + layer + (char)(row - i) + col);
            else break;
        }

        for (int i = 1; i < 8; i++)
        {
            if (i == 1 && PieceCheck(layer, row, (char)(col + i), _tag, false)) results.Add("" + layer + row + (char)(col + i));
            else if (PieceCheck(layer, row, (char)(col + i - 1), _tag, true) && PieceCheck(layer, row, (char)(col + i), _tag, false)) results.Add("" + layer + row + (char)(col + i));
            else break;
        }
        for (int i = 1; i < 8; i++)
        {
            if (i == 1 && PieceCheck(layer, row, (char)(col - i), _tag, false)) results.Add("" + layer + row + (char)(col - i));
            else if (PieceCheck(layer, row, (char)(col - i + 1), _tag, true) && PieceCheck(layer, row, (char)(col - i), _tag, false)) results.Add("" + layer + row + (char)(col - i));
            else break;
        }
        return results;
    }

    static List<string> BishopRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();
        char layer = _tile[0];
        char row = _tile[1];
        char col = _tile[2];

        if (layer == 't')
        {
            if (PieceCheck('m', (char)(row + 1), col, _tag, false)) results.Add("m" + (char)(row + 1) + col);
            if (PieceCheck('m', (char)(row + 1), col, _tag, true) && PieceCheck('b', (char)(row + 2), col, _tag, false)) results.Add("b" + (char)(row + 2) + col);

            if (PieceCheck('m', (char)(row - 1), col, _tag, false)) results.Add("m" + (char)(row - 1) + col);
            if (PieceCheck('m', (char)(row - 1), col, _tag, true) && PieceCheck('b', (char)(row - 2), col, _tag, false)) results.Add("b" + (char)(row - 2) + col);

            if (PieceCheck('m', row, (char)(col + 1), _tag, false)) results.Add("m" + row + (char)(col + 1));
            if (PieceCheck('m', row, (char)(col + 1), _tag, true) && PieceCheck('b', row, (char)(col + 2), _tag, false)) results.Add("b" + row + (char)(col + 2));

            if (PieceCheck('m', row, (char)(col - 1), _tag, false)) results.Add("m" + row + (char)(col - 1));
            if (PieceCheck('m', row, (char)(col - 1), _tag, true) && PieceCheck('b', row, (char)(col - 2), _tag, false)) results.Add("b" + row + (char)(col - 2));
        }
        else if (layer == 'm')
        {
            if (PieceCheck('t', (char)(row + 1), col, _tag, false)) results.Add("t" + (char)(row + 1) + col);
            if (PieceCheck('t', (char)(row - 1), col, _tag, false)) results.Add("t" + (char)(row - 1) + col);
            if (PieceCheck('b', (char)(row + 1), col, _tag, false)) results.Add("b" + (char)(row + 1) + col);
            if (PieceCheck('b', (char)(row - 1), col, _tag, false)) results.Add("b" + (char)(row - 1) + col);
            
            if (PieceCheck('t', row, (char)(col + 1), _tag, false)) results.Add("t" + row + (char)(col + 1));
            if (PieceCheck('t', row, (char)(col - 1), _tag, false)) results.Add("t" + row + (char)(col - 1));
            if (PieceCheck('b', row, (char)(col + 1), _tag, false)) results.Add("b" + row + (char)(col + 1));
            if (PieceCheck('b', row, (char)(col - 1), _tag, false)) results.Add("b" + row + (char)(col - 1));
        }
        else if (layer == 'b')
        {
            if (PieceCheck('m', (char)(row + 1), col, _tag, false)) results.Add("m" + (char)(row + 1) + col);
            if (PieceCheck('m', (char)(row + 1), col, _tag, true) && PieceCheck('t', (char)(row + 2), col, _tag, false)) results.Add("t" + (char)(row + 2) + col);

            if (PieceCheck('m', (char)(row - 1), col, _tag, false)) results.Add("m" + (char)(row - 1) + col);
            if (PieceCheck('m', (char)(row - 1), col, _tag, true) && PieceCheck('t', (char)(row - 2), col, _tag, false)) results.Add("t" + (char)(row - 2) + col);

            if (PieceCheck('m', row, (char)(col + 1), _tag, false)) results.Add("m" + row + (char)(col + 1));
            if (PieceCheck('m', row, (char)(col + 1), _tag, true) && PieceCheck('t', row, (char)(col + 2), _tag, false)) results.Add("t" + row + (char)(col + 2));

            if (PieceCheck('m', row, (char)(col - 1), _tag, false)) results.Add("m" + row + (char)(col - 1));
            if (PieceCheck('m', row, (char)(col - 1), _tag, true) && PieceCheck('t', row, (char)(col - 2), _tag, false)) results.Add("t" + row + (char)(col - 2));
        }

        //進行先タイルが空or敵駒
        //進行先タイルの手前タイルが空
        for (int i = 1; i < 8; i++)//++
        {
            if (i == 1 && PieceCheck(layer, (char)(row + i), (char)(col + i), _tag, false)) results.Add("" + layer + (char)(row + i) + (char)(col + i));
            else if (PieceCheck(layer, (char)(row + i - 1), (char)(col + i -1), _tag, true) && PieceCheck(layer, (char)(row + i), (char)(col + i), _tag, false)) results.Add("" + layer + (char)(row + i) + (char)(col + i));
            else break;
        }

        for (int i = 1; i < 8; i++)//--
        {
            if (i == 1 && PieceCheck(layer, (char)(row - i), (char)(col - i), _tag, false)) results.Add("" + layer + (char)(row - i) + (char)(col - i));
            else if (PieceCheck(layer, (char)(row - i + 1), (char)(col - i + 1), _tag, true) && PieceCheck(layer, (char)(row - i), (char)(col - i), _tag, false)) results.Add("" + layer + (char)(row - i) + (char)(col - i));
            else break;
        }

        for (int i = 1; i < 8; i++)//-+
        {
            if (i == 1 && PieceCheck(layer, (char)(row - i), (char)(col + i), _tag, false)) results.Add("" + layer + (char)(row - i) + (char)(col + i));
            else if (PieceCheck(layer, (char)(row - i + 1), (char)(col + i - 1), _tag, true) && PieceCheck(layer, (char)(row - i), (char)(col + i), _tag, false)) results.Add("" + layer + (char)(row - i) + (char)(col + i));
            else break;
        }
        for (int i = 1; i < 8; i++)//+-
        {
            if (i == 1 && PieceCheck(layer, (char)(row + i), (char)(col - i), _tag, false)) results.Add("" + layer + (char)(row + i) + (char)(col - i));
            else if (PieceCheck(layer, (char)(row + i - 1), (char)(col - i + 1), _tag, true) && PieceCheck(layer, (char)(row + i), (char)(col - i), _tag, false)) results.Add("" + layer + (char)(row + i) + (char)(col - i));
            else break;
        }
        return results;
    }

    static List<string> QueenRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();

        results.AddRange(RookRule(_tag, _tile));
        results.AddRange(BishopRule(_tag, _tile));
        return results;
    }

    static List<string> KingRule(string _tag, char[] _tile)
    {
        List<string> results = new List<string>();

        char layer = _tile[0];
        char row = _tile[1];
        char col = _tile[2];
        //逃げやすすぎるためm面上のみの移動
        if (PieceCheck(layer, (char)(row + 1), col, _tag, false)) results.Add("" + layer + (char)(row + 1) + col);
        if (PieceCheck(layer, (char)(row - 1), col, _tag, false)) results.Add("" + layer + (char)(row - 1) + col);

        if (PieceCheck(layer, row, (char)(col - 1), _tag, false)) results.Add("" + layer + row + (char)(col - 1));
        if (PieceCheck(layer, (char)(row + 1), (char)(col - 1), _tag, false)) results.Add("" + layer + (char)(row + 1) + (char)(col - 1));
        if (PieceCheck(layer, (char)(row - 1), (char)(col - 1), _tag, false)) results.Add("" + layer + (char)(row - 1) + (char)(col - 1));

        if (PieceCheck(layer, row, (char)(col + 1), _tag, false)) results.Add("" + layer + row + (char)(col + 1));
        if (PieceCheck(layer, (char)(row + 1), (char)(col + 1), _tag, false)) results.Add("" + layer + (char)(row + 1) + (char)(col + 1));
        if (PieceCheck(layer, (char)(row - 1), (char)(col + 1), _tag, false)) results.Add("" + layer + (char)(row - 1) + (char)(col + 1));

        //キャスリング可能状態
        GameObject kingTileObj = GetTile("" + layer + row + col);
        if (kingTileObj.transform.childCount > 0 && kingTileObj.transform.GetChild(0).GetComponent<PieceTouch>().movecnt.Value == 0)
        {
            bool checkA = false, checkB = false;
            if (GetTile("" + layer + row + 8).transform.childCount == 1 && GetTile("" + layer + row + 8).transform.GetChild(0).GetComponent<PieceTouch>().movecnt.Value == 0) checkA = true;
            if (GetTile("" + layer + row + 1).transform.childCount == 1 && GetTile("" + layer + row + 1).transform.GetChild(0).GetComponent<PieceTouch>().movecnt.Value == 0) checkB = true;
            if (checkA || checkB)//ルークが動いていない
            {
                if (checkA)
                {
                    if(PieceCheck(layer, row, '6', _tag, true) && PieceCheck(layer, row, '7', _tag, true))//間に駒がない
                    {
                        results.Add("" + layer + row + 7);
                    }
                }
                if(checkB)
                {
                    if(PieceCheck(layer, row, '4', _tag, true) && PieceCheck(layer, row, '3', _tag, true) && PieceCheck(layer, row, '2', _tag, true))
                    {
                        results.Add("" + layer + row + 3);
                    }
                }
            }
        }
        return results;
    }



    public static void ClearCache()
    {
        tileCache.Clear();
    }
}
