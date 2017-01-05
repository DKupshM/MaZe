using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LocalNetworkManager : NetworkManager
{

	public override void OnClientConnect (NetworkConnection conn)
	{
		GameManager manager = ((GameObject)GameObject.Instantiate (spawnPrefabs [0], Vector3.zero, Quaternion.identity)).GetComponent<GameManager> ();
	}
}
