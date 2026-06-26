using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectTextWrite : MonoBehaviour
{
    [SerializeField] private Text playerText;

    void Update()
    {
        if (NetworkManager.Singleton != null)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            playerText.text = "Players : " + playerCount;
        }
    }
}
