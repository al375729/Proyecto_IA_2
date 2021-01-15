using UnityEngine;
using System.Collections;

public class IA_UnitPathFind : MonoBehaviour {


	//public Transform target;
	float speed = 1;
	Vector3[] path;
	int targetIndex;
	int maxPasos = 0;
	//public GameObject contenedorGrid;
	private Grid grid;

	//Para llamar al método AcabarPathFinding
	private IA_UnitControl unitControl;

	void Start() {
		grid = FindObjectOfType<Grid>();
		unitControl = GetComponent<IA_UnitControl>();
		
	}

    private void Update()
    {
		
	}

	//Esta es la funcion que hay que llamar para que un objeto se mueva
	//Target       --> Transform.Position o Vector3() a donde se movera la ficha. NULL EN CASO DE ATACAR VALOR SI NOS MOVEMOS A UN HUECO VACIO
	//maxPasos     --> Pasos maximos que puede hacer la ficha
	//estaAtacando --> Booleano de si el objetivo esta atacando a una ficha o se esta moviendo
	// enemyTarget --> GameObject del objeto que vamos a atacar. NULL SI NO ESTAMOS MOVIENDO A UN TILE VACIO
	public void camino(Transform target,int maxpasos,bool estaAtacando,GameObject enemyTarget)
    {
		maxPasos = maxpasos;
		if (estaAtacando)
		{
			setLayers0(enemyTarget);
			setLayers0(this.gameObject);

			grid.CreateGrid();
			PathRequestManager.RequestPath(transform.position, enemyTarget.transform.position, OnPathFound);

			setLayers9(enemyTarget);
			setLayers9(this.gameObject);
		}

		else 
		{
			grid.CreateGrid();
			PathRequestManager.RequestPath(transform.position, target.transform.position, OnPathFound);
		}
		
	}


	//Pone los layers del objeto a mover y el objetivo en caso de que sea un enemigo a 0 porque de lo contrario serian obstaculos y los dos nodos serian no transitables
	void setLayers0(GameObject obj)
	{
		obj.layer = 0;
		foreach (Transform t in obj.transform)
		{
			t.gameObject.layer = 0;
		}
	}

	//Pone los dos objetos en el layer de obstaculos otra vez para que no sean transitables
	void setLayers9(GameObject obj)
	{
		obj.layer = 9;
		foreach (Transform t in obj.transform)
		{
			t.gameObject.layer = 9;
		}
	}




	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			speed = GetComponent<Unit>().tileSpeed;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
		Vector3 currentWaypoint = path[0];
		
		targetIndex = 0;
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= maxPasos-1 || targetIndex >= path.Length) {
					//yield break;
					break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;
			/*
			if (transform.position.x != currentWaypoint.x) { // first aligns him with the new tile's x pos
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentWaypoint.x, transform.position.y), speed * Time.deltaTime);
            yield return null;
			}
			else if (transform.position.y != currentWaypoint.y) // then y
			{
				transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, currentWaypoint.y), speed * Time.deltaTime);
				yield return null;
			}*/

		}

		//Cuando se acaba el desplazamiento
		unitControl.AcabarPathFinding();

	}

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}
}
