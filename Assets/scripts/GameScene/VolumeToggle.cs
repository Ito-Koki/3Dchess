using UnityEngine;
using UnityEngine.UI;

public class VolumeToggle : MonoBehaviour
{
    [SerializeField] private Image volumeIcon;
    [SerializeField] private Sprite soundOnSprite;   // kkrn_icon_onsei_1
    [SerializeField] private Sprite soundOffSprite;  // kkrn_icon_onsei_4

    private bool isMuted = false;

    void Start()
    {
        // 初期状態の音量を反映
        UpdateIcon();
    }

    public void ToggleVolume()
    {
        isMuted = !isMuted;

        // 全体の音量をミュート/解除
        AudioListener.volume = isMuted ? 0f : 1f;

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        volumeIcon.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}