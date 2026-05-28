using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    // Adjust this in the Inspector if the shark faces the wrong way (e.g. 0,90,0 or 0,180,0)
    public Vector3 rotationOffset = Vector3.zero;

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
    }
}
