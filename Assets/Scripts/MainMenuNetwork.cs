using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuNetwork : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public GameObject mainMenuPanel;

    void Start()
    {
        if (ipInputField != null) ipInputField.text = "localhost";
    }

    public void OnHostButtonClicked()
    {
        // Chặn lỗi: Chỉ tạo phòng nếu server và client đều chưa chạy
        if (!NetworkServer.active && !NetworkClient.active)
        {
            NetworkManager.singleton.StartHost();
            HideMenu();
        }
        else
        {
            Debug.LogWarning("Mạng đã được bật rồi, không thể tạo phòng thêm!");
            HideMenu();
        }
    }

    public void OnClientButtonClicked()
    {
        // Chặn lỗi: Chỉ tìm phòng nếu chưa kết nối
        if (!NetworkClient.active)
        {
            NetworkManager.singleton.networkAddress = ipInputField.text;
            NetworkManager.singleton.StartClient();
            HideMenu();
        }
    }

    void HideMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }
}