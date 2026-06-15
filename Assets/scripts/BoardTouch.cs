using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoardTouch : NetworkBehaviour
{
    public Material[] materials;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = materials[0]; // 初期化
    }

    public void SetMaterial(int index)
    {
        var mats = rend.materials;
        mats[0] = materials[index];
        rend.materials = mats;
    }

    void OnMouseUp()
    {
        if (this.tag == "lighted")
        {
            SoundManager.Instance.PlayPlace();

            GameObject transPiece = GameObject.FindWithTag("ownertile").transform.GetChild(0).gameObject;

            NetworkObject pieceNetObj = transPiece.GetComponent<NetworkObject>();
            NetworkObject tileNetObj = this.GetComponent<NetworkObject>();

            SelectionManager.Instance.RequestTurnEndServerRpc(pieceNetObj.NetworkObjectId, tileNetObj.NetworkObjectId);

            //この駒と同じ色の駒全てのRoleManagerを行い違う色の王がcheck状態にあるかを判断
            SelectionManager.Instance.CheckConfig(transPiece.tag, true);


        }
        SelectionManager.Instance.ResetSelection();
    }

}
