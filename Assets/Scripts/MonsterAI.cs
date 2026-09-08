using UnityEngine;
using UnityEngine.AI;
using Mirror; // Dùng cho Multiplayer

public class MonsterAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator anim; // Thêm biến chứa Animator

    [Header("Tuần Tra (Wander)")]
    public float wanderRadius = 10f;
    public float wanderSpeed = 3.5f;

    [Header("Trạng Thái Bị Choáng")]
    private bool isStunned = false;
    private float stunTimer;

    public float waitTime = 2.5f; // Thời gian quái vật đứng nghỉ (giây)
    private float waitTimer;      // Bộ đếm thời gian
    private bool isWaiting;       // Trạng thái có đang đứng nghỉ hay không

    [Header("Phát Hiện & Rượt Đuổi (Chase)")]
    public float detectionRadius = 6f; // Khoảng cách nhìn thấy người chơi
    public float chaseSpeed = 5.5f;     // Tốc độ khi đuổi theo (chạy nhanh hơn)

    [Header("Tấn Công (Attack)")]
    public float attackRadius = 2f;    // Khoảng cách để vồ lấy tấn công
    public float attackCooldown = 1.5f; // Thời gian giãn cách giữa các cú vồ
    public int attackDamage = 25;      // Sát thương mỗi lần cắn
    private float nextAttackTime;

    private Transform targetPlayer;     // Mục tiêu đang bị đuổi

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); // Lấy Animator của quái vật

        agent.speed = wanderSpeed;
    }

    void Update()
    {
        // 1. CẬP NHẬT HOẠT ẢNH (Chạy trên cả Server và Client để ai cũng thấy quái di chuyển)
        if (anim != null && agent != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        // 2. LOGIC TÌM ĐƯỜNG & TẤN CÔNG (Chỉ Server mới được tính toán hành vi của AI)
        if (!isServer) return;

        // --- NẾU ĐANG BỊ CHOÁNG THÌ KHÔNG LÀM GÌ CẢ ---
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false; // Hết choáng, chuẩn bị đuổi tiếp
            }
            return; // Lệnh return này sẽ ngắt toàn bộ logic dò tìm và chạy theo bên dưới
        }

        FindClosestPlayer();

        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRadius)
            {
                // --- CHẾ ĐỘ TẤN CÔNG ---
                agent.isStopped = true; // Dừng chạy để đứng lại cắn

                // Mẹo nhỏ: Ép quái vật luôn xoay mặt nhìn thẳng vào lính SWAT khi cắn
                Vector3 direction = (targetPlayer.position - transform.position).normalized;
                direction.y = 0; // Giữ y=0 để quái không bị ngửa mặt lên trời
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

                AttackPlayer();
            }
            else
            {
                // --- CHẾ ĐỘ RƯỢT ĐUỔI ---
                agent.isStopped = false; // Tiếp tục chạy
                agent.speed = chaseSpeed;
                agent.SetDestination(targetPlayer.position);
            }
        }
        else
        {
            // --- CHẾ ĐỘ ĐI TUẦN TRA (CÓ DỪNG NGHỈ) ---

            // Nếu đang trong trạng thái chờ
            if (isWaiting)
            {
                agent.isStopped = true; // Bắt buộc quái vật dừng lại
                waitTimer -= Time.deltaTime; // Trừ dần thời gian chờ

                // Khi hết thời gian chờ
                if (waitTimer <= 0f)
                {
                    isWaiting = false; // Tắt trạng thái chờ

                    // Tìm điểm ngẫu nhiên mới để đi tiếp
                    Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
                    randomDirection += transform.position;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
                    {
                        agent.SetDestination(hit.position);
                    }
                }
            }
            // Nếu không trong trạng thái chờ (đang đi)
            else
            {
                agent.isStopped = false;
                agent.speed = wanderSpeed;

                // Kiểm tra xem đã đi đến đích chưa (khoảng cách còn lại < 0.5f và không phải đang tính toán đường)
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    // Chuyển sang trạng thái chờ nghỉ ngơi
                    isWaiting = true;
                    waitTimer = waitTime; // Bắt đầu đếm ngược
                }
            }
        }
    }

    // Hàm quét tìm người chơi gần nhất trong bán kính phát hiện
    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = detectionRadius;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        targetPlayer = closestPlayer; // Nếu tìm thấy sẽ gắn vào mục tiêu, không thấy sẽ là null
    }

    // Hàm thực hiện đòn đánh
    void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            // 1. Bật Trigger hoạt ảnh đánh trong Animator
            if (anim != null)
            {
                anim.SetTrigger("Attack"); // Cần đảm bảo trong Animator của bạn có parameter tên là "Attack" (kiểu Trigger)
            }

            Debug.Log("Quái vật đang vồ lính SWAT!");

            // 2. Gây sát thương lên người chơi (Gọi file PlayerHealth)
            PlayerHealth playerHealth = targetPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            // 3. Reset lại thời gian chờ cho cú đánh tiếp theo
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // Vẽ các vòng tròn trong Scene để căn chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Vẽ thêm vòng tròn màu vàng cho tầm cắn (Attack Radius)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    // Hàm này sẽ được người chơi gọi để làm choáng quái vật
    public void ApplyStun(float stunDuration)
    {
        if (!isServer) return;

        isStunned = true;
        stunTimer = stunDuration;

        agent.isStopped = true; // Ép dừng lại ngay lập tức
        if (anim != null) anim.SetFloat("Speed", 0f); // Tắt hoạt ảnh chạy

        Debug.Log("Quái vật bị chói mắt!");
    }
}