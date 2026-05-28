using UnityEngine;

public class Barnacle : MonoBehaviour
{
    public float scrapeNeeded = 3f;

    private float scrapeAmount = 0f;

    private bool removed = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void Scrape(float amount)
    {
        if (removed) return;

        scrapeAmount += amount;

        if (scrapeAmount >= scrapeNeeded)
        {
            PopOff();
        }
    }

    public void PopOff()
    {
        removed = true;

        GameManager.instance.BarnacleRemoved();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.down * 0.5f;
            rb.linearDamping = 2f;
            rb.angularDamping = 2f;
        }

        Destroy(gameObject, 5f);
    }
}