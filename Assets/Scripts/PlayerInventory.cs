using UnityEngine;
using Mirror;
using TMPro;

public class PlayerInventory : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnKeysChanged))]
    public int keysCollected = 0;

    private TextMeshProUGUI keyUIText;
    public GameObject victoryPanel;

    void Start()
    {
        if (isLocalPlayer)
        {
            GameObject uiObj = GameObject.Find("KeyText");
            if (uiObj != null)
            {
                keyUIText = uiObj.GetComponent<TextMeshProUGUI>();
                UpdateUIText(keysCollected);
            }

            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                Transform panelTransform = canvasObj.transform.Find("VictoryPanel");
                if (panelTransform != null)
                {
                    victoryPanel = panelTransform.gameObject;
                    victoryPanel.SetActive(false); // Đảm bảo luôn ẩn lúc đầu
                }
            }
        }
    }

    public void AddKey()
    {
        if (!isServer) return;
        keysCollected++;
    }

    void OnKeysChanged(int oldValue, int newValue)
    {
        if (isLocalPlayer)
        {
            UpdateUIText(newValue);

            // Thay vì cho thắng luôn, chúng ta đổi chữ nhắc nhở người chơi
            if (newValue >= 3)
            {

            }
        }
    }

    void UpdateUIText(int currentKeys)
    {
        if (keyUIText != null)
        {
            keyUIText.text = "Chìa khóa: " + currentKeys + " / 3";
        }
    }

    // Hàm này sẽ được cái Cửa gọi khi người chơi chạy tới chạm vào nó
    public void TriggerVictory()
    {
        if (!isLocalPlayer) return;

        Debug.Log("Kích hoạt bảng chiến thắng từ Cửa thoát!");
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Khóa di chuyển nhân vật khi đã thắng
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }
}