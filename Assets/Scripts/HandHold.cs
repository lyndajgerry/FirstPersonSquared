﻿using System;
using System.Collections;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using LeapInternal;
using UnityEngine;
using Hand = Leap.Hand;

public class HandHold : MonoBehaviour {






	public float RecordingCullTime = 10f;

	private RigidFinger _leftThumb;
	private RigidFinger _leftIndex;
	private RigidFinger _leftMiddle;
	private RigidFinger _leftRing;
	private RigidFinger _leftPinky;
	
//	private RigidFinger _rightThumb;
//	private RigidFinger _rightIndex;
//	private RigidFinger _rightMiddle;
//	private RigidFinger _rightRing;
//	private RigidFinger _rightPinky;

//	public HandModelBase RightHand;
	
	public List<HandData> HandDatas = new List<HandData>();
	
	
		
	
	[Header("Below only needed on local.")] 
	public Transform HeadTransform;
	
	
	[HideInInspector]
	public bool IsLocalHands;

	public HandModelBase Hand;
	
//	public List<HandData> RightHandDatas = new List<HandData>();

	void Awake()
	{
//		//find our hands, this method also finds inactive objects because user hands might not be visible on network start
//		foreach (var o in Resources.FindObjectsOfTypeAll(typeof(RigidHand)))
//		{
//			var h = (RigidHand) o;
//			switch (h.Handedness)
//			{
//				case Chirality.Left:
//					LeftHand = h;
//					break;
//				case Chirality.Right:
//					RightHand = h;
//					break;
//				default:
//					throw new ArgumentOutOfRangeException();
//			}
//		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("NetowrkTime=" + Network.time);
		
		
		//get current timestamp
		double timestamp = Util.GetTimestamp();
		//double timestamp = Network.time;
		
		//cull data that is too old
		CullData(HandDatas,timestamp);
//		CullData(RightHandDatas,timestamp);
		
		//only the local player can modify their own sync list of structs
		if (!IsLocalHands) return;
		
		//if both hands are not initialized (both hands must first be detected by leap motion), dont run.
		if (Hand.GetLeapHand() == null) return;
//		if (RightHand.GetLeapHand() == null) return;
		

		
		//create new data and add it to our sync list of structs
		HandData leftHandData = AddData(HandDatas,Hand,false,timestamp);
//		HandData rightHandData = AddData(RightHandDatas,RightHand, true,timestamp);
		
//		Debug.Log("Index Finger Angle: L:" + leftHandData.Index.CalculateFullFingerAngle() + " R:"+rightHandData.Index.CalculateFullFingerAngle());
//		Debug.DrawLine(leftHandData.PalmPosition,leftHandData.PalmPosition + leftHandData.PalmDirection.normalized);
//		Debug.DrawLine(rightHandData.PalmPosition,rightHandData.PalmPosition + rightHandData.PalmDirection.normalized);
//		
//		Debug.DrawLine(leftHandData.PalmPosition,leftHandData.PalmPosition + leftHandData.PalmNormal.normalized,Color.red);
//		Debug.DrawLine(rightHandData.PalmPosition,rightHandData.PalmPosition + rightHandData.PalmNormal.normalized,Color.red);
		

	}
	
	
	private void CullData(List<HandData> handDatas, double timestamp)
	{
		//figure out which data should be removed.
		List<HandData> datasToRemove = new List<HandData>();
		foreach (HandData data in handDatas)
		{
			if (data.NetworkTimeStamp + (double)RecordingCullTime <= timestamp)
			{
				datasToRemove.Add(data);
			}
		}

		//remove the data
		foreach (HandData data in datasToRemove)
		{
			handDatas.Remove(data);
		}
	}

	private HandData AddData(List<HandData>  handDatas, HandModelBase hand, bool isRightHand, double timestamp)
	{

		Finger thumb = null;
		Finger index = null;
		Finger middle = null;
		Finger ring = null;
		Finger pinky = null;
		
		
		//HACK: this is a very bad way perofrmance wise to find the individual fingers of each hand..
		//there is an array of fingers referenced by the hand which by default is in order
		//but no error message will appear if they are out of order which can cause confusion
		//(compared to expection thrown if reference is missing)
		//so I am prioritizing making sure fingers are correctly referenced (e.g. index finger = index finger) over performance here.
		
		foreach (var fingerModel in hand.GetLeapHand().Fingers)
		{
			var f = (Finger) fingerModel;
			switch (f.Type)
			{
				case Finger.FingerType.TYPE_THUMB:
					thumb = f;
					break;
				case Finger.FingerType.TYPE_INDEX:
					index = f;
					break;
				case Finger.FingerType.TYPE_MIDDLE:
					middle = f;
					break;
				case Finger.FingerType.TYPE_RING:
					ring = f;
					break;
				case Finger.FingerType.TYPE_PINKY:
					pinky = f;
					break;
				case Finger.FingerType.TYPE_UNKNOWN:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		
			HandData data = new HandData();


			//data.IsRightHand = isRightHand;

			data.HeadPosition = new Leap.Vector(HeadTransform.position.x,HeadTransform.position.y,HeadTransform.position.z);
			data.HeadEulerAngles = new Leap.Vector(HeadTransform.eulerAngles.x,HeadTransform.eulerAngles.y,HeadTransform.eulerAngles.z);
		
			data.NetworkTimeStamp = timestamp;

			data.LeapHand = hand.GetLeapHand();

//			data.PalmPosition = hand.GetPalmPosition();
//			data.PalmDirection = hand.GetPalmDirection();
//			data.PalmNormal = hand.GetPalmNormal();
//
//			data.PalmWorldPosition = hand.palm.position;
//			data.PalmWorldEulerAngles = hand.palm.eulerAngles;
//			
//			data.Thumb = new FingerData();
//			data.Thumb.TipPosition = thumb.GetTipPosition();
//			data.Thumb.DistalDirection = thumb.GetBoneDirection((int) Bone.BoneType.TYPE_DISTAL);
//			data.Thumb.IntermediateDirection = thumb.GetBoneDirection((int) Bone.BoneType.TYPE_INTERMEDIATE);
//			data.Thumb.ProximalDirection = thumb.GetBoneDirection((int) Bone.BoneType.TYPE_PROXIMAL);
//			data.Thumb.MetaCarpalDirection = thumb.GetBoneDirection((int) Bone.BoneType.TYPE_METACARPAL);
//
//			data.Thumb.Bone1Position = thumb.bones[1].position;
//			data.Thumb.Bone1Rotation = thumb.bones[1].rotation.eulerAngles;
//			data.Thumb.Bone2Position = thumb.bones[2].position;
//			data.Thumb.Bone2Rotation = thumb.bones[2].rotation.eulerAngles;
//			data.Thumb.Bone3Position = thumb.bones[3].position;
//			data.Thumb.Bone3Rotation = thumb.bones[3].rotation.eulerAngles;
//			
//			data.Index = new FingerData();
//			data.Index.TipPosition = index.GetTipPosition();
//			data.Index.DistalDirection = index.GetBoneDirection((int) Bone.BoneType.TYPE_DISTAL);
//			data.Index.IntermediateDirection = index.GetBoneDirection((int) Bone.BoneType.TYPE_INTERMEDIATE);
//			data.Index.ProximalDirection = index.GetBoneDirection((int) Bone.BoneType.TYPE_PROXIMAL);
//			data.Index.MetaCarpalDirection = index.GetBoneDirection((int) Bone.BoneType.TYPE_METACARPAL);
//		
//			data.Index.Bone1Position = index.bones[1].position;
//			data.Index.Bone1Rotation = index.bones[1].rotation.eulerAngles;
//			data.Index.Bone2Position = index.bones[2].position;
//			data.Index.Bone2Rotation = index.bones[2].rotation.eulerAngles;
//			data.Index.Bone3Position = index.bones[3].position;
//			data.Index.Bone3Rotation = index.bones[3].rotation.eulerAngles;
//			
//			data.Middle = new FingerData();
//			data.Middle.TipPosition = middle.GetTipPosition();
//			data.Middle.DistalDirection = middle.GetBoneDirection((int) Bone.BoneType.TYPE_DISTAL);
//			data.Middle.IntermediateDirection = middle.GetBoneDirection((int) Bone.BoneType.TYPE_INTERMEDIATE);
//			data.Middle.ProximalDirection = middle.GetBoneDirection((int) Bone.BoneType.TYPE_PROXIMAL);
//			data.Middle.MetaCarpalDirection = middle.GetBoneDirection((int) Bone.BoneType.TYPE_METACARPAL);
//		
//			data.Middle.Bone1Position = middle.bones[1].position;
//			data.Middle.Bone1Rotation = middle.bones[1].rotation.eulerAngles;
//			data.Middle.Bone2Position = middle.bones[2].position;
//			data.Middle.Bone2Rotation = middle.bones[2].rotation.eulerAngles;
//			data.Middle.Bone3Position = middle.bones[3].position;
//			data.Middle.Bone3Rotation = middle.bones[3].rotation.eulerAngles;
//			
//			data.Ring = new FingerData();
//			data.Ring.TipPosition = ring.GetTipPosition();
//			data.Ring.DistalDirection = ring.GetBoneDirection((int) Bone.BoneType.TYPE_DISTAL);
//			data.Ring.IntermediateDirection = ring.GetBoneDirection((int) Bone.BoneType.TYPE_INTERMEDIATE);
//			data.Ring.ProximalDirection = ring.GetBoneDirection((int) Bone.BoneType.TYPE_PROXIMAL);
//			data.Ring.MetaCarpalDirection = ring.GetBoneDirection((int) Bone.BoneType.TYPE_METACARPAL);
//		
//			data.Ring.Bone1Position = ring.bones[1].position;
//			data.Ring.Bone1Rotation = ring.bones[1].rotation.eulerAngles;
//			data.Ring.Bone2Position = ring.bones[2].position;
//			data.Ring.Bone2Rotation = ring.bones[2].rotation.eulerAngles;
//			data.Ring.Bone3Position = ring.bones[3].position;
//			data.Ring.Bone3Rotation = ring.bones[3].rotation.eulerAngles;
//			
//			data.Pinky = new FingerData();
//			data.Pinky.TipPosition = pinky.GetTipPosition();
//			data.Pinky.DistalDirection = pinky.GetBoneDirection((int) Bone.BoneType.TYPE_DISTAL);
//			data.Pinky.IntermediateDirection = pinky.GetBoneDirection((int) Bone.BoneType.TYPE_INTERMEDIATE);
//			data.Pinky.ProximalDirection = pinky.GetBoneDirection((int) Bone.BoneType.TYPE_PROXIMAL);
//			data.Pinky.MetaCarpalDirection = pinky.GetBoneDirection((int) Bone.BoneType.TYPE_METACARPAL);
//		
//			data.Pinky.Bone1Position = pinky.bones[1].position;
//			data.Pinky.Bone1Rotation = pinky.bones[1].rotation.eulerAngles;
//			data.Pinky.Bone2Position = pinky.bones[2].position;
//			data.Pinky.Bone2Rotation = pinky.bones[2].rotation.eulerAngles;
//			data.Pinky.Bone3Position = pinky.bones[3].position;
//			data.Pinky.Bone3Rotation = pinky.bones[3].rotation.eulerAngles;
//				

			handDatas.Add(data);

		return data;
	}
	
}