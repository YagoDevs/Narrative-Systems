using UnityEngine;
using UnityEngine.Events;

public class UnityEventTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider> { }

    public TriggerEvent OnTriggerEnterEvent;
    public TriggerEvent OnTriggerExitEvent;

    [Tooltip("If true, the trigger will activate on collision.")]
    public bool activateOnCollision = true;

    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!activateOnCollision) return;

        if (other.GetComponent<CharacterController>() != null && !isPlayerInside)
        {
            OnTriggerEnterEvent.Invoke(other);
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activateOnCollision) return;

        if (other.GetComponent<CharacterController>() != null && isPlayerInside)
        {
            OnTriggerExitEvent.Invoke(other);
            isPlayerInside = false;
        }
    }

}
