using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Atualiza uma barra (Slider ou Image fill) com base no Health alvo.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Tooltip("Componente de vida observado.")]
    public Health target;

    [Header("UI")]
    [Tooltip("Slider para exibir a vida (opcional).")]
    public Slider slider;

    [Tooltip("Imagem com fill para exibir a vida (opcional).")]
    public Image fillImage;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        Refresh(target != null ? target.CurrentHealth : 0, target != null ? target.maxHealth : 100);
    }

    private void Subscribe()
    {
        if (target != null)
        {
            target.OnHealthChanged += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (target != null)
        {
            target.OnHealthChanged -= Refresh;
        }
    }

    private void Refresh(int current, int max)
    {
        float t = max > 0 ? (float)current / max : 0f;

        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = t;
        }
    }
}

