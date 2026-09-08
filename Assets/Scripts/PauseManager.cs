using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel; // Kéo PausePanel vào đây

    void Update()
    {
        // Bấm ESC để bật/tắt Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isPaused = !pausePanel.activeSelf;
            pausePanel.SetActive(isPaused);

            // Mở khóa chuột để người chơi có thể bấm nút
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Gắn hàm này vào Nút "Tiếp tục"
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}