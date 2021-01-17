using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer rend;
    public Color highlightedColor;
    public Color creatableColor;

    public LayerMask obstacles;

    public bool isWalkable;
    public bool isCreatable;

    private GM gm;

    public float amount;
    private bool sizeIncrease;

	private AudioSource source;

    //Per als mapes d'influencia
    public InfluTile influTile;

    //Per a identificar-se en la matriu de tiles
    public int matrizX;
    public int matrizY;

    private void Awake()
    {
		source = GetComponent<AudioSource>();
        gm = FindObjectOfType<GM>();
        rend = GetComponent<SpriteRenderer>();
        influTile = GetComponent<InfluTile>();
    }

    public bool isClear() // para detectar a los gameObjects con layer Obstacle
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 0.2f, obstacles);
        if (col == null)
        {
            return true;
        }
        else {
            return false;
        }
    }

    //Cuando se selecciona una unidad para moverla, se resaltan los tiles a los que puede ir
    public void Highlight() {
		
        rend.color = highlightedColor;
        isWalkable = true;
    }

    public void Reset()
    {
        rend.color = Color.white;
        isWalkable = false;
        isCreatable = false;
    }

    public void SetCreatable() {

        //Sólo se cambia de color si lo maneja el jugador
        if(gm.playerTurn==1)
            rend.color = creatableColor;
        isCreatable = true;
    }

    //Se ha hecho publico este método para que gm acceda a él a la hora de comprar unidades de la IA
   public void OnMouseDown()
    {
        //Después de seleccionar el personaje y que resalten los tiles alcanzables,
        //pulsamos en uno de éstos para que vaya hacia él
        if (isWalkable == true) {
            gm.selectedUnit.tilePosicion = this;
            gm.selectedUnit.Move(this.transform);

            //Para cuando se quiera spawnear una unidad ya seleccionada del menú
        } else if (isCreatable == true && gm.createdUnit != null 
            && ((gm.createdUnit.playerNumber == 1 && gm.createdUnit.cost <= gm.player1Gold) 
            || (gm.createdUnit.playerNumber == 2 && gm.createdUnit.cost <= gm.player2Gold))) {

            Unit unit = Instantiate(gm.createdUnit, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            //Nada más añadirlo, se pinta el mapa de influencia
            //unit.PintarInfluencia(true);
            //Si es una unidad de la IA, se añade a la lista del IAPlayer
            unit.hasMoved = true;
            unit.hasAttacked = true;
            gm.ResetTiles();

            //Se realiza el pago por la creación de la unidad
            if(gm.createdUnit.playerNumber == 1)  gm.player1Gold -= gm.createdUnit.cost;
            else                                  gm.player2Gold -= gm.createdUnit.cost;

            gm.UpdateGoldText();

            //Comprando unidades para la IA:
            //Si aun queda suficiente dinero, se compra otra figura más
            if(gm.playerTurn==2 && gm.player2Gold >= gm.createdUnit.cost)
                gm.CrearUnidadIA();
            else
            {
                gm.createdUnit = null;
                if(gm.playerTurn==2)
                    gm.AcabarAccionUnidadIA();
            }


            
            //Para cua
        } else if (isCreatable == true && gm.createdVillage != null) {
            Instantiate(gm.createdVillage, new Vector3(transform.position.x, transform.position.y, 0) , Quaternion.identity);
            gm.ResetTiles();

            //Se realiza el pago por la creación de la unidad
            if(gm.createdUnit.playerNumber == 1)  gm.player1Gold -= gm.createdUnit.cost;
            else                                  gm.player2Gold -= gm.createdUnit.cost;

            gm.UpdateGoldText();

            //Comprando unidades para la IA:
            //Si aun queda suficiente dinero, se compra otra figura más
            if(gm.playerTurn==2 && gm.player2Gold >= gm.createdUnit.cost)
                gm.CrearAldeaIA();
            else
            {
                gm.createdVillage = null;
                if(gm.playerTurn==2)
                    gm.AcabarAccionUnidadIA();
            }
            
        }
    }


    private void OnMouseEnter()
    {
        //Aumentar tamaño del tile cuando el ratón esté encima
        if (isClear() == true) {
			source.Play();
			sizeIncrease = true;
            transform.localScale += new Vector3(amount, amount, amount);
        }
        
    }

    private void OnMouseExit()
    {
        //El tile vuelve a su tamaño cuando el ratón sale
        if (isClear() == true)
        {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }

        if (isClear() == false && sizeIncrease == true) {
            sizeIncrease = false;
            transform.localScale -= new Vector3(amount, amount, amount);
        }
    }
}
