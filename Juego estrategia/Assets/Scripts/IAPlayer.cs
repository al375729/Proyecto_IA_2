using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPlayer : MonoBehaviour
{
    
    public string EstrategiaIA;

    List<IA_UnitControl> unidades;

    public IA_UnitControl unidadActual;

    //Cantidad de influencia que tiene que recibir el rey de las unidades enemigas
    public float minSumaInfluenciaAliada = 3f;

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
        {
            unidadActual = unidad;
            EstrategiaUnidad();
        }


    }

    public void EstrategiaUnidad()
    {
        //Si no tiene una estrategia, asignarle una según el estado actual
        if(unidadActual.nombreObjetivo == "")
            DecidirEstrategiaUnidad();

        //Caso de la unidad rey: "Quiero_Protegerme"
        //      Comprobar la suma de influencias de unidades aliadas que recibe 
        //      el tile sobre el cual está
        //      Si no es suficiente: 
        //          buscar tile alcanzable con mayor suma de influencia
        //          Si sigue sin ser suficiente, mantener "Quiero_Protegerme"
        //      Si es suficiente:
        //          cambiar nombreObjetivo a "Estoy_Protegido"
        //
        //Caso de la unidad rey: "Estoy_Protegido"
        //      Comprobar la suma de influencias de unidades aliadas que recibe 
        //      el tile sobre el cual está
        //      Si no es suficiente:
        //          Cambiar nombreObjetivo a "Quiero_Protegerme" y moverte como tal
        //      Si es suficiente:
        //          Si hay un enemigo atacándote: ataca
        //          No moverse del sitio
        
        //if(unidadActual.nombreObjetivo == "Quiero_Protegerme")
        if(unidadActual.unit.tipoUnidad =="rey")
        {
            //Comprobar la suma de influencias de unidades aliadas que recibe 
            float influenciaActual = unidadActual.unit.tilePosicion.influTile.getSumaInfluenciaAliada();
            if(influenciaActual < minSumaInfluenciaAliada)
            {
                unidadActual.nombreObjetivo = "Quiero_Protegerme";
                //buscar tile alcanzable con mayor suma de influencia
                Tile ir_a_tile = unidadActual.unit.MaximaInfluenciaAlcanzable(); 
                //(Posible de cambiar cuando se implemente el PathFinding)
                unidadActual.unit.Move(ir_a_tile.transform);
                //Si sigue sin ser suficiente, mantener "Quiero_Protegerme"
                influenciaActual = ir_a_tile.influTile.getSumaInfluenciaAliada();
                if(influenciaActual >= minSumaInfluenciaAliada)
                    unidadActual.nombreObjetivo = "Estoy_Protegido";
            }
        }
        else
        {
            
        }
    }

    private void DecidirEstrategiaUnidad()
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


            if(!unidadActual.esRey)
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
                    distancia = Mathf.Abs(unidadActual.transform.position.x - este.transform.position.x) 
                        + Mathf.Abs(unidadActual.transform.position.y - este.transform.position.y);
                    //Distancia hasta rey jugador
                    if(este.playerNumber == 2 && este.isKing)
                        distanciaReyIa = distancia;

                    else if(este.playerNumber ==1 && este.isKing)
                        distanciaReyJug = distancia;

                    else if(este.playerNumber ==1 && distancia < distanciaEnemigoMasCercano)
                            distanciaEnemigoMasCercano = distancia;
                    
                }

                float maxDistancia = Mathf.Min(distanciaReyIa, Mathf.Min(distanciaEnemigoMasCercano,distanciaReyJug));

                if(maxDistancia == distanciaReyIa)          unidadActual.nombreObjetivo = "Quiero_DefenderRey";
                else if(maxDistancia == distanciaReyJug)    unidadActual.nombreObjetivo = "Quiero_AtacarRey";
                else                                        unidadActual.nombreObjetivo = "Quiero_AtacarEnemigos";
            }
            else
                unidadActual.nombreObjetivo = "Quiero_Protegerme";

    }
}
