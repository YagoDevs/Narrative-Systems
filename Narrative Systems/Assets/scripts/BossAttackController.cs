using System.Collections;
using UnityEngine;

/// <summary>
/// Controla dois ataques do boss alternando entre hit simples e duplo.
/// Usa Animation Events para aplicar o dano.
/// </summary>
[RequireComponent(typeof(Animator))]
public class BossAttackController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Transform do alvo (player).")]
    public Transform target;

    [Tooltip("Vida do player para receber o dano.")]
    public Health playerHealth;

    [Header("Ataque")]
    [Tooltip("Alcance máximo do ataque.")]
    public float attackRange = 2.5f;

    [Tooltip("Tempo mínimo entre ataques.")]
    public float attackCooldown = 2f;

    [Tooltip("Dano do ataque simples (1 hit).")]
    public int simpleDamage = 10;

    [Tooltip("Dano aplicado em cada hit do ataque duplo.")]
    public int doubleDamagePerHit = 10;

    [Tooltip("Nome do trigger de animação para o ataque simples.")]
    public string simpleAttackTrigger = "Attack1";

    [Tooltip("Nome do trigger de animação para o ataque duplo.")]
    public string doubleAttackTrigger = "Attack2";

    private Animator animator;
    private float cooldownTimer;
    private bool useDoubleNext;

    // Guardamos quantos hits ainda restam para o ataque atual e o dano por hit.
    private int pendingHits;
    private int pendingDamagePerHit;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerHealth == null || playerHealth.IsDead || target == null)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && IsTargetInRange())
        {
            PerformAttack();
            cooldownTimer = attackCooldown;
        }
    }

    private bool IsTargetInRange()
    {
        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    private void PerformAttack()
    {
        bool isDouble = useDoubleNext;
        useDoubleNext = !useDoubleNext; // alterna a cada tentativa

        if (isDouble)
        {
            pendingHits = 2;
            pendingDamagePerHit = doubleDamagePerHit;
            animator.SetTrigger(doubleAttackTrigger);
        }
        else
        {
            pendingHits = 1;
            pendingDamagePerHit = simpleDamage;
            animator.SetTrigger(simpleAttackTrigger);
        }
    }

    /// <summary>
    /// Chame este método via Animation Event em cada frame de impacto.
    /// </summary>
    public void AnimationEvent_ApplyDamage()
    {
        if (playerHealth == null || pendingHits <= 0)
        {
            return;
        }

        playerHealth.TakeDamage(pendingDamagePerHit);
        pendingHits--;
    }
}

