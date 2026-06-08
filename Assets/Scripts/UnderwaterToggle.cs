using UnityEngine;

public class UnderwaterToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Water plane here.")]
    public Transform waterPlane;

    [Tooltip("Drag the GameObject holding your Post-Processing Volume here.")]
    public GameObject underwaterEffects;

    [Header("Settings")]
    [Tooltip("Adjust this to fine-tune exactly where the camera 'breaks' the surface.")]
    public float surfaceOffset = 0f;

    [Tooltip("The solid color the camera will show when looking into the deep water.")]
    public Color underwaterBackgroundColor = new Color(0.1f, 0.4f, 0.7f); // A nice deep blue

    [Tooltip("If true, keep the skybox visible underwater (sky shows through the water surface) instead of an opaque background color.")]
    public bool showSkyboxUnderwater = false;

    private Camera cam;
    private bool wasUnderwater; // Tracks state to prevent running code every single frame

    void Start()
    {
        // Automatically grab the camera component attached to this object
        cam = GetComponent<Camera>();

        // Force an initial check right when the game starts
        wasUnderwater = !CheckIfUnderwater();
    }

    void Update()
    {
        if (waterPlane == null) return;

        bool isUnderwater = CheckIfUnderwater();

        // Only update the settings when we cross the water boundary
        if (isUnderwater != wasUnderwater)
        {
            wasUnderwater = isUnderwater;
            ApplyUnderwaterSettings(isUnderwater);
        }
    }

    bool CheckIfUnderwater()
    {
        return transform.position.y < (waterPlane.position.y + surfaceOffset);
    }

    void ApplyUnderwaterSettings(bool isUnderwater)
    {
        // 1. Toggle your Underwater Volume GameObject
        if (underwaterEffects != null)
        {
            underwaterEffects.SetActive(isUnderwater);
        }

        // 2. Toggle the Built-in Lighting Fog
        RenderSettings.fog = isUnderwater;

        // 3. Switch Camera Background between Solid Color and Skybox
        if (cam != null)
        {
            // When showSkyboxUnderwater is on, keep the skybox so the sky stays
            // visible through the water surface instead of an opaque color.
            if (isUnderwater && !showSkyboxUnderwater)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = underwaterBackgroundColor;
            }
            else
            {
                cam.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }
}