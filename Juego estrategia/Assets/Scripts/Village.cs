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
    public int resistance;  //Equivalente a armor en Unit

    public Tile tilePosicion;

    //Para que sean atacables
    public GameObject weaponIcon;
    public DamageIcon damageIcon;

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

    private void OnMouseDown() //Seleccionar aldea para atacarla
    {
        //Sólo se atacan si hay una unidad seleccionada, es del bando contrario y la tiene a su alcance de ataque
        /*Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
        if (col != null)
        {
            // Obtener enemigo al que atacar y comprobar si es atacable por la unidad seleccionada
            Village aldea = col.GetComponent<Village>(); // double check that what we clicked on is a unit
            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
                { // does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);

                }
            }
        }*/
        ResetWeaponIcon();
        //Sólo se atacan si hay una unidad seleccionada, es del bando contrario y la tiene a su alcance de ataque
        if(gm.selectedUnit!=null && gm.selectedUnit.enemyVillagesInRange.Contains(this) && !gm.selectedUnit.hasAttacked)
        {
            gm.selectedUnit.Attack(this);
        }

    }

    public void AldeaDestruida()
    {
        PintarInfluencia(false);
        Destroy(gameObject);
    }

    public void ResetWeaponIcon() {
        //Se resetean los iconos de enemigos y aldeas
        Village[] aldeas = FindObjectsOfType<Village>();
        foreach (Village aldea in aldeas)
        {
            aldea.weaponIcon.SetActive(false);
        }

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
    }
    
}


