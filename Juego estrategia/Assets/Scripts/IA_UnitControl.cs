using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_UnitControl : MonoBehaviour
{
    
    public Unit unit;
    public string nombreObjetivo;
    public bool esRey;
    public Tile tileObjetivo;

    public Tile casillaObjetivoTurno = null;
    public Unit enemigoObjetivoTurno = null;

    void Awake()
    {
        unit = GetComponent<Unit>();
        esRey = unit.isKing;
    }

    public string identifica(int num)
    {
        return unit.identifica(num);
    }

    
}
