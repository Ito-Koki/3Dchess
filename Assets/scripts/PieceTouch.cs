using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PieceTouch : NetworkBehaviour
{
    public List<string> movableTiles = new List<string>();
    public NetworkVariable<int> movecnt = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    bool islast = true;
    char[] tmptile;

    void Start()
    {
        string tag = gameObject.tag;
        string name = gameObject.name;
    }

    void Update()
    {

        if (!IsOwner) return;

        tmptile = this.transform.parent.gameObject.name.ToCharArray();

        if (tag == "black" && name == "Pawn(Clone)" && tmptile[1] == 'h' && islast)
        {
            islast = false;
            TryRequestPromotion();
        }
        if (tag == "white" && name == "Pawn(Clone)" && tmptile[1] == 'a' && islast)
        {
            islast = false;
            TryRequestPromotion();
        }
    }

    void TryRequestPromotion()
    {
        // どちら側でも昇格要求をサーバへ送る
        if (IsSpawned && NetworkManager.Singleton.IsConnectedClient)
        {
            RequestPromotionServerRpc(NetworkObjectId);
        }
        else
        {
            // オフラインやエディタ単体実行時
            SpawnPromotionUI();
        }
    }

    // ==============================
    // 🔹 サーバRPC：昇格要求
    // ==============================
    [ServerRpc(RequireOwnership = false)]
    void RequestPromotionServerRpc(ulong pieceId, ServerRpcParams rpcParams = default)
    {
        ulong requesterId = rpcParams.Receive.SenderClientId;

        // 昇格要求を送ってきたクライアントのみにUIを出す
        var rpcSendParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { requesterId } }
        };

        ShowPromotionUIClientRpc(pieceId, rpcSendParams);
    }

    // ==============================
    // 🔹 ClientRpc：特定クライアントのみUI生成
    // ==============================
    [ClientRpc]
    void ShowPromotionUIClientRpc(ulong pieceId, ClientRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(pieceId, out var netObj))
            return;

        var piece = netObj.GetComponent<PieceTouch>();
        if (piece == null) return;

        piece.SpawnPromotionUI();
    }

    // ==============================
    // 🔹 ローカルUI生成処理
    // ==============================
    void SpawnPromotionUI()
    {
        Debug.Log($"Promotion UI spawned on {NetworkManager.Singleton.LocalClientId}");
        GameObject PromotionUI = Resources.Load<GameObject>("Prefabs/PromotionCanvas");
        GameObject pUI = Instantiate(PromotionUI);

        for (int i = 0; i < 4; i++)
        {
            PromotionButton btn = pUI.transform.GetChild(0).GetChild(i).GetComponent<PromotionButton>();
            btn.tile = this.transform.parent.gameObject.name.ToCharArray();
        }
    }

    void OnMouseDown()
    {
        int currentTurn = SelectionManager.Instance.turn.Value;

        bool canMove = false;

        if (currentTurn % 2 == 0 && IsHost  && tag == "white") canMove = true;
        if (currentTurn % 2 == 1 && !IsHost && tag == "black") canMove = true;

        if (!canMove) return; // 動けないなら終了

        // 自分が操作権を持たない（クライアント側）場合はサーバに通知だけする
        if (!IsOwner)
        {
            return;
        }
        SoundManager.Instance.PlaySelect();
        HandlePieceSelection();

    }

    private void HandlePieceSelection()
    {
        char[] tile = this.transform.parent.gameObject.name.ToCharArray(); // 例: mb2


        // 駒の役と位置から移動可能マスを計算
        movableTiles = RoleManager.PreAct(tag, name, tile, true, this.gameObject);

        // 移動可能マスを色づけ
        SelectionManager.Instance.LightPiece(this.gameObject, movableTiles);

        this.transform.parent.gameObject.tag = "ownertile";

        
    }
}
