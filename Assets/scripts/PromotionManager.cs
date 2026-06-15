using Unity.Netcode;
using UnityEngine;

public class PromotionManager : NetworkBehaviour
{
    public static PromotionManager Instance;

    void Awake() => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void RequestPromotionServerRpc(int selectNum, string tileName, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"昇格リクエスト受信: {tileName}");

        ulong requesterId = rpcParams.Receive.SenderClientId;
        GameObject targetTile = GameObject.Find(tileName);
        if (targetTile == null)
        {
            Debug.LogError("タイルが見つかりません: " + tileName);
            return;
        }

        GameObject piecePrefab = GetPiecePrefab(selectNum, tileName);
        if (piecePrefab == null)
        {
            Debug.LogError("Prefabが見つかりません");
            return;
        }

        GameObject oldPieceObj = null;

        // 駒の削除（サーバーのみ）
        if (targetTile.transform.childCount > 0)
        {
            oldPieceObj = targetTile.transform.GetChild(0).gameObject;
            var oldPieceNet = oldPieceObj.GetComponent<NetworkObject>();
            if (oldPieceNet != null && oldPieceNet.IsSpawned)
                oldPieceNet.Despawn(true);
        }

        // 新しい駒を生成
        GameObject newPiece = Instantiate(piecePrefab, targetTile.transform.position, Quaternion.identity);
        NetworkObject pieceNetObj = newPiece.GetComponent<NetworkObject>();
        NetworkObject tileNetObj = targetTile.GetComponent<NetworkObject>();

        if (pieceNetObj == null || tileNetObj == null)
        {
            Debug.LogError("NetworkObjectが足りません。");
            Destroy(newPiece);
            return;
        }

        pieceNetObj.SpawnWithOwnership(requesterId, true);
        pieceNetObj.TrySetParent(tileNetObj);

        // Transform設定
        newPiece.transform.localPosition = new Vector3(0f, 1f, 0f);
        newPiece.transform.localRotation = Quaternion.identity;
        newPiece.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);

        // クライアントに通知して補正
        FixScaleClientRpc(pieceNetObj.NetworkObjectId, newPiece.transform.localScale);

        if (HistoryManager.Instance != null)
        {
            if (oldPieceObj != null)
            {
                HistoryManager.Instance.Pieces.Remove(oldPieceObj);
                Debug.Log($"[History] 旧駒 {oldPieceObj.name} を削除");
            }

            HistoryManager.Instance.Pieces.Add(newPiece);
            Debug.Log($"[History] 新駒 {newPiece.name} を追加");
        }
        else
        {
            Debug.LogWarning("[History] HistoryManager.Instance が見つかりませんでした");
        }
    }

    [ClientRpc]
    void FixScaleClientRpc(ulong pieceId, Vector3 scale)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(pieceId, out var obj))
        {
            obj.transform.localScale = scale;
        }
    }

    private GameObject GetPiecePrefab(int selectNum, string tileName)
    {
        bool isBlack = tileName.Contains("h"); // 例：h行なら黒側

        switch (selectNum)
        {
            case 1: return Resources.Load<GameObject>(isBlack ? "Prefabs/BlackPieces/Rook" : "Prefabs/WhitePieces/Rook");
            case 2: return Resources.Load<GameObject>(isBlack ? "Prefabs/BlackPieces/Knight" : "Prefabs/WhitePieces/Knight");
            case 3: return Resources.Load<GameObject>(isBlack ? "Prefabs/BlackPieces/Bishop" : "Prefabs/WhitePieces/Bishop");
            case 4: return Resources.Load<GameObject>(isBlack ? "Prefabs/BlackPieces/Queen" : "Prefabs/WhitePieces/Queen");
            default: return null;
        }
    }
}
