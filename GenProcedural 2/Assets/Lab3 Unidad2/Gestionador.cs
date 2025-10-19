using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.UI.Image;
using System.Reflection;

public class Gestionador : MonoBehaviour
{
    public Modificador mod1;
    public Modificador mod2;
    public Modificador mod3;
    public Modificador mod4;


    public int width = 10;
    public int height = 10;

    [Tooltip("Posición de inicio (x, y) dentro del laberinto")]
    public Vector2Int start /*= new Vector2Int(1, 1)*/;

    [Tooltip("Posición de meta (x, y) dentro del laberinto")]
    public Vector2Int goal  /*= new Vector2Int(8, 8)*/;

    public GameObject[] terrainPrefabs;
    public Vector3 offset = new Vector3(15, 0, 0); // Desplazamiento para no encimar el otro laberinto
    [Range(0, 1)] public float probabilidadMuro = 0.4f; // Menos muros = laberinto más abierto

    private List<GameObject> cubosInstanciados = new List<GameObject>();

    public int[,] laberintoGeneral;
    public int[,] laberintoReserva; //para que no genere un lab con peor puntuación

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
        GenerarGrillaYCrearLaberinto();

        EstablecerAlturas();
        EstablecerStartGoal();
        EstablecerOffset();

        if (laberintoGeneral != null)
        {
            ClonarTodoAlLaberintoGeneral();
        }
        
    }


    private void Update()
    {
        if(height != mod1.height || width  != mod1.width || start != mod1.start || goal != mod1.goal)
        {
            Debug.Log("antes del desastre");
            StartCoroutine(PausaDeEdicion());

            ResetManual();
          

        }
        else if(mod1.laberintoAnalizado && mod2.laberintoAnalizado && mod3.laberintoAnalizado && mod4.laberintoAnalizado)  ///Remplazo completo de los laberintos
        {

            //sacamos puntaje maximo
            puntajeMaximo = Mathf.Max(mod1.puntuacionDeEvaluacion, mod2.puntuacionDeEvaluacion, mod3.puntuacionDeEvaluacion, mod4.puntuacionDeEvaluacion, puntajeMinimo);

            //vemos cual es y remplazamos los laberintos segun cual sea mayor
            if (mod1.puntuacionDeEvaluacion == puntajeMaximo)
            {
                RemplazoManualLaberinto(laberintoGeneral, mod1.laberintoGeneral);
                RemplazoManualLaberinto(laberintoReserva, mod1.laberintoGeneral);

                //
                RemplazoManualLaberinto(mod2.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod3.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod4.laberintoDeReservaModificador, laberintoReserva);

                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("1");
            }
            else if (mod2.puntuacionDeEvaluacion == puntajeMaximo)
            {
                RemplazoManualLaberinto(laberintoGeneral, mod2.laberintoGeneral);
                RemplazoManualLaberinto(laberintoReserva, mod2.laberintoGeneral);

                RemplazoManualLaberinto(mod1.laberintoDeReservaModificador, laberintoReserva);
                //
                RemplazoManualLaberinto(mod3.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod4.laberintoDeReservaModificador, laberintoReserva);

                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("2");
            }
            else if (mod3.puntuacionDeEvaluacion == puntajeMaximo)
            {
                RemplazoManualLaberinto(laberintoGeneral, mod3.laberintoGeneral);
                RemplazoManualLaberinto(laberintoReserva, mod3.laberintoGeneral);

                RemplazoManualLaberinto(mod1.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod2.laberintoDeReservaModificador, laberintoReserva);
                //
                RemplazoManualLaberinto(mod4.laberintoDeReservaModificador, laberintoReserva);

                GenerarLaberintoReserva(laberintoReserva);
                puntajeMinimo = puntajeMaximo;
                Debug.Log("3");
            }
            else if (mod4.puntuacionDeEvaluacion == puntajeMaximo)
            {
                RemplazoManualLaberinto(laberintoGeneral, mod4.laberintoGeneral);
                RemplazoManualLaberinto(laberintoReserva, mod4.laberintoGeneral);

                RemplazoManualLaberinto(mod1.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod2.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod3.laberintoDeReservaModificador, laberintoReserva);
                //


                puntajeMinimo = puntajeMaximo;
                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("4");
            }
            else if (puntajeMinimo == puntajeMaximo)
            {
                //laberintoGeneral = ClonLaberinto(laberintoReserva);

                RemplazoManualLaberinto(laberintoGeneral, laberintoReserva);

                RemplazoManualLaberinto(mod1.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod2.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod3.laberintoDeReservaModificador, laberintoReserva);
                RemplazoManualLaberinto(mod4.laberintoDeReservaModificador, laberintoReserva);

                GenerarLaberintoReserva(laberintoReserva);
                Debug.Log("0");
            }



            //vuelvo a false el check para saber que no esta listo todaavia la info del siguiente lab
            mod1.laberintoAnalizado = false;
            mod2.laberintoAnalizado = false;
            mod3.laberintoAnalizado = false;
            mod4.laberintoAnalizado = false;


            Debug.Log("puntaje maximo:" + puntajeMaximo.ToString("F10"));
            Debug.Log("puntaje minimo:" + puntajeMinimo.ToString("F10"));
        }
    }


    //--------------------------------------------------------------------------------------------------------------------------------

    void RemplazoManualLaberinto(int[,] laberintoARemplazar, int[,] laberintoBase)
    {

        if(laberintoARemplazar == null)
        {
            Debug.Log("fallo laberintoRemplazar");
        }
        else if(laberintoBase == null)
        {
            Debug.Log("fallo laberintoBase");
        }
        else
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    laberintoARemplazar[x, y] = laberintoBase[x, y];
                }
        }
    }


    void GenerarGrillaYCrearLaberinto()
    {
        BorrarCubos();

        int[,] laberinto;
        List<Vector2Int> camino;

        if (laberintoGeneral == null)
        {
            do
            {

                laberintoGeneral = GenerarLaberintoAleatorio();

                laberinto = ClonLaberinto(laberintoGeneral);
                //laberinto = AnalisarYEvaluar(laberintoGeneral);
                camino = EncontrarCamino(laberinto, start.x, start.y, goal.x, goal.y);

                cortafuego++;
                if (camino != null)
                {
                    cortafuego = 0;
                }

            } while (camino == null && cortafuego <= 50);
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

            }

        // Asegura inicio y fin libres
        laberinto[start.x, start.y] = 0;
        laberinto[goal.x, goal.y] = 0;





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
        BorrarCubos();

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

    void ClonarTodoAlLaberintoGeneral()
    {
        mod1.laberintoGeneral = ClonLaberinto(laberintoGeneral);
        mod2.laberintoGeneral = ClonLaberinto(laberintoGeneral);
        mod3.laberintoGeneral = ClonLaberinto(laberintoGeneral);
        mod4.laberintoGeneral = ClonLaberinto(laberintoGeneral);

        mod1.laberintoDeReservaModificador = ClonLaberinto(laberintoGeneral);
        mod2.laberintoDeReservaModificador = ClonLaberinto(laberintoGeneral);
        mod3.laberintoDeReservaModificador = ClonLaberinto(laberintoGeneral);
        mod4.laberintoDeReservaModificador = ClonLaberinto(laberintoGeneral);

        laberintoReserva = ClonLaberinto(laberintoGeneral);
    }



    IEnumerator PausaDeEdicion()
    {
        Debug.Log("Antes de la pausa");

        // Espera 2 segundos sin congelar el juego
        yield return new WaitForSeconds(4f);

        Debug.Log("Después de la pausa");
    }



    void EstablecerAlturas()
    {
        mod1.height = height;
        mod1.width = width;
        mod2.height = height;
        mod2.width = width;
        mod3.height = height;
        mod3.width = width;
        mod4.height = height;
        mod4.width = width;
    }

    void EstablecerStartGoal()
    {
        mod1.start.x = start.x;
        mod1.start.y = start.y;
        mod1.goal.x = goal.x;
        mod1.goal.y = goal.y;

        mod2.start.x = start.x;
        mod2.start.y = start.y;
        mod2.goal.x = goal.x;
        mod2.goal.y = goal.y;

        mod3.start.x = start.x;
        mod3.start.y = start.y;
        mod3.goal.x = goal.x;
        mod3.goal.y = goal.y;

        mod4.start.x = start.x;
        mod4.start.y = start.y;
        mod4.goal.x = goal.x;
        mod4.goal.y = goal.y;
    }

    void EstablecerOffset()
    {
        mod2.offset.x = mod1.offset.x + width + 5;
        mod3.offset.x = mod2.offset.x + width + 5;
        mod4.offset.x = mod3.offset.x + width + 5;

        mod2.offset.z = mod1.offset.z;
        mod3.offset.z = mod2.offset.z;
        mod4.offset.z = mod3.offset.z;

        offset.x = mod1.offset.x;
        offset.z = mod3.offset.z + height + 5;
    }

    public void BorrarCubos()
    {
        foreach (var cubo in cubosInstanciados)
        {
            if (cubo != null)
                DestroyImmediate(cubo);
        }
        cubosInstanciados.Clear();
    }

    void ResetManual()
    {

        mod1.ReiniciarModificador();
        mod2.ReiniciarModificador();
        mod3.ReiniciarModificador();
        mod4.ReiniciarModificador();
        ReiniciarArreglos();

        puntajeMinimo = 0;
        cortafuego = 0;
        GenerarGrillaYCrearLaberinto();

        EstablecerAlturas();
        EstablecerStartGoal();
        EstablecerOffset();

        if (laberintoGeneral != null)
        {
            ClonarTodoAlLaberintoGeneral();
        }
    }

    public void ReiniciarArreglos()
    {
        laberintoGeneral = new int[width, height];
        laberintoReserva = new int[width, height];

        laberintoGeneral = null;
        laberintoReserva = null;
    }

}
