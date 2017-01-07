using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float rotateHorizontal = Input.GetAxis ("Horizontal");
		transform.Rotate (0.0f, 0.0f,rotateHorizontal);	
	}
}
