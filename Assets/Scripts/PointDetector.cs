using UnityEngine;
using System.Collections;

public class PointDetector : MonoBehaviour {

	public float clickTolerance;
	public float duplicationTolerance;
	public float vertexSelectionTolerance;

	Camera cam;

	void Start() {
		cam = GetComponent<Camera>();
	}


	void Update() 
	{
		// process only if mouse is down
		if (!Input.GetMouseButtonDown(0))
			return;
		Ray camRay = cam.ScreenPointToRay (Input.mousePosition);
		Vector3 closestEdgePointFrtSide;
		float dstncToEdgPntFrtSide;
		RaycastHit hit;
		bool collided = DetectClosestEdgePoint (camRay, out closestEdgePointFrtSide, out dstncToEdgPntFrtSide, out hit);
		if (collided) 
		{
			Vector3 origin4ExitingRay = camRay.origin + camRay.direction.normalized * cam.farClipPlane;
			Ray exitingRay = new Ray (origin4ExitingRay, -camRay.direction);
			Vector3 closestEdgePointHiddenSide;
			float dstncToEdgPntHdnSide;
			collided = DetectClosestEdgePoint (exitingRay, out closestEdgePointHiddenSide, out dstncToEdgPntHdnSide, out hit);
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
					DrawSphere (closestPoint, hit.transform);
			}
		}
	}

	bool DetectClosestEdgePoint(Ray camRay, out Vector3 closestEdgePoint, out float minDistance, out RaycastHit hit)
	{
		closestEdgePoint = new Vector3 ();
		minDistance = 0.0f;
		if (!Physics.Raycast(camRay, out hit))
			return false;
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null)
			return false;
		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
		Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
		Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
		Transform hitTransform = hit.collider.transform;
		p0 = hitTransform.TransformPoint(p0);
		p1 = hitTransform.TransformPoint(p1);
		p2 = hitTransform.TransformPoint(p2);

		if (IsVertexClose (hit.point, p0)) 
		{
			closestEdgePoint = p0;
			minDistance = 0.0f;
			return true;
		}
		if (IsVertexClose (hit.point, p1)) 
		{
			closestEdgePoint = p1;
			minDistance = 0.0f;
			return true;
		}
		if (IsVertexClose (hit.point, p2)) 
		{
			closestEdgePoint = p2;
			minDistance = 0.0f;
			return true;
		}
			
		Vector3 intscPnt = PointToSegment (hit.point, p0, p1);
		float crtDistance = Vector3.Distance (intscPnt,hit.point);
		minDistance = crtDistance;
		closestEdgePoint = intscPnt;

		intscPnt = PointToSegment (hit.point, p0, p2);
		crtDistance = Vector3.Distance (intscPnt,hit.point);
		if (crtDistance < minDistance) 
		{
			minDistance = crtDistance;
			closestEdgePoint = intscPnt;
		}

		intscPnt = PointToSegment (hit.point, p1, p2);
		crtDistance = Vector3.Distance (intscPnt,hit.point);
		if (crtDistance < minDistance) 
		{
			minDistance = crtDistance;
			closestEdgePoint = intscPnt;
		}
		return true;
	}

	public bool IsVertexClose(Vector3 p, Vector3 vertex)
	{
		return (Vector3.Distance(p, vertex) < vertexSelectionTolerance);
	}

	/// <summary>
	/// Calculates an intersection of a normal from a point p to a line formed by points x and y
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

	GameObject DrawSphere(Vector3 center, Transform parent = null, float radius = 0.02f)
	{
		GameObject sphere  = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		sphere.transform.localScale = new Vector3 (radius, radius, radius);
		sphere.transform.position = center;
		if (parent != null)
			sphere.transform.SetParent (parent);
		return sphere;
	}
}
