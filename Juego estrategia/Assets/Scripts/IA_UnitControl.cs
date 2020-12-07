using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_UnitControl : MonoBehaviour
{
    
    public Unit unit;
    public string nombreObjetivo;
    public bool esRey;
    public Tile objetivo;

    void Awake()
    {
        unit = GetComponent<Unit>();
        esRey = unit.isKing;
    }

    
}
