using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;
using System;
using System.Linq;

public class SelectionManager : NetworkBehaviour
{
    public static SelectionManager Instance;

    private GameObject selectedPiece;
    private List<GameObject> highlightedTiles = new List<GameObject>();
    PieceTouch pieceTouch  = new PieceTouch();


    public NetworkVariable<int> turn = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public static NetworkVariable<bool> ischeck = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<ulong> lastMovedPieceId = new NetworkVariable<ulong>(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    

    void Awake()
    {
        Instance = this;
    }



    // 駒を選択
    public void LightPiece(GameObject piece, List<string> movableTiles)
    {
        ResetSelection(); // 前回のハイライトを消す
        selectedPiece = piece;

        // 移動可能マスをハイライト
        foreach (string tileName in movableTiles)
        {
            if (string.IsNullOrEmpty(tileName)) continue;
            GameObject tile = GameObject.Find(tileName);
            if (tile != null)
            {
                highlightedTiles.Add(tile);

                BoardTouch bt = tile.GetComponent<BoardTouch>();
                bt.SetMaterial(1);
                tile.tag = "lighted";
            }
        }
    }

    // リセット（ハイライト解除）
    public void ResetSelection()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            if (tile != null)
            {
                BoardTouch bt = tile.GetComponent<BoardTouch>();
                bt.SetMaterial(0);
                tile.tag = "tile";
            }
        }

        if (GameObject.FindWithTag("ownertile") != null) GameObject.FindWithTag("ownertile").tag = "tile";

        highlightedTiles.Clear();
        pieceTouch.movableTiles.Clear();
        selectedPiece = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTurnEndServerRpc(ulong pieceId, ulong tileId)
    {


        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(pieceId, out var pieceNetObj) &&
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(tileId, out var tileNetObj))
        {

            string fromTile = pieceNetObj.transform.parent.name;
            string toTile = tileNetObj.name;
            bool kill = false;
            // 既に駒がある場合は削除
            if (tileNetObj.transform.childCount > 0)
            {
                var child = tileNetObj.transform.GetChild(0);
                //HistoryManager.Instance.Pieces.Remove(child.gameObject);
                Destroy(child.gameObject);
                kill = true;
            }

            // 駒の親子関係を更新（サーバ側で実行）
            pieceNetObj.TrySetParent(tileNetObj);
            var netTransform = pieceNetObj.GetComponent<NetworkTransform>();
            netTransform.SetState(
                new Vector3(0f, 1f, 0f),       // position
                pieceNetObj.transform.localRotation, // rotation
                new Vector3(0.3f, 0.8f, 0.3f),                    // scale
            false                            // teleportDisabled
            );
            //キャスリング
            if (pieceNetObj.name == "King(Clone)" && pieceNetObj.GetComponent<PieceTouch>().movecnt.Value == 0)
            {
                char[] tmptilename = tileNetObj.name.ToCharArray();
                if(tmptilename[2] == '7')
                {
                    GameObject rook = GameObject.Find("" + tmptilename[0] + tmptilename[1] + '8').transform.GetChild(0).gameObject;
                    NetworkObject _rookNetObj = rook.GetComponent<NetworkObject>();
                    GameObject rooktile = GameObject.Find("" + tmptilename[0] + tmptilename[1] + '6').gameObject;
                    NetworkObject _rooktileNetObj = rooktile.GetComponent<NetworkObject>();

                    RequestTurnEndServerRpc(_rookNetObj.NetworkObjectId, _rooktileNetObj.NetworkObjectId);
                    turn.Value--;
                }
                if (tmptilename[2] == '3')
                {
                    GameObject rook = GameObject.Find("" + tmptilename[0] + tmptilename[1] + '1').transform.GetChild(0).gameObject;
                    NetworkObject _rookNetObj = rook.GetComponent<NetworkObject>();
                    GameObject rooktile = GameObject.Find("" + tmptilename[0] + tmptilename[1] + '4').gameObject;
                    NetworkObject _rooktileNetObj = rooktile.GetComponent<NetworkObject>();

                    RequestTurnEndServerRpc(_rookNetObj.NetworkObjectId, _rooktileNetObj.NetworkObjectId);
                    turn.Value--;
                }
            }

            //アンパッサン
            //ポーンを動かした時に移動先手前の駒に相手のポーンがありニマス移動をしていて,かつ直前に動いている
            if(pieceNetObj.name == "Pawn(Clone)" && pieceNetObj.tag == "white")
            {
                char[] tmptilename = tileNetObj.name.ToCharArray();
                GameObject unptile = GameObject.Find("" + tmptilename[0] + 'd' + tmptilename[2]);//敵駒の置いてあるタイル
                NetworkObject unptileNetObj = unptile.GetComponent<NetworkObject>();
                if(unptile.transform.childCount != 0 && tmptilename[1] == 'c')
                {
                    GameObject unppawn = unptile.transform.GetChild(0).gameObject;
                    NetworkObject unppawnNetObj = unppawn.GetComponent<NetworkObject>();
                    if(unppawnNetObj.name == "Pawn(Clone)" && unppawnNetObj.tag == "black" && unppawnNetObj.GetComponent<PieceTouch>().movecnt.Value == 1)
                    {
                        if (lastMovedPieceId.Value == unppawnNetObj.NetworkObjectId)
                        {
                            //HistoryManager.Instance.Pieces.Remove(unppawn);
                            Destroy(unppawn);
                        }
                    }
                }
            }
            if (pieceNetObj.name == "Pawn(Clone)" && pieceNetObj.tag == "black")
            {
                char[] tmptilename = tileNetObj.name.ToCharArray();
                GameObject unptile = GameObject.Find("" + tmptilename[0] + 'e' + tmptilename[2]);
                NetworkObject unptileNetObj = unptile.GetComponent<NetworkObject>();
                if (unptile.transform.childCount != 0 && tmptilename[1] == 'f')
                {
                    GameObject unppawn = unptile.transform.GetChild(0).gameObject;
                    NetworkObject unppawnNetObj = unppawn.GetComponent<NetworkObject>();
                    if (unppawnNetObj.name == "Pawn(Clone)" && unppawnNetObj.tag == "white" && unppawnNetObj.GetComponent<PieceTouch>().movecnt.Value == 1)
                    {
                        if (lastMovedPieceId.Value == unppawnNetObj.NetworkObjectId)
                        {
                            //HistoryManager.Instance.Pieces.Remove(unppawn);
                            Destroy(unppawn);
                        }
                    }
                }
            }
            // 駒の移動回数更新
            pieceNetObj.GetComponent<PieceTouch>().movecnt.Value++;
            lastMovedPieceId.Value = pieceNetObj.NetworkObjectId;
            turn.Value++;

            AddLogClientRpc(
                pieceNetObj.name.Replace("(Clone)", ""),
                toTile,
                kill,
                false,
                false
            );



            // 1フレーム待ってからCheckConfig（同期後に実行）
            StartCoroutine(DelayedCheckConfig(pieceNetObj.tag, pieceNetObj));

            bool success = pieceNetObj.TrySetParent(tileNetObj);
            Debug.Log($"[TrySetParent] 結果:{success} 駒:{pieceNetObj.name} ID:{pieceNetObj.NetworkObjectId} 移動先:{tileNetObj.name}");
        }

        
    }

    [ClientRpc]
    private void AddLogClientRpc(
    string pieceName,
    string toTile,
    bool kill,
    bool check,
    bool checkmate)
    {
        LogText.Instance.AddRecord(
            pieceName,
            toTile,
            kill,
            check,
            checkmate
        );
    }


    private System.Collections.IEnumerator DelayedCheckConfig(string pieceTag, NetworkObject pieceNetObj)
    {
        yield return null; // 1フレーム待つ
        pieceNetObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        ApplyCheck(pieceTag, true);
    }
    public void CheckConfig(string pieceTag, bool checkMate)
    {
        if (!IsServer)
        {
            SubmitCheckServerRpc(pieceTag, checkMate);
        }
        else
        {
            ApplyCheck(pieceTag, checkMate);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitCheckServerRpc(string tag, bool check)
    {
        ApplyCheck(tag, check);
    }

    private void ApplyCheck(string pieceTag, bool checkMate)
    {
        ischeck.Value = false;

        string enemyTag = pieceTag == "black" ? "white" : "black";


        GameObject[] enemyPieces = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject enemyKing = GameObject.FindGameObjectsWithTag(enemyTag).FirstOrDefault(obj => obj.name == "King(Clone)");
        if (enemyKing == null) return;
        string enemyKingTile = enemyKing.transform.parent.name;

        GameObject[] pieces = GameObject.FindGameObjectsWithTag(pieceTag);
        foreach (GameObject piece in pieces)
        {
            if (!piece.activeSelf) continue;
            char[] tile = piece.transform.parent.name.ToCharArray();
            List<string> moves = RoleManager.PreAct(pieceTag, piece.name, tile, false, piece);

            if (moves == null) continue;
            
            if (moves.Contains(enemyKingTile) && piece.activeSelf)
            {
                ischeck.Value = true;
                break;
            }
        }

        // チェックメイト判定
        if (ischeck.Value && checkMate)
        {
            bool isMate = true;

            foreach (GameObject piece in enemyPieces)
            {
                char[] tile = piece.transform.parent.name.ToCharArray();
                List<string> moves = RoleManager.PreAct(enemyTag, piece.name, tile, true, piece);

                if (moves.Count > 0)
                {
                    isMate = false;
                    break;
                }
            }

            if (isMate)
            {
                TurnTextWrite.isMate = true;
                GenerateUI();
                ShowEndUIClientRpc();
                return;
            }
        }

        //Draw判定
        bool hasLegalMove = false;
        foreach (GameObject piece in enemyPieces)
        {
            char[] tile = piece.transform.parent.name.ToCharArray();
            List<string> moves = RoleManager.PreAct(enemyTag, piece.name, tile, true, piece);
            if (moves != null && moves.Count > 0)
            {
                hasLegalMove = true;
                break;
            }
        }
        if (!hasLegalMove)
        {
            if (ischeck.Value)
            {
                // チェックメイト
                TurnTextWrite.isMate = true;
                GenerateUI();
                ShowEndUIClientRpc();
            }
            else
            {
                // ステイルメイト
                TurnTextWrite.isDraw = true;
                GenerateUI();
                ShowEndUIClientRpc();
            }
        }

    }

    void OnEnable()
    {
        ischeck.OnValueChanged += OnCheckChanged;
    }

    void OnDisable()
    {
        ischeck.OnValueChanged -= OnCheckChanged;
    }

    private void OnCheckChanged(bool prev, bool current)
    {
        if (current)
        {
            Debug.Log("チェック！");
            // チェック表示UIの更新をここで行う
        }
    }


    void GenerateUI()
    {
        Debug.Log("end_d");
        GameObject EndUI = Resources.Load<GameObject>("Prefabs/EndUI");
        GameObject uiInstance = Instantiate(EndUI);
        return;
    }

    [ClientRpc]
    void ShowEndUIClientRpc()
    {
        if (GameObject.Find("EndUI(Clone)") == null)
        {
            Debug.Log("EndUI shown on client");
            GameObject EndUI = Resources.Load<GameObject>("Prefabs/EndUI");
            Instantiate(EndUI);

        }
    }
}
