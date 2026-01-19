using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD simples para vida do Boss (barra no topo da tela).
/// Anexe em qualquer GameObject da cena (de preferência no Canvas/HUD).
/// </summary>
[DisallowMultipleComponent]
public class BossHealthBarHUD : MonoBehaviour
{
    [Header("Boss (optional)")]
    public BossController boss;
    public EnemyHealth bossHealth;
    public bool autoFindBoss = true;
    [Min(0.1f)] public float reacquireInterval = 1.0f;

    [Header("UI (auto-criada se vazio)")]
    public Canvas targetCanvas;
    public RectTransform root;
    public Slider hpSlider;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;

    [Header("Layout")]
    public Vector2 anchoredPos = new Vector2(0f, -40f);
    public Vector2 size = new Vector2(720f, 70f);

    [Header("Behavior")]
    public bool hideWhenNoBoss = true;
    public bool hideWhenDead = true;
    [Tooltip("Se true, só mostra quando o BossController estiver ativo (isActive == true).")]
    public bool showOnlyWhenBossActive = true;

    float nextReacquireAt;

    void Awake()
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        EnsureUI();
        TryResolveBoss(force: true);
    }

    void OnEnable()
    {
        Hook();
        Paint();
    }

    void OnDisable()
    {
        Unhook();
    }

    void Update()
    {
        // Se já temos boss e ele está pronto, não precisa ficar reacquirindo.
        if (bossHealth != null && IsBossReadyToShow())
            return;

        // Se temos boss mas ele ainda não "spawnou/ativou", mantém escondido e continua checando.
        if (bossHealth != null && !IsBossReadyToShow())
        {
            if (hideWhenNoBoss) SetVisible(false);
            // Still update the UI values in case you want it pre-filled, but keep hidden.
            Paint();
        }

        if (!autoFindBoss) return;

        if (Time.unscaledTime < nextReacquireAt) return;
        nextReacquireAt = Time.unscaledTime + Mathf.Max(0.1f, reacquireInterval);
        TryResolveBoss(force: false);
    }

    void TryResolveBoss(bool force)
    {
        if (!autoFindBoss && !force) return;

        // If already set, keep it.
        if (bossHealth != null) return;

        if (boss == null)
            boss = FindObjectOfType<BossController>();

        if (boss != null)
        {
            bossHealth = boss.enemyHealth != null ? boss.enemyHealth : boss.GetComponent<EnemyHealth>();
        }
        else
        {
            // Fallback: any EnemyHealth that also has BossController
            var anyBoss = FindObjectOfType<BossController>();
            if (anyBoss != null)
            {
                boss = anyBoss;
                bossHealth = boss.enemyHealth != null ? boss.enemyHealth : boss.GetComponent<EnemyHealth>();
            }
        }

        Unhook();
        Hook();
        Paint();
    }

    void Hook()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged += HandleHealthChanged;
    }

    void Unhook()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }

        if (hpText != null)
            hpText.text = $"{current}/{max}";

        if (!IsBossReadyToShow())
        {
            if (hideWhenNoBoss) SetVisible(false);
            return;
        }

        if (hideWhenDead && bossHealth != null && bossHealth.IsDead)
            SetVisible(false);
        else
            SetVisible(true);
    }

    void Paint()
    {
        if (bossHealth == null)
        {
            if (hideWhenNoBoss) SetVisible(false);
            return;
        }

        // Name
        if (nameText != null)
        {
            string n = bossHealth.config != null && !string.IsNullOrEmpty(bossHealth.config.displayName)
                ? bossHealth.config.displayName
                : (boss != null ? boss.name : bossHealth.name);
            nameText.text = n;
        }

        // Atualiza valores, mas a visibilidade depende de IsBossReadyToShow()
        HandleHealthChanged(bossHealth.CurrentHP, bossHealth.MaxHP);
    }

    void SetVisible(bool visible)
    {
        if (root != null)
            root.gameObject.SetActive(visible);
    }

    bool IsBossReadyToShow()
    {
        if (bossHealth == null) return false;
        if (hideWhenDead && bossHealth.IsDead) return false;

        // Se não tiver BossController, pelo menos exige que o GO esteja ativo na hierarquia.
        if (boss == null) return bossHealth.gameObject.activeInHierarchy;

        if (!boss.gameObject.activeInHierarchy) return false;
        if (!showOnlyWhenBossActive) return true;
        return boss.isActive;
    }

    void EnsureUI()
    {
        if (root != null && hpSlider != null) return;

        if (targetCanvas == null)
        {
            Debug.LogWarning("BossHealthBarHUD: não achei Canvas na cena. Crie/adicione um Canvas e/ou atribua 'targetCanvas'.");
            return;
        }

        // Root
        var rootGO = new GameObject("BossHealthBarHUD_Root");
        rootGO.transform.SetParent(targetCanvas.transform, false);
        root = rootGO.AddComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = anchoredPos;
        root.sizeDelta = size;

        // Background
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(root, false);
        var bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Name text
        var nameGO = new GameObject("BossName");
        nameGO.transform.SetParent(root, false);
        nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 28;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.55f);
        nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.offsetMin = new Vector2(12f, 0f);
        nameRT.offsetMax = new Vector2(-12f, -6f);

        // Slider container
        var sliderGO = new GameObject("HpSlider");
        sliderGO.transform.SetParent(root, false);
        hpSlider = sliderGO.AddComponent<Slider>();
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.06f, 0.15f);
        sliderRT.anchorMax = new Vector2(0.94f, 0.52f);
        sliderRT.offsetMin = Vector2.zero;
        sliderRT.offsetMax = Vector2.zero;

        // Slider visuals
        var sliderBgGO = new GameObject("Background");
        sliderBgGO.transform.SetParent(sliderGO.transform, false);
        var sliderBg = sliderBgGO.AddComponent<Image>();
        sliderBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        var sliderBgRT = sliderBgGO.GetComponent<RectTransform>();
        sliderBgRT.anchorMin = Vector2.zero;
        sliderBgRT.anchorMax = Vector2.one;
        sliderBgRT.offsetMin = Vector2.zero;
        sliderBgRT.offsetMax = Vector2.zero;

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0f);
        fillAreaRT.anchorMax = new Vector2(1f, 1f);
        fillAreaRT.offsetMin = new Vector2(6f, 6f);
        fillAreaRT.offsetMax = new Vector2(-6f, -6f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        hpSlider.targetGraphic = sliderBg;
        hpSlider.fillRect = fillRT;
        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;
        hpSlider.value = 1f;
        hpSlider.interactable = false;

        // HP text
        var hpTextGO = new GameObject("HpText");
        hpTextGO.transform.SetParent(root, false);
        hpText = hpTextGO.AddComponent<TextMeshProUGUI>();
        hpText.fontSize = 22;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        var hpRT = hpTextGO.GetComponent<RectTransform>();
        hpRT.anchorMin = new Vector2(0f, 0f);
        hpRT.anchorMax = new Vector2(1f, 0.22f);
        hpRT.offsetMin = new Vector2(12f, 6f);
        hpRT.offsetMax = new Vector2(-12f, 0f);
    }
}

