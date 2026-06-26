using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using System.Threading.Tasks;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class RelayUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private string gameSceneName = "GameScene";
    public static string JoinCode;

    private async void Start()
    {
        hostButton.onClick.AddListener(async () => await StartHostRelay());
        joinButton.onClick.AddListener(async () => await StartClientRelay());

#if UNITY_EDITOR
    if (ClonesManager.IsClone())
    {
        await RelayManager.Instance.WaitUntilInitialized();

        string cloneArg = ClonesManager.GetArgument();
        if (!string.IsNullOrEmpty(cloneArg)) // 空欄なのでここは実行されない
        {
            joinCodeInput.text = cloneArg;
            await StartClientRelay();
        }
    }
#endif
    }

    private async Task StartHostRelay()
    {
        hostButton.onClick.RemoveAllListeners();
        string code = await RelayManager.Instance.CreateRelay();

        if (!string.IsNullOrEmpty(code))
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);

            JoinCode = code;

        }
    }

    private async Task StartClientRelay()
    {
        string code = joinCodeInput.text.Trim();

        Debug.Log($"Join Code: '{code}' / Length: {code.Length}");

        // 🔹 入力チェック
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        try
        {
            bool joined = await RelayManager.Instance.JoinRelay(code);

            if (joined)
            {
                NetworkManager.Singleton.StartClient();
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            }
            else
            {
                joinCodeInput.text = ""; // 入力欄クリア
                joinCodeInput.Select(); // 再入力しやすくフォーカス
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Not expected error: {ex.Message}");
        }
    }
}
