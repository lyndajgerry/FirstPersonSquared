using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour {

    public MeshRenderer localLeftRend;
    public MeshRenderer localRightRend;
    public MeshRenderer remoteLeftRend;
    public MeshRenderer remoteRightRend;
    public MeshEffectController meshController;
    [Range(0,1)]
    public float fader;
    public float localTransparency;
    public float remoteTransparency;

    [Range(0, 1)]
    public float strengthControl;
    public float strengthMultiplier;


    private Material localLeft;
    private Material localRight;
    private Material remoteLeft;
    private Material remoteRight;

    // Use this for initialization
    void Awake ()
    {
        localLeft = localLeftRend.material;
        localRight = localRightRend.material;
        remoteLeft = remoteLeftRend.material;
        remoteRight = remoteRightRend.material;

        strengthControl = 1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateFader();
        UpdateStrength();
	}

    void UpdateFader()
    {
        localTransparency = fader * 1;
        remoteTransparency = 1 - localTransparency;

        UpdateTransparency(localLeft, localTransparency);
        UpdateTransparency(localRight, localTransparency);
        UpdateTransparency(remoteLeft, remoteTransparency);
        UpdateTransparency(remoteRight, remoteTransparency);
    }

    private void UpdateTransparency (Material mat, float value)
    {
        Color color = mat.color;
        color.a = value;
        mat.color = color;
    }

    private void UpdateStrength()
    {

        meshController.Strength = strengthControl * strengthMultiplier;
    } 
}
