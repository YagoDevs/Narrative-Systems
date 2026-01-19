using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class TimelineUIController : MonoBehaviour
{
    [Header("Timeline Directors")]
    [Tooltip("A timeline que controla a imagem (deve estar ativa para mostrar a imagem)")]
    public PlayableDirector timeline1;
    
    [Tooltip("Timelines que desativam toda a UI quando ativas")]
    public PlayableDirector timeline2;

    [Header("UI Elements")]
    [Tooltip("Imagem do canvas que será ativada/desativada baseado na timeline1")]
    public Image targetImage;
    
    [Tooltip("Painel raiz da UI que será desativada quando timeline1 ou timeline2 estiverem ativas")]
    public GameObject uiRootPanel;

    [Header("Settings")]
    [Tooltip("Se marcado, procura automaticamente os PlayableDirectors na cena")]
    public bool autoFindDirectors = true;
    
    [Tooltip("Se marcado, procura automaticamente o Canvas principal")]
    public bool autoFindUI = true;

    [Header("UI Behavior")]
    [Tooltip("Se true, este script desativa/reativa o uiRootPanel durante timelines. Desmarque para NÃO mexer no seu Canvas.")]
    public bool disableUiRootDuringTimelines = false;

    private bool wasTimeline1Playing;
    private bool wasAnyTimelinePlaying;

    void Start()
    {
        if (autoFindDirectors)
        {
            if (timeline1 == null)
            {
                var directors = FindObjectsOfType<PlayableDirector>();
                foreach (var dir in directors)
                {
                    if (dir.playableAsset != null && dir.playableAsset.name.Contains("Timeline"))
                    {
                        timeline1 = dir;
                        break;
                    }
                }
            }

            if (timeline2 == null)
            {
                var directors = FindObjectsOfType<PlayableDirector>();
                foreach (var dir in directors)
                {
                    if (dir.playableAsset != null && dir.playableAsset.name.Contains("Timeline2"))
                    {
                        timeline2 = dir;
                        break;
                    }
                }
            }
        }

        if (autoFindUI && uiRootPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                uiRootPanel = canvas.gameObject;
        }

        // Inicializa o estado
        UpdateImageVisibility();
        UpdateUIVisibility();
    }

    void Update()
    {
        bool timeline1Playing = IsTimelinePlaying(timeline1);
        bool timeline2Playing = IsTimelinePlaying(timeline2);
        bool anyTimelinePlaying = timeline1Playing || timeline2Playing;

        // Atualiza a imagem apenas se o estado mudou
        if (timeline1Playing != wasTimeline1Playing)
        {
            wasTimeline1Playing = timeline1Playing;
            UpdateImageVisibility();
        }

        // Atualiza a UI apenas se o estado mudou
        if (anyTimelinePlaying != wasAnyTimelinePlaying)
        {
            wasAnyTimelinePlaying = anyTimelinePlaying;
            UpdateUIVisibility();
        }
    }

    bool IsTimelinePlaying(PlayableDirector director)
    {
        if (director == null) return false;
        
        // Verifica se a timeline está rodando
        PlayState state = director.state;
        return state == PlayState.Playing;
    }

    void UpdateImageVisibility()
    {
        if (targetImage == null) return;

        // Ativa a imagem apenas quando timeline1 está rodando
        bool shouldShow = wasTimeline1Playing;
        targetImage.gameObject.SetActive(shouldShow);
    }

    void UpdateUIVisibility()
    {
        if (uiRootPanel == null) return;
        if (!disableUiRootDuringTimelines) return;

        // Desativa toda a UI quando timeline1 OU timeline2 estão rodando
        bool shouldHideUI = wasAnyTimelinePlaying;
        uiRootPanel.SetActive(!shouldHideUI);
    }

    void OnValidate()
    {
        // Validação no editor
        if (targetImage != null && !targetImage.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"TimelineUIController: Target Image '{targetImage.name}' não está ativo na hierarquia.");
        }

        if (uiRootPanel != null && !uiRootPanel.activeInHierarchy)
        {
            Debug.LogWarning($"TimelineUIController: UI Root Panel '{uiRootPanel.name}' não está ativo na hierarquia.");
        }
    }
}
