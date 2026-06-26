using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TurnTextWrite : MonoBehaviour
{
    [SerializeField] private Text infoText;
    public static bool isMate = false;
    public static bool isDraw = false;

    void Update()
    {
        if (isDraw)
        {
            infoText.text = "Draw";
        }
        else if (!isMate)
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
