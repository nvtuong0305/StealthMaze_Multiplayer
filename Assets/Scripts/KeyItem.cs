using UnityEngine;
using Mirror;

public class KeyItem : NetworkBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // Chỉ Server xử lý việc nhặt vật phẩm để chống hack[cite: 9]

        if (other.CompareTag("Player"))
        {
            // Thay vì chỉ lấy túi đồ của người chạm vào, 
            // chúng ta sẽ tìm TẤT CẢ các túi đồ (PlayerInventory) đang có trong game.
            PlayerInventory[] allPlayers = FindObjectsOfType<PlayerInventory>();

            // Duyệt qua từng người chơi và cộng chìa khóa cho họ
            foreach (PlayerInventory inventory in allPlayers)
            {
                inventory.AddKey();
            }

            // Sau khi chia đều điểm cho cả đội, xóa chìa khóa trên mạng (biến mất ở mọi máy)[cite: 9]
            NetworkServer.Destroy(gameObject);
        }
    }
}