using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageSwitcher : MonoBehaviour
{
    [SerializeField] private Image targetImage;      // 表示するImageコンポーネント
    [SerializeField] private Sprite[] images;         // 切り替える画像一覧
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private string[] texts;

    private int currentIndex = 0;

    void Start()
    {
        if (images.Length > 0)
        {
            targetImage.sprite = images[currentIndex];
        }
    }

    public void PreviousImage()
    {
        if (images.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = images.Length - 1; // 最初の前は最後に戻る

        targetImage.sprite = images[currentIndex];
        targetText.text = texts[currentIndex];
    }

    public void NextImage()
    {
        if (images.Length == 0) return;

        currentIndex++;
        if (currentIndex >= images.Length) currentIndex = 0; // 最後の次は最初に戻る

        targetImage.sprite = images[currentIndex];
        targetText.text = texts[currentIndex];
    }
}