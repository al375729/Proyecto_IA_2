using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public bool isSelected;
    //Detectar si la unidad se ha movido en este turno
    public bool hasMoved;       

    // Distancia de tiles que puede caminar en cada turno
    public int tileSpeed;
    public float moveSpeed;

    private GM gm;

    //Rango de los enemigos atacables
    public int attackRadius;
    public bool hasAttacked;
    //Lista de los enemigos atacables en ese momento
    public List<Unit> enemiesInRange = new List<Unit>();

    public int playerNumber;

    //Icono que indica a los enemigos atacables
    public GameObject weaponIcon;

    // Attack Stats
    public int health;
    public int attackDamage;
    public int defenseDamage;
    public int armor;

    public DamageIcon damageIcon;

    public int cost;

	public GameObject deathEffect;

	private Animator camAnim;

    public bool isKing;

	private AudioSource source;

    public Text displayedText;

    //Para los mapas de influencia
    public string tipoUnidad;

    //Para que la IA lo maneje según una estrategia
    public IA_UnitControl unitControl; 

    public Tile tilePosicion;

    private void Start()
    {
		source = GetComponent<AudioSource>();
		camAnim = Camera.main.GetComponent<Animator>();
        gm = FindObjectOfType<GM>();
        UpdateHealthDisplay();
        
        //Si es una unidad del bando de la IA, conectar con su IA_UnitControl
        if(playerNumber==2)
            unitControl = GetComponent<IA_UnitControl>();

        

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

    private void PintarInfluencia(bool signo)
    {   
        Debug.Log("Pintar Influencia");
        /*
        InfluTile[] tiles = FindObjectsOfType<InfluTile>();
        foreach (InfluTile tile in tiles) 
            tile.PintarInfluencia(this, signo);
        */

        //Obtindre quantitat parcial de la matriu de tiles del GM
        //segons el tile on s'està i el tileSpeed
        
        int iniX = tilePosicion.matrizX - tileSpeed;
        int finX = tilePosicion.matrizX + tileSpeed;
        int iniY = tilePosicion.matrizY - tileSpeed;
        int finY = tilePosicion.matrizY + tileSpeed;

        for(int i = iniX; i <= finX; i++)
            for(int j = iniY; j <= finY; j++)
                //if(gm.matrizTile[i,j]!=null)  
                if(i>=0 && i<gm.matrizTile.GetLength(0) && j>=0 && j<gm.matrizTile.GetLength(1))  
                    gm.matrizTile[i,j].influTile.PintarInfluencia(this,signo);

    }

    public Tile MaximaInfluenciaAlcanzable()
    {
        int iniX = tilePosicion.matrizX - tileSpeed;
        int finX = tilePosicion.matrizX + tileSpeed;
        int iniY = tilePosicion.matrizY - tileSpeed;
        int finY = tilePosicion.matrizY + tileSpeed;

        Tile maxInflu = gm.matrizTile[iniX, iniY];

        for(int i = iniX; i <= finX; i++)
            for(int j = iniY; j <= finY; j++) 
                //Se recoge un tile de la matriz (no se sale de ésta) 
                if(i>=0 && i<gm.matrizTile.GetLength(0) && j>=0 && j<gm.matrizTile.GetLength(1))
                {
                    Tile tile = gm.matrizTile[i,j];
                    //El tile tiene que ser alcanzable
                    if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
                    { // Si no está ocupado
                        if (tile.isClear() == true)
                        { // is the tile clear from any obstacles
                            if(tile.influTile.getSumaInfluenciaAliada() > maxInflu.influTile.getSumaInfluenciaAliada())
                                maxInflu = tile;
                        }
                    }
                }

        return maxInflu;
    }

    //Cambiar los números de la vida del rey
    private void UpdateHealthDisplay ()
    {
        if (isKing)
        {
            displayedText.text = health.ToString();
        }
    }

    private void OnMouseDown() // select character or deselect if already selected
    {
        
        ResetWeaponIcon();

        if (isSelected == true)
        {
            
            isSelected = false;
            gm.selectedUnit = null;
            gm.ResetTiles();

        }
        else {
            //El jugador lo ha seleccionado para moverlo o atacar a enemigos
            if (playerNumber == gm.playerTurn) { // select unit only if it's his turn
                if (gm.selectedUnit != null)
                { // deselect the unit that is currently selected, so there's only one isSelected unit at a time
                    gm.selectedUnit.isSelected = false;
                }
                gm.ResetTiles();

                gm.selectedUnit = this;

                isSelected = true;
				if(source != null){
					source.Play();
				}
				
                GetWalkableTiles();
                GetEnemies();

                if(playerNumber == 2)
                    Debug.Log(unitControl.nombreObjetivo);
            }

        }

        //El adversario lo ha seleccionado para atacarlo
        //(Se comprueba si ya hay una unidad seleccionada que no es él)
        Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
        if (col != null)
        {
            // Obtener enemigo al que atacar y comprobar si es atacable por la unidad seleccionada
            Unit unit = col.GetComponent<Unit>(); // double check that what we clicked on is a unit
            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
                { // does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);

                }
            }
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            gm.UpdateInfoPanel(this);
        }
    }



    void GetWalkableTiles() { // Looks for the tiles the unit can walk on
        if (hasMoved == true) {
            return;
        }

        //Guardar en un vector todos los tiles en la partida 
        //(se obtinenen con FindObjectsOfType<Tile>())
        /*Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles) {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            { // how far he can move
                if (tile.isClear() == true)
                { // is the tile clear from any obstacles
                    tile.Highlight();
                }

            }          
        }*/
        int iniX = tilePosicion.matrizX - tileSpeed;
        int finX = tilePosicion.matrizX + tileSpeed;
        int iniY = tilePosicion.matrizY - tileSpeed;
        int finY = tilePosicion.matrizY + tileSpeed;

        for(int i = iniX; i <= finX; i++)
            for(int j = iniY; j <= finY; j++)
                //if(gm.matrizTile[i,j]!=null)  
                if(i>=0 && i<gm.matrizTile.GetLength(0) && j>=0 && j<gm.matrizTile.GetLength(1))
                {  
                    Tile tile = gm.matrizTile[i,j];
                    if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
                    { // how far he can move
                        if (tile.isClear() == true)
                        { // is the tile clear from any obstacles
                            tile.Highlight();
                        }
                    }
                
                }

    }

    //Al seleccionar la unidad, se muestran los enemigos atacables
    void GetEnemies() {
    
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= attackRadius) // check is the enemy is near enough to attack
            {
                if (enemy.playerNumber != gm.playerTurn && !hasAttacked) { // make sure you don't attack your allies
                    enemiesInRange.Add(enemy);
                    enemy.weaponIcon.SetActive(true);
                }

            }
        }
    }

    //Al seleccionar el tile, el personaje irá hacia él
    public void Move(Transform movePos)
    {
        gm.ResetTiles();
        PintarInfluencia(false);
        StartCoroutine(StartMovement(movePos));
    }

    // La unidad seleccionada por GM ataca a la seleccionada en OnMouseDown (enemy)
    void Attack(Unit enemy) {
        hasAttacked = true;

        int enemyDamege = attackDamage - enemy.armor;
        int unitDamage = enemy.defenseDamage - armor;

        if (enemyDamege >= 1)
        {
            enemy.health -= enemyDamege;
            enemy.UpdateHealthDisplay();
            DamageIcon d = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            d.Setup(enemyDamege);
        }

        if (transform.tag == "Archer" && enemy.tag != "Archer")
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= 1) // check is the enemy is near enough to attack
            {
                if (unitDamage >= 1)
                {
                    health -= unitDamage;
                    UpdateHealthDisplay();
                    DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                    d.Setup(unitDamage);
                }
            }
        } else {
            if (unitDamage >= 1)
            {
                health -= unitDamage;
                UpdateHealthDisplay();
                DamageIcon d = Instantiate(damageIcon, transform.position, Quaternion.identity);
                d.Setup(unitDamage);
            }
        }

        if (enemy.health <= 0)
        {
         
            if (deathEffect != null){
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

            if (enemy.isKing)
            {
                gm.ShowVictoryPanel(enemy.playerNumber);
            }

            GetWalkableTiles(); // check for new walkable tiles (if enemy has died we can now walk on his tile)
            gm.RemoveInfoPanel(enemy);
            Destroy(enemy.gameObject);
        }

        if (health <= 0)
        {

            if (deathEffect != null)
			{
				Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
				camAnim.SetTrigger("shake");
			}

			if (isKing)
            {
                gm.ShowVictoryPanel(playerNumber);
            }

            gm.ResetTiles(); // reset tiles when we die
            gm.RemoveInfoPanel(this);
            Destroy(gameObject);
        }

        gm.UpdateInfoStats();
  

    }

    // Desactivar el icono de atacar
    public void ResetWeaponIcon() {
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
    }

    IEnumerator StartMovement(Transform movePos) { // Moves the character to his new position.

        
        while (transform.position.x != movePos.position.x) { // first aligns him with the new tile's x pos
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(movePos.position.x, transform.position.y), moveSpeed * Time.deltaTime);
            yield return null;
        }
        while (transform.position.y != movePos.position.y) // then y
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, movePos.position.y), moveSpeed * Time.deltaTime);
            yield return null;
        }

        hasMoved = true;
        PintarInfluencia(true);
        ResetWeaponIcon();
        GetEnemies();
        gm.MoveInfoPanel(this);
    }




}
