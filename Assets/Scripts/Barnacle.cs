using UnityEngine;

public class Barnacle : MonoBehaviour
{
    public float scrapeNeeded = 3f;

    [Header("Underwater sink physics")]
    [Tooltip("Net downward acceleration after buoyancy (m/s^2). Lower = slower sink.")]
    public float sinkAcceleration = 1.2f;

    [Tooltip("Water linear drag. Higher = slower terminal velocity.")]
    public float waterDrag = 1.8f;

    [Tooltip("Water angular drag for gentle tumbling.")]
    public float waterAngularDrag = 0.8f;

    [Tooltip("Gentle random tumble speed on pop-off (radians/sec).")]
    public float tumbleStrength = 1.2f;

    [Tooltip("Seconds before the popped barnacle is removed.")]
    public float lifetime = 12f;

    private float scrapeAmount = 0f;

    private bool removed = false;
    private bool sinking = false;

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

            // Sink under our own buoyancy-reduced gravity instead of full gravity,
            // with heavy water drag so it drifts down slowly rather than dropping.
            rb.useGravity = false;
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDrag;

            // A gentle tumble as it detaches (set directly so it's independent of mass/inertia).
            rb.angularVelocity = Random.insideUnitSphere * tumbleStrength;

            sinking = true;
        }

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (sinking && rb != null)
        {
            // Weight minus buoyancy: a small constant downward acceleration that the
            // water drag quickly balances, giving a slow terminal sink speed.
            rb.AddForce(Vector3.down * sinkAcceleration, ForceMode.Acceleration);
        }
    }
}
