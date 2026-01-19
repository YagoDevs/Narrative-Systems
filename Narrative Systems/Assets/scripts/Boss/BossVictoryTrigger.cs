using UnityEngine;

/// <summary>
/// Dispara o "fim de jogo" quando o Boss morrer.
/// Pluge este componente no Boss (mesmo GO do EnemyHealth, ou aponte no Inspector).
/// </summary>
[DisallowMultipleComponent]
public class BossVictoryTrigger : MonoBehaviour
{
    [Header("Boss")]
    public EnemyHealth bossHealth;

    [Header("Audio (optional)")]
    public AudioClip successSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;
    public AudioSource audioSource;

    [Header("UI")]
    public bool showGameCompleteUI = true;
    public string title = "VOCÊ VENCEU!";
    public string body = "Você derrotou o boss.";
    public string buttonText = "Jogar novamente";

    bool fired;

    void Reset()
    {
        bossHealth = GetComponent<EnemyHealth>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    void Awake()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<EnemyHealth>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
    }

    void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDied += HandleBossDied;
    }

    void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDied -= HandleBossDied;
    }

    void HandleBossDied()
    {
        if (fired) return;
        fired = true;

        PlaySfx(successSfx, transform.position);

        if (showGameCompleteUI)
            GameCompleteUI.Show(title, body, buttonText);
    }

    void PlaySfx(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;
        if (sfxVolume <= 0f) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
    }
}

