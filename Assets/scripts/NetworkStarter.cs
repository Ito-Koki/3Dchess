using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class NetworkStarter : MonoBehaviour
{
    private string joinCodeInput = "";

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 280, 220), GUI.skin.box);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("=== Relay Network ===");

            if (GUILayout.Button("🟢 Relay Hostとして開始"))
            {
                _ = StartHostRelay();
            }

            GUILayout.Space(10);
            GUILayout.Label("Relay Join Code:");
            joinCodeInput = GUILayout.TextField(joinCodeInput);

            if (GUILayout.Button("🔵 クライアントとして参加"))
            {
                _ = StartClientRelay(joinCodeInput);
            }
        }
        else
        {
            GUILayout.Label("接続中: ClientID = " + NetworkManager.Singleton.LocalClientId);
            GUILayout.Label("現在の接続数: " + NetworkManager.Singleton.ConnectedClients.Count);

            if (GUILayout.Button("切断"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }

    private async Task StartHostRelay()
    {
        string joinCode = await RelayManager.Instance.CreateRelay();

        if (joinCode != null)
        {
            Debug.Log($"Relay Host開始 - Join Code: {joinCode}");
            NetworkManager.Singleton.StartHost();
        }
    }

    private async Task StartClientRelay(string code)
    {
        if (await RelayManager.Instance.JoinRelay(code))
        {
            Debug.Log("Relay Client開始");
            NetworkManager.Singleton.StartClient();
        }
    }
}
