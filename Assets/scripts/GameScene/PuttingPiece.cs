using System.Linq;
using Unity.Netcode;
using UnityEngine;


[System.Serializable]
public class ExtraPieceData
{
    public string position;      // 例: "ma3"
    public GameObject piecePrefab; // PrefabをInspectorからドラッグ
    public string color;
}

public class PuttingPiece : MonoBehaviour
{
    public int colnum = 8;
    [SerializeField]
    private ExtraPieceData[] extraPieces; // ← Inspectorで追加駒を設定

    void Start()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.OnServerStarted += SpawnPiecesAfterStart;
            return;
        }

        SpawnPiecesAfterStart();
    }

    private void SpawnPiecesAfterStart()
    {
        RoleManager.ClearCache();
        GameObject black_pawn = Resources.Load<GameObject>("Prefabs/BlackPieces/Pawn");
        GameObject black_rook = Resources.Load<GameObject>("Prefabs/BlackPieces/Rook");
        GameObject black_knight = Resources.Load<GameObject>("Prefabs/BlackPieces/Knight");
        GameObject black_bishop = Resources.Load<GameObject>("Prefabs/BlackPieces/Bishop");
        GameObject black_queen = Resources.Load<GameObject>("Prefabs/BlackPieces/Queen");
        GameObject black_king = Resources.Load<GameObject>("Prefabs/BlackPieces/King");

        GameObject white_pawn = Resources.Load<GameObject>("Prefabs/WhitePieces/Pawn");
        GameObject white_rook = Resources.Load<GameObject>("Prefabs/WhitePieces/Rook");
        GameObject white_knight = Resources.Load<GameObject>("Prefabs/WhitePieces/Knight");
        GameObject white_bishop = Resources.Load<GameObject>("Prefabs/WhitePieces/Bishop");
        GameObject white_queen = Resources.Load<GameObject>("Prefabs/WhitePieces/Queen");
        GameObject white_king = Resources.Load<GameObject>("Prefabs/WhitePieces/King");

        if (!NetworkManager.Singleton.IsServer) return; // クライアント側で生成しないようにする

        var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        ulong whiteClientId = clientIds.Count > 0 ? clientIds[0] : 0UL; // ホスト
        ulong blackClientId = clientIds.Count > 1 ? clientIds[1] : 1UL; // クライアント

        Debug.Log($"White ClientId: {whiteClientId}, Black ClientId: {blackClientId}");



        SpawnRow(black_pawn, "mb", 1, blackClientId);
        SpawnRowWithPattern("ma", 1, black_rook, black_knight, black_bishop, black_queen, black_king, blackClientId);
        SpawnRow(white_pawn, "mg", 1, whiteClientId);
        SpawnRowWithPattern("mh", 1, white_rook, white_knight, white_bishop, white_queen, white_king, whiteClientId);
        if (extraPieces != null)
        {
            foreach (var data in extraPieces)
            {
                if (data.piecePrefab == null || string.IsNullOrEmpty(data.position)) continue;

                // colorからownerIdを自動決定
                ulong ownerId = data.color == "white" ? 0UL : 1UL;
                PutEx(data.position, ownerId, data.piecePrefab, data.color);
            }
        }

        HistoryManager.Instance.LateStart();
    }

    // ポーンなど同じ駒を並べる行
    private void SpawnRow(GameObject prefab, string rowPrefix, int startIndex, ulong ownerId)
    {
        for (int i = startIndex; i <= colnum; i++)
        {
            var parent = GameObject.Find(rowPrefix + i);
            if (parent == null) continue;

            // Pieceを生成してTileの子にする場合
            GameObject pieceInstance = Instantiate(prefab, parent.transform.position, Quaternion.identity);

            NetworkObject pieceNetObj = pieceInstance.GetComponent<NetworkObject>();
            NetworkObject tileNetObj = parent.GetComponent<NetworkObject>();

            pieceNetObj.SpawnWithOwnership(ownerId); // ★ 所有権を指定して生成

            pieceNetObj.TrySetParent(tileNetObj);


            pieceNetObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            pieceNetObj.transform.localRotation = Quaternion.identity;
            pieceNetObj.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);

        }
    }

    // ルーク、ナイト、ビショップ、クイーン、キングの配置
    private void SpawnRowWithPattern(string rowPrefix, int startIndex, GameObject rook, GameObject knight, GameObject bishop, GameObject queen, GameObject king, ulong ownerId)
    {
        for (int i = startIndex; i <= colnum; i++)
        {
            GameObject prefab = null;
            if (i == 1 || i == 8) prefab = rook;
            if (i == 2 || i == 7) prefab = knight;
            if (i == 3 || i == 6) prefab = bishop;
            if (i == 4) prefab = queen;
            if (i == 5) prefab = king;

            if (prefab == null) continue;

            var parent = GameObject.Find(rowPrefix + i);
            if (parent == null) continue;
            // Pieceを生成してTileの子にする場合
            GameObject pieceInstance = Instantiate(prefab, parent.transform.position, Quaternion.identity);

            NetworkObject pieceNetObj = pieceInstance.GetComponent<NetworkObject>();
            NetworkObject tileNetObj = parent.GetComponent<NetworkObject>();

            pieceNetObj.SpawnWithOwnership(ownerId); // ★ 所有権を指定して生成

            pieceNetObj.TrySetParent(tileNetObj);

            pieceNetObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            pieceNetObj.transform.localRotation = Quaternion.identity;
            pieceNetObj.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);

        }
    }

    private void PutEx(string Position, ulong ownerId, GameObject piece, string color)
    {

        var parent = GameObject.Find(Position);
        GameObject pieceInstance = Instantiate(piece, parent.transform.position, Quaternion.identity);

        NetworkObject pieceNetObj = pieceInstance.GetComponent<NetworkObject>();
        NetworkObject tileNetObj = parent.GetComponent<NetworkObject>();

        pieceNetObj.SpawnWithOwnership(ownerId); // ★ 所有権を指定して生成

        pieceNetObj.TrySetParent(tileNetObj);

        pieceNetObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        pieceNetObj.transform.localRotation = Quaternion.identity;
        pieceNetObj.transform.localScale = new Vector3(0.3f, 0.8f, 0.3f);
    }

}
