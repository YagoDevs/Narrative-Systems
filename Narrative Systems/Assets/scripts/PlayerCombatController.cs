using UnityEngine;

/// <summary>
/// Controla dois ataques do player (1 hit e 2 hits) com aplicação de dano via Animation Events.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Alvo")]
    [Tooltip("Vida do boss.")]
    public Health bossHealth;

    [Tooltip("Origem para checar distância; padrão = este transform.")]
    public Transform attackOrigin;

    [Tooltip("Distância máxima para considerar acerto.")]
    public float attackRange = 2.5f;

    [Header("Input")]
    public KeyCode attack1Key = KeyCode.Mouse0;
    public KeyCode attack2Key = KeyCode.Mouse1;

    [Header("Dano")]
    [Tooltip("Dano do ataque simples (1 hit).")]
    public int attack1Damage = 10;

    [Tooltip("Dano aplicado em cada hit do ataque duplo.")]
    public int attack2DamagePerHit = 10;

    [Tooltip("Tempo mínimo entre ataques.")]
    public float attackCooldown = 0.5f;

    [Header("Animação")]
    public string attack1Trigger = "Attack1";
    public string attack2Trigger = "Attack2";

    private Animator animator;
    private float cooldownTimer;
    private int pendingHits;
    private int pendingDamagePerHit;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f || bossHealth == null || bossHealth.IsDead)
        {
            return;
        }

        if (Input.GetKeyDown(attack1Key))
        {
            DoAttack(isDouble: false);
        }
        else if (Input.GetKeyDown(attack2Key))
        {
            DoAttack(isDouble: true);
        }
    }

    private void DoAttack(bool isDouble)
    {
        if (isDouble)
        {
            pendingHits = 2;
            pendingDamagePerHit = attack2DamagePerHit;
            animator.SetTrigger(attack2Trigger);
        }
        else
        {
            pendingHits = 1;
            pendingDamagePerHit = attack1Damage;
            animator.SetTrigger(attack1Trigger);
        }

        cooldownTimer = attackCooldown;
    }

    private bool IsBossInRange()
    {
        if (bossHealth == null)
        {
            return false;
        }

        Transform origin = attackOrigin != null ? attackOrigin : transform;
        return Vector3.Distance(origin.position, bossHealth.transform.position) <= attackRange;
    }

    /// <summary>
    /// Chame este método via Animation Event em cada frame de impacto.
    /// </summary>
    public void AnimationEvent_ApplyDamage()
    {
        if (pendingHits <= 0 || bossHealth == null || bossHealth.IsDead)
        {
            return;
        }

        if (IsBossInRange())
        {
            bossHealth.TakeDamage(pendingDamagePerHit);
        }

        pendingHits--;
    }
}

