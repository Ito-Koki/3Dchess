using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class RelayTest : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Authentication 成功: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Authentication 失敗: " + e.Message);
            return;
        }

        try
        {
            // 名前空間を明示的に指定
            Unity.Services.Relay.Models.Allocation allocation =
                await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(4);

            string joinCode =
                await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("✅ Relay サーバー作成成功: Join Code = " + joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Relay 作成失敗: " + e.Message);
        }
    }
}
