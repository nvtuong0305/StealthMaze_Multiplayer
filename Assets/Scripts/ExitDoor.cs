using UnityEngine;
using Mirror;

public class ExitDoor : NetworkBehaviour
{
    [Header("Cấu Hình Cửa 2 Phần")]
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Âm Thanh Cửa")]
    public AudioSource audioSource;
    public AudioClip creakSound;

    [Header("Giao Diện")]
    public int keysRequired = 3;
    public GameObject victoryPanel; // Biến chứa bảng Victory

    // Biến đồng bộ trạng thái cửa qua mạng
    [SyncVar(hook = nameof(OnDoorStateChanged))]
    private bool isOpen = false;

    private Quaternion targetRotation;

    void Start()
    {
        // Lưu lại góc xoay ban đầu của cánh cửa
        if (doorPivot != null)
        {
            targetRotation = doorPivot.localRotation;
        }

        // Tự động tìm VictoryPanel dù nó đang bị ẩn (tương tự GameOver)
        if (victoryPanel == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                Transform panelTransform = canvasObj.transform.Find("VictoryPanel"); // Đảm bảo bạn đặt tên đúng là VictoryPanel trong Hierarchy nhé
                if (panelTransform != null)
                {
                    victoryPanel = panelTransform.gameObject;
                }
            }
        }
    }

    void Update()
    {
        if (isOpen && doorPivot != null)
        {
            doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                if (inventory.keysCollected >= keysRequired && !isOpen)
                {
                    Debug.Log("CHÚC MỪNG! Bạn đã mở cửa và THẮNG GAME!");

                    isOpen = true;
                    targetRotation = Quaternion.Euler(0, openAngle, 0);

                    RpcPlayCreakSound();
                    RpcShowVictory(); // Gọi hàm hiện UI cho TẤT CẢ máy con
                }
                else if (!isOpen)
                {
                    Debug.Log("Cửa khóa chặt! Bạn cần tìm đủ " + keysRequired + " chìa khóa!");
                }
            }
        }
    }

    void OnDoorStateChanged(bool oldState, bool newState)
    {
        if (newState && doorPivot != null)
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
        }
    }

    [ClientRpc]
    void RpcPlayCreakSound()
    {
        if (audioSource != null && creakSound != null)
        {
            audioSource.PlayOneShot(creakSound);
        }
    }

    // Lệnh này sẽ chạy trên màn hình của tất cả mọi người trong phòng
    [ClientRpc]
    void RpcShowVictory()
    {
        // 1. Hiện bảng chiến thắng
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // 2. Mở khóa chuột để người chơi có thể bấm nút Restart/Quit
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 3. Khóa di chuyển của tất cả lính SWAT để đứng im ăn mừng
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            MonoBehaviour move = p.GetComponent("PlayerMovement") as MonoBehaviour;
            if (move != null) move.enabled = false;
        }
    }
}