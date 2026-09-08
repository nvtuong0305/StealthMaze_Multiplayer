using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void RestartGame()
    {
        Debug.Log("Đang dọn dẹp hệ thống mạng...");

        // 1. Tắt hết các kết nối đang chạy của Mirror trước
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }

        // 2. Gọi hàm hủy đối tượng NetworkManager tạm thời để tránh lỗi "Multiple NetworkManagers" khi reload
        if (NetworkManager.singleton != null)
        {
            Destroy(NetworkManager.singleton.gameObject);
        }

        // 3. Khởi động lại màn chơi mới hoàn toàn sạch sẽ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}