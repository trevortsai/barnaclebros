using UnityEngine;

/// <summary>
/// Attached to the shark in the animation test scene.
/// Automatically finds the swim animation and loops it.
/// </summary>
public class SharkSwimLoop : MonoBehaviour
{
    private const string CLIP_NAME = "Swim Horizontal";

    void Start()
    {
        Animation anim = GetComponentInChildren<Animation>();

        if (anim == null)
        {
            Debug.LogWarning("[SharkSwimLoop] No Animation component found on shark.");
            return;
        }

        AnimationState state = anim[CLIP_NAME];
        if (state == null)
        {
            Debug.LogWarning($"[SharkSwimLoop] Clip '{CLIP_NAME}' not found. " +
                             "Available clips: " + GetClipNames(anim));
            return;
        }

        state.wrapMode = WrapMode.Loop;
        anim.Play(CLIP_NAME);
    }

    static string GetClipNames(Animation anim)
    {
        var names = new System.Text.StringBuilder();
        foreach (AnimationState s in anim)
            names.Append(s.name).Append(", ");
        return names.Length > 0 ? names.ToString() : "(none)";
    }
}
