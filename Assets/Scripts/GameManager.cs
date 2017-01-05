using UnityEngine;
using UnityEngine.Networking;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MazeConfigurations
{
	public MazeGenerator mazeGen;
	public int height = 101;
	public int width = 101;
	public int seed = 0;
	public bool genSeed = true;
}

[System.Serializable]
public class SceneConfiguratons
{
	public Transform target;
	public Transform startingPositions;
	public Terrain terrain;
	public int border = 20;
}

[System.Serializable]
public class AiConfigurations
{
	public AstarPath graph;
	public int aiBorder = 20;
}

[System.Serializable]
public class BallConfigurations
{
	public List<Color> colors = new List<Color> () {
		Color.blue,
		Color.cyan,
		Color.red,
		Color.magenta,
		Color.yellow,
		Color.green
	};

	public GameObject aiBall;
	public GameObject playerBall;
	public bool usePlayer = true;
	public int ballCount = 2;
}



public class GameManager : NetworkBehaviour
{

	public MazeConfigurations mazeConfig;
	public SceneConfiguratons sceneConfig;
	public AiConfigurations aiConfig;
	public BallConfigurations ballConfig;

	public bool runOnStart = false;

	private List<Vector3> deadEnds;

	void Start ()
	{
		if (isServer) {
			mazeConfig.seed = (int)Random.Range ((int)-System.DateTime.Now.Ticks, (int)System.DateTime.Now.Ticks);
		}

		if (runOnStart) {
			CmdGetSeed ();
			//SetUpGame ();
			//StartGame ();
		}
	}

	[Command]
	public void CmdGetSeed ()
	{
		RpcSetSeed (mazeConfig.seed);
	}

	[ClientRpc]
	public void RpcSetSeed (int seed)
	{
		this.mazeConfig.seed = seed;
		Debug.Log ("Seed is equal to " + seed);
	}

	public void SetUpGame ()
	{
		GenerateMaze ();
		ResizeTerrain ();
		SetTargetPosition ();
	}

	public void StartGame ()
	{
		GenerateAiGraphs ();
		//CreateBalls ();
		
	}

	void CreateBalls ()
	{
		List<Color> colorsLocal = ballConfig.colors.GetRange (0, ballConfig.colors.Count);
		for (int i = 0; i < ballConfig.ballCount; i++) {
			if (colorsLocal.Count == 0) {
				colorsLocal = ballConfig.colors.GetRange (0, ballConfig.colors.Count);
				Debug.Log ("Not enough colors. Reusing some Colors");
			}
			int colorNumber = Random.Range (0, colorsLocal.Count);
			if (i == 0 && ballConfig.usePlayer) {
				SpawnPlayer (colorsLocal [colorNumber]);
			} else {
				SpawnAi (colorsLocal [colorNumber]);
			}
			colorsLocal.RemoveAt (colorNumber);
		}
	}

	void SpawnPlayer (Color c)
	{
		Vector3 rndPosWithin = new Vector3 (Random.Range (-1f, 1f), -.335f, Random.Range (-1f, 1f));
		rndPosWithin = sceneConfig.startingPositions.TransformPoint (rndPosWithin * .5f);
		GameObject obj = Instantiate (ballConfig.playerBall, rndPosWithin, ballConfig.playerBall.transform.rotation) as GameObject;
		obj.GetComponent<MeshRenderer> ().material.shader = Shader.Find ("Standard");
		obj.GetComponent<MeshRenderer> ().material.SetColor ("_Color", c);
		obj.GetComponent<TrailRenderer> ().material.shader = Shader.Find ("Standard");
		obj.GetComponent<TrailRenderer> ().material.SetColor ("_Color", c);
		Debug.Log ("Player Ball Created, Color: " + c.ToString ());

	}

	void SpawnAi (Color c)
	{
		Vector3 rndPosWithin = new Vector3 (Random.Range (-1f, 1f), -.335f, Random.Range (-1f, 1f));
		rndPosWithin = sceneConfig.startingPositions.TransformPoint (rndPosWithin * .5f);

		GameObject obj = Instantiate (ballConfig.aiBall, rndPosWithin, ballConfig.aiBall.transform.rotation) as GameObject;
		AiTargets targets = obj.GetComponent<AiTargets> ();
		targets.endTarget = sceneConfig.target.transform.position;
		targets.deadends = new List<Vector3> (deadEnds);
		targets.StartPaths ();

		obj.GetComponent<MeshRenderer> ().material.shader = Shader.Find ("Standard");
		obj.GetComponent<MeshRenderer> ().material.SetColor ("_Color", c);
		obj.GetComponent<TrailRenderer> ().material.shader = Shader.Find ("Standard");
		obj.GetComponent<TrailRenderer> ().material.SetColor ("_Color", c);

		Debug.Log ("AI Ball Created, Color: " + c.ToString ());
	}

	void GenerateMaze ()
	{
		mazeConfig.mazeGen.height = mazeConfig.height;
		mazeConfig.mazeGen.width = mazeConfig.width;
		mazeConfig.mazeGen.MakeBlocks (mazeConfig.seed);
		deadEnds = mazeConfig.mazeGen.deadEnds;
		Debug.Log ("Maze Created with seed: " + mazeConfig.seed);
	}

	void SetTargetPosition ()
	{
		sceneConfig.target.position = new Vector3 (mazeConfig.width, -.5f, mazeConfig.height);
	}

	void ResizeTerrain ()
	{
		sceneConfig.terrain.terrainData.size = new Vector3 (mazeConfig.width + (sceneConfig.border * 2), 1, mazeConfig.height + (sceneConfig.border * 2));
	}

	void GenerateAiGraphs ()
	{
		GridGraph gridGraph = (GridGraph)aiConfig.graph.graphs [0];
		gridGraph.depth = mazeConfig.height + (2 * aiConfig.aiBorder);
		gridGraph.width = mazeConfig.width + (2 * aiConfig.aiBorder);
		gridGraph.center = new Vector3 (mazeConfig.width / 2, -.5f, mazeConfig.height / 2);
		gridGraph.UpdateSizeFromWidthDepth ();
		aiConfig.graph.Scan ();
	}


}
