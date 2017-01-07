using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PointDetector : MonoBehaviour {

	public float clickTolerance;
	public float duplicationTolerance;
	public float vertexSelectionTolerance;

	public List<Vector3> Points 
	{
		get 
		{
			return points;
		}
	}

	List<Vector3> points;

	Camera cam;

	Transform hitTransform;

	bool intersectionDrawn = false;

	void Start() 
	{
		hitTransform = null;
		cam = GetComponent<Camera>();
		points = new List<Vector3> ();
	}
		
	void Update() 
	{
		// only 3 points were needed to form the plane
		if (points.Count >= 3) 
		{
			if (!intersectionDrawn) 
			{
				for (int i = 0; i < 1; ++i) 
				{
					DrawLine(points[i],points[(i+1)%3],parent: hitTransform);
				}
				intersectionDrawn = true;
			}
			return;
		}
		// process only if mouse is down
		if (!Input.GetMouseButtonDown(0))
			return;
		Ray camRay = cam.ScreenPointToRay (Input.mousePosition);
		Vector3 closestEdgePointFrtSide;
		float dstncToEdgPntFrtSide;
		bool collided = DetectClosestEdgePoint (camRay, out closestEdgePointFrtSide, out dstncToEdgPntFrtSide);
		if (collided) 
		{
			Vector3 origin4ExitingRay = camRay.origin + camRay.direction.normalized * cam.farClipPlane;
			Ray exitingRay = new Ray (origin4ExitingRay, -camRay.direction);
			Vector3 closestEdgePointHiddenSide;
			float dstncToEdgPntHdnSide;
			collided = DetectClosestEdgePoint (exitingRay, out closestEdgePointHiddenSide, out dstncToEdgPntHdnSide);
			if (collided) 
			{
				Vector3 closestPoint;
				float distanceToClosestPoint;
				if (dstncToEdgPntFrtSide < dstncToEdgPntHdnSide) 
				{
					distanceToClosestPoint = dstncToEdgPntFrtSide;
					closestPoint = closestEdgePointFrtSide;
				}
				else 
				{
					distanceToClosestPoint = dstncToEdgPntHdnSide;
					closestPoint = closestEdgePointHiddenSide;
				}
				if (distanceToClosestPoint < clickTolerance) 
				{
					points.Add (closestPoint);	
					DrawSphere (closestPoint, parent: hitTransform);
				}
			}
		}
	}

	bool DetectClosestEdgePoint(Ray camRay, out Vector3 closestEdgePoint, out float minDistance)
	{
		closestEdgePoint = new Vector3 ();
		minDistance = 0.0f;
		//raycast and detect if we hit any collider
		RaycastHit hit;
		if (!Physics.Raycast(camRay, out hit))
			return false;

		if (IsCloseToExisting (hit.point))
			return false;
		
		// get the triangle we hit
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null)
			return false;
		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] meshVertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3[] trVertices = new Vector3[3]; // hold the triangle vertices
		for (byte i =0; i<3; ++i)
			trVertices[i] = meshVertices[triangles[hit.triangleIndex * 3 + i]];
		hitTransform = hit.collider.transform;
		for (byte i =0; i<3; ++i)
			trVertices[i] = hitTransform.TransformPoint(trVertices[i]);

		if (IsCloseToVertex (hit.point, trVertices, out closestEdgePoint)) 
		{
			minDistance = 0.0f;
			return true;
		}

		minDistance = float.MaxValue;

		for (byte i = 0; i < 3; ++i) 
		{
			Vector3 intscPnt = PointToSegment (hit.point, trVertices[i], trVertices[(i+1)%3]);
			float crtDistance = Vector3.Distance (intscPnt,hit.point);
			if (crtDistance < minDistance) 
			{
				minDistance = crtDistance;
				closestEdgePoint = intscPnt;
			}
		}

		return true;
	}

	public bool IsCloseToVertex(Vector3 p, Vector3[] trVertices, out Vector3 closeVertex)
	{
		closeVertex = new Vector3 ();
		for (byte i = 0; i < 3; ++i) 
		{
			if (Vector3.Distance (p, trVertices [i]) < vertexSelectionTolerance) 
			{
				closeVertex = trVertices [i];
				return true;
			}
		}
		return false;
	}

	bool IsCloseToExisting(Vector3 newPoint)
	{
		for (int i = 0; i < points.Count; ++i) 
		{
			if (Vector3.Distance (newPoint, points [i]) < duplicationTolerance)
				return true;
		}
		return false;
	}

	/// <summary>
	/// Calculates an intersection of a normal from a point p to a line formed by points x and y,
	/// in other words the point on the segment x-y, which is closest to the specified point p
	/// </summary>
	/// <returns>an intersection of a normal from a point p to a line formed by points x and y</returns>
	/// <param name="p">point from which to cast normal</param>
	/// <param name="x">First point on a line</param>
	/// <param name="y">Second point on a line</param>
	Vector3 PointToSegment(Vector3 p, Vector3 x, Vector3 y)
	{
		Vector3 v = y - x;
		Vector3 w = p - x;

		float c1 = Vector3.Dot (w,v);
		if (c1 <= 0.0f)
			return (p-x);

		float c2 = Vector3.Dot (v, v);
		if (c2 <= c1)
			return (p - y);

		float b = c1 / c2;

		return (x + b * v);
	}
		
	void DrawSphere(Vector3 center, Transform parent = null, float radius = 0.05f)
	{
		GameObject sphere  = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		if (parent != null)
			sphere.transform.SetParent (parent);
		sphere.GetComponent<Renderer> ().material.color = Color.red;
		sphere.transform.localScale = new Vector3 (radius, radius, radius);
		sphere.transform.position = center;
	}

	void DrawLine(Vector3 start, Vector3 end, Transform parent = null)
	{
		GameObject myLine = new GameObject ();
		if (parent != null)
			myLine.transform.parent = parent;
		myLine.transform.position = start;
		myLine.AddComponent <LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer> ();
		//lr.material = new Material (Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors (Color.red, Color.red);
		lr.SetWidth (0.05f, 0.05f);
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
	}


}
