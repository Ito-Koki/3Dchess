using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 0.3f;

    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Update()
    {
        // マウスボタンが押された瞬間
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        // マウスボタンが離された瞬間
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // ドラッグ中
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // X方向のドラッグ → Y軸回転（左右）
            // Y方向のドラッグ → X軸回転（上下）
            transform.Rotate(Vector3.up, -delta.x * rotateSpeed, Space.World);
            transform.Rotate(Vector3.right, delta.y * rotateSpeed, Space.World);

            lastMousePosition = Input.mousePosition;
        }
    }
}