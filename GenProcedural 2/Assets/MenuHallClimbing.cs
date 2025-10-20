using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuHallClimbing : MonoBehaviour
{
    public Gestionador gestionador;

    [SerializeField] private Scrollbar width;
    [SerializeField] private TMP_InputField widthValue;
    [SerializeField] private Scrollbar height;
    [SerializeField] private TMP_InputField heightValue;
    [SerializeField] private TMP_InputField startNodeX;
    [SerializeField] private TMP_InputField startNodeY;
    [SerializeField] private TMP_InputField goalNodeX;
    [SerializeField] private TMP_InputField goalNodeY;

    // Coincidir con los [Range] usados en Gestionador/RandomInitState
    private const int minWidth = 10;
    private const int maxWidth = 25;
    private const int minHeight = 10;
    private const int maxHeight = 25;

    // Espera un frame para asegurar que Gestionador haya inicializado sus valores en Start()
    IEnumerator Start()
    {
        if (gestionador == null)
        {
            Debug.LogWarning("MenuHallClimbing: asigna 'gestionador' en el inspector.");
            yield break;
        }

        yield return null; // espera a que otros Start() corran

        SyncUIFromGestionador();

        // Listeners: scrollbars
        if (width != null) width.onValueChanged.AddListener(v => OnWidthScrollbarChanged(v));
        if (height != null) height.onValueChanged.AddListener(v => OnHeightScrollbarChanged(v));

        // Listeners: input fields (al terminar edición)
        if (widthValue != null) widthValue.onEndEdit.AddListener(s => OnWidthInputEdited(s));
        if (heightValue != null) heightValue.onEndEdit.AddListener(s => OnHeightInputEdited(s));

        if (startNodeX != null) startNodeX.onEndEdit.AddListener(s => OnStartNodeEdited(s, true));
        if (startNodeY != null) startNodeY.onEndEdit.AddListener(s => OnStartNodeEdited(s, false));
        if (goalNodeX != null) goalNodeX.onEndEdit.AddListener(s => OnGoalNodeEdited(s, true));
        if (goalNodeY != null) goalNodeY.onEndEdit.AddListener(s => OnGoalNodeEdited(s, false));
    }

    void Update() { }

    // Sincroniza UI <- Gestionador
    private void SyncUIFromGestionador()
    {
        if (gestionador == null) return;

        if (widthValue != null) widthValue.text = gestionador.width.ToString();
        if (width != null) width.value = IntToScrollbar(gestionador.width, minWidth, maxWidth);

        if (heightValue != null) heightValue.text = gestionador.height.ToString();
        if (height != null) height.value = IntToScrollbar(gestionador.height, minHeight, maxHeight);

        if (startNodeX != null) startNodeX.text = gestionador.start.x.ToString();
        if (startNodeY != null) startNodeY.text = gestionador.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = gestionador.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = gestionador.goal.y.ToString();
    }

    // Callbacks scrollbars
    private void OnWidthScrollbarChanged(float value)
    {
        int newWidth = ScrollbarToInt(value, minWidth, maxWidth);
        gestionador.width = newWidth;
        if (widthValue != null) widthValue.text = newWidth.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightScrollbarChanged(float value)
    {
        int newHeight = ScrollbarToInt(value, minHeight, maxHeight);
        gestionador.height = newHeight;
        if (heightValue != null) heightValue.text = newHeight.ToString();
        ClampStartGoalToBounds();
    }

    // Callbacks input fields (width/height)
    private void OnWidthInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromGestionador(); return; }
        v = Mathf.Clamp(v, minWidth, maxWidth);
        gestionador.width = v;
        if (width != null) width.value = IntToScrollbar(v, minWidth, maxWidth);
        if (widthValue != null) widthValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromGestionador(); return; }
        v = Mathf.Clamp(v, minHeight, maxHeight);
        gestionador.height = v;
        if (height != null) height.value = IntToScrollbar(v, minHeight, maxHeight);
        if (heightValue != null) heightValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    // Callbacks Start/Goal edits
    private void OnStartNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? gestionador.width : gestionador.height, isX);
        if (isX) gestionador.start = new Vector2Int(v, gestionador.start.y);
        else gestionador.start = new Vector2Int(gestionador.start.x, v);

        RefreshStartGoalFields();
    }

    private void OnGoalNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? gestionador.width : gestionador.height, isX);
        if (isX) gestionador.goal = new Vector2Int(v, gestionador.goal.y);
        else gestionador.goal = new Vector2Int(gestionador.goal.x, v);

        RefreshStartGoalFields();
    }

    private void RefreshStartGoalFields()
    {
        if (startNodeX != null) startNodeX.text = gestionador.start.x.ToString();
        if (startNodeY != null) startNodeY.text = gestionador.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = gestionador.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = gestionador.goal.y.ToString();
    }

    // Utilidades
    private int ClampCoordinate(int value, int dim, bool isX)
    {
        int min = 1;
        int max = dim - 2;
        return Mathf.Clamp(value, min, Mathf.Max(min, max));
    }

    private void ClampStartGoalToBounds()
    {
        if (gestionador == null) return;

        int sx = Mathf.Clamp(gestionador.start.x, 1, Mathf.Max(1, gestionador.width - 2));
        int sy = Mathf.Clamp(gestionador.start.y, 1, Mathf.Max(1, gestionador.height - 2));
        int gx = Mathf.Clamp(gestionador.goal.x, 1, Mathf.Max(1, gestionador.width - 2));
        int gy = Mathf.Clamp(gestionador.goal.y, 1, Mathf.Max(1, gestionador.height - 2));
        gestionador.start = new Vector2Int(sx, sy);
        gestionador.goal = new Vector2Int(gx, gy);
        RefreshStartGoalFields();
    }

    private float IntToScrollbar(int value, int min, int max)
    {
        if (max <= min) return 0f;
        return (value - min) / (float)(max - min);
    }

    private int ScrollbarToInt(float scrollbarValue, int min, int max)
    {
        int v = Mathf.RoundToInt(scrollbarValue * (max - min) + min);
        return Mathf.Clamp(v, min, max);
    }
}
