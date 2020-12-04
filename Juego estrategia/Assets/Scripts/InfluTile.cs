using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluTile : MonoBehaviour
{
    // Start is called before the first frame update
    
    //Las influencias del jugador (Player Number: 1)
    public float jugRey;
    public float jugCaballero;
    public float jugArquero;
    public float jugDragon;
    public float jugAldea;

    //Las influencias de la IA  (Player Number: 1)
    public float iaRey;
    public float iaCaballero;
    public float iaArquero;
    public float iaDragon;
    public float iaAldea;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PintarInfluencia(Unit unidad, bool sumar)
    {
        float cantidad = unidad.tileSpeed / (Mathf.Abs(unidad.transform.position.x - transform.position.x) + Mathf.Abs(unidad.transform.position.y - transform.position.y));
        if (cantidad < 1)    return;

        //Sumar: true -> pintar influencia; false -> restar influencia
        if(!sumar)  cantidad = -cantidad;

        //Unidades del jugador
        if(unidad.playerNumber==1)
            switch(unidad.tipoUnidad)
            {
                case "rey":         jugRey += cantidad; break;
                case "caballero":   jugCaballero += cantidad; break;
                case "arquero":     jugArquero += cantidad; break;
                case "dragon":      jugDragon += cantidad; break;
            }

        else
            switch(unidad.tipoUnidad)
            {
                case "rey":         iaRey += cantidad; break;
                case "caballero":   iaCaballero += cantidad; break;
                case "arquero":     iaArquero += cantidad; break;
                case "dragon":      iaDragon += cantidad; break;
            }

    }
}
