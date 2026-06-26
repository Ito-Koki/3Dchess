using UnityEngine;
using UnityEngine.UI;

public class PromotionButton : MonoBehaviour
{
    private Button button;
    public int Selectnum;
    public char[] tile;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        string tileName = new string(tile);

        // 自分のUIを閉じる（ローカルのみ）
        GameObject ui = GameObject.Find("PromotionCanvas(Clone)");
        if (ui != null) Destroy(ui);

        // ネットワーク経由で昇格リクエスト
        PromotionManager.Instance.RequestPromotionServerRpc(Selectnum, tileName);
    }
}
