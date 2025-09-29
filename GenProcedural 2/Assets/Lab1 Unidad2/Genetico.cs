using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Genetico : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int padres = 10; // Número de padres
    public int hijos = 20; // Número de hijos
    public int generations = 50;

    // 0: pasto, 1: tierra, 2: cerro
    public GameObject[] terrainPrefabs; // Asigna los prefabs en el inspector

    public float probPasto = 0.33f;
    public float probTierra = 0.33f;
    public float probCerro = 0.34f;

    List<int[,]> population = new List<int[,]>();
    List<GameObject> cubosInstanciados = new List<GameObject>();

    void Start()
    {
        InicializarPoblacion();
        StartCoroutine(RegenerarTerrenoCada3Segundos());
    }

    IEnumerator RegenerarTerrenoCada3Segundos()
    {
        while (true)
        {
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
            int[,] individuo = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    individuo[x, y] = TipoAleatorioPorProbabilidad();
            population.Add(individuo);
        }
    }

    int[,] SeleccionarPadre()
    {
        return population[Random.Range(0, population.Count)];
    }

    int[,] Mutar(int[,] padre)
    {
        int[,] hijo = padre.Clone() as int[,];
        for (int i = 0; i < 3; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            hijo[x, y] = TipoAleatorioPorProbabilidad();
        }
        return hijo;
    }

    List<int[,]> SeleccionarMejores(List<int[,]> poblacion, int cantidad)
    {
        poblacion.Sort((a, b) => Fitness(b).CompareTo(Fitness(a)));
        return poblacion.GetRange(0, cantidad);
    }

    int Fitness(int[,] individuo)
    {
        int pasto = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (individuo[x, y] == 0) pasto++;
        return pasto;
    }

    void DibujarTerreno(int[,] terreno)
    {
        // Elimina los cubos anteriores
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

    int TipoAleatorioPorProbabilidad()
    {
        // Normaliza las probabilidades
        float total = probPasto + probTierra + probCerro;
        float pPasto = probPasto / total;
        float pTierra = probTierra / total;
        float pCerro = probCerro / total;

        float r = Random.value;
        if (pPasto > 0 && r < pPasto)
            return 0; // pasto
        else if (pTierra > 0 && r < pPasto + pTierra)
            return 1; // tierra
        else
            return 2; // cerro
    }
}
