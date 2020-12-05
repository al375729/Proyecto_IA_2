using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPlayer : MonoBehaviour
{
    
    public string EstrategiaIA;

    List<IA_UnitControl> unidades;

    void Start()
    {
        EstrategiaIA = "Neutral";

        //Recoger todas las unidades en el inicio
        unidades = new List<IA_UnitControl>();
        IA_UnitControl[] todos = FindObjectsOfType<IA_UnitControl>();
        foreach(IA_UnitControl unidad in todos) 
            unidades.Add(unidad);
        //Falta que se recoja cada unidad spawneada durante la partida
    }

    public void TurnoIA()
    {
        Debug.Log("Turno de la IA");
        //Recoger todas las unidades del juego y trabajar sólo con
        foreach(IA_UnitControl unidad in unidades)
            EstrategiaUnidad(unidad);


    }

    public void EstrategiaUnidad(IA_UnitControl unidad)
    {
        //Si no tiene una estrategia, asignarle una según el estado actual
        if(unidad.nombreObjetivo == "")
        {
            //EstrategiaIA por defecto: Neutral
            // Quiero_Protegerme
            //      Decidir si: la unidad es el rey IA
            // Quiero_DefenderRey
            //      Decidir si: el rey es el elemento más cercano y 
            //      nombreObjetivo del rey == "Quiero_Protegerme" o null
            // Quiero_AtacarEnemigos
            //      Decidir si: un enemigo (Caballero, Arquero, Dragón, Aldea)
            //      es el elemento más cercano
            // Quiero_AtacarRey
            //      Decidir si: el rey enemigo es el elemento más cercano


            if(!unidad.esRey)
            {
                Debug.Log("Fijar nueva estrategia");

                float distanciaReyIa = float.PositiveInfinity;
                float distanciaEnemigoMasCercano = float.PositiveInfinity;
                float distanciaReyJug = float.PositiveInfinity;

                float distancia;

                //Recoger todas las unidades (Ia y Jugador) para recoger datos
                Unit[] todos = FindObjectsOfType<Unit>();

                foreach(Unit este in todos)
                {
                    distancia = Mathf.Abs(unidad.transform.position.x - este.transform.position.x) 
                        + Mathf.Abs(unidad.transform.position.y - este.transform.position.y);
                    //Distancia hasta rey jugador
                    if(este.playerNumber == 2 && este.isKing)
                        distanciaReyIa = distancia;

                    else if(este.playerNumber ==1 && este.isKing)
                        distanciaReyJug = distancia;

                    else if(este.playerNumber ==1 && distancia < distanciaEnemigoMasCercano)
                            distanciaEnemigoMasCercano = distancia;
                    
                }

                float maxDistancia = Mathf.Min(distanciaReyIa, Mathf.Min(distanciaEnemigoMasCercano,distanciaReyJug));

                if(maxDistancia == distanciaReyIa)          unidad.nombreObjetivo = "Quiero_DefenderRey";
                else if(maxDistancia == distanciaReyJug)    unidad.nombreObjetivo = "Quiero_AtacarRey";
                else                                        unidad.nombreObjetivo = "Quiero_AtacarEnemigos";
            }
            else
                unidad.nombreObjetivo = "Quiero_Protegerme";

            
        }


    }
}
