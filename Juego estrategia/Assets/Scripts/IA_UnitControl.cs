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
    public Unit unidadObjetivoTurno = null;

    //Para utilizar el pathfinding
    public IA_UnitPathFind pathFind;
    private GM gm;


    void Awake()
    {
        unit = GetComponent<Unit>();
        pathFind = GetComponent<IA_UnitPathFind>();
        esRey = unit.isKing;

        gm = FindObjectOfType<GM>();
    }

    public string identifica(int num)
    {
        return unit.identifica(num);
    }

    public void CaminoPathFinding(bool estaAtacando)
    {
        if(!estaAtacando) //Se busca a un tile sin enemigo
        {
            pathFind.camino(casillaObjetivoTurno.transform, unit.tileSpeed, false, null);
        }
        else    // Se busca a un enemigo
        {
            pathFind.camino(null, unit.tileSpeed, true, unidadObjetivoTurno.gameObject);
        }
    }

    public void AcabarPathFinding()
    {
        unit.hasMoved = true;
        unit.PintarInfluencia(true);
        unit.ResetWeaponIcon();
        unit.GetEnemies(null);
        gm.MoveInfoPanel(unit);
        gm.AcabarAccionUnidadIA();
    }

    
}
