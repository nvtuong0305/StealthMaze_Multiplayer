using UnityEngine;

public class MonsterDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Bật hoạt ảnh tấn công
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            // Xóa sổ Player sau 0.5 giây
            Destroy(other.gameObject, 0.5f);
        }
    }
}