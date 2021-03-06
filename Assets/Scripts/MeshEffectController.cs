﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEffectController : MonoBehaviour
{


    public float Strength;
    public GameObject[] Meshs;

    private float LastStrength;
    private Color TintColor;

    // Use this for initialization
    void Start()
    {

        LastStrength = Strength;

        TintColor = Meshs[0].GetComponent<Renderer>().material.GetColor("_TintColor");

    }

    // Update is called once per frame
    void Update () {
		//foreach (PSMeshRendererUpdater updater in GameObject.FindObjectsOfType<PSMeshRendererUpdater>())
		//{
		//	updater.MeshObject.gameObject.transform.parent.localScale = new Vector3(Strength,Strength,Strength);
			
		//	//complete hack
			
		//	updater.MeshObject.gameObject.transform.parent.localPosition = new Vector3(0f,(3f*Strength) -3f,(4f*Strength) -4f);

			
		//	//Debug.Log(updater.MeshObject.gameObject.transform.parent);
		//	//Util.ScaleAround(updater.MeshObject.gameObject.transform,updater.MeshObject.gameObject.transform,new Vector3(Strength,Strength,Strength));
		//}

        if (LastStrength != Strength)
        {
            foreach (GameObject obj in Meshs)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                ParticleSystem particles = obj.GetComponent<ParticleSystem>();
                var emission = particles.emission;

                renderer.material.mainTextureScale = new Vector2(Strength, Strength);
                emission.rateOverTimeMultiplier = Strength * 5;
            

                ParticleSystem.ShapeModule shape = particles.shape;
                shape.radius = 0.0025f + (Strength * 0.001f);

                Color newColor = renderer.material.GetColor("_TintColor");
                newColor.r = TintColor.r * Strength;
                newColor.g = TintColor.g * Strength;
                newColor.b = TintColor.b * Strength;
                //renderer.material.SetFloat("_Brightness", Strength);
                renderer.material.SetColor("_TintColor", newColor);
            }
            LastStrength = Strength;
        }
    }
}
