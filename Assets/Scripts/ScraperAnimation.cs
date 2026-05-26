using UnityEngine;

public class ScraperAnimation : MonoBehaviour
{
    public Vector3 restingPosition = new Vector3(0.22f, -0.28f, 0.55f);
    public Vector3 scrapingPosition = new Vector3(0.22f, -0.12f, 0.70f);

    public float animationSpeed = 12f;

    void Start()
    {
        transform.localPosition = restingPosition;
    }

    void Update()
    {
        Vector3 target = restingPosition;

        if (Input.GetMouseButton(0))
        {
            target = scrapingPosition;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            target,
            animationSpeed * Time.deltaTime
        );
    }
}