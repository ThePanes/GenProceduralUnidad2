using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Gestionador : MonoBehaviour
{
    public Modificador mod1;
    public Modificador mod2;
    public Modificador mod3;
    public Modificador mod4;


    public int width = 10;
    public int height = 10;

    [Tooltip("Posición de inicio (x, y) dentro del laberinto")]
    public Vector2Int start = new Vector2Int(1, 1);

    [Tooltip("Posición de meta (x, y) dentro del laberinto")]
    public Vector2Int goal = new Vector2Int(8, 8);

    public GameObject[] terrainPrefabs;
    public Vector3 offset = new Vector3(15, 0, 0); // Desplazamiento para no encimar el otro laberinto
    [Range(0, 1)] public float probabilidadMuro = 0.4f; // Menos muros = laberinto más abierto

    private List<GameObject> cubosInstanciados = new List<GameObject>();

    public int[,] laberintoGeneral;
    public int[,] laberintoReserva; //para que no genere un lab con peor puntuación


    /// Mod Ignacio - Evaluador
    /// 
    /// Tengo la teoria de que crear algo que evalue la cantidad de bloques totales recorribles y la cantidad de bloques para la meta 
    /// seria una buena función de evaluación. Por lo que aplicare ello:


    private float cortafuego;
    private float puntajeMaximo;
    private float puntajeMinimo;



    void OnValidate()
    {
        // Limita start y goal a estar dentro del rango permitido
        start.x = Mathf.Clamp(start.x, 1, width - 2);
        start.y = Mathf.Clamp(start.y, 1, height - 2);
        goal.x = Mathf.Clamp(goal.x, 1, width - 2);
        goal.y = Mathf.Clamp(goal.y, 1, height - 2);
    }

    void Start()
    {
        puntajeMinimo = 0;
        cortafuego = 0;
        GenerarYLlenarLaberinto();

        if(laberintoGeneral != null)
        {
            mod1.laberintoGeneral = ClonLaberinto(laberintoGeneral);
            mod2.laberintoGeneral = ClonLaberinto(laberintoGeneral);
            mod3.laberintoGeneral = ClonLaberinto(laberintoGeneral);
            mod4.laberintoGeneral = ClonLaberinto(laberintoGeneral);
        }
        
    }


    private void Update()
    {
        ///Remplazo completo de los laberintos
        if (mod1.laberintoAnalizado && mod2.laberintoAnalizado && mod3.laberintoAnalizado && mod4.laberintoAnalizado)
        {

            //sacamos puntaje maximo
            puntajeMaximo = Mathf.Max(mod1.puntuacionDeEvaluacion, mod2.puntuacionDeEvaluacion, mod3.puntuacionDeEvaluacion, mod4.puntuacionDeEvaluacion, puntajeMinimo);






            //vemos cual es y remplazamos los laberintos segun cual sea mayor

            if (puntajeMinimo == puntajeMaximo)
            {
                laberintoGeneral = ClonLaberinto(laberintoReserva);

                ///Si no generaron nada mejor vuelvan a la base
                mod1.laberintoGeneral = laberintoReserva;
                mod2.laberintoGeneral = laberintoReserva;
                mod3.laberintoGeneral = laberintoReserva;
                mod4.laberintoGeneral = laberintoReserva;

                Debug.Log("0");
            }
            else if(mod1.puntuacionDeEvaluacion == puntajeMaximo)
            {
                laberintoGeneral = ClonLaberinto(mod1.laberintoGeneral);
                laberintoReserva = ClonLaberinto(mod1.laberintoGeneral);

                //
                mod2.laberintoGeneral = laberintoGeneral;
                mod3.laberintoGeneral = laberintoGeneral;
                mod4.laberintoGeneral = laberintoGeneral;

                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("1");
            }
            else if(mod2.puntuacionDeEvaluacion == puntajeMaximo)
            {
                laberintoGeneral = ClonLaberinto(mod2.laberintoGeneral);
                laberintoReserva = ClonLaberinto(mod2.laberintoGeneral);

                mod1.laberintoGeneral = laberintoGeneral;
                //
                mod3.laberintoGeneral = laberintoGeneral;
                mod4.laberintoGeneral = laberintoGeneral;

                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("2");
            }
            else if(mod3.puntuacionDeEvaluacion == puntajeMaximo)
            {
                laberintoGeneral = ClonLaberinto(mod3.laberintoGeneral);
                laberintoReserva = ClonLaberinto(mod3.laberintoGeneral);

                mod1.laberintoGeneral = laberintoGeneral;
                mod2.laberintoGeneral = laberintoGeneral;
                //
                mod4.laberintoGeneral = laberintoGeneral;

                GenerarLaberintoReserva(laberintoReserva);
                puntajeMinimo = puntajeMaximo;
                Debug.Log("3");
            }
            else if(mod4.puntuacionDeEvaluacion == puntajeMaximo)
            {
                laberintoGeneral = ClonLaberinto(mod4.laberintoGeneral);
                laberintoReserva = ClonLaberinto(mod4.laberintoGeneral);

                mod1.laberintoGeneral = laberintoGeneral;
                mod2.laberintoGeneral = laberintoGeneral;
                mod3.laberintoGeneral = laberintoGeneral;
                //

                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("4");
            }

            

            //vuelvo a false el check para saber que no esta listo todaavia la info del siguiente lab
            mod1.laberintoAnalizado = false;
            mod2.laberintoAnalizado = false;
            mod3.laberintoAnalizado = false;
            mod4.laberintoAnalizado = false;


            Debug.Log("puntaje maximo:" + puntajeMaximo);
            Debug.Log("puntaje minimo:" + puntajeMinimo);
        }
    }


    //--------------------------------------------------------------------------------------------------------------------------------

    void GenerarYLlenarLaberinto()
    {
        // Elimina los cubos anteriores
        foreach (var cubo in cubosInstanciados)
        {
            if (cubo != null)
                DestroyImmediate(cubo);
        }
        cubosInstanciados.Clear();

        int[,] laberinto;
        List<Vector2Int> camino;

        if (laberintoGeneral == null)
        {
            do
            {

                laberintoGeneral = GenerarLaberintoAleatorio();

                laberinto = AnalisarYEvaluar(laberintoGeneral);
                camino = EncontrarCamino(laberinto, start.x, start.y, goal.x, goal.y);

                cortafuego++;
                if (camino != null)
                {
                    cortafuego = 0;
                }

            } while (camino == null && cortafuego <= 50);
        }



        // Intenta hasta que haya un camino
        do
        {
            laberinto = AnalisarYEvaluar(laberintoGeneral);
            camino = EncontrarCamino(laberinto, start.x, start.y, goal.x, goal.y);


            cortafuego++;
            if (camino != null)
            {
                cortafuego = 0;
            }

        } while (camino == null && cortafuego <= 20);

        /*
        // Marca el camino correcto con tierra (1)
        if (camino != null)
        {
            foreach (var pos in camino)
            {
                if (laberinto[pos.x, pos.y] == 0)
                    laberinto[pos.x, pos.y] = 1;
            }
            // Asegura inicio y fin como pasto
            laberinto[start.x, start.y] = 0;
            laberinto[goal.x, goal.y] = 0;

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
        else
        {
            Debug.Log("camino nulo");
        }*/


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

            }

        // Asegura inicio y fin libres
        laberinto[start.x, start.y] = 0;
        laberinto[goal.x, goal.y] = 0;





        return laberinto;
    }

    int[,] AnalisarYEvaluar(int[,] laberinto)
    {

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {

                if (laberinto[x, y] == 1)
                {
                    laberinto[x, y] = 0;
                }


                if (laberinto[x, y] == 0)
                {
                    laberinto[x, y] = 0;
                    laberinto[x, y] = Random.value < probabilidadMuro / 4 ? 2 : 0;
                }
                else if (laberinto[x, y] == 2)
                {
                    laberinto[x, y] = 0;
                    laberinto[x, y] = Random.value < probabilidadMuro * 2 ? 2 : 0;
                }

            }


        if (laberinto[start.x, start.y] != 0)
        {
            laberinto[start.x, start.y] = 0;
        }

        if (laberinto[goal.x, goal.y] != 0)
        {
            laberinto[goal.x, goal.y] = 0;
        }

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

            if (actual.x == endX && actual.y == endY)
            {
                // Reconstruye el camino
                List<Vector2Int> camino = new List<Vector2Int>();
                Vector2Int paso = actual;
                while (paso != new Vector2Int(startX, startY))
                {
                    camino.Add(paso);
                    paso = previo[paso.x, paso.y];
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

        if (caminoEncontrado != null)
        {
            return caminoEncontrado;
        }
        return null; // No hay camino
    }

    void GenerarLaberintoReserva(int[,] laberinto)
    {
        // Elimina los cubos anteriores
        foreach (var cubo in cubosInstanciados)
        {
            if (cubo != null)
                DestroyImmediate(cubo);
        }
        cubosInstanciados.Clear();

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


    ///Resulta que por como funciona int[,] no se puede asignar directamente, si no que se debe crear una copia y esa asignarla, por lo tanto...
    int[,] ClonLaberinto(int[,] original)
    {
        int width = original.GetLength(0);
        int height = original.GetLength(1);
        int[,] copia = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                copia[x, y] = original[x, y];

        return copia;
    }

}
