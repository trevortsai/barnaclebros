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

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Destroy(gameObject, 10f);
    }
}