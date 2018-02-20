//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Leap;
//using Leap.Unity;
//using UnityEngine;
//
//public class GhostHand : MonoBehaviour
//{
//
//
//	public float GhostTime;
//
//	public RiggedHand RiggedHand;
//	///public GameObject Transform;
//
//	public HandSync HandSync;
//
//	void Awake()
//	{
//	}
//	
//	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		//handsync must be created by network, currently this will only pick up one hand.
//		if (HandSync == null)
//		{
//			HandSync = GameObject.FindObjectOfType<HandSync>();
//		}
//
//		if (HandSync == null) return;
//		if (RiggedHand == null) return;
//
//		HandData? datan = HandMath.GetMostRecentHandDataFromTime(HandSync.LeftHandDatas,Network.time - GhostTime);
//
//		if (datan == null) return;
//
//		HandData data = (HandData) datan;
//
//		//Transform.transform.position = data.PalmPosition + new Vector3(0f,-0.3f,0.3f);
//
//		RiggedHand.palm.transform.position = data.PalmPosition;
//		RiggedHand.palm.transform.eulerAngles= data.PalmWorldEulerAngles;
//
//		//RiggedHand.palm.transform.rotation.SetFromToRotation(Vector3.zero, data.PalmDirection);
//		
//		RiggedFinger thumb = null;
//		RiggedFinger index = null;
//		RiggedFinger middle = null;
//		RiggedFinger ring = null;
//		RiggedFinger pinky = null;
//		
//		foreach (var fingerModel in RiggedHand.fingers)
//		{
//			var f = (RiggedFinger) fingerModel;
//			switch (f.fingerType)
//			{
//				case Finger.FingerType.TYPE_THUMB:
//					thumb = f;
//					break;
//				case Finger.FingerType.TYPE_INDEX:
//					index = f;
//					break;
//				case Finger.FingerType.TYPE_MIDDLE:
//					middle = f;
//					break;
//				case Finger.FingerType.TYPE_RING:
//					ring = f;
//					break;
//				case Finger.FingerType.TYPE_PINKY:
//					pinky = f;
//					break;
//				case Finger.FingerType.TYPE_UNKNOWN:
//					break;
//				default:
//					throw new ArgumentOutOfRangeException();
//			}
//		}
//
//		thumb.bones[1].position = data.Thumb.Bone1Position;
//		thumb.bones[1].eulerAngles = data.Thumb.Bone1Rotation;
//		thumb.bones[2].position = data.Thumb.Bone2Position;
//		thumb.bones[2].eulerAngles = data.Thumb.Bone2Rotation;
//		thumb.bones[3].position = data.Thumb.Bone3Position;
//		thumb.bones[3].eulerAngles = data.Thumb.Bone3Rotation;
//		
//		index.bones[index.bones.Length - 1].position = data.Index.TipPosition;
//		
//		middle.bones[middle.bones.Length - 1].position = data.Middle.TipPosition;
//		
//		ring.bones[ring.bones.Length - 1].position = data.Ring.TipPosition;
//		
//		pinky.bones[pinky.bones.Length - 1].position = data.Pinky.TipPosition;
//
//		//RiggedHand.palm.transform.eulerAngles = data.PalmWorldEulerAngles;
//		//RiggedHand.modelPalmFacing = data.PalmDirection;
//		//RiggedHand.palm.transform.localEulerAngles = data.PalmDirection;
//
//		//Debug.Log(data.PalmDirection);
//
//
//	}
//}
