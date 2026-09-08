using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth;

    [SyncVar]
    public bool isDowned = false; // Trạng thái gục ngã

    public GameObject gameOverPanel;
    public Image healthBarFill;

    void Update()
    {
        if (!isLocalPlayer) return;

        // Nếu mình VẪN CÒN SỐNG và bấm phím E
        if (!isDowned && Input.GetKeyDown(KeyCode.E))
        {
            CmdTryReviveTeammate(); // Gửi yêu cầu lên Server để cứu người
        }
    }

    void Start()
    {
        if (isServer)
        {
            currentHealth = maxHealth;
        }

        // CHỈ cho phép nhân vật "chính chủ" đi tìm và kết nối với UI
        if (isLocalPlayer)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                if (gameOverPanel == null)
                {
                    Transform panelTransform = canvasObj.transform.Find("GameOverPanel");
                    if (panelTransform != null) gameOverPanel = panelTransform.gameObject;
                }

                if (healthBarFill == null)
                {
                    Transform fillTransform = canvasObj.transform.Find("HealthyBarCurrent");
                    if (fillTransform != null) healthBarFill = fillTransform.GetComponent<Image>();
                }
            }
        }
    }

    [Server]
    public void TakeDamage(int damage)
    {
        // Nếu đã gục rồi thì không bị cắn trừ máu thêm nữa
        if (currentHealth <= 0 || isDowned) return;

        currentHealth -= damage;
        Debug.Log($"Player bị cắn! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDowned = true; // Đánh dấu là đã gục ngã
            RpcPlayerDie();
        }
    }

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        // CHỈ cập nhật thanh máu UI nếu đây là nhân vật của người chơi đang ngồi trên máy đó
        if (isLocalPlayer)
        {
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = (float)newHealth / maxHealth;
            }
        }
    }

    [ClientRpc]
    void RpcPlayerDie()
    {
        if (!isLocalPlayer) return;

        Debug.Log("Bạn đã hy sinh!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        MonoBehaviour movement = GetComponent("PlayerMovement") as MonoBehaviour;
        if (movement != null) movement.enabled = false;
    }

    // 1. Máy con gửi lệnh lên Server yêu cầu cứu người xung quanh
    [Command]
    void CmdTryReviveTeammate()
    {
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();
        foreach (PlayerHealth teammate in allPlayers)
        {
            // Tìm người chơi KHÁC mình và đang trong trạng thái GỤC NGÃ
            if (teammate != this && teammate.isDowned)
            {
                // Đo khoảng cách giữa mình và người đó
                float distance = Vector3.Distance(transform.position, teammate.transform.position);
                if (distance <= 3f) // Phải đứng đủ gần (dưới 3 mét)
                {
                    teammate.ServerRevive(); // Ra lệnh hồi sinh người đó
                    break; // Cứu thành công 1 người là dừng vòng lặp
                }
            }
        }
    }

    // 2. Server xử lý việc hồi máu
    [Server]
    public void ServerRevive()
    {
        isDowned = false;
        currentHealth = maxHealth / 2; // Hồi lại 50% máu khi được cứu
        RpcOnRevived(); // Báo về cho các máy con để tắt UI Game Over
    }

    // 3. Cập nhật giao diện trên màn hình của người vừa được cứu
    [ClientRpc]
    void RpcOnRevived()
    {
        if (isLocalPlayer)
        {
            // Tắt bảng Game Over đi
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Mở khóa lại quyền di chuyển
            MonoBehaviour movement = GetComponent("PlayerMovement") as MonoBehaviour;
            if (movement != null) movement.enabled = true;

            Debug.Log("Bạn đã được đồng đội cứu sống!");
        }
    }
}