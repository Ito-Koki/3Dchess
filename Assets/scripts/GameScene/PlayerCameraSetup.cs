using UnityEngine;
using Unity.Netcode;

public class PlayerCameraSetup : NetworkBehaviour
{
    private Camera playerCamera;
    private Canvas playerCanvas;

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>(true);
        playerCanvas = GetComponentInChildren<Canvas>(true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 自分のカメラを有効化
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);

            // 自分のUIも有効化
            //playerCanvas.enabled = true;

            // カメラの位置をプレイヤー番号で調整
            Vector3 camPos = (OwnerClientId == 0)
                ? new Vector3(0, 10, -10)  // プレイヤー1視点
                : new Vector3(0, 10, 10);  // プレイヤー2視点（反対側）

            playerCamera.transform.position = camPos;
            playerCamera.transform.LookAt(Vector3.zero);
        }
        else
        {
            // 他人のカメラとUIは無効化
            playerCamera.enabled = false;
            playerCanvas.enabled = false;
        }
    }
}
