using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TurnTextWrite : MonoBehaviour
{
    [SerializeField] private Text infoText;
    public static bool isMate = false;
    void Update()
    {
        if (!isMate)
        {
            if (SelectionManager.Instance.turn.Value % 2 == 0)
            {
                infoText.text = "white turn";
            }
            else
            {
                infoText.text = "black turn";
            }
        }
        else
        {
            if (SelectionManager.Instance.turn.Value % 2 == 0)
            {
                infoText.text = "black win";
            }
            else
            {
                infoText.text = "white win";
            }
        }

    }
}
