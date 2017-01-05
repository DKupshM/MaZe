using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerSetup : NetworkBehaviour
{
	[SerializeField]
	Behaviour[] componentsToDisable;

	private Camera sceneCamera;

	void Start ()
	{
		if (!isLocalPlayer) {
			foreach (Behaviour b in componentsToDisable) {
				b.enabled = false;
			}
			this.gameObject.tag = "OtherPlayer";
		} else {
			sceneCamera = Camera.main;
			if (sceneCamera) {
				sceneCamera.gameObject.SetActive (false);
			}

		}

	}

	void OnDisable ()
	{
		if (sceneCamera) {
			sceneCamera.gameObject.SetActive (true);
		}
	}
}
