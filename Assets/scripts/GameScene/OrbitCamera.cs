using UnityEngine;
using Unity.Netcode;

public class OrbitCamera : NetworkBehaviour
{
    [Header("注視対象（中心となるオブジェクト）")]
    public Transform target;

    [Header("カメラの距離設定")]
    public float distance = 3.0f;      // 初期距離
    public float zoomSpeed = 2.0f;     // ズーム速度
    public float minDistance = 2.0f;   // 最小距離
    public float maxDistance = 5.0f;  // 最大距離

    [Header("回転速度")]
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    [Header("縦方向の回転制限")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    public override void OnNetworkSpawn()
    {
        // ローカルプレイヤー以外は無効化
        if (!IsOwner)
        {
            // 自分以外のプレイヤーのカメラはOFF
            if (GetComponent<Camera>() != null)
                GetComponent<Camera>().enabled = false;

            enabled = false;
            return;
        }

        // カメラを有効化（自分のものだけ）
        if (GetComponent<Camera>() != null)
            GetComponent<Camera>().enabled = true;



        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        /*
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
        */
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject obj = GameObject.Find("chessboards");

            if (obj != null)
                target = obj.transform;

            return;
        }

        if (!IsOwner || target == null) return;

        // マウスクリックで回転
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y += Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // マウスホイールでズーム
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);


        // タッチ操作
        if (Input.touchCount == 1) // 1本指 → 回転
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                x += touch.deltaPosition.x * xSpeed * 0.02f * 0.5f; // スマホ向けに感度調整
                y += touch.deltaPosition.y * ySpeed * 0.02f * 0.5f;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }
        }
        else if (Input.touchCount == 2) // 2本指 → ピンチでズーム
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 pos0 = t0.position - t0.deltaPosition;
            Vector2 pos1 = t1.position - t1.deltaPosition;

            float prevDist = (pos0 - pos1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = prevDist - currDist;
            distance += delta * zoomSpeed * 0.01f; // ピンチの変化量でズーム
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }




        // 回転を反映
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }

    // 角度制限
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
