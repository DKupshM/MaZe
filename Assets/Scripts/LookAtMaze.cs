using UnityEngine;
using System.Collections;

public class LookAtMaze : MonoBehaviour
{
	public Transform maze;

	void Update ()
	{ 
		Vector3 center = getCenter ();
		transform.position = new Vector3 ((int)center.x, 100, (int)center.z);
		transform.LookAt (new Vector3 ((int)center.x, 100, (int)center.z));
	}

	private Vector3 getCenter ()
	{
		Vector3 p = Vector3.zero;
		for (int i = 0; i < maze.childCount; i++) {
			p.x += maze.GetChild (i).position.x;
			p.y += maze.GetChild (i).position.y;
			p.z += maze.GetChild (i).position.z;
		}
		p.x /= maze.childCount;
		p.y /= maze.childCount;
		p.z /= maze.childCount;
		return p;
	}
}
