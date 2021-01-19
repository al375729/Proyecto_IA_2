using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{

    public int goldPerTurn;
    public int playerNumber;
    public int cost;

    private GM gm;

    //Variable por la cual se pintará la influencia
    public int factorInfluencia = 3;
    public int health;

    public Tile tilePosicion;

    private void Start()
    {
        gm = FindObjectOfType<GM>();

        //Encontrar Tile sobre el que está
        if(tilePosicion == null)
        {
            Tile [] todos = FindObjectsOfType<Tile>();
            foreach(Tile tilo in todos)
            {
                if(tilo.transform.position.x == transform.position.x && tilo.transform.position.y == transform.position.y)
                {
                    tilePosicion = tilo;
                    break;
                }
            }
        }

        //Pintar influencias iniciales
        PintarInfluencia(true);
    }

    public void PintarInfluencia(bool signo)
    {   
        //Debug.Log(tipoUnidad +" de jugador "+playerNumber+" va a Pintar Influencia");
        /*
        InfluTile[] tiles = FindObjectsOfType<InfluTile>();
        foreach (InfluTile tile in tiles) 
            tile.PintarInfluencia(this, signo);
        */

        //Obtindre quantitat parcial de la matriu de tiles del GM
        //segons el tile on s'està i el tileSpeed
        
        int iniX = tilePosicion.matrizX - factorInfluencia;
        int finX = tilePosicion.matrizX + factorInfluencia;
        int iniY = tilePosicion.matrizY - factorInfluencia;
        int finY = tilePosicion.matrizY + factorInfluencia;

        for(int i = iniX; i <= finX; i++)
            for(int j = iniY; j <= finY; j++)
                //if(gm.matrizTile[i,j]!=null)  
                if(i>=0 && i<gm.matrizTile.GetLength(0) && j>=0 && j<gm.matrizTile.GetLength(1))  
                    gm.matrizTile[i,j].influTile.PintarInfluencia(this,signo);

    }

    public void AldeaDestruida()
    {
        PintarInfluencia(false);
        Destroy(gameObject);
    }
    
}


