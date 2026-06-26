using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneBackButton : MonoBehaviour
{
    string startSceneName = "Start_Scene";

    public void Back()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            // ホスト/サーバーはシャットダウン→Start画面へ
            NetworkManager.Singleton.Shutdown();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // クライアントは切断→Start画面へ
            NetworkManager.Singleton.Shutdown();
        }

        // シャットダウン後にローカルでシーン遷移（Netcode管理外）
        SceneManager.LoadScene(startSceneName);
    }
}