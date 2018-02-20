using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEffectController : MonoBehaviour
{


	public float Strength;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		foreach (PSMeshRendererUpdater updater in GameObject.FindObjectsOfType<PSMeshRendererUpdater>())
		{
			updater.MeshObject.gameObject.transform.parent.localScale = new Vector3(Strength,Strength,Strength);
			
			//complete hack
			
			updater.MeshObject.gameObject.transform.parent.localPosition = new Vector3(0f,(3f*Strength) -3f,(4f*Strength) -4f);

			
			//Debug.Log(updater.MeshObject.gameObject.transform.parent);
			//Util.ScaleAround(updater.MeshObject.gameObject.transform,updater.MeshObject.gameObject.transform,new Vector3(Strength,Strength,Strength));
		}
	}
}
