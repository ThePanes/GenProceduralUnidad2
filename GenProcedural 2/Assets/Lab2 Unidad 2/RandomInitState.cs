using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomInitState : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject[] terrainPrefabs;
    public Vector3 offset = new Vector3(15, 0, 0); // Desplazamiento para no encimar el otro laberinto
    [Range(0, 1)] public float probabilidadMuro = 0.4f; // Menos muros = laberinto más abierto

    private List<GameObject> cubosInstanciados = new List<GameObject>();


    /// Mod Ignacio - Evaluador
    /// 
    /// Tengo la teoria de que crear algo que evalue la cantidad de bloques totales recorribles y la cantidad de bloques para la meta 
    /// seria una buena función de evaluación. Por lo que aplicare ello:

    private float bloquesParaLaMeta;
    private float bloquesRecorriblesTotales;
    private float bloquesTotales;
    private float bloquesNoAccesibles;

    private float cortafuego;
    



    void Start()
    {
        cortafuego = 0;
        StartCoroutine(RegenerarLaberintoCada3Segundos());
    }

    IEnumerator RegenerarLaberintoCada3Segundos()
    {
        while (true)
        {
            GenerarYLlenarLaberinto();
            yield return new WaitForSeconds(3f);
        }
    }

    void GenerarYLlenarLaberinto()
    {
        // Elimina los cubos anteriores
        foreach (var cubo in cubosInstanciados)
        {
            if (cubo != null)
                Destroy(cubo);
        }
        cubosInstanciados.Clear();

        int[,] laberinto;
        List<Vector2Int> camino;

        // Intenta hasta que haya un camino
        do
        {
            bloquesParaLaMeta = 0;
            bloquesRecorriblesTotales = 0;
            bloquesTotales = 0;

            laberinto = GenerarLaberintoAleatorio();
            camino = EncontrarCamino(laberinto, 1, 1, width - 2, height - 2);


            bloquesNoAccesibles = bloquesTotales - bloquesRecorriblesTotales;


            ///por como funciona el codigo esto tiene un error estimado de 2 XD
            Debug.Log("Bloques para la meta: " + bloquesParaLaMeta);
            Debug.Log("Bloques recorribles: " + bloquesRecorriblesTotales);
            Debug.Log("Bloques totales: " + bloquesTotales);
            Debug.Log("Bloques no accesibles: " + bloquesNoAccesibles);


            //este cortafuego es muy barato pero al no se crachea unity XD
            cortafuego++;
            if(camino != null)
            {
                cortafuego = 0; 
            }

        } while (camino == null && cortafuego <= 20);

        // Marca el camino correcto con tierra (1)
        foreach (var pos in camino)
        {
            if (laberinto[pos.x, pos.y] == 0)
                laberinto[pos.x, pos.y] = 1;
        }
        // Asegura inicio y fin como pasto
        laberinto[1, 1] = 0;
        laberinto[width - 2, height - 2] = 0;

        // Dibuja el laberinto
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int tipo = laberinto[x, y];
                Vector3 pos = new Vector3(x, 0, y) + offset;
                GameObject cubo = Instantiate(terrainPrefabs[tipo], pos, Quaternion.identity);
                cubosInstanciados.Add(cubo);
            }
    }

    int[,] GenerarLaberintoAleatorio()
    {
        int[,] laberinto = new int[width, height];
        // Bordes como muros
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                laberinto[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? 2 : 0;

        // Rellena el interior aleatoriamente con muros y caminos
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                laberinto[x, y] = Random.value < probabilidadMuro ? 2 : 0;  

                //cuenta los bloques sin contar los muros
                if(laberinto[x, y] == 0)
                {
                    bloquesTotales++;
                }
            }

        // Asegura inicio y fin libres
        laberinto[1, 1] = 0;
        laberinto[width - 2, height - 2] = 0;
        //bloque inicio y final
        bloquesTotales+= 2;



        return laberinto;
    }

    // BFS para encontrar el camino más corto
    //Ignacio: lo modifique para que recorriera todo
    List<Vector2Int> EncontrarCamino(int[,] laberinto, int startX, int startY, int endX, int endY)
    {
        int w = laberinto.GetLength(0);
        int h = laberinto.GetLength(1);
        bool[,] visitado = new bool[w, h];
        Vector2Int[,] previo = new Vector2Int[w, h];
        Queue<Vector2Int> cola = new Queue<Vector2Int>();
        cola.Enqueue(new Vector2Int(startX, startY));
        visitado[startX, startY] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        List<Vector2Int> caminoEncontrado = new List<Vector2Int>();
        caminoEncontrado = null; //lo limpio del anterior


        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();

            bloquesRecorriblesTotales++;

            if (actual.x == endX && actual.y == endY)
            {
                // Reconstruye el camino
                List<Vector2Int> camino = new List<Vector2Int>();
                Vector2Int paso = actual;
                while (paso != new Vector2Int(startX, startY))
                {
                    camino.Add(paso);
                    paso = previo[paso.x, paso.y];

                    bloquesParaLaMeta++;
                }
                camino.Reverse();
                caminoEncontrado = camino;
            }
            for (int dir = 0; dir < 4; dir++)
            {
                int nx = actual.x + dx[dir];
                int ny = actual.y + dy[dir];
                if (nx >= 1 && nx < w - 1 && ny >= 1 && ny < h - 1)
                {
                    if (!visitado[nx, ny] && laberinto[nx, ny] != 2)
                    {
                        visitado[nx, ny] = true;
                        previo[nx, ny] = actual;
                        cola.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        if(caminoEncontrado != null)
        {
            return caminoEncontrado;
        }
        return null; // No hay camino
    }
}
