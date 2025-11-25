using UnityEngine;

/// <summary>
/// Mostra o FPS atual na tela usando GUI imediato.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    [Range(0.05f, 1f)]
    [Tooltip("Intervalo em segundos para atualizar o valor exibido.")]
    public float updateInterval = 0.25f;

    [Tooltip("Deslocamento em pixels a partir do canto superior esquerdo.")]
    public Vector2 screenOffset = new Vector2(12f, 12f);

    [Tooltip("Cor usada para o texto do FPS.")]
    public Color textColor = Color.white;

    [Tooltip("Tamanho da fonte exibida.")]
    public int fontSize = 24;

    [Tooltip("Exibe também o tempo por quadro em ms.")]
    public bool showFrameTime = true;

    private float elapsed;
    private int frameCount;
    private float fps;
    private GUIStyle guiStyle;

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        frameCount++;

        if (elapsed >= updateInterval)
        {
            fps = frameCount / elapsed;
            frameCount = 0;
            elapsed = 0f;
        }
    }

    private void OnGUI()
    {
        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.label);
        }

        guiStyle.fontSize = fontSize;
        guiStyle.normal.textColor = textColor;

        string label = $"{fps:0.} FPS";

        if (showFrameTime && fps > 0f)
        {
            label += $" ({1000f / fps:0.0} ms)";
        }

        GUI.Label(new Rect(screenOffset.x, screenOffset.y, 250f, 40f), label, guiStyle);
    }
}

