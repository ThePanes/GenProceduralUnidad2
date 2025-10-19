using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Modificador : MonoBehaviour
{
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
    public int[,] laberintoDeReservaModificador;

    public float puntuacionDeEvaluacion;

    private float cortafuego;

    public bool laberintoAnalizado;


    void OnValidate()
    {
        // Limita start y goal a estar dentro del rango permitido
        start.x = Mathf.Clamp(start.x, 1, width - 2);
        start.y = Mathf.Clamp(start.y, 1, height - 2);
        goal.x = Mathf.Clamp(goal.x, 1, width - 2);
        goal.y = Mathf.Clamp(goal.y, 1, height - 2);
    }

    IEnumerator Start()
    {

        yield return new WaitForSeconds(2f);///retrazo de 2 seg



        yield return null; 
        cortafuego = 0;
        StartCoroutine(RegenerarLaberintoCada3Segundos());
    }

    IEnumerator RegenerarLaberintoCada3Segundos()
    {
        do
        {
            GenerarYLlenarLaberinto();
            yield return new WaitForSeconds(0.2f);
        } while (true);
    }

    private void GenerarYLlenarLaberinto()
    {
        BorrarCubos();

        int[,] laberinto;
        List<Vector2Int> camino;

        if(laberintoGeneral == null)
        {
            laberintoGeneral = ClonLaberinto(laberintoDeReservaModificador);

            do
            {
                RemplazoManualLaberinto(laberintoGeneral, laberintoDeReservaModificador);

                laberinto = CrearVecino(laberintoGeneral);
                camino = EncontrarCamino(laberinto, start.x, start.y, goal.x, goal.y);

                
        

                cortafuego++;
                if (camino != null)
                {
                    cortafuego = 0;
                }


            } while (camino == null && cortafuego <= 50);
        }
        else
        {
            // Intenta hasta que haya un camino
            do
            {
                RemplazoManualLaberinto(laberintoGeneral, laberintoDeReservaModificador);

                laberinto = CrearVecino(laberintoGeneral);
                camino = EncontrarCamino(laberinto, start.x, start.y, goal.x, goal.y);

                cortafuego++;
                if (camino != null)
                {
                    cortafuego = 0;
                }

            } while (camino == null && cortafuego <= 100);
        }



           



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

            RemplazoManualLaberinto(laberintoDeReservaModificador, laberintoGeneral);
        }
        else
        {
            Debug.Log("camino nulo");
        }

        puntuacionDeEvaluacion = EvaluarLaberinto(laberinto);
        Debug.Log("Puntuación total: " + puntuacionDeEvaluacion.ToString("F10"));
        laberintoAnalizado = true;
    }

    ///------------------------------------------------------------------------------------------------------------------------


    void RemplazoManualLaberinto(int[,] laberintoARemplazar, int[,] laberintoBase)
    {

        if (laberintoARemplazar == null)
        {
            Debug.Log("fallo laberintoRemplazar");
        }
        else if (laberintoBase == null)
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


    int[,] CrearVecino(int[,] laberinto)
    {
        int randomWidth = Random.Range(1,width-1);
        int randomHeight = Random.Range(1, height - 1);

  
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (laberinto[x, y] == 1)//limpia camino
                {
                    laberinto[x, y] = 0;
                }
            }


        if(laberinto[start.x, start.y] != 0)
        {
            laberinto[start.x, start.y] = 0;
        }
        if(laberinto[goal.x, goal.y] != 0)
        {
            laberinto[goal.x, goal.y] = 0;
        }

        //cambio del bloque random
        if (laberinto[randomWidth, randomHeight] == 0)
            laberinto[randomWidth, randomHeight] = 2;
        else
        {
            laberinto[randomWidth, randomHeight] = 0;
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

    float EvaluarLaberinto(int[,] laberinto)
    {
        int bloquesRecorribles = ContarBloquesRecorribles(laberinto, start.x, start.y);
        int largoCamino = EvaluarLargoCamimno(laberinto, start.x, start.y, goal.x, goal.y);
        
        Vector2Int bloquesLaberinto = BloquesSinMuroYMuroLaberinto(laberinto);
        int noMurosLab = bloquesLaberinto.x;
        int MurosLab = bloquesLaberinto.y;
        int totalBloques = MurosLab + noMurosLab;

        int bloquesNoAccesibles = noMurosLab - bloquesRecorribles;

        // --- Equilibrio 70/30 ---
        float porcentajeNoMuros = (float)noMurosLab / totalBloques;
        float diferenciaNoMuros = Mathf.Abs(0.5f - porcentajeNoMuros);
        float equilibrio = 1f - diferenciaNoMuros; // más simple, más directo

        // --- Métricas ---
        float proporcionRecorrible = (noMurosLab > 0) ? (float)bloquesRecorribles / noMurosLab : 0f;
        float proporcionCamino = (totalBloques > 0) ? (float)largoCamino / totalBloques : 0f;
        float proporcionNoAccesibles = (noMurosLab > 0) ? (float)bloquesNoAccesibles / noMurosLab : 0f;

        // --- Ponderaciones ---
        float puntuacion =
            (equilibrio * 0.5f) +            // 70/30 balance
            (proporcionRecorrible * 0.2f) +  // accesibilidad general
            (proporcionCamino * 0.15f) -     // camino largo = bueno
            (proporcionNoAccesibles * 0.15f); // castigo suave



        Debug.Log("equilibrio" + equilibrio);
        Debug.Log("proporcionRecorrible" + proporcionRecorrible);
        Debug.Log("proporcionCamino" + proporcionCamino);
        Debug.Log("proporcionNoAccesibles" + proporcionNoAccesibles);

        return puntuacion;
    }

    int EvaluarLargoCamimno(int[,] laberinto, int startX, int startY, int endX, int endY)//contador de camino
    {
        var camino = EncontrarCamino(laberinto, startX, startY, endX, endY);

        if (camino == null)
            return 0;

        return camino.Count;
    }

    Vector2Int BloquesSinMuroYMuroLaberinto(int[,] laberinto)
    {

        int BloquesNoMuro = 0;
        int BloquesMuro = 0;

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (laberinto[x, y] == 0 || laberinto[x, y] == 1)
                {
                    BloquesNoMuro++;
                }
                else if (laberinto[x, y] == 2)
                {
                    BloquesMuro++;
                }
            }

        return new Vector2Int(BloquesNoMuro, BloquesMuro);
    }


    int ContarBloquesRecorribles(int[,] laberinto, int startX, int startY)//busqueda amplpia simple para contar camino
    {
        int w = laberinto.GetLength(0);
        int h = laberinto.GetLength(1);

        bool[,] visitado = new bool[w, h];
        Queue<Vector2Int> cola = new Queue<Vector2Int>();

        cola.Enqueue(new Vector2Int(startX, startY));
        visitado[startX, startY] = true;

        int recorribles = 0;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            recorribles++;

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = actual.x + dx[dir];
                int ny = actual.y + dy[dir];

                if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                {
                    if (!visitado[nx, ny] && laberinto[nx, ny] != 2)
                    {
                        visitado[nx, ny] = true;
                        cola.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return recorribles;
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


    public void ReiniciarModificador()
    {
        StopAllCoroutines(); 
        BorrarCubos();
        ReiniciarArreglos();
        laberintoAnalizado = false;
        cortafuego = 0;
        puntuacionDeEvaluacion = 0f;
        StartCoroutine(Start());
    }

    public void ReiniciarArreglos()
    {
        laberintoGeneral = new int[width, height];
        laberintoDeReservaModificador = new int[width, height];
    }


}
