using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandData {

    //public bool IsRightHand;

    public Vector3 HeadPosition;
    public Vector3 HeadEulerAngles;
    
    public double NetworkTimeStamp;

    //public Leap.Hand LeapHand;
    public Vector3 PalmPosition;
    public Vector3 ThumbPosition;
    public Vector3 IndexPosition;
    public Vector3 MiddlePosition;
    public Vector3 RingPosition;
    public Vector3 PinkyPosition;

}
