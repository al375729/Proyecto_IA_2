using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPlayer : MonoBehaviour
{
    public GM gm;
    public string EstrategiaIA;

    IA_UnitControl[] todos;
    Unit jugadorRey;
    IA_UnitControl iaRey;

    public IA_UnitControl unidadActual;

    //Cantidad de influencia que tiene que recibir el rey de las unidades enemigas
    public float minSumaInfluenciaAliada = 3f;

    //Para las ordenes en SiguienteAccionUnidad
    //Posibles valores: "Mover","Atacar","Nada", "PathFindingHaciaTile", "PathFindingHaciaUnidad"
    private string[] ordenesUnidad = {"Nada","Nada","Nada"};
    

    private int cuentaUnidad;
    //Esta variable es pública porque el gm indica que una unidad ha
    //realizado una acción
    public int cuentaOrden;

    //Para controlar lo que se muestra por la consola
    int cuentaTurno;

    void Start()
    {
        gm = FindObjectOfType<GM>();
        EstrategiaIA = "Neutral";

        //Recoger todas las unidades en el inicio
        todos = FindObjectsOfType<IA_UnitControl>();
        //foreach(IA_UnitControl unidad in todos) 
        for(int i=0; i<todos.Length; i++)
        {
            //if(todos[i].unit.tipoUnidad=="rey" && todos[i].unit.playerNumber==1)
            //    jugadorRey = todos[i];
            if(todos[i].unit.tipoUnidad=="rey" && todos[i].unit.playerNumber==2)
                iaRey = todos[i];
        }
        //Encontrar al rey del jugador
        Unit [] listaUnidades = FindObjectsOfType<Unit>();
        for(int i=0; i<listaUnidades.Length; i++)
        {
            if(listaUnidades[i].tipoUnidad=="rey" && listaUnidades[i].playerNumber==1)
                jugadorRey = listaUnidades[i];
            
        }

        
        cuentaTurno = 0;
    }

    public void TurnoIA()
    {
        cuentaTurno++;
        Debug.Log("----------TURNO "+cuentaTurno+" DE LA IA ----------");
        //Recoger todas las unidades del juego y trabajar sólo con
        todos = FindObjectsOfType<IA_UnitControl>();
        cuentaUnidad = 0;
        SiguienteUnidad();
        /*for(int i = 0; i<todos.Length; i++)
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
            }*/


    }

    public void SiguienteUnidad()
    {
        if(cuentaUnidad>=todos.Length || todos[cuentaUnidad]==null)
        {
            Debug.Log("La IA ha acabado el turno "+cuentaTurno+", le toca al jugador");
            gm.EndTurn();
        }
        else
        {
            unidadActual = todos[cuentaUnidad];
            
            //Si no tiene una estrategia, asignarle una según el estado actual
            if(unidadActual.nombreObjetivo == "")
                DecidirEstrategiaUnidad("");

            //Se vacía el vector de ordenes
            ordenesUnidad[0] = "Nada";
            ordenesUnidad[1] = "Nada";
            cuentaOrden = 0;
            
            if(unidadActual == iaRey)
                EstrategiaRey();
            else
                EstrategiaUnidad();
        }

    }

    //Para mostrar información por la consola
    private void MuestraConsola(string info)
    {
        Debug.Log(  "Turno "+cuentaTurno+": "+ unidadActual.identifica(3)+"\n"+
                    info);
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
        MuestraConsola("Influencia del tile actual:"+influenciaActual);
        if(influenciaActual < minSumaInfluenciaAliada)
        {
            unidadActual.nombreObjetivo = "Quiero_Protegerme";
            //buscar tile alcanzable con mayor suma de influencia
            //(Posible de cambiar ir_a_tile por unidadActual.objetivo)
            Tile ir_a_tile = unidadActual.unit.MaximaInfluenciaAlcanzable();
            unidadActual.casillaObjetivoTurno = ir_a_tile; 
            //(Posible de cambiar cuando se implemente el PathFinding)
            ordenesUnidad[cuentaOrden++] = "Mover";
            //Si sigue sin ser suficiente, mantener "Quiero_Protegerme"
            influenciaActual = ir_a_tile.influTile.getSumaInfluenciaAliada();
            if(influenciaActual >= minSumaInfluenciaAliada)
                unidadActual.nombreObjetivo = "Estoy_Protegido";
        }

        MuestraConsola("Realizará: 1º: "+ordenesUnidad[0]+"; 2º: "+ordenesUnidad[1]);
        SiguienteAccionUnidad();
        
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
            MuestraConsola("Busca proteger al rey");
            if(iaRey.nombreObjetivo=="Estoy_Protegido")
            {
                if(unidadActual.nombreObjetivo=="Quiero_DefenderRey" || unidadActual.unit.tilePosicion.influTile.iaRey<1)
                {
                    MuestraConsola(" decide cambiar de estrategia");
                    DecidirEstrategiaUnidad("Descarta_Quiero_DefenderRey");
                    EstrategiaUnidad();
                }
                else
                {
                    //Comprobar si hay enemigos cerca
                    //Si los hay: moverse hacia el más cercano y atacarle
                    //Si no los hay: quedarse en el sitio y no moverse
                    Tile tilo = unidadActual.unit.ComprobarEnemigoAlcanzable();
                    if(tilo!=null)
                    {
                        MuestraConsola(" decide: Atacar enemigo cercano");
                        unidadActual.casillaObjetivoTurno = tilo;
                        ordenesUnidad[cuentaOrden++] = "Mover";
                        //Atacar enemigo
                        DecidirEnemigo("El mas cercano al rey aliado");
                    }
                    else
                        MuestraConsola(" decide: Quedarme quieto");
                }
            }
            else
            {
                //Si se recibe influencia del rey en un tile alcanzable (si ser Infinite) ir hacia él
                //Si no es alcanzable, PathFinding
                Tile tilo = unidadActual.unit.ComprobarReyAliadoAlcanzable();
                if(tilo!=null)
                {
                    MuestraConsola(" decide: Acercarse al rey");
                    unidadActual.casillaObjetivoTurno = tilo;
                    ordenesUnidad[0] = "Mover";
                    
                }
                else
                {
                    MuestraConsola(": Rey no alcanzable en este turno, avanzar con PathFinding");
                    //PathFinding hacia el rey
                    unidadActual.unidadObjetivoTurno = iaRey.unit;
                    ordenesUnidad[0] = "PathFindingHaciaUnidad";
                }
            }
        }

        else if(unidadActual.nombreObjetivo=="Quiero_AtacarEnemigos" || unidadActual.nombreObjetivo=="Estoy_AtacandoEnemigos")
        {
            MuestraConsola( "Busca atacar a enemigos cercanos");
            if(unidadActual.unidadObjetivoTurno==null )    
                DecidirEnemigo("");

            if(unidadActual.unidadObjetivoTurno!=null && 
            gm.calculaDistancia(unidadActual.unit, unidadActual.unidadObjetivoTurno) <= unidadActual.unit.attackRadius)
            {
                //unidadActual.unit.Attack(unidadActual.enemigoObjetivoTurno);
                ordenesUnidad[cuentaOrden++] = "Atacar";
            }
            else
            {
                Tile tilo = unidadActual.unit.ComprobarEnemigoAlcanzable();
                if(tilo!=null)
                {
                    unidadActual.casillaObjetivoTurno = tilo;
                    ordenesUnidad[cuentaOrden++] = "Mover";
                    DecidirEnemigo("");
                    ordenesUnidad[cuentaOrden++] = "Atacar";

                }
                else //Al no haber enemigos cercanos, decide ir a atacar al rey
                {
                    unidadActual.nombreObjetivo="Quiero_AtacarRey";
                    EstrategiaUnidad();
                }
            }
            

            
        }
        else if(unidadActual.nombreObjetivo=="Quiero_AtacarRey" || unidadActual.nombreObjetivo=="Estoy_AtacandoRey")
        {
            MuestraConsola( "Busca atacar al rey enemigo");
            if(unidadActual.unidadObjetivoTurno==jugadorRey && 
            gm.calculaDistancia(unidadActual.unit, unidadActual.unidadObjetivoTurno) <= unidadActual.unit.attackRadius)
            {
                //unidadActual.unit.Attack(unidadActual.enemigoObjetivoTurno);
                ordenesUnidad[cuentaOrden++] = "Atacar";
            }
            else
            {
                Tile tilo = unidadActual.unit.ComprobarEnemigoAlcanzable();
                if(tilo!=null)
                {
                    unidadActual.casillaObjetivoTurno = tilo;
                    ordenesUnidad[cuentaOrden++] = "Mover";
                    DecidirEnemigo("");
                    //ordenesUnidad[cuentaOrden++] = "Atacar";

                }
                else //Al no haber enemigos cercanos, decide ir a atacar al rey
                {
                    ordenesUnidad[cuentaOrden++] = "PathFindingHaciaUnidad";
                    unidadActual.unidadObjetivoTurno = jugadorRey;
                }
            }


            
        }

        MuestraConsola("Realizará: 1º: "+ordenesUnidad[0]+"; 2º: "+ordenesUnidad[1]);
        cuentaOrden = 0;
        SiguienteAccionUnidad();
    }

    void DecidirEnemigo(string info)
    {
        //Si ya tiene un enemigo definido y es alcanzable en el momento, continua como enemigo a atacar
        if(unidadActual.unidadObjetivoTurno!=null && 
        gm.calculaDistancia(unidadActual.unit, unidadActual.unidadObjetivoTurno) <= unidadActual.unit.attackRadius)
        {
            //unidadActual.unit.Attack(unidadActual.enemigoObjetivoTurno);
            //ordenesUnidad[cuentaOrden++] = "Atacar";
            return;
        }

        //Obtener lista de los enemigos alcanzables
        if(unidadActual.casillaObjetivoTurno!=null)
            unidadActual.unit.GetEnemies(unidadActual.casillaObjetivoTurno);
        else
            unidadActual.unit.GetEnemies(null);

        //Si la lista es vacía, no se registra orden de atacar
        if(unidadActual.unit.enemiesInRange.Count==0)   return;

        //Si se ordena atacar al enemigo más próximo al rey IA
        if(info=="El mas cercano al rey aliado")
        {
            Unit enemigoMasCercano = null;
            float distanciaEnemigoMasCercano = Mathf.Infinity;
            foreach(Unit enemigo in unidadActual.unit.enemiesInRange)
            {
                float distanciaEnemigo = gm.calculaDistancia(iaRey.unit,enemigo);
                if(enemigoMasCercano==null || distanciaEnemigo<distanciaEnemigoMasCercano)
                {
                    enemigoMasCercano = enemigo;
                    distanciaEnemigoMasCercano = distanciaEnemigo;
                }
            }

            ordenesUnidad[cuentaOrden++] = "Atacar";
            unidadActual.unidadObjetivoTurno = enemigoMasCercano;
        }

        else if(info == "El rey enemigo")
        {
            foreach(Unit enemigo in unidadActual.unit.enemiesInRange)
            {
                if(enemigo.tipoUnidad == "rey")
                {
                    ordenesUnidad[cuentaOrden++] = "Atacar";
                    unidadActual.unidadObjetivoTurno = enemigo;
                    break;
                }
            }
        }

        //En caso de que se quiera atacar al enemigo más cercano a la unidad
        else
        {
            Unit enemigoMasCercano = null;
            float distanciaEnemigoMasCercano = Mathf.Infinity;
            foreach(Unit enemigo in unidadActual.unit.enemiesInRange)
            {
                float distanciaEnemigo = gm.calculaDistancia(unidadActual.unit,enemigo);
                if(enemigoMasCercano==null || distanciaEnemigo<distanciaEnemigoMasCercano)
                {
                    enemigoMasCercano = enemigo;
                    distanciaEnemigoMasCercano = distanciaEnemigo;
                }
            }

            //ordenesUnidad[cuentaOrden++] = "Atacar";
            unidadActual.unidadObjetivoTurno = enemigoMasCercano;
        }
        MuestraConsola(" elige como enemigo a"+unidadActual.unidadObjetivoTurno.identifica(3));
    }

    //Cuando un enemigo ha muerto y una o más unidades lo señalaban como objetivo, se ha de borrar
    public void OlvidarEnemigoMatado(Unit enemigo)
    {
        foreach(IA_UnitControl unidad in todos)
        {
            if(unidad.unidadObjetivoTurno == enemigo)
                unidad.unidadObjetivoTurno = null;
        }
    }

    // Para_presentar parte 3: el orden de las acciones
    public void SiguienteAccionUnidad()
    {
        if(cuentaOrden>=2 || ordenesUnidad[cuentaOrden]=="Nada")
        {
            ++cuentaUnidad;
            SiguienteUnidad();
        }
        else if(ordenesUnidad[cuentaOrden]=="Mover")
        {
            if(!unidadActual.unit.hasMoved && unidadActual.casillaObjetivoTurno!=null)
            {
                unidadActual.unit.tilePosicion=unidadActual.casillaObjetivoTurno;
                unidadActual.unit.Move(unidadActual.casillaObjetivoTurno.transform);
            }
            else 
            {
                cuentaOrden++;
                SiguienteAccionUnidad();
            }
        }
        else if(ordenesUnidad[cuentaOrden]=="PathFindingHaciaTile" || ordenesUnidad[cuentaOrden]=="PathFindingHaciaUnidad")
        {
            if(!unidadActual.unit.hasMoved && unidadActual.casillaObjetivoTurno!=null)
            {
                //unidadActual.unit.tilePosicion=unidadActual.casillaObjetivoTurno;
                //unidadActual.unit.Move(unidadActual.casillaObjetivoTurno.transform);
                if(ordenesUnidad[cuentaOrden]=="PathFindingHaciaTile")
                {
                    unidadActual.CaminoPathFinding(false);
                }
                else
                {
                    unidadActual.CaminoPathFinding(true);
                }
            }
            else 
            {
                cuentaOrden++;
                SiguienteAccionUnidad();
            }
        }
        else if(ordenesUnidad[cuentaOrden]=="Atacar")
        {
            if(!unidadActual.unit.hasAttacked && unidadActual.unidadObjetivoTurno!=null)
            {
                unidadActual.unit.Attack(unidadActual.unidadObjetivoTurno);
            }
            else 
            {
                cuentaOrden++;
                SiguienteAccionUnidad();
            }
        }


    }

    private void DecidirEstrategiaUnidad(string info)
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
                MuestraConsola("Fijar nueva estrategia");

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
                        if(info=="Descarta_Quiero_DefenderRey")
                            distanciaReyIa = Mathf.Infinity;
                        else
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

                MuestraConsola("Nueva estrategia: "+unidadActual.nombreObjetivo);
            }
            else
                unidadActual.nombreObjetivo = "Quiero_Protegerme";

    }
}
