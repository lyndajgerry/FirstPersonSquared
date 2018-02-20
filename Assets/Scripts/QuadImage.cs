using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuadImage : MonoBehaviour {
    

    public RawImage camImage;
    // Use this for initialization
    void Start()
    {
        //imageTexture = gameObject.GetComponent<RawImage>();
        //if (imageTexture == null)
        //    Debug.Log("imageTexture not found");
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Renderer>().material.mainTexture = camImage.texture;
        
    }
}
