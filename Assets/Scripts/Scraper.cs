using UnityEngine;

public class Scraper : MonoBehaviour
{
    public float scrapeDistance = 15f;
    public float scrapeSpeed = 4f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, scrapeDistance))
            {
                Barnacle barnacle = hit.collider.GetComponent<Barnacle>();

                if (barnacle != null)
                {
                    barnacle.Scrape(scrapeSpeed * Time.deltaTime);
                }
            }
        }
    }
}