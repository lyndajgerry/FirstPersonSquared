//
//using MessagePack;
//using UnityEngine;
//
//[System.Serializable]
//[MessagePackObject(keyAsPropertyName: true)]
//public struct FingerData
//{
//    public Vector3 TipPosition;
//		
//    public Vector3 DistalDirection;
//
//    public Vector3 IntermediateDirection;
//
//    public Vector3 ProximalDirection;
//
//    public Vector3 MetaCarpalDirection;
//
//    public Vector3 Bone1Position;
//    public Vector3 Bone2Position;
//    public Vector3 Bone3Position;
//
//    public Vector3 Bone1Rotation;
//    public Vector3 Bone2Rotation;
//    public Vector3 Bone3Rotation;
//
////    public float CalculateFullFingerAngle()
////    {
////        float angle = 0f;
////
////        angle += Vector3.Angle(MetaCarpalDirection,ProximalDirection);
////        angle += Vector3.Angle(ProximalDirection, IntermediateDirection);
////        angle += Vector3.Angle(IntermediateDirection, DistalDirection);
////
////        return angle;
////    }
//}
