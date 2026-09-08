using UnityEngine;

public class ItemSpin : MonoBehaviour
{
    public float spinSpeed = 100f; // Tốc độ xoay

    void Update()
    {
        // Làm chìa khóa xoay đều quanh trục Y
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
    }
}