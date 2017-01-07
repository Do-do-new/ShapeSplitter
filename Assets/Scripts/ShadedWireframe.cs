using UnityEngine;
using System.Collections;

public class ShadedWireframe : MonoBehaviour {

	void OnPreRender()
	{
		//GL.wireframe = true;
	}

	void OnPostRender()
	{
		//GL.wireframe = false;
	}
}
