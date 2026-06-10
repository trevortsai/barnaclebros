using UnityEngine;

// Shows a pulsing red border and a "SharkNearby" label whenever the shark comes
// within warningDistance of the player. Drives the alpha of a CanvasGroup that
// holds the border images and the label.
public class SharkProximityWarning : MonoBehaviour
{
    [Header("References")]
    public Transform shark;
    public Transform player;

    [Tooltip("Holds the red border + 'SharkNearby' label; its alpha is pulsed.")]
    public CanvasGroup warningGroup;

    [Header("Settings")]
    [Tooltip("Show the warning when the shark is within this distance of the player.")]
    public float warningDistance = 12f;

    [Tooltip("How fast the border pulses.")]
    public float pulseSpeed = 3f;

    [Range(0f, 1f)] public float minAlpha = 0.25f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    void Start()
    {
        if (warningGroup != null)
        {
            warningGroup.alpha = 0f;
            warningGroup.blocksRaycasts = false;
            warningGroup.interactable = false;
        }
    }

    void Update()
    {
        if (warningGroup == null || shark == null || player == null) return;

        bool near = Vector3.Distance(shark.position, player.position) <= warningDistance;

        if (near)
        {
            // 0..1 sine pulse mapped to the alpha range.
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            warningGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        }
        else
        {
            warningGroup.alpha = 0f;
        }
    }
}
