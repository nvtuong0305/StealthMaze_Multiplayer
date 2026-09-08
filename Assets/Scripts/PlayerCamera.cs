using UnityEngine;
using Mirror;

public class PlayerCamera : NetworkBehaviour
{
    private Camera mainCam;
    public Vector3 cameraOffset = new Vector3(0f, 15f, -7f); // Độ cao và khoảng cách của Camera

    void Start()
    {
        // Chỉ lấy Camera nếu đây là nhân vật của mình (Local Player)
        if (isLocalPlayer)
        {
            mainCam = Camera.main;
        }
    }

    void LateUpdate()
    {
        // Bám theo vị trí nhân vật mỗi khung hình
        if (isLocalPlayer && mainCam != null)
        {
            mainCam.transform.position = transform.position + cameraOffset;
            mainCam.transform.LookAt(transform.position); // Luôn nhìn chúi xuống nhân vật
        }
    }
}