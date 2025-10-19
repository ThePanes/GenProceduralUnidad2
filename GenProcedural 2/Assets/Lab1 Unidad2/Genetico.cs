using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Genetico : MonoBehaviour
{
    [Range(10, 25)] public int width = 10;
    [Range(10, 25)] public int height = 10;

    [Tooltip("Posición de inicio (x, y) dentro del laberinto")]
    public Vector2Int start = new Vector2Int(1, 1);

    [Tooltip("Posición de meta (x, y) dentro del laberinto")]
    public Vector2Int goal = new Vector2Int(8, 8);

    public Vector3 offset = new Vector3(15, 0, 0); // Desplazamiento del Laberinto

    // 0: pasto (camino principal), 1: tierra (camino difícil), 2: cerro (muro)
    public GameObject[] terrainPrefabs;

    [Tooltip("Índice del prefab usado para representar inicio y meta (asegúrate que exista en terrainPrefabs)")]
    public int startGoalPrefabIndex = 2; // usar por defecto el tercer elemento (índice 2)

    public int padres = 10;
    public int hijos = 20;
    public int generations = 50;

    // Parámetros para controlar la proporción de cada tipo de celda
    [Range(0, 1)] public float porcentajeCaminos = 0.15f; // pasto (camino principal)
    [Range(0, 1)] public float porcentajeTierra = 0.15f;  // tierra (camino secundario)
    [Range(0, 1)] public float porcentajeMuros = 0.7f;    // cerro (muro)

    List<int[,]> population = new List<int[,]>();
    List<GameObject> cubosInstanciados = new List<GameObject>();

    void OnValidate()
    {
        // Asegura que start/goal estén dentro de los límites válidos
        start.x = Mathf.Clamp(start.x, 1, Mathf.Max(1, width - 2));
        start.y = Mathf.Clamp(start.y, 1, Mathf.Max(1, height - 2));
        goal.x = Mathf.Clamp(goal.x, 1, Mathf.Max(1, width - 2));
        goal.y = Mathf.Clamp(goal.y, 1, Mathf.Max(1, height - 2));

        // Valida índice de prefab para start/goal respecto a terrainPrefabs si está asignado
        if (terrainPrefabs != null && terrainPrefabs.Length > 0)
        {
            startGoalPrefabIndex = Mathf.Clamp(startGoalPrefabIndex, 0, terrainPrefabs.Length - 1);
        }
        else
        {
            // si no hay prefabs asignados, mantener valor por defecto no negativo
            startGoalPrefabIndex = Mathf.Max(0, startGoalPrefabIndex);
        }
    }

    void Start()
    {
        StartCoroutine(RegenerarTerrenoCada3Segundos());
    }

    IEnumerator RegenerarTerrenoCada3Segundos()
    {
        while (true)
        {
            InicializarPoblacion();
            GenerarTerreno();
            yield return new WaitForSeconds(3f);
        }
    }

    void GenerarTerreno()
    {
        for (int gen = 0; gen < generations; gen++)
        {
            List<int[,]> offspring = new List<int[,]>();
            for (int i = 0; i < hijos; i++)
            {
                int[,] padre = SeleccionarPadre();
                int[,] hijo = Mutar(padre);
                offspring.Add(hijo);
            }
            population.AddRange(offspring);
            population = SeleccionarMejores(population, padres);
        }
        int[,] mejor = population[0];
        DibujarTerreno(mejor);
    }

    void InicializarPoblacion()
    {
        population.Clear();
        for (int i = 0; i < padres; i++)
        {
            int[,] individuo = GenerarLaberintoConCamino();
            population.Add(individuo);
        }
    }

    // Genera un laberinto con un camino garantizado de start a goal
    int[,] GenerarLaberintoConCamino()
    {
        int[,] laberinto = new int[width, height];
        // Inicializa todo como muro (2)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                laberinto[x, y] = 2;

        // Asegura start/goal dentro de los límites válidos
        Vector2Int s = new Vector2Int(
            Mathf.Clamp(start.x, 1, Mathf.Max(1, width - 2)),
            Mathf.Clamp(start.y, 1, Mathf.Max(1, height - 2))
        );
        Vector2Int g = new Vector2Int(
            Mathf.Clamp(goal.x, 1, Mathf.Max(1, width - 2)),
            Mathf.Clamp(goal.y, 1, Mathf.Max(1, height - 2))
        );

        // Genera un camino directo (aleatorio entre moverse en x o y) desde s hasta g
        int xActual = s.x;
        int yActual = s.y;
        laberinto[xActual, yActual] = 0;
        System.Random rnd = new System.Random();
        while (xActual != g.x || yActual != g.y)
        {
            int dir = rnd.Next(2); // 0 -> mover en x si posible, 1 -> mover en y si posible
            if (xActual != g.x && yActual != g.y)
            {
                if (dir == 0)
                    xActual += Math.Sign(g.x - xActual);
                else
                    yActual += Math.Sign(g.y - yActual);
            }
            else if (xActual != g.x)
            {
                xActual += Math.Sign(g.x - xActual);
            }
            else if (yActual != g.y)
            {
                yActual += Math.Sign(g.y - yActual);
            }

            // Clamp por seguridad
            xActual = Mathf.Clamp(xActual, 1, width - 2);
            yActual = Mathf.Clamp(yActual, 1, height - 2);

            laberinto[xActual, yActual] = 0;
        }

        // Rellena el resto según los porcentajes definidos, sin sobreescribir el camino generado
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (laberinto[x, y] == 0) continue; // No sobreescribir el camino principal

                float r = UnityEngine.Random.value;
                if (r < porcentajeCaminos)
                    laberinto[x, y] = 0; // pasto
                else if (r < porcentajeCaminos + porcentajeTierra)
                    laberinto[x, y] = 1; // tierra
                else
                    laberinto[x, y] = 2; // muro
            }
        }

        // Asegura inicio y fin libres (por si acaso)
        laberinto[s.x, s.y] = 0;
        laberinto[g.x, g.y] = 0;
        return laberinto;
    }

    int[,] SeleccionarPadre()
    {
        return population[UnityEngine.Random.Range(0, population.Count)];
    }

    int[,] Mutar(int[,] padre)
    {
        int w = padre.GetLength(0);
        int h = padre.GetLength(1);
        int[,] hijo = padre.Clone() as int[,];
        for (int i = 0; i < 3; i++)
        {
            int x = UnityEngine.Random.Range(1, w - 1);
            int y = UnityEngine.Random.Range(1, h - 1);

            // No mutar las celdas de start o goal (considerando coordenadas actuales dentro de bounds)
            if ((x == Mathf.Clamp(start.x, 1, w - 2) && y == Mathf.Clamp(start.y, 1, h - 2)) ||
                (x == Mathf.Clamp(goal.x, 1, w - 2) && y == Mathf.Clamp(goal.y, 1, h - 2)))
                continue;

            if (hijo[x, y] == 0) continue; // No mutar el camino principal

            float r = UnityEngine.Random.value;
            if (r < porcentajeCaminos)
                hijo[x, y] = 0;
            else if (r < porcentajeCaminos + porcentajeTierra)
                hijo[x, y] = 1;
            else
                hijo[x, y] = 2;
        }

        // Asegura start y goal como camino en la matriz (el dibujo usará el prefab especial)
        int sx = Mathf.Clamp(start.x, 0, w - 1);
        int sy = Mathf.Clamp(start.y, 0, h - 1);
        int gx = Mathf.Clamp(goal.x, 0, w - 1);
        int gy = Mathf.Clamp(goal.y, 0, h - 1);
        hijo[sx, sy] = 0;
        hijo[gx, gy] = 0;

        return hijo;
    }

    List<int[,]> SeleccionarMejores(List<int[,]> poblacion, int cantidad)
    {
        poblacion.Sort((a, b) => Fitness(b).CompareTo(Fitness(a)));
        return poblacion.GetRange(0, cantidad);
    }

    // Fitness: premia si hay camino entre inicio y fin, penaliza muros y caminos difíciles
    int Fitness(int[,] individuo)
    {
        if (!HayCamino(individuo, start.x, start.y, goal.x, goal.y))
            return 0;

        int muros = 0, tierra = 0, pasto = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (individuo[x, y] == 2) muros++;
                else if (individuo[x, y] == 1) tierra++;
                else if (individuo[x, y] == 0) pasto++;
            }
        // Puedes ajustar los pesos según la dificultad deseada
        return 1000 + pasto * 2 + tierra - muros * 2;
    }

    bool HayCamino(int[,] laberinto, int startX, int startY, int endX, int endY)
    {
        int w = laberinto.GetLength(0);
        int h = laberinto.GetLength(1);
        bool[,] visitado = new bool[w, h];
        Queue<Vector2Int> cola = new Queue<Vector2Int>();
        cola.Enqueue(new Vector2Int(startX, startY));
        visitado[startX, startY] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            if (actual.x == endX && actual.y == endY)
                return true;
            for (int dir = 0; dir < 4; dir++)
            {
                int nx = actual.x + dx[dir];
                int ny = actual.y + dy[dir];
                if (nx >= 1 && nx < w - 1 && ny >= 1 && ny < h - 1)
                {
                    if (!visitado[nx, ny] && laberinto[nx, ny] != 2)
                    {
                        visitado[nx, ny] = true;
                        cola.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return false;
    }

    void DibujarTerreno(int[,] terreno)
    {
        foreach (var cubo in cubosInstanciados)
        {
            if (cubo != null)
                Destroy(cubo);
        }
        cubosInstanciados.Clear();

        if (terrainPrefabs == null || terrainPrefabs.Length == 0)
        {
            Debug.LogWarning("Genetico: terrainPrefabs no asignado o vacío.");
            return;
        }

        // índices seguros para start/goal
        int sx = Mathf.Clamp(start.x, 0, width - 1);
        int sy = Mathf.Clamp(start.y, 0, height - 1);
        int gx = Mathf.Clamp(goal.x, 0, width - 1);
        int gy = Mathf.Clamp(goal.y, 0, height - 1);

        // índice seguro del prefab para start/goal
        int startGoalIndex = Mathf.Clamp(startGoalPrefabIndex, 0, terrainPrefabs.Length - 1);

        if (terrainPrefabs[startGoalIndex] == null)
        {
            Debug.LogWarning($"Genetico: terrainPrefabs[{startGoalIndex}] es null. Usando sprite normal en su lugar.");
        }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int tipo = terreno[x, y];

                // Si la casilla es start o goal, usa el prefab especial configurado (si existe)
                if ((x == sx && y == sy) || (x == gx && y == gy))
                {
                    if (terrainPrefabs.Length > startGoalIndex && terrainPrefabs[startGoalIndex] != null)
                        tipo = startGoalIndex;
                    else
                        tipo = Mathf.Clamp(tipo, 0, terrainPrefabs.Length - 1);
                }
                else
                {
                    // Asegura tipo válido
                    tipo = Mathf.Clamp(tipo, 0, terrainPrefabs.Length - 1);
                }

                Vector3 pos = new Vector3(x, 0, y) + offset;
                GameObject cubo = Instantiate(terrainPrefabs[tipo], pos, Quaternion.identity);
                cubosInstanciados.Add(cubo);
            }
    }

    bool EsBorde(int x, int y)
    {
        return x == 0 || y == 0 || x == width - 1 || y == height - 1;
    }
}
