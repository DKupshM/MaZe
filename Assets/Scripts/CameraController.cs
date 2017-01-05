using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

	private Quaternion startRotation;
	private Vector3 startPosition;

	void Start ()
	{
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

	void Update ()
	{
		transform.position = startPosition + transform.parent.position;
		transform.rotation = startRotation;
	}


}