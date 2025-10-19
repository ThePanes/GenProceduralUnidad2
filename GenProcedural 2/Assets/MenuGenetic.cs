using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuGenetic : MonoBehaviour
{
    public Genetico genetico;
    [SerializeField] private Scrollbar width;
    [SerializeField] private TMP_InputField widthValue;
    [SerializeField] private Scrollbar height;
    [SerializeField] private TMP_InputField heightValue;
    [SerializeField] private TMP_InputField startNodeX;
    [SerializeField] private TMP_InputField startNodeY;
    [SerializeField] private TMP_InputField goalNodeX;
    [SerializeField] private TMP_InputField goalNodeY;
    [SerializeField] private TMP_InputField Parents;
    [SerializeField] private TMP_InputField Childs;
    [SerializeField] private TMP_InputField Generations;
    [SerializeField] private Scrollbar Paths;
    [SerializeField] private TMP_InputField pathsValue;
    [SerializeField] private Scrollbar Decorative;
    [SerializeField] private TMP_InputField decorativeValue;
    [SerializeField] private Scrollbar Walls;
    [SerializeField] private TMP_InputField wallsValue;

    // Rango esperado en Genetico (coincidir con los [Range] en la clase Genetico)
    private const int minWidth = 10;
    private const int maxWidth = 25;
    private const int minHeight = 10;
    private const int maxHeight = 25;

    void Start()
    {
        if (genetico == null)
        {
            Debug.LogWarning("MenuGenetic: faltó asignar 'genetico' en el inspector.");
            return;
        }

        // Inicializa UI con valores actuales de Genetico
        SyncUIFromGenetico();

        // Listeners: Scrollbars
        if (width != null)
            width.onValueChanged.AddListener(v => OnWidthScrollbarChanged(v));
        if (height != null)
            height.onValueChanged.AddListener(v => OnHeightScrollbarChanged(v));
        if (Paths != null)
            Paths.onValueChanged.AddListener(v => OnPathsScrollbarChanged(v));
        if (Decorative != null)
            Decorative.onValueChanged.AddListener(v => OnDecorativeScrollbarChanged(v));
        if (Walls != null)
            Walls.onValueChanged.AddListener(v => OnWallsScrollbarChanged(v));

        // Listeners: InputFields (edit terminado)
        if (widthValue != null)
            widthValue.onEndEdit.AddListener(s => OnWidthInputEdited(s));
        if (heightValue != null)
            heightValue.onEndEdit.AddListener(s => OnHeightInputEdited(s));

        if (startNodeX != null) startNodeX.onEndEdit.AddListener(s => OnStartNodeEdited(s, true));
        if (startNodeY != null) startNodeY.onEndEdit.AddListener(s => OnStartNodeEdited(s, false));
        if (goalNodeX != null) goalNodeX.onEndEdit.AddListener(s => OnGoalNodeEdited(s, true));
        if (goalNodeY != null) goalNodeY.onEndEdit.AddListener(s => OnGoalNodeEdited(s, false));

        if (Parents != null) Parents.onEndEdit.AddListener(s => OnParentsEdited(s));
        if (Childs != null) Childs.onEndEdit.AddListener(s => OnChildsEdited(s));
        if (Generations != null) Generations.onEndEdit.AddListener(s => OnGenerationsEdited(s));

        if (pathsValue != null) pathsValue.onEndEdit.AddListener(s => OnPathsInputEdited(s));
        if (decorativeValue != null) decorativeValue.onEndEdit.AddListener(s => OnDecorativeInputEdited(s));
        if (wallsValue != null) wallsValue.onEndEdit.AddListener(s => OnWallsInputEdited(s));
    }

    // Sin uso en este script, pero mantenido por compatibilidad
    void Update() { }

    // --- Sincronización UI <- Genetico ---
    private void SyncUIFromGenetico()
    {
        // Width
        if (widthValue != null) widthValue.text = genetico.width.ToString();
        if (width != null) width.value = IntToScrollbar(genetico.width, minWidth, maxWidth);

        // Height
        if (heightValue != null) heightValue.text = genetico.height.ToString();
        if (height != null) height.value = IntToScrollbar(genetico.height, minHeight, maxHeight);

        // Start / Goal
        if (startNodeX != null) startNodeX.text = genetico.start.x.ToString();
        if (startNodeY != null) startNodeY.text = genetico.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = genetico.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = genetico.goal.y.ToString();

        // Genetico params
        if (Parents != null) Parents.text = genetico.padres.ToString();
        if (Childs != null) Childs.text = genetico.hijos.ToString();
        if (Generations != null) Generations.text = genetico.generations.ToString();

        // Nuevos porcentajes (0..1) mostrados con 2 decimales
        if (pathsValue != null) pathsValue.text = genetico.porcentajeCaminos.ToString("F2");
        if (Paths != null) Paths.value = Mathf.Clamp01(genetico.porcentajeCaminos);

        if (decorativeValue != null) decorativeValue.text = genetico.porcentajeTierra.ToString("F2");
        if (Decorative != null) Decorative.value = Mathf.Clamp01(genetico.porcentajeTierra);

        if (wallsValue != null) wallsValue.text = genetico.porcentajeMuros.ToString("F2");
        if (Walls != null) Walls.value = Mathf.Clamp01(genetico.porcentajeMuros);
    }

    // --- Callbacks de Scrollbar ---
    private void OnWidthScrollbarChanged(float value)
    {
        int newWidth = ScrollbarToInt(value, minWidth, maxWidth);
        genetico.width = newWidth;
        if (widthValue != null) widthValue.text = newWidth.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightScrollbarChanged(float value)
    {
        int newHeight = ScrollbarToInt(value, minHeight, maxHeight);
        genetico.height = newHeight;
        if (heightValue != null) heightValue.text = newHeight.ToString();
        ClampStartGoalToBounds();
    }

    private void OnPathsScrollbarChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        genetico.porcentajeCaminos = v;
        if (pathsValue != null) pathsValue.text = v.ToString("F2");
    }

    private void OnDecorativeScrollbarChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        genetico.porcentajeTierra = v;
        if (decorativeValue != null) decorativeValue.text = v.ToString("F2");
    }

    private void OnWallsScrollbarChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        genetico.porcentajeMuros = v;
        if (wallsValue != null) wallsValue.text = v.ToString("F2");
    }

    // --- Callbacks de InputField (width/height) ---
    private void OnWidthInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromGenetico(); return; }
        v = Mathf.Clamp(v, minWidth, maxWidth);
        genetico.width = v;
        if (width != null) width.value = IntToScrollbar(v, minWidth, maxWidth);
        if (widthValue != null) widthValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    private void OnHeightInputEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { SyncUIFromGenetico(); return; }
        v = Mathf.Clamp(v, minHeight, maxHeight);
        genetico.height = v;
        if (height != null) height.value = IntToScrollbar(v, minHeight, maxHeight);
        if (heightValue != null) heightValue.text = v.ToString();
        ClampStartGoalToBounds();
    }

    // --- Callbacks Start/Goal edits ---
    private void OnStartNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? genetico.width : genetico.height, isX);
        if (isX) genetico.start = new Vector2Int(v, genetico.start.y);
        else genetico.start = new Vector2Int(genetico.start.x, v);

        RefreshStartGoalFields();
    }

    private void OnGoalNodeEdited(string text, bool isX)
    {
        if (!int.TryParse(text, out int v)) { RefreshStartGoalFields(); return; }

        v = ClampCoordinate(v, isX ? genetico.width : genetico.height, isX);
        if (isX) genetico.goal = new Vector2Int(v, genetico.goal.y);
        else genetico.goal = new Vector2Int(genetico.goal.x, v);

        RefreshStartGoalFields();
    }

    private void RefreshStartGoalFields()
    {
        if (startNodeX != null) startNodeX.text = genetico.start.x.ToString();
        if (startNodeY != null) startNodeY.text = genetico.start.y.ToString();
        if (goalNodeX != null) goalNodeX.text = genetico.goal.x.ToString();
        if (goalNodeY != null) goalNodeY.text = genetico.goal.y.ToString();
    }

    // Clamp coordenada para que quede dentro de [1, width-2] o [1, height-2]
    private int ClampCoordinate(int value, int dim, bool isX)
    {
        int min = 1;
        int max = dim - 2;
        return Mathf.Clamp(value, min, Mathf.Max(min, max));
    }

    private void ClampStartGoalToBounds()
    {
        // Ajusta start/goal si cambian width/height
        int sx = Mathf.Clamp(genetico.start.x, 1, Mathf.Max(1, genetico.width - 2));
        int sy = Mathf.Clamp(genetico.start.y, 1, Mathf.Max(1, genetico.height - 2));
        int gx = Mathf.Clamp(genetico.goal.x, 1, Mathf.Max(1, genetico.width - 2));
        int gy = Mathf.Clamp(genetico.goal.y, 1, Mathf.Max(1, genetico.height - 2));
        genetico.start = new Vector2Int(sx, sy);
        genetico.goal = new Vector2Int(gx, gy);
        RefreshStartGoalFields();
    }

    // --- Callbacks para parametros geneticos adicionales ---
    private void OnParentsEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { Parents.text = genetico.padres.ToString(); return; }
        genetico.padres = Mathf.Max(1, v);
        Parents.text = genetico.padres.ToString();
    }

    private void OnChildsEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { Childs.text = genetico.hijos.ToString(); return; }
        genetico.hijos = Mathf.Max(1, v);
        Childs.text = genetico.hijos.ToString();
    }

    private void OnGenerationsEdited(string text)
    {
        if (!int.TryParse(text, out int v)) { Generations.text = genetico.generations.ToString(); return; }
        genetico.generations = Mathf.Max(1, v);
        Generations.text = genetico.generations.ToString();
    }

    // --- Callbacks para nuevos porcentajes (paths, decorative, walls) desde InputFields ---
    private void OnPathsInputEdited(string text)
    {
        if (!float.TryParse(text, out float v)) { SyncUIFromGenetico(); return; }
        v = Mathf.Clamp01(v);
        genetico.porcentajeCaminos = v;
        if (Paths != null) Paths.value = v;
        if (pathsValue != null) pathsValue.text = v.ToString("F2");
    }

    private void OnDecorativeInputEdited(string text)
    {
        if (!float.TryParse(text, out float v)) { SyncUIFromGenetico(); return; }
        v = Mathf.Clamp01(v);
        genetico.porcentajeTierra = v;
        if (Decorative != null) Decorative.value = v;
        if (decorativeValue != null) decorativeValue.text = v.ToString("F2");
    }

    private void OnWallsInputEdited(string text)
    {
        if (!float.TryParse(text, out float v)) { SyncUIFromGenetico(); return; }
        v = Mathf.Clamp01(v);
        genetico.porcentajeMuros = v;
        if (Walls != null) Walls.value = v;
        if (wallsValue != null) wallsValue.text = v.ToString("F2");
    }

    // --- Utilidades de mapeo Scrollbar <-> int ---
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
