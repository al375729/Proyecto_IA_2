using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testGrid : MonoBehaviour
{
    public Tile[,] matrizTile;
    // Start is called before the first frame update
    public Unidad unidad;
    public Transform target;
    public GameObject targetGO;
    void Awake()
    {
        ObtenerTilesParaMatriz();
    }

    // Esta funcion no hace nada mas que la parte del GM de poner valor a los tiles y usarse como debug SI LE DAS A ARRIBA SE LLAMARA A LA FUNCION MOVERSE
    //En el juego deberá llamarse a esta funcion y proporcionarle los argumentos necesarios dependiedno del tipo de movimiento (EXPLICADO EN SCRIPT UNIDAD)
    void Update()
    {
        if (Input.GetKey("up"))
        {
            unidad.camino(target, 4,true,targetGO);
        }
    }
    private void ObtenerTilesParaMatriz()
    {
        Tile[] arrayTile = FindObjectsOfType<Tile>();
        List<float> coordenadasX = new List<float>();
        List<float> coordenadasY = new List<float>();

        //Obtener todas las posibles coordenadas x e y de los tiles
        //para definir el número de filas y columnas de la matriz
        foreach (Tile tilo in arrayTile)
        {
            if (!coordenadasX.Contains(tilo.transform.position.x))
                coordenadasX.Add(tilo.transform.position.x);
            if (!coordenadasY.Contains(tilo.transform.position.y))
                coordenadasY.Add(tilo.transform.position.y);
        }

        //Sort para ordenar las listas recientemente creadas
        coordenadasX.Sort();
        coordenadasY.Sort();

        //Inicializar matriz y colocar Tiles en ella
        matrizTile = new Tile[coordenadasX.Count, coordenadasY.Count];
        foreach (Tile tilo in arrayTile)
        {
            //IndexOf para encontrar la primera posición del float deseado
            int i = coordenadasX.IndexOf(tilo.transform.position.x);
            int j = coordenadasY.IndexOf(tilo.transform.position.y);

            matrizTile[i, j] = tilo;
            //Para que el tile se pueda autoidentificar
            tilo.matrizX = i;
            tilo.matrizY = j;
        }
    }
    
}
