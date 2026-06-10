using UnityEngine;

/// <summary>
/// Central audio hub. Place one on a GameObject in the scene and assign the clips.
/// - Ambient water-bubble loop plays automatically on Start (background music).
/// - PlayChomp() fires the cartoon chomp when the shark bites.
/// - PlayWin() fires the win jingle when the game is won.
/// Accessed globally via AudioManager.instance.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Clips")]
    [Tooltip("Looping water-bubble ambience used as background music.")]
    public AudioClip ambientLoop;
    [Tooltip("Cartoon chomp played when the shark bites.")]
    public AudioClip chompClip;
    [Tooltip("Jingle played when the player wins.")]
    public AudioClip winClip;
    [Tooltip("Looping scrape sound played while actively scraping a barnacle.")]
    public AudioClip scrapeLoop;
    [Tooltip("Played when the crab attacks the shark.")]
    public AudioClip crabAttackClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float ambientVolume    = 0.45f;
    [Range(0f, 1f)] public float chompVolume      = 1f;
    [Range(0f, 1f)] public float winVolume        = 1f;
    [Range(0f, 1f)] public float scrapeVolume     = 0.7f;
    [Range(0f, 1f)] public float crabAttackVolume = 1f;

    private AudioSource _ambientSource;   // looping background
    private AudioSource _sfxSource;       // one-shot effects
    private AudioSource _scrapeSource;    // looping scrape (gated each frame)

    // Set true by Scraper each frame it scrapes; consumed in LateUpdate.
    private bool _scrapingThisFrame;

    void Awake()
    {
        // Simple singleton — last one wins, but warn on duplicates.
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[AudioManager] Duplicate AudioManager found; using the newest.");
        }
        instance = this;

        // Dedicated looping source for ambience (2D, ignores listener position).
        _ambientSource = gameObject.AddComponent<AudioSource>();
        _ambientSource.clip          = ambientLoop;
        _ambientSource.loop          = true;
        _ambientSource.playOnAwake   = false;
        _ambientSource.volume        = ambientVolume;
        _ambientSource.spatialBlend  = 0f;   // 2D — consistent background level

        // Separate source for one-shots so they never cut off the ambience.
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake  = false;
        _sfxSource.spatialBlend = 0f;        // 2D — always audible

        // Looping scrape source, gated on/off each frame by Scraper.
        _scrapeSource = gameObject.AddComponent<AudioSource>();
        _scrapeSource.clip         = scrapeLoop;
        _scrapeSource.loop         = true;
        _scrapeSource.playOnAwake  = false;
        _scrapeSource.volume       = scrapeVolume;
        _scrapeSource.spatialBlend = 0f;     // 2D
    }

    void Start()
    {
        if (ambientLoop != null)
            _ambientSource.Play();
    }

    // Runs after all Update()s. If no barnacle was scraped this frame, stop the loop.
    void LateUpdate()
    {
        if (_scrapingThisFrame)
        {
            if (scrapeLoop != null && !_scrapeSource.isPlaying)
                _scrapeSource.Play();
        }
        else if (_scrapeSource.isPlaying)
        {
            _scrapeSource.Stop();
        }
        _scrapingThisFrame = false;   // reset for next frame
    }

    // ── public API ───────────────────────────────────────────────────────
    public void PlayChomp()
    {
        if (chompClip != null)
            _sfxSource.PlayOneShot(chompClip, chompVolume);
    }

    public void PlayWin()
    {
        if (winClip != null)
            _sfxSource.PlayOneShot(winClip, winVolume);
    }

    public void PlayCrabAttack()
    {
        if (crabAttackClip != null)
            _sfxSource.PlayOneShot(crabAttackClip, crabAttackVolume);
    }

    /// <summary>
    /// Call every frame a barnacle is actively being scraped. The looping scrape
    /// sound starts on the first such frame and stops automatically when calls cease.
    /// </summary>
    public void NotifyScraping()
    {
        _scrapingThisFrame = true;
    }
}
