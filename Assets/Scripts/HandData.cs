using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandData {

    //public bool IsRightHand;

    public Vector3 HeadPosition;
    public Vector3 HeadEulerAngles;
    
    public double NetworkTimeStamp;

    public Leap.Hand LeapHand;

}
