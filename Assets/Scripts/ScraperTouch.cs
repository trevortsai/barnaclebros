using UnityEngine;

public class ScraperTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Barnacle barnacle = other.GetComponent<Barnacle>();

        if (barnacle != null)
        {
            barnacle.PopOff();
        }
    }
}