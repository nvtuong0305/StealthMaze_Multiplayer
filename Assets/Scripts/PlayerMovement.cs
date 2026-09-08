using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    private CharacterController controller;
    private Animator anim;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // --- 1. XỬ LÝ TRỌNG LỰC ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 2. XỬ LÝ DI CHUYỂN NGANG ---
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(moveX, 0f, moveZ).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Xoay mặt nhân vật
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Tiến lên
            controller.Move(direction * moveSpeed * Time.deltaTime);

            // BẬT HOẠT ẢNH CHẠY: Truyền tốc độ vào biến "Speed" của Animator
            if (anim != null)
            {
                anim.SetFloat("Speed", moveSpeed);

                // Asset StarterAssets thường cần thêm biến MotionSpeed để hoạt ảnh chạy mượt
                anim.SetFloat("MotionSpeed", 1f);
            }
        }
        else
        {
            // TẮT HOẠT ẢNH CHẠY: Đưa "Speed" về 0 để nhân vật đứng im
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("MotionSpeed", 0f);
            }
        }

        // --- 3. ÁP DỤNG LỰC RƠI ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}