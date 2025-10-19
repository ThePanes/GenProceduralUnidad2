using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MenuRandomInit : MonoBehaviour
{
    public RandomInitState randomInit;

    [SerializeField] private Scrollbar width;
    [SerializeField] private TMP_InputField widthValue;
    [SerializeField] private Scrollbar height;
    [SerializeField] private TMP_InputField heightValue;
    [SerializeField] private TMP_InputField startNodeX;
    [SerializeField] private TMP_InputField startNodeY;
    [SerializeField] private TMP_InputField goalNodeX;
    [SerializeField] private TMP_InputField goalNodeY;
    [SerializeField] private Scrollbar Walls;
    [SerializeField] private TMP_InputField wallsValue;

    // Coincidir con los [Range] de RandomInitState
    private const int minWidth = 10;
    private const int maxWidth = 25;
    private const int minHeight = 10;
    private const int maxHeight = 25;

    // Espera un frame para asegurar que RandomInitState haya inicializado sus valores en Start()
    IEnumerator Start()
    {
        if (randomInit == null)
        {
            Debug.LogWarning("MenuRandomInit: asigna 'randomInit' en el inspector.");
            yield break;
        }

        yield return null; // espera a que otros Start() corran

        SyncUIFromRandomInit();

        // listeners
        if (width != null) width.onValueChanged.AddListener(v => OnWidthScrollbarChanged(v));
        if (height != null) height.onValueChanged.AddListener(v => OnHeightScrollbarChanged(v));
        if (Walls != null) Walls.onValueChanged.AddListener(v => OnWallsScrollbarChanged(v));

        if (widthValue != null) widthValue.onEndEdit.AddListener(s => OnWidthInputEdited(s));
        if (heightValue != null) heightValue.onEndEdit.AddListener(s => OnHeightInputEdited(s));

        if (startNodeX != null) startNodeX.onEndEdit.AddListener(s => OnStartNodeEdited(s, true));
        if (startNodeY != null) startNodeY.onEndEdit.AddListener(s => OnStartNodeEdited(s, false));
        if (goalNodeX != null) goalNodeX.onEndEdit.AddListener(s => OnGoalNodeEdited(s, true));
        if (goalNodeY != null) goalNodeY.onEndEdit.AddListener(s => OnGoalNodeEdited(s, false));

        if (wallsValue != null) wallsValue.onEndEdit.AddListener(s => OnWallsInputEdited(s));
    }

    void Update() { }

    // -- Sincronización UI <- RandomInitState --
    private void SyncUIFromRandomInit()
    {
        if (randomInit == null) return;

        if (widthValue != null) widthValue.text = randomInit.width.ToString();
        if (width != null) width.value = IntToScrollbar(randomInit.width, minWidth, maxWidth);

        if (heightValue != null) heightValue.text = randomInit.height.ToString();
        if (height != null) height.value = IntToScrollbar(randomInit.height, minHeight, maxHeight);

        if (startNodeX != null) startNodeX.text = randomInit.start.x.ToString();
        if (startNodeY != null) startNodeY.text = randomInit.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = randomInit.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = randomInit.goal.y.ToString();

        if (wallsValue != null) wallsValue.text = randomInit.probabilidadMuro.ToString("F2");
        if (Walls != null) Walls.value = Mathf.Clamp01(randomInit.probabilidadMuro);
    }

    // -- Callbacks scrollbars --
    private void OnWidthScrollbarChanged(float value)
    {
        int newWidth = ScrollbarToInt(value, minWidth, maxWidth);
        randomInit.width = newWidth;
        if (widthValue != null) widthValue.text = newWidth.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightScrollbarChanged(float value)
    {
        int newHeight = ScrollbarToInt(value, minHeight, maxHeight);
        randomInit.height = newHeight;
        if (heightValue != null) heightValue.text = newHeight.ToString();
        ClampStartGoalToBounds();
    }

    private void OnWallsScrollbarChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        randomInit.probabilidadMuro = v;
        if (wallsValue != null) wallsValue.text = v.ToString("F2");
    }

    // -- Callbacks InputFields (width/height) --
    private void OnWidthInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromRandomInit(); return; }
        v = Mathf.Clamp(v, minWidth, maxWidth);
        randomInit.width = v;
        if (width != null) width.value = IntToScrollbar(v, minWidth, maxWidth);
        if (widthValue != null) widthValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromRandomInit(); return; }
        v = Mathf.Clamp(v, minHeight, maxHeight);
        randomInit.height = v;
        if (height != null) height.value = IntToScrollbar(v, minHeight, maxHeight);
        if (heightValue != null) heightValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    // -- Callbacks Start/Goal edits --
    private void OnStartNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? randomInit.width : randomInit.height, isX);
        if (isX) randomInit.start = new Vector2Int(v, randomInit.start.y);
        else randomInit.start = new Vector2Int(randomInit.start.x, v);

        RefreshStartGoalFields();
    }

    private void OnGoalNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? randomInit.width : randomInit.height, isX);
        if (isX) randomInit.goal = new Vector2Int(v, randomInit.goal.y);
        else randomInit.goal = new Vector2Int(randomInit.goal.x, v);

        RefreshStartGoalFields();
    }

    private void RefreshStartGoalFields()
    {
        if (startNodeX != null) startNodeX.text = randomInit.start.x.ToString();
        if (startNodeY != null) startNodeY.text = randomInit.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = randomInit.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = randomInit.goal.y.ToString();
    }

    // -- Walls input callback --
    private void OnWallsInputEdited(string text)
    {
        if (!float.TryParse(text, out float v)) { SyncUIFromRandomInit(); return; }
        v = Mathf.Clamp01(v);
        randomInit.probabilidadMuro = v;
        if (Walls != null) Walls.value = v;
        if (wallsValue != null) wallsValue.text = v.ToString("F2");
    }

    // -- Utilidades --
    private int ClampCoordinate(int value, int dim, bool isX)
    {
        int min = 1;
        int max = dim - 2;
        return Mathf.Clamp(value, min, Mathf.Max(min, max));
    }

    private void ClampStartGoalToBounds()
    {
        int sx = Mathf.Clamp(randomInit.start.x, 1, Mathf.Max(1, randomInit.width - 2));
        int sy = Mathf.Clamp(randomInit.start.y, 1, Mathf.Max(1, randomInit.height - 2));
        int gx = Mathf.Clamp(randomInit.goal.x, 1, Mathf.Max(1, randomInit.width - 2));
        int gy = Mathf.Clamp(randomInit.goal.y, 1, Mathf.Max(1, randomInit.height - 2));
        randomInit.start = new Vector2Int(sx, sy);
        randomInit.goal = new Vector2Int(gx, gy);
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
