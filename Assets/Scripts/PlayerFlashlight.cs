using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerFlashlight : NetworkBehaviour
{
    public GameObject spotlight; // Kéo GameObject Flashlight vào đây
    public float flashRange = 10f; // Tầm chiếu xa để làm choáng
    public float stunDuration = 3f; // Thời gian quái bị choáng (giây)
    public float cooldownTime = 15f; // Thời gian hồi chiêu
    private float nextUseTime = 0f;

    void Update()
    {
        if (!isLocalPlayer) return;

        // Bấm phím F để chớp đèn
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextUseTime)
        {
            CmdUseFlash();
            nextUseTime = Time.time + cooldownTime; // Kích hoạt hồi chiêu
        }
    }

    [Command]
    void CmdUseFlash()
    {
        // Kích hoạt hiệu ứng ánh sáng trên tất cả các máy
        RpcShowFlashEffect();

        // Quét tìm tất cả quái vật trong màn chơi
        MonsterAI[] monsters = FindObjectsOfType<MonsterAI>();
        foreach (MonsterAI monster in monsters)
        {
            // Đo khoảng cách từ người chơi đến quái vật
            float distance = Vector3.Distance(transform.position, monster.transform.position);

            // Nếu quái nằm trong bán kính chiếu sáng
            if (distance <= flashRange)
            {
                // Tính hướng từ nhân vật đến quái vật
                Vector3 dirToMonster = (monster.transform.position - transform.position).normalized;

                // Dot Product kiểm tra xem quái vật có nằm phía ĐẰNG TRƯỚC mặt nhân vật không (chống việc rọi đèn sau lưng mà quái đằng trước vẫn bị choáng)
                if (Vector3.Dot(transform.forward, dirToMonster) > 0.5f)
                {
                    monster.ApplyStun(stunDuration);
                }
            }
        }
    }

    [ClientRpc]
    void RpcShowFlashEffect()
    {
        if (spotlight != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    // Hiệu ứng chớp sáng nhanh rồi tắt đi
    IEnumerator FlashRoutine()
    {
        spotlight.SetActive(true);
        yield return new WaitForSeconds(0.5f); // Đèn chỉ sáng lên nửa giây
        spotlight.SetActive(false);
    }
}