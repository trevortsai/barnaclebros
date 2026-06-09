using UnityEngine;

public class BarnSpaw : MonoBehaviour
{
    public GameObject barnaclePrefab;
    public Transform boatTransform;
    public Collider boatCollider;

    // How far away from the boat the ray starts. 
    // Make sure this is larger than the boat itself.
    public float raycastStartDistance = 10f;

    // The number of barnacles to spawn
    public int barnaclesToSpawn = 20;

    public float waterLevelY = 0f;

    void Start()
    {
        SpawnBarnacles();
    }

    void SpawnBarnacles()
    {
        int spawned = 0;
        int attempts = 0; // Prevent infinite loops

        while (spawned < barnaclesToSpawn && attempts < 1000)
        {
            attempts++;

            // 1. Pick a random direction around the boat
            Vector3 randomDirection = Random.onUnitSphere;

            // 2. Start outside the boat and look inward toward the boat's center
            Vector3 rayStartPoint = boatTransform.position + (randomDirection * raycastStartDistance);
            Vector3 rayDirection = (boatTransform.position - rayStartPoint).normalized;

            // 3. Shoot the ray
            if (Physics.Raycast(rayStartPoint, rayDirection, out RaycastHit hit))
            {
                // Ensure we actually hit the boat and not the water or something else
                if (hit.collider == boatCollider)
                {
                    if (hit.point.y <= waterLevelY)
                    {
                        // 4. Align the barnacle to the surface of the boat
                        // This assumes the "Up" (Y-axis) of your barnacle model points outward.
                        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                        Vector3 sunkenPosition = hit.point - (hit.normal * 0.05f);

                        // 5. Spawn the barnacle using the new sunken position
                        GameObject newBarnacle = Instantiate(barnaclePrefab, sunkenPosition, surfaceRotation);

                        // Optional: Make the barnacle a child of the boat so it moves with it
                        newBarnacle.transform.SetParent(boatTransform);

                        spawned++;
                    }
                }
            }
        }
    }
}