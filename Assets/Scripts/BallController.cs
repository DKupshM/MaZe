using UnityEngine;
using System.Collections;

[System.Serializable]
public class Movement
{
	public float sensH = 10;
	public float sensV = 10;
	public float smooth = 0.5f;
	
	public bool isDynamic;
	public bool isDynamicVertical;
	public float vertical;
	public float horizontal;
}

public class BallController : MonoBehaviour
{
	public float speed = 10f;
	public float jump = 2f;

	private float distToGround;
	private Rigidbody rb;

	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
		distToGround = GetComponent<Collider> ().bounds.extents.y;
	}

	void Update ()
	{
		if (Input.GetKey (KeyCode.Space) && IsGrounded ()) {
			Jump ();
		}
	}

	void FixedUpdate ()
	{
		float moveHorizontal = 0;
		float moveVertical = 0;
		if (SystemInfo.deviceType != DeviceType.Handheld) {
			// DeskTop
			moveHorizontal = Input.GetAxis ("Horizontal");
			moveVertical = Input.GetAxis ("Vertical");
		} else {
			// Phone
			moveVertical = Input.acceleration.y;
			moveHorizontal = Input.acceleration.x;
		}
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		rb.AddForce (movement * speed * Time.deltaTime);
	}

	private bool IsGrounded ()
	{
		return Physics.Raycast (transform.position, -Vector3.up, (float)(distToGround + 0.1));
	}

	void Jump ()
	{
		rb.velocity = new Vector3 (rb.velocity.x, jump, rb.velocity.z);
	}
}
