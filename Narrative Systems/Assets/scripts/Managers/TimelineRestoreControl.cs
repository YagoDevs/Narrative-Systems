using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Resolve o caso clássico: após terminar uma Timeline, o Player fica sem controle.
/// Coloque este script no mesmo GameObject do PlayableDirector (Timeline).
/// </summary>
[DisallowMultipleComponent]
public class TimelineRestoreControl : MonoBehaviour
{
    [Header("Director")]
    public PlayableDirector director;

    [Header("Player (optional)")]
    public GameObject player;
    public string playerTag = "Player";

    [Header("Fixes")]
    [Tooltip("Força o diretor a NÃO 'segurar' (Hold) o último frame após terminar.")]
    public bool forceWrapModeNone = true;
    [Tooltip("Reseta o tempo da Timeline para 0 e reavalia para soltar bindings.")]
    public bool resetDirectorOnStop = true;

    [Header("Restore Input/Cursor")]
    public bool lockCursorOnStop = true;
    public bool resumeTimeScaleOnStop = false; // deixe false se você usa timeline em game time normal

    [Header("Restore common player scripts")]
    [Tooltip("Reabilita scripts comuns no Player ao final (se existirem).")]
    public bool reenablePlayerScriptsOnStop = true;

    void Reset()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        if (player == null && !string.IsNullOrEmpty(playerTag))
        {
            try { player = GameObject.FindGameObjectWithTag(playerTag); }
            catch (UnityException) { /* tag pode não existir */ }
        }

        if (director != null && forceWrapModeNone)
            director.extrapolationMode = DirectorWrapMode.None;
    }

    void OnEnable()
    {
        if (director == null) return;
        director.stopped += HandleStopped;
    }

    void OnDisable()
    {
        if (director == null) return;
        director.stopped -= HandleStopped;
    }

    void HandleStopped(PlayableDirector d)
    {
        // 1) Solta o "Hold" (se existir) e reseta o director
        if (forceWrapModeNone && d != null)
            d.extrapolationMode = DirectorWrapMode.None;

        if (resetDirectorOnStop && d != null)
        {
            // Stop() sozinho às vezes deixa o último frame aplicado dependendo do modo.
            // Reset explícito + Evaluate ajuda a soltar tracks que controlam Transform/Active.
            d.time = 0;
            d.Evaluate();
            d.Stop();
        }

        // 2) Se alguém pausou o jogo durante a timeline, desfaz (opcional)
        if (resumeTimeScaleOnStop && Time.timeScale == 0f)
            Time.timeScale = 1f;

        // 3) Devolve controle do cursor
        if (lockCursorOnStop)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 4) Reabilita scripts comuns do Player (caso a Timeline tenha desativado algo)
        if (reenablePlayerScriptsOnStop && player != null)
        {
            // Movement + combat + interaction (se existirem)
            EnableIfExists(player, "SimpleMovement");
            EnableIfExists(player, "PlayerCombat");
            EnableIfExists(player, "PlayerInteraction");

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
        }
    }

    static void EnableIfExists(GameObject go, string componentTypeName)
    {
        // Evita dependência direta de tipos (caso você renomeie scripts).
        var c = go.GetComponent(componentTypeName) as Behaviour;
        if (c != null) c.enabled = true;
    }
}

