using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreation : MonoBehaviour
{

    GM gm;

    public Button player1openButton;
    public Button player2openButton;

    public GameObject player1Menu;
    public GameObject player2Menu;

    //Para que la IA detecte que ya puede seleccionar tiles para crear
    //public bool tilesPreparados;


    private void Start()
    {
        //tilesPreparados = false;
        gm = FindObjectOfType<GM>();
    }

    private void Update()
    {
        if (gm.playerTurn == 1)
        {
            player1openButton.interactable = true;
            player2openButton.interactable = false;
        }
        else
        {
            player2openButton.interactable = true;
            player1openButton.interactable = false;
        }
    }

    public void ToggleMenu(GameObject menu) {
        menu.SetActive(!menu.activeSelf);
    }

    public void CloseCharacterCreationMenus() {
        player1Menu.SetActive(false);
        player2Menu.SetActive(false);
    }

    public void BuyUnit (Unit unit) {
        bool compra = false;
        if (unit.playerNumber == 1 && unit.cost <= gm.player1Gold)
        {
            player1Menu.SetActive(false);
            compra = true;
            //gm.player1Gold -= unit.cost;  //El pago se realizará después de colocar la unidad
        } else if (unit.playerNumber == 2 && unit.cost <= gm.player2Gold)
        {
            compra = true;
            player2Menu.SetActive(false);
            //gm.player2Gold -= unit.cost;
        } 
        if(compra)
        {
            //gm.UpdateGoldText();
            gm.createdUnit = unit;

            DeselectUnit();
            SetCreatableTiles("unidad");
        }
        else {
            print("NOT ENOUGH GOLD, SORRY!");
            if(gm.playerTurn==1)    return;
            else                    gm.AcabarAccionUnidadIA();
        }

        
    }

    public void BuyVillage(Village village) {
        bool compra = false;
        if (village.playerNumber == 1 && village.cost <= gm.player1Gold)
        {
            compra = true;
            player1Menu.SetActive(false);
            //gm.player1Gold -= village.cost;
        }
        else if (village.playerNumber == 2 && village.cost <= gm.player2Gold)
        {
            compra = true;
            player2Menu.SetActive(false);
            //gm.player2Gold -= village.cost;
        }
        
        if(compra)
        {
            //gm.UpdateGoldText();
        gm.createdVillage = village;

        DeselectUnit();

        SetCreatableTiles("aldea");
        }
        else
        {
            print("NOT ENOUGH GOLD, SORRY!");
            if(gm.playerTurn==1)    return;
            else                    gm.AcabarAccionUnidadIA();
        }
        
    }

    void SetCreatableTiles(string elemento) {
        gm.ResetTiles();

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            if (tile.isClear())
            {
                tile.SetCreatable();
            }
        }

        if(gm.playerTurn==2)
        {
            if(elemento=="unidad")      gm.CrearUnidadIA();
            else if(elemento=="aldea")  gm.CrearAldeaIA();
        }
    }

    void DeselectUnit() {
        if (gm.selectedUnit != null)
        {
            gm.selectedUnit.isSelected = false;
            gm.selectedUnit = null;
        }
    }




}
