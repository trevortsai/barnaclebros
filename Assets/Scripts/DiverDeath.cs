using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Diver root object.
/// The diver.glb must be an instantiated child of this object.
/// Called by SharkBehavior when the shark bites the player.
///
/// FPS mode  → all SkinnedMeshRenderers on the model are hidden (camera is inside the torso).
/// On death  → renderers are revealed, camera pulls back to 3rd-person, knockback plays,
///             then CharacterController is swapped for a Rigidbody and the body sinks.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class DiverDeath : MonoBehaviour
{
    // ── 3rd-person camera ────────────────────────────────────────────
    [Header("3rd-Person Camera")]
    [Tooltip("How many units behind the diver the camera ends up.")]
    public float camOffsetBack = 3.5f;
    [Tooltip("How many units above the diver the camera ends up.")]
    public float camOffsetUp   = 2f;
    [Tooltip("Seconds to slide the camera to its 3rd-person position.")]
    public float camTransitionDuration = 0.55f;

    // ── sinking physics ──────────────────────────────────────────────
    [Header("Sinking Physics")]
    [Tooltip("Linear drag applied to the Rigidbody — simulates water resistance.")]
    public float sinkDrag        = 2.5f;
    public float sinkAngularDrag = 4f;
    [Tooltip("Gravity scale applied to the Rigidbody when sinking (1 = full gravity, " +
             "0.25 = slow underwater drift). Lower values make the diver float down gently.")]
    public float sinkGravityScale = 0.25f;
    [Tooltip("Extra pause (seconds) between knockback clip ending and sinking.")]
    public float sinkDelay       = 0.08f;

    // ── state ────────────────────────────────────────────────────────
    public bool IsDead { get; private set; }

    // ── private refs ─────────────────────────────────────────────────
    private Animation              _diverAnim;
    private SkinnedMeshRenderer[]  _smrs;
    private Transform              _camTransform;
    private MouseLook              _mouseLook;
    private Rigidbody              _sinkRb;        // set after death, used by gravity coroutine

    // ─────────────────────────────────────────────────────────────────
    void Start()
    {
        // Locate the diver.glb child (instantiated via PrefabUtility in the scene)
        _diverAnim = GetComponentInChildren<Animation>(includeInactive: true);
        _smrs      = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);

        if (_diverAnim == null)
            Debug.LogWarning("[DiverDeath] No Animation component found. " +
                             "Make sure diver.glb is a child and imported with Legacy animation.");

        // ── Hide the body mesh in FPS mode ────────────────────────────
        // The FPS camera sits inside the torso (Y≈0.6), so the mesh clips into
        // the view. We hide it here and reveal it the moment the death camera
        // pulls back to 3rd-person.
        SetRenderersEnabled(false);

        // Idle swim animation loops silently (body hidden, but ready)
        if (_diverAnim != null)
        {
            AnimationState idle = _diverAnim["Swim_Idle_Loop"];
            if (idle != null) { idle.wrapMode = WrapMode.Loop; _diverAnim.Play("Swim_Idle_Loop"); }
        }

        // Camera is the Main Camera (child of this Diver object)
        _camTransform = Camera.main?.transform;
        if (_camTransform != null)
            _mouseLook = _camTransform.GetComponent<MouseLook>();
    }

    // ── public API ───────────────────────────────────────────────────
    public void TriggerDeath()
    {
        if (IsDead) return;
        IsDead = true;
        StartCoroutine(DeathSequence());
    }

    // ── helpers ──────────────────────────────────────────────────────
    void SetRenderersEnabled(bool on)
    {
        if (_smrs == null) return;
        foreach (var smr in _smrs)
            if (smr != null) smr.enabled = on;
    }

    // ── death sequence coroutine ─────────────────────────────────────
    IEnumerator DeathSequence()
    {
        // 1. Freeze player controls & hide viewmodel ─────────────────
        var movement = GetComponent<DiverMovement>();
        if (movement) movement.enabled = false;
        if (_mouseLook) _mouseLook.enabled = false;

        // Hide the putty-knife viewmodel (child of Main Camera)
        if (_camTransform != null)
        {
            Transform knife = _camTransform.Find("Putty Knife");
            if (knife != null) knife.gameObject.SetActive(false);
        }

        // 2. Reveal the body mesh (camera is about to pull back) ──────
        SetRenderersEnabled(true);

        // 3. Play knockback animation ─────────────────────────────────
        float clipLength = 1.2f;
        if (_diverAnim != null)
        {
            AnimationState state = _diverAnim["Hit_Knockback"];
            if (state != null)
            {
                clipLength     = state.length;
                state.wrapMode = WrapMode.Once;
                _diverAnim.CrossFade("Hit_Knockback", 0.1f);
            }
        }

        // 4. Swing camera to 3rd-person ───────────────────────────────
        if (_camTransform != null)
        {
            Vector3    startPos = _camTransform.position;
            Quaternion startRot = _camTransform.rotation;
            _camTransform.SetParent(null, worldPositionStays: true);

            Vector3 targetPos = transform.position
                               - transform.forward * camOffsetBack
                               + Vector3.up        * camOffsetUp;

            float elapsed = 0f;
            while (elapsed < camTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / camTransitionDuration);

                _camTransform.position = Vector3.Lerp(startPos, targetPos, t);

                Vector3    lookTarget = transform.position + Vector3.up * 0.9f;
                Quaternion lookRot    = Quaternion.LookRotation(lookTarget - _camTransform.position);
                _camTransform.rotation = Quaternion.Slerp(startRot, lookRot, t);

                yield return null;
            }
        }

        // 5. Wait for knockback clip to finish ────────────────────────
        float wait = clipLength - camTransitionDuration + sinkDelay;
        if (wait > 0f)
            yield return new WaitForSeconds(wait);

        // 6. Swap CharacterController for Rigidbody → body sinks ──────
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.linearDamping  = sinkDrag;
        rb.angularDamping = sinkAngularDrag;
        // useGravity is kept OFF so we can apply a scaled fraction of gravity manually.
        // ForceMode.Acceleration bypasses mass and mirrors exactly how built-in gravity works.
        rb.useGravity     = false;
        rb.AddTorque(transform.right * 1.8f - transform.up * 0.4f, ForceMode.Impulse);
        _sinkRb = rb;

        // 7. Camera softly tracks the sinking body ────────────────────
        if (_camTransform != null)
            StartCoroutine(TrackSinkingDiver());
    }

    IEnumerator TrackSinkingDiver()
    {
        while (_camTransform != null)
        {
            // Apply scaled gravity manually (useGravity is off so we control the rate).
            // ForceMode.Acceleration is mass-independent — identical to built-in gravity.
            if (_sinkRb != null)
                _sinkRb.AddForce(Physics.gravity * sinkGravityScale, ForceMode.Acceleration);

            Vector3 desired = transform.position
                            - transform.forward * camOffsetBack
                            + Vector3.up        * camOffsetUp;
            _camTransform.position = Vector3.Lerp(_camTransform.position, desired, 1.8f * Time.deltaTime);

            Vector3 lookTarget = transform.position + Vector3.up * 0.9f;
            if ((lookTarget - _camTransform.position).sqrMagnitude > 0.01f)
            {
                Quaternion desired_rot = Quaternion.LookRotation(lookTarget - _camTransform.position);
                _camTransform.rotation = Quaternion.Slerp(_camTransform.rotation, desired_rot, 3f * Time.deltaTime);
            }

            yield return null;
        }
    }
}
