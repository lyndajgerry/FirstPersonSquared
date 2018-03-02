using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public class GhostHand : MonoBehaviour
{

	public float GhostTime;

	public HandModelBase HandModel;

	public HandHold HandHold;

	public bool UseRightHand;

	
	// Use this for initialization
	void Start () {
		HandModel.InitHand();	
	}
	
	// Update is called once per frame
	void Update () {
		HandData data = null;
			
//		if (UseRightHand) data = HandMath.GetMostRecentHandDataFromTime(HandHold.RightHandDatas,Util.GetTimestamp() - GhostTime);
//		else data = HandMath.GetMostRecentHandDataFromTime(HandHold.HandDatas,Util.GetTimestamp() - GhostTime);

		if (data == null)
		{
			Debug.Log("GhostHand, data not found.");
			return;
		}
		
		//Debug.Log(data.LeapHand.PalmPosition);

		
		//HandModel.SetLeapHand(data.LeapHand);
		HandModel.UpdateHand();
		
	}
}
