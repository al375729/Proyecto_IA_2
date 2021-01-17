using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM : MonoBehaviour
{
    public Unit selectedUnit;

    public int playerTurn = 1;

    public Transform selectedUnitSquare;


    private Animator camAnim;
    public Image playerIcon; 
    public Sprite playerOneIcon;
    public Sprite playerTwoIcon;

    public GameObject unitInfoPanel;
    public Vector2 unitInfoPanelShift;
    Unit currentInfoUnit;
    public Text heathInfo;
    public Text attackDamageInfo;
    public Text armorInfo;
    public Text defenseDamageInfo;

    public int player1Gold;
    public int player2Gold;

    public Text player1GoldText;
    public Text player2GoldText;

    public Unit createdUnit;
    public Village createdVillage;

    public GameObject blueVictory;
    public GameObject darkVictory;

	private AudioSource source;

    //Para las decisiones de la IA
    private IAPlayer iaPlayer;

    //Vector 2D para almacenar Tiles y acceder a ellos de manera
    //más eficiente
    public Tile[,] matrizTile;

    private void Awake()
    {
		source = GetComponent<AudioSource>();
        camAnim = Camera.main.GetComponent<Animator>();
        GetGoldIncome(1);

        iaPlayer = GetComponent<IAPlayer>();
        ObtenerTilesParaMatriz();
    
    }

    private void ObtenerTilesParaMatriz()
    {
        Tile[] arrayTile = FindObjectsOfType<Tile>();
        List<float> coordenadasX = new List<float>();
        List<float> coordenadasY = new List<float>();

        //Obtener todas las posibles coordenadas x e y de los tiles
        //para definir el número de filas y columnas de la matriz
        foreach(Tile tilo in arrayTile)
        {
            if(!coordenadasX.Contains(tilo.transform.position.x))
                coordenadasX.Add(tilo.transform.position.x);
            if(!coordenadasY.Contains(tilo.transform.position.y))
                coordenadasY.Add(tilo.transform.position.y);
        }

        //Sort para ordenar las listas recientemente creadas
        coordenadasX.Sort();
        coordenadasY.Sort();
        
        //Inicializar matriz y colocar Tiles en ella
        matrizTile = new Tile[coordenadasX.Count, coordenadasY.Count];
        foreach(Tile tilo in arrayTile)
        {
            //IndexOf para encontrar la primera posición del float deseado
            int i = coordenadasX.IndexOf(tilo.transform.position.x);
            int j = coordenadasY.IndexOf(tilo.transform.position.y);

            matrizTile[i,j] = tilo;
            //Para que el tile se pueda autoidentificar
            tilo.matrizX = i;
            tilo.matrizY = j;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("b")) {
            EndTurn();
        }

        if (selectedUnit != null) // moves the white square to the selected unit!
        {
            selectedUnitSquare.gameObject.SetActive(true);
            selectedUnitSquare.position = selectedUnit.transform.position;
        }
        else
        {
            selectedUnitSquare.gameObject.SetActive(false);
        }

    }

    // Sets panel active/inactive and moves it to the correct place
    public void UpdateInfoPanel(Unit unit) {

        if (unit.Equals(currentInfoUnit) == false)
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
            unitInfoPanel.SetActive(true);

            currentInfoUnit = unit;

            UpdateInfoStats();

        } else {
            unitInfoPanel.SetActive(false);
            currentInfoUnit = null;
        }

    }

    // Updates the stats of the infoPanel
    public void UpdateInfoStats() {
        if (currentInfoUnit != null)
        {
            attackDamageInfo.text = currentInfoUnit.attackDamage.ToString();
            defenseDamageInfo.text = currentInfoUnit.defenseDamage.ToString();
            armorInfo.text = currentInfoUnit.armor.ToString();
            heathInfo.text = currentInfoUnit.health.ToString();
        }
    }

    // Moves the udpate panel (if the panel is actived on a unit and that unit moves)
    public void MoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.transform.position = (Vector2)unit.transform.position + unitInfoPanelShift;
        }
    }

    // Deactivate info panel (when a unit dies)
    public void RemoveInfoPanel(Unit unit) {
        if (unit.Equals(currentInfoUnit))
        {
            unitInfoPanel.SetActive(false);
			currentInfoUnit = null;
        }
    }

    public void ResetTiles() {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    
    public void EndTurn() {
		source.Play();
        camAnim.SetTrigger("shake");

        // deselects the selected unit when the turn ends
        if (selectedUnit != null) {
            selectedUnit.ResetWeaponIcon();
            selectedUnit.isSelected = false;
            selectedUnit = null;
        }

        ResetTiles();

        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units) {
            unit.hasAttacked = false;
            unit.hasMoved = false;
            unit.ResetWeaponIcon();
        }

        if (playerTurn == 1) {
            playerIcon.sprite = playerTwoIcon;
            playerTurn = 2;

            iaPlayer.TurnoIA();
        } else if (playerTurn == 2) {
            playerIcon.sprite = playerOneIcon;
            playerTurn = 1;    
        }

        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;
    }

    //Para que la IA sólo pueda finalizar sus turnos y no los del jugador
    public void EndIATurn()
    {
        source.Play();
        camAnim.SetTrigger("shake");

        // deselects the selected unit when the turn ends
        if (selectedUnit != null) {
            selectedUnit.ResetWeaponIcon();
            selectedUnit.isSelected = false;
            selectedUnit = null;
        }

        ResetTiles();

        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units) {
            unit.hasAttacked = false;
            unit.hasMoved = false;
            unit.ResetWeaponIcon();
        }

        playerIcon.sprite = playerOneIcon;
        playerTurn = 1; 

        GetGoldIncome(playerTurn);
        GetComponent<CharacterCreation>().CloseCharacterCreationMenus();
        createdUnit = null;
    }

    //Si la unidad realiza una acción (Mover/Atacar) y es de la IA,
    //el gm notifica a IAPlayer que esa unidad debe continuar con otra acción
    public void AcabarAccionUnidadIA()
    {
        iaPlayer.cuentaOrden++;
        iaPlayer.SiguienteAccionUnidad();
    }

    public void AcabarAccionYMatarUnidadIA(GameObject unidad)
    {
        Destroy(unidad);
        iaPlayer.cuentaOrden++;
        iaPlayer.SiguienteAccionUnidad();
    }

    public void AcabarCreacionUnidad()
    {
        //FindObjectOfType<CharacterCreation>().tilesPreparados = false;
    }

    public void CrearUnidadIA()
    {
        if(createdUnit!=null)
        {
            Debug.Log("Se busca un tile donde spawnear la unidad comprada");
            int filas = matrizTile.GetLength(0);
            int columnas = matrizTile.GetLength(1);
            Tile tileRandom = matrizTile[0,0];  //Le asigno valor para que no de errores el código
            for(int i=0; i<=5; i++)
            {
                int randomColumna = Random.Range(0,columnas);
                int randomFila = Random.Range(0,filas);
                Debug.Log("Prueba de tile ["+randomFila+","+randomColumna+"]");
                tileRandom = matrizTile[randomFila,randomColumna];
                if(tileRandom.isCreatable==true)
                    break;

                /*else if(i==5)
                {
                    //Se cancela la compra para no bloquear el código en un bucle
                    Debug.Log("Se cancela la compra");
                    player2Gold += createdUnit.cost;
                    return;
                }*/

                
            }
            if(tileRandom.isCreatable==true)
                tileRandom.OnMouseDown();
            else
            {
                //Se cancela la compra para no bloquear el código en un bucle
                    Debug.Log("Se cancela la compra");
                    player2Gold += createdUnit.cost;
                    AcabarAccionUnidadIA();
            }
        }
    }

    public void CrearAldeaIA()
    {
        if(createdVillage!=null)
        {
            Debug.Log("Se busca un tile donde spawnear la unidad comprada");
            int filas = matrizTile.GetLength(0);
            int columnas = matrizTile.GetLength(1);
            Tile tileRandom = matrizTile[0,0];  //Le asigno valor para que no de errores el código
            for(int i=0; i<=5; i++)
            {
                Debug.Log("Prueba de tile "+i+1);
                tileRandom = matrizTile[Random.Range(0,filas),Random.Range(0,columnas)];
                if(tileRandom.isCreatable==true)
                    break;

                /*else if(i==5)
                {
                    //Se cancela la compra para no bloquear el código en un bucle
                    Debug.Log("Se cancela la compra");
                    player2Gold += createdUnit.cost;
                    return;
                }*/

                
            }
            if(tileRandom.isCreatable==true)
                tileRandom.OnMouseDown();
            else
            {
                //Se cancela la compra para no bloquear el código en un bucle
                    Debug.Log("Se cancela la compra");
                    player2Gold += createdVillage.cost;
                    AcabarAccionUnidadIA();
            }
        }
    }

    //Para realizar más eficientemente los cálculos de distancia entre unidades
    public float calculaDistancia(Unit unidad, Unit enemigo)
    {
        return Mathf.Abs(unidad.transform.position.x - enemigo.transform.position.x) 
        + Mathf.Abs(unidad.transform.position.y - enemigo.transform.position.y);
    }

    public float calculaDistancia(Unit unidad, Transform elemento)
    {
        return Mathf.Abs(unidad.transform.position.x - elemento.position.x) 
        + Mathf.Abs(unidad.transform.position.y - elemento.position.y);
    }

    public void OlvidarEnemigoMatado(Unit enemigo)
    {
        iaPlayer.OlvidarEnemigoMatado(enemigo);
    }

    void GetGoldIncome(int playerTurn) {
        foreach (Village village in FindObjectsOfType<Village>())
        {
            if (village.playerNumber == playerTurn)
            {
                if (playerTurn == 1)
                {
                    player1Gold += village.goldPerTurn;
                }
                else
                {
                    player2Gold += village.goldPerTurn;
                }
            }
        }
        UpdateGoldText();
    }

    public void UpdateGoldText()
    {
        player1GoldText.text = player1Gold.ToString();
        player2GoldText.text = player2Gold.ToString();
    }

    // Victory UI

    public void ShowVictoryPanel(int playerNumber) {

        if (playerNumber == 1)
        {
            blueVictory.SetActive(true);
        } else if (playerNumber == 2) {
            darkVictory.SetActive(true);
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



}
