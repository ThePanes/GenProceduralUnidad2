using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Genetico : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int padres = 10;
    public int hijos = 20;
    public int generations = 50;

    // Parámetros para controlar la proporción de cada tipo de celda
    [Range(0, 1)] public float porcentajeCaminos = 0.15f; // pasto (camino principal)
    [Range(0, 1)] public float porcentajeTierra = 0.15f;  // tierra (camino secundario)
    [Range(0, 1)] public float porcentajeMuros = 0.7f;    // cerro (muro)

    // 0: pasto (camino principal), 1: tierra (camino difícil), 2: cerro (muro)
    public GameObject[] terrainPrefabs;

    List<int[,]> population = new List<int[,]>();
    List<GameObject> cubosInstanciados = new List<GameObject>();

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

    // Genera un laberinto con un camino garantizado de (1,1) a (width-2,height-2)
    int[,] GenerarLaberintoConCamino()
    {
        int[,] laberinto = new int[width, height];
        // Bordes como muros
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                laberinto[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? 2 : 2;

        // Genera un camino principal de pasto (0)
        int xActual = 1, yActual = 1;
        laberinto[xActual, yActual] = 0;
        System.Random rnd = new System.Random();
        while (xActual < width - 2 || yActual < height - 2)
        {
            if (xActual < width - 2 && (yActual == height - 2 || rnd.Next(2) == 0))
                xActual++;
            else if (yActual < height - 2)
                yActual++;
            laberinto[xActual, yActual] = 0;
        }

        // Rellena el resto según los porcentajes definidos
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (laberinto[x, y] == 0) continue; // No sobreescribir el camino principal

                float r = Random.value;
                if (r < porcentajeCaminos)
                    laberinto[x, y] = 0; // pasto
                else if (r < porcentajeCaminos + porcentajeTierra)
                    laberinto[x, y] = 1; // tierra
                else
                    laberinto[x, y] = 2; // muro
            }
        }

        // Asegura inicio y fin libres
        laberinto[1, 1] = 0;
        laberinto[width - 2, height - 2] = 0;
        return laberinto;
    }

    int[,] SeleccionarPadre()
    {
        return population[Random.Range(0, population.Count)];
    }

    int[,] Mutar(int[,] padre)
    {
        int w = padre.GetLength(0);
        int h = padre.GetLength(1);
        int[,] hijo = padre.Clone() as int[,];
        for (int i = 0; i < 3; i++)
        {
            int x = Random.Range(1, w - 1);
            int y = Random.Range(1, h - 1);
            if (hijo[x, y] == 0) continue; // No mutar el camino principal

            float r = Random.value;
            if (r < porcentajeCaminos)
                hijo[x, y] = 0;
            else if (r < porcentajeCaminos + porcentajeTierra)
                hijo[x, y] = 1;
            else
                hijo[x, y] = 2;
        }
        hijo[1, 1] = 0;
        hijo[w - 2, h - 2] = 0;
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
        if (!HayCamino(individuo, 1, 1, width - 2, height - 2))
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

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int tipo = terreno[x, y];
                Vector3 pos = new Vector3(x, 0, y);
                GameObject cubo = Instantiate(terrainPrefabs[tipo], pos, Quaternion.identity);
                cubosInstanciados.Add(cubo);
            }
    }

    bool EsBorde(int x, int y)
    {
        return x == 0 || y == 0 || x == width - 1 || y == height - 1;
    }
}
