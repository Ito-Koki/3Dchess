using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private string joinCode;

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        // シングルトンにして複数生成を防止
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await InitializeUnityServices();
        IsInitialized = true; // 完了したらフラグを立てる
        Debug.Log("✅ RelayManager 初期化完了");
    }

    private async Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Signed in to Unity Services (Relay ready)");
        }
    }

    public async Task WaitUntilInitialized()
    {
        while (!IsInitialized)
        {
            await Task.Delay(100); // 100msごとに確認
        }
    }

    // === Relayをホストとして作成 ===
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2); // 最大2人

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"🎫 Relay Join Code: {joinCode}");

            // Relay接続設定をUnityTransportに適用
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Relay create failed: {e.Message}");
            return null;
        }


    }

    // === Relayにクライアントとして参加 ===
    public async Task<bool> JoinRelay(string inputJoinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);
            Debug.Log($"🔗 Joined Relay with code: {inputJoinCode}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

            return true;
        }
        catch (System.Exception e)
        {
            //Debug.LogError($"Relay join failed: {e.Message}");
            return false;
        }
    }
}
