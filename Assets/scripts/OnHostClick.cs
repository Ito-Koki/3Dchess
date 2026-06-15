using UnityEngine;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;

    public async void OnHostClick()
    {
        string code = await RelayManager.Instance.CreateRelay();
        Debug.Log($"Join Code: {code}");
        // 生成したコードを画面に表示するUIなどを作ってもOK
    }

    public async void OnJoinClick()
    {
        string joinCode = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("Join Codeを入力してください");
            return;
        }

        await RelayManager.Instance.JoinRelay(joinCode);
    }
}
