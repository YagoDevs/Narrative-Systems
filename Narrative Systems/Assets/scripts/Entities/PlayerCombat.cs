using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 2.0f;
    public float attackCooldown = 0.5f;
    public int baseDamage = 10;
    public float altAttackRange = 2.6f;
    public float altAttackCooldown = 0.75f;
    public int altBaseDamage = 14;
    public float altAttackWindup = 0.2f; // delay before damage/projectile (sync with animation)
    public LayerMask enemyLayers = ~0; // everything by default (simple)

    [Header("Primary Attack: Fireball (Left Click)")]
    public bool attackIsFireball = true; // left click is fireball
    public FireballProjectile fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 18f;
    public float fireballSpawnForwardOffset = 1.1f;
    public float fireballAttackWindup = 0.2f; // delay before fireball launch (sync with animation)

    [Header("Aiming")]
    [Tooltip("Se true, a direção do ataque (melee/fireball) vem da câmera (Cinemachine output), não do forward do personagem.")]
    public bool aimFromCamera = true;
    [Tooltip("Gira o personagem para a direção da mira quando atacar (ajuda MUITO com FreeLook).")]
    public bool rotatePlayerTowardAimOnAttack = true;
    [Min(0f)] public float rotateTowardAimSpeed = 18f;

    [Header("Alt Attack: Fireball (optional)")]
    public bool altAttackIsFireball = false;

    [Header("Animation (optional)")]
    public Animator animator; // assign Rogue_Hooded Animator (or leave empty to auto-find)
    public string attack1Trigger = "Attack1"; // left click
    public string attack2Trigger = "Attack2"; // right click

    [Header("Audio (optional)")]
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip attack1Sfx;
    public AudioClip attack2Sfx;
    public AudioClip fireballLaunchSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("Feedback (optional)")]
    public bool logAttacks = true;
    public bool logMisses = true;
    public bool showCombatTextOnHit = true;
    public bool showCombatTextOnMiss = false;
    public string missText = "MISS";

    float nextAttackTime;
    float nextAltAttackTime;
    PlayerProgression progression;

    void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        // Don't attack when UI is open / cursor unlocked
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryAttackPrimary();

        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryAttackAlt();
    }

    void TryAttackPrimary()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        int dmg = baseDamage;
        if (progression != null)
            dmg = Mathf.RoundToInt(baseDamage * progression.damageMultiplier);

        if (logAttacks)
            Debug.Log($"Player attacked for {dmg}!");

        if (animator != null && !string.IsNullOrEmpty(attack1Trigger))
            animator.SetTrigger(attack1Trigger);

        // If fireball attack, launch fireball after windup
        if (attackIsFireball && fireballPrefab != null)
        {
            StartCoroutine(FireballAttackAfterDelay(dmg));
            return;
        }

        // Otherwise, melee attack
        PlaySfx(attack1Sfx, transform.position);

        // Simple melee: hit any EnemyHealth near the player
        // Include triggers too (robust across different collider setups).
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var eh = hits[i].GetComponentInParent<EnemyHealth>();
            if (eh != null && !eh.IsDead)
            {
                eh.TakeDamage(dmg);
                if (logAttacks)
                {
                    string n = (eh.config != null && !string.IsNullOrEmpty(eh.config.displayName)) ? eh.config.displayName : eh.name;
                    Debug.Log($"Hit confirmed on {n}. Enemy HP: {eh.CurrentHP}/{eh.MaxHP}");
                }
                if (showCombatTextOnHit && CombatTextManager.Instance != null)
                    CombatTextManager.Instance.SpawnWorldText(hits[i].transform.position + Vector3.up * 2f, "HIT", new Color(1f, 0.25f, 0.25f));
                // 1 target per swing (simple + readable)
                return;
            }
        }

        if (logMisses)
            Debug.Log("Player attack MISS (nenhum EnemyHealth dentro do range).");
        if (showCombatTextOnMiss && CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, missText, new Color(0.6f, 0.7f, 1f));
    }

    System.Collections.IEnumerator FireballAttackAfterDelay(int dmg)
    {
        if (fireballAttackWindup > 0f)
            yield return new WaitForSeconds(fireballAttackWindup);

        // If UI opened during windup, cancel
        if (Cursor.lockState != CursorLockMode.Locked)
            yield break;

        SpawnFireball(dmg);
    }

    void TryAttackAlt()
    {
        if (Time.time < nextAltAttackTime) return;
        nextAltAttackTime = Time.time + altAttackCooldown;

        int dmg = altBaseDamage;
        if (progression != null)
            dmg = Mathf.RoundToInt(altBaseDamage * progression.damageMultiplier);

        if (logAttacks)
            Debug.Log($"Player ALT attacked for {dmg}!");

        if (animator != null && !string.IsNullOrEmpty(attack2Trigger))
            animator.SetTrigger(attack2Trigger);

        // Delay to sync with animation timing
        StartCoroutine(AltAttackAfterDelay(dmg));
    }

    System.Collections.IEnumerator AltAttackAfterDelay(int dmg)
    {
        if (altAttackWindup > 0f)
            yield return new WaitForSeconds(altAttackWindup);

        // If UI opened during windup, cancel
        if (Cursor.lockState != CursorLockMode.Locked)
            yield break;

        if (altAttackIsFireball && fireballPrefab != null)
        {
            SpawnFireball(dmg);
            yield break;
        }

        PlaySfx(attack2Sfx, transform.position);

        Collider[] hits = Physics.OverlapSphere(transform.position, altAttackRange, enemyLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var eh = hits[i].GetComponentInParent<EnemyHealth>();
            if (eh != null && !eh.IsDead)
            {
                eh.TakeDamage(dmg);
                if (logAttacks)
                {
                    string n = (eh.config != null && !string.IsNullOrEmpty(eh.config.displayName)) ? eh.config.displayName : eh.name;
                    Debug.Log($"ALT hit confirmed on {n}. Enemy HP: {eh.CurrentHP}/{eh.MaxHP}");
                }
                if (showCombatTextOnHit && CombatTextManager.Instance != null)
                    CombatTextManager.Instance.SpawnWorldText(hits[i].transform.position + Vector3.up * 2f, "HIT", new Color(1f, 0.25f, 0.25f));
                yield break;
            }
        }

        if (logMisses)
            Debug.Log("Player ALT attack MISS (nenhum EnemyHealth dentro do range).");
        if (showCombatTextOnMiss && CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, missText, new Color(0.6f, 0.7f, 1f));
    }

    void SpawnFireball(int dmg)
    {
        Vector3 spawnPos;
        Vector3 dir = GetAimDirectionPlanar();

        if (fireballSpawnPoint != null)
        {
            spawnPos = fireballSpawnPoint.position;
            // Se não estiver mirando pela câmera, respeita o forward do spawn point.
            if (!aimFromCamera)
                dir = fireballSpawnPoint.forward;
        }
        else
        {
            spawnPos = transform.position + Vector3.up * 1.2f + transform.forward * fireballSpawnForwardOffset;
        }

        if (rotatePlayerTowardAimOnAttack)
            RotatePlayerToward(dir);

        var fb = Instantiate(fireballPrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));

        // Only hit enemies: use enemyLayers (same mask used by melee scan)
        fb.Launch(dir, dmg, fireballSpeed, enemyLayers);

        PlaySfx(fireballLaunchSfx != null ? fireballLaunchSfx : attack2Sfx, spawnPos);
    }

    Vector3 GetAimDirectionPlanar()
    {
        // Default: personagem
        Vector3 dir = transform.forward;

        if (aimFromCamera)
        {
            var cam = Camera.main;
            if (cam != null)
                dir = cam.transform.forward;
        }

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;
        return dir.normalized;
    }

    void RotatePlayerToward(Vector3 planarDir)
    {
        planarDir.y = 0f;
        if (planarDir.sqrMagnitude < 0.0001f) return;
        var targetRot = Quaternion.LookRotation(planarDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateTowardAimSpeed * Time.deltaTime);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(0.3f, 0.6f, 1f);
        Gizmos.DrawWireSphere(transform.position, altAttackRange);
    }
}

