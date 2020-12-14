using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPlayer : MonoBehaviour
{
    
    public string EstrategiaIA;

    IA_UnitControl[] todos;
    IA_UnitControl posJugRey;
    IA_UnitControl posIaRey;

    public IA_UnitControl unidadActual;

    //Cantidad de influencia que tiene que recibir el rey de las unidades enemigas
    public float minSumaInfluenciaAliada = 3f;

    void Start()
    {
        EstrategiaIA = "Neutral";

        //Recoger todas las unidades en el inicio
        todos = FindObjectsOfType<IA_UnitControl>();
        //foreach(IA_UnitControl unidad in todos) 
        for(int i=0; i<todos.Length; i++)
        {
            if(todos[i].unit.tipoUnidad=="rey" && todos[i].unit.playerNumber==1)
                posJugRey = todos[i];
            else if(todos[i].unit.tipoUnidad=="rey" && todos[i].unit.playerNumber==2)
                posIaRey = todos[i];
        }
        //Falta que se recoja cada unidad spawneada durante la partida
    }

    public void TurnoIA()
    {
        Debug.Log("Turno de la IA");
        //Recoger todas las unidades del juego y trabajar sólo con
        todos = FindObjectsOfType<IA_UnitControl>();
        for(int i = 0; i<todos.Length; i++)
            if(todos[i]!=null)
            {
                unidadActual = todos[i];
                //Si no tiene una estrategia, asignarle una según el estado actual
                if(unidadActual.nombreObjetivo == "")
                    DecidirEstrategiaUnidad();
                
                if(unidadActual == posIaRey)
                    EstrategiaRey();
                else
                    EstrategiaUnidad();
            }


    }

    public void EstrategiaRey()
    {
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
        
        //Comprobar la suma de influencias de unidades aliadas que recibe 
        float influenciaActual = unidadActual.unit.tilePosicion.influTile.getSumaInfluenciaAliada();
        Debug.Log(  "Rey IA\n"+
                    "Influencia del tile actual:"+influenciaActual);
        if(influenciaActual < minSumaInfluenciaAliada)
        {
            unidadActual.nombreObjetivo = "Quiero_Protegerme";
            //buscar tile alcanzable con mayor suma de influencia
            //(Posible de cambiar ir_a_tile por unidadActual.objetivo)
            Tile ir_a_tile = unidadActual.unit.MaximaInfluenciaAlcanzable(); 
            //(Posible de cambiar cuando se implemente el PathFinding)
            unidadActual.unit.Move(ir_a_tile.transform);
            //Si sigue sin ser suficiente, mantener "Quiero_Protegerme"
            influenciaActual = ir_a_tile.influTile.getSumaInfluenciaAliada();
            if(influenciaActual >= minSumaInfluenciaAliada)
                unidadActual.nombreObjetivo = "Estoy_Protegido";
        }
        
    }

    public void EstrategiaUnidad()
{
        // Caso: Quiero_DefenderRey y Estoy_DefendiendoRey"
        //      Comprobar si el rey está en "Estoy_Protegido"
        //          Si lo está: comprobar si estamos cerca o lejos del rey
        //              Si estamos lejos: decidir otra estrategia y moverse según esta
        //              Si estamos cerca: mantener o cambiar a "Estoy_DefendiendoRey" 
        //              y no moverse a no ser que haya un enemigo cerca del rey
        //          Si el rey está en "Quiero_Protegerme": avanzar para situarse cerca del rey
        if(unidadActual.nombreObjetivo=="Quiero_DefenderRey" || unidadActual.nombreObjetivo=="Estoy_DefendiendoRey")
        {
            Debug.Log(  unidadActual.unit.tipoUnidad+"\n"+
                        "Busca proteger al rey");
            if(posIaRey.nombreObjetivo=="Estoy_Protegido")
            {
                if(unidadActual.unit.tilePosicion.influTile.iaRey<1)
                {
                    Debug.Log(unidadActual.unit.tipoUnidad+" decide cambiar de estrategia");
                    DecidirEstrategiaUnidad();
                }
                else
                {
                    //Comprobar si hay enemigos cerca
                    //Si los hay: moverse hacia el más cercano y atacarle
                    //Si no los hay: quedarse en el sitio y no moverse
                    Tile tilo = unidadActual.unit.ComprobarEnemigoAlcanzable();
                    if(tilo!=null)
                    {
                        Debug.Log("Decision del "+unidadActual.unit.tipoUnidad+": Atacar enemigo cercano");
                        unidadActual.unit.Move(tilo.transform);
                        //Atacar enemigo
                    }
                    else
                        Debug.Log("Decision del "+unidadActual.unit.tipoUnidad+": Quedarme quieto");
                }
            }
            else
            {
                //Si se recibe influencia del rey en un tile alcanzable (si ser Infinite) ir hacia él
                //Si no es alcanzable, PathFinding
                Tile tilo = unidadActual.unit.ComprobarEnemigoAlcanzable();
                if(tilo!=null)
                {
                    Debug.Log("Decision del "+unidadActual.unit.tipoUnidad+": Acercarse al rey");
                    unidadActual.unit.Move(tilo.transform);
                    
                }
                else
                {
                    Debug.Log("Rey no alcanzable en este turno");
                    //PathFinding hacia el rey
                }
            }
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
                Debug.Log("Fijar nueva estrategia para "+unidadActual.unit.tipoUnidad);

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

                Debug.Log("Estrategia para "+unidadActual.unit.tipoUnidad+": "+unidadActual.nombreObjetivo);
            }
            else
                unidadActual.nombreObjetivo = "Quiero_Protegerme";

    }
}
