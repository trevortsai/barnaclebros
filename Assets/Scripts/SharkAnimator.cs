using UnityEngine;

/// <summary>
/// Drives shark animations. Assign the clips from the shark.glb sub-assets
/// in the Inspector (expand shark.glb in the Project window to find them).
/// </summary>
public class SharkAnimator : MonoBehaviour
{
    public const string SWIM = "Swim Horizontal";
    public const string BITE = "Bite";
    public const string HIT  = "Hit";
    public const string IDLE = "Idle";

    [Header("Animation Clips (drag from shark.glb sub-assets)")]
    public AnimationClip swimHorizontal;
    public AnimationClip bite;
    public AnimationClip hit;
    public AnimationClip idle;

    private Animation _anim;
    private string    _currentAnim = "";

    void Awake()
    {
        // Disable root motion on any Animator present (empty controller from GLB import)
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.applyRootMotion = false;

        // Use a Legacy Animation component — works without a controller
        _anim = GetComponentInChildren<Animation>();
        if (_anim == null)
            _anim = gameObject.AddComponent<Animation>();

        _anim.playAutomatically = false;
        _anim.Stop();

        AddClip(swimHorizontal, SWIM);
        AddClip(bite,           BITE);
        AddClip(hit,            HIT);
        AddClip(idle,           IDLE);
    }

    void AddClip(AnimationClip clip, string name)
    {
        if (clip == null) return;
        _anim.AddClip(clip, name);
    }

    // ── public API called by SharkBehavior ───────────────────────────────

    public void Loop(string clipName)
    {
        if (_anim == null || _anim[clipName] == null) return;

        if (_currentAnim != clipName)
        {
            _currentAnim = clipName;
            _anim[clipName].wrapMode = WrapMode.Loop;
            _anim.CrossFade(clipName, 0.2f);
        }
    }

    public void Play(string clipName)
    {
        if (_anim == null || _anim[clipName] == null) return;

        if (_currentAnim != clipName)
        {
            _currentAnim = clipName;
            _anim[clipName].wrapMode = WrapMode.Once;
            _anim.CrossFade(clipName, 0.1f);
        }
    }

    public bool IsFinished(string clipName)
    {
        if (_anim == null) return true;
        return !_anim.IsPlaying(clipName);
    }
}
