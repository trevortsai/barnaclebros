using UnityEngine;

public class SeaweedSway : MonoBehaviour
{
    public float swayAmount = 8f;
    public float swaySpeed = 1.5f;
    public float randomOffset = 0f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAmount;
        transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, sway);
    }
}