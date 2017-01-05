using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class AiTargets : MonoBehaviour
{
	public float repathRate = 0.5F;
	public float turningSpeed = 5;
	public float speed = 3;
	public float endReachedDistance = 0.2F;
	public float pickNextWaypointDist = 1;
	public float slowdownDistance = 0.6F;
	public float forwardLook = 1;

	[HideInInspector]
	public List<Vector3> deadends;
	[HideInInspector]
	public Vector3 endTarget;

	private Vector3 targetDirection;
	private int currentWaypointIndex = 0;
	private bool targetReached = false;

	private float minMoveScale = 0.05F;

	private Vector3 startPosition;
	private List<Vector3> targets = new List<Vector3> ();
	private int heuristicScale = 1;

	private WaypointPath p;

	private float XZSqrMagnitude (Vector3 a, Vector3 b)
	{
		float dx = b.x - a.x;
		float dz = b.z - a.z;

		return dx * dx + dz * dz;
	}

	public void StartPaths ()
	{
		startPosition = transform.position;
		SetTargets ();
		SetPath ();
	}

	private void SetPath ()
	{
		WaypointPath p = new WaypointPath (targets.ToArray (), OnPathComplete);
		p.StartPath ();
	}

	void OnPathComplete (WaypointPath p)
	{
		if (p.HasError ()) {
			Debug.LogError ("Path Error");
			return;
		} else {
			this.p = p;
			List<Vector3> vp = p.vectorPath;
			for (int i = 0; i < vp.Count - 1; i++)
				Debug.DrawLine (vp [i], vp [i + 1], Color.cyan, 2);
		}
	}

	void Update ()
	{
		if (p == null)
			return;
		else if (p.HasError ())
			return;

		Vector3 dir = CalculateVelocity (transform.position);

		//Rotate towards targetDirection (filled in by CalculateVelocity)
		RotateTowards (targetDirection);

		transform.Translate (dir * Time.deltaTime, Space.World);
		List<Vector3> vp = p.vectorPath;
		for (int i = 0; i < vp.Count - 1; i++)
			Debug.DrawLine (vp [i], vp [i + 1], Color.cyan, 2);
		
	}

	private void SetTargets ()
	{ 
		targets.Add (new Vector3 (1, 0, 1));
		deadends.Add (endTarget);
		Vector3 currentTarget = startPosition;
		while (currentTarget != endTarget && deadends.Count > 0) {
			int index = Random.Range (0, deadends.Count);
			Vector3 testTarget = deadends [index];
			if (GetHScore (currentTarget) > GetHScore (testTarget)) {
				currentTarget = testTarget;
				if (currentTarget != endTarget) {
					targets.Add (currentTarget);
				}
			}
			deadends.Remove (testTarget);
		}
		targets.Add (endTarget);
	}

	private int GetHScore (Vector3 position)
	{
		return (int)(((int)(endTarget - position).magnitude) * heuristicScale);
	}

	private void RotateTowards (Vector3 dir)
	{
		if (dir == Vector3.zero)
			return;

		Quaternion rot = transform.rotation;
		Quaternion toTarget = Quaternion.LookRotation (dir);

		rot = Quaternion.Slerp (rot, toTarget, turningSpeed * Time.deltaTime);
		Vector3 euler = rot.eulerAngles;
		euler.z = 0;
		euler.x = 0;
		rot = Quaternion.Euler (euler);

		transform.rotation = rot;
	}

	protected Vector3 CalculateVelocity (Vector3 currentPosition)
	{
		if (p == null || p.vectorPath == null || p.vectorPath.Count == 0) {
			Debug.LogError ("error");
			return Vector3.zero;
		}
		List<Vector3> vPath = p.vectorPath;

		if (vPath.Count == 1) {
			vPath.Insert (0, currentPosition);
		}

		if (currentWaypointIndex >= vPath.Count) {
			currentWaypointIndex = vPath.Count - 1;
		}

		if (currentWaypointIndex <= 1)
			currentWaypointIndex = 1;

		while (true) {
			if (currentWaypointIndex < vPath.Count - 1) {
				//There is a "next path segment"
				float dist = XZSqrMagnitude (vPath [currentWaypointIndex], currentPosition);
				//Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
				if (dist < pickNextWaypointDist * pickNextWaypointDist) {
					currentWaypointIndex++;
				} else {
					break;
				}
			} else {
				break;
			}
		}

		Vector3 dir = vPath [currentWaypointIndex] - vPath [currentWaypointIndex - 1];
		Vector3 targetPosition = CalculateTargetPoint (currentPosition, vPath [currentWaypointIndex - 1], vPath [currentWaypointIndex]);


		dir = targetPosition - currentPosition;
		dir.y = 0;
		float targetDist = dir.magnitude;

		float slowdown = Mathf.Clamp01 (targetDist / slowdownDistance);

		this.targetDirection = dir;

		if (currentWaypointIndex == vPath.Count - 1 && targetDist <= endReachedDistance) {
			if (!targetReached) {
				targetReached = true;
			}

			//Send a move request, this ensures gravity is applied
			return Vector3.zero;
		}

		Vector3 forward = transform.forward;
		float dot = Vector3.Dot (dir.normalized, forward);
		float sp = speed * Mathf.Max (dot, minMoveScale) * slowdown;


		if (Time.deltaTime > 0) {
			sp = Mathf.Clamp (sp, 0, targetDist / (Time.deltaTime * 2));
		}
		return forward * sp;
	}

	private Vector3 CalculateTargetPoint (Vector3 p, Vector3 a, Vector3 b)
	{
		a.y = p.y;
		b.y = p.y;

		float magn = (a - b).magnitude;
		if (magn == 0)
			return a;

		float closest = Mathf.Clamp01 (VectorMath.ClosestPointOnLineFactor (a, b, p));
		Vector3 point = (b - a) * closest + a;
		float distance = (point - p).magnitude;

		float lookAhead = Mathf.Clamp (forwardLook - distance, 0.0F, forwardLook);

		float offset = lookAhead / magn;
		offset = Mathf.Clamp (offset + closest, 0.0F, 1.0F);
		return (b - a) * offset + a;
	}
}