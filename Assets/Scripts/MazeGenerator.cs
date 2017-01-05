using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	public static int MAX_VERTEX_COUNT = 7000;

	[HideInInspector]
	public int width;
	[HideInInspector]
	public int height;

	public Material wall;
	public GameObject endBox;
	public GameObject startBox;

	[HideInInspector]
	public int[,] Maze;

	public List<Vector3> deadEnds = new List<Vector3> ();

	private Stack<Vector2> _tiletoTry = new Stack<Vector2> ();
	private List<Vector2> offsets = new List<Vector2> {
		new Vector2 (0, 1),
		new Vector2 (0, -1),
		new Vector2 (1, 0),
		new Vector2 (-1, 0)
	};
	private System.Random rnd;
	private int _width, _height;
	private Vector2 _currentTile;

	public Vector2 CurrentTile {
		get { return _currentTile; }
		private set {
			if (value.x < 1 || value.x >= this.width - 1 || value.y < 1 || value.y >= this.height - 1) {
				throw new ArgumentException ("Width and Height must be greater than 2 to make a maze");
			} else if (this.width % 2 == 0 || this.height % 2 == 0) {
				throw new ArgumentException ("Width and Height can't be divisible by 2");
			}
			_currentTile = value;
		}
	}

	public void MakeBlocks (int seed)
	{
		rnd = new System.Random (seed);
		Maze = new int[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Maze [x, y] = 1;
			}
		}
		CurrentTile = Vector2.one;
		_tiletoTry.Push (CurrentTile);
		Maze = CreateMaze ();
		CreateExits ();

		DeleteChildren ();

		//Create Start
		GameObject temp1 = Instantiate (startBox, new Vector3 (0, 0, 0), startBox.transform.rotation) as GameObject;
		temp1.transform.parent = transform;

		//Create Walls
		GameObject ptype = null;
		deadEnds.Clear ();
		for (int i = 0; i <= Maze.GetUpperBound (0); i++) {
			for (int j = 0; j <= Maze.GetUpperBound (1); j++) {
				if (Maze [i, j] == 1) {
					ptype = GameObject.CreatePrimitive (PrimitiveType.Cube);
					ptype.transform.position = new Vector3 (i * ptype.transform.localScale.x, 0, j * ptype.transform.localScale.z);
					if (wall != null) {
						ptype.GetComponent<Renderer> ().material = wall;
					}
					ptype.name = "Wall: " + i + ", " + j;
					ptype.transform.parent = transform;
				} else if (Maze [i, j] == 0) {
					if (HasThreeWallsIntact (new Vector2 (i, j))) {
						deadEnds.Add (new Vector3 (i, 0, j));
					}	
				}
			}
		}
		//Create End
		GameObject temp = Instantiate (endBox, new Vector3 (width - 1, 0, height - 1), endBox.transform.rotation) as GameObject;
		temp.transform.parent = transform;

		//Combine the meshes
		CombineMesh ();

	}

	private void CombineMesh ()
	{
		MeshFilter[] meshesRaw = GetComponentsInChildren<MeshFilter> ();
		List<MeshFilter> meshFilters = new List<MeshFilter> ();
		meshFilters.AddRange (meshesRaw);

		int totalVertexCount = 0;

		foreach (MeshFilter meshfilter in meshFilters) {
			totalVertexCount += meshfilter.sharedMesh.vertexCount;
		}

		List<Mesh> meshes = new List<Mesh> ();
		while (totalVertexCount > 0) {
			meshes.Add (CreateMeshes (meshFilters));
			totalVertexCount -= MAX_VERTEX_COUNT;
		}


		DeleteChildren ();
		for (int i = 0; i < meshes.Count; i++) {
			Mesh mesh = meshes [i];
			GameObject obj = new GameObject ();
			obj.AddComponent<MeshFilter> ().mesh = mesh;
			obj.AddComponent<MeshCollider> ().sharedMesh = mesh;
			obj.AddComponent<MeshRenderer> ().material = wall;
			obj.transform.SetParent (this.transform);
			obj.isStatic = true;
			obj.layer = 8;
			obj.name = "Maze: Batch " + (i + 1);
		}

		transform.gameObject.SetActive (true);
	}

	private Mesh CreateMeshes (List<MeshFilter> meshFilters)
	{
		Mesh mesh = new Mesh ();
		List<CombineInstance> combine = new List<CombineInstance> ();
		int vertex = 0;
		int amount = 0;
		for (int i = 0; i < meshFilters.Count; i++) {
			if (meshFilters [i].gameObject.activeSelf) {
				CombineInstance temp = new CombineInstance ();
				temp.mesh = meshFilters [i].sharedMesh;
				temp.transform = meshFilters [i].transform.localToWorldMatrix;
				combine.Add (temp);
				vertex += meshFilters [i].sharedMesh.vertexCount;
				meshFilters [i].gameObject.SetActive (false);
				amount++;
			}
			if (vertex > MAX_VERTEX_COUNT) {
				break;
			}
		}

		mesh.CombineMeshes (combine.ToArray ());
		for (int i = 0; i < amount; i++) {
			meshFilters.RemoveAt (0);
		}
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		mesh.Optimize ();
		return mesh;
	}

	private void DeleteChildren ()
	{
		while (transform.childCount > 0) {
			DestroyImmediate (transform.GetChild (0).gameObject);
		}
	}


	private void CreateExits ()
	{
		Maze [0, 0] = 0;
		Maze [1, 0] = 0;
		Maze [0, 1] = 0;
		Maze [width - 1, height - 1] = 0;
		Maze [width - 2, height - 1] = 0;
		Maze [width - 1, height - 2] = 0;
	}

	public int[,] CreateMaze ()
	{
		List<Vector2> neighbors;
		while (_tiletoTry.Count > 0) {
			Maze [(int)CurrentTile.x, (int)CurrentTile.y] = 0;
			neighbors = GetValidNeighbors (CurrentTile);
			if (neighbors.Count > 0) {
				_tiletoTry.Push (CurrentTile);
				CurrentTile = neighbors [rnd.Next (neighbors.Count)];
			} else {
				CurrentTile = _tiletoTry.Pop ();
			}
		}
		return Maze;
	}

	private List<Vector2> GetValidNeighbors (Vector2 centerTile)
	{
		List<Vector2> validNeighbors = new List<Vector2> ();
		foreach (var offset in offsets) {
			Vector2 toCheck = new Vector2 (centerTile.x + offset.x, centerTile.y + offset.y);
			if (toCheck.x % 2 == 1 || toCheck.y % 2 == 1) {
				if (Maze [(int)toCheck.x, (int)toCheck.y] == 1 && HasThreeWallsIntact (toCheck)) {        
					validNeighbors.Add (toCheck);
				}
			}
		}
		return validNeighbors;
	}

	private bool HasThreeWallsIntact (Vector2 Vector2ToCheck)
	{
		int intactWallCounter = 0;
		foreach (var offset in offsets) {
			Vector2 neighborToCheck = new Vector2 (Vector2ToCheck.x + offset.x, Vector2ToCheck.y + offset.y);
			if (IsInside (neighborToCheck) && Maze [(int)neighborToCheck.x, (int)neighborToCheck.y] == 1) {
				intactWallCounter++;
			}
		}
		return intactWallCounter == 3;
	}

	private bool IsInside (Vector2 p)
	{
		return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
	}
}