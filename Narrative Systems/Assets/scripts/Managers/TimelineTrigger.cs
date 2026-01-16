using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    [Header("Timeline")]
    [Tooltip("A timeline que será iniciada quando o player passar")]
    public PlayableDirector timeline;

    [Header("Settings")]
    [Tooltip("Se marcado, só ativa uma vez")]
    public bool triggerOnce = true;
    
    [Tooltip("Se marcado, procura automaticamente a timeline2 na cena")]
    public bool autoFindTimeline = true;

    private bool hasTriggered;

    void Start()
    {
        if (autoFindTimeline && timeline == null)
        {
            var directors = FindObjectsOfType<PlayableDirector>();
            foreach (var dir in directors)
            {
                if (dir.playableAsset != null && dir.playableAsset.name.Contains("Timeline2"))
                {
                    timeline = dir;
                    break;
                }
            }
        }

        // Verifica se tem collider trigger
        var col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"TimelineTrigger: {name} não tem Collider. Adicione um Collider e marque 'Is Trigger'.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"TimelineTrigger: {name} tem Collider mas 'Is Trigger' não está marcado.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (timeline == null) return;

        hasTriggered = true;
        timeline.Play();
    }

    void OnTriggerStay(Collider other)
    {
        // Fallback caso OnTriggerEnter não funcione
        if (triggerOnce && hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (timeline == null) return;

        hasTriggered = true;
        timeline.Play();
    }
}
