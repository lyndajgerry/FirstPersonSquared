using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandData {

    //public bool IsRightHand;

    public Leap.Vector HeadPosition;
    public Leap.Vector HeadEulerAngles;
    
    public double NetworkTimeStamp;

    //public Leap.Hand LeapHand;

}
