using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap;
using UnityEngine;
using UnityEngine.Timeline;


//jonathon hart
//jonathon.hart@mymail.unisa.edu.au

public static class HandMath 
{

//    public static Dictionary<HandData, Vector3> CalculateVelocities(List<HandData>  handData)
//    {
//        Dictionary<HandData, Vector3> velocities = new Dictionary<HandData, Vector3>();
//        List<HandData> handDatas = GetOrderedList(handData);
//        for (int i = 0; i < handDatas.Count; i++)
//        {
//            if (i == 0)
//            {
//                velocities.Add(handData[i],Vector3.zero);
//                continue;
//            }
//            velocities.Add(handData[i],handData[i].PalmPosition-handData[i-1].PalmPosition);
//        }
//
//        return velocities;
//    }


    public static List<HandData> GetOrderedList(List<HandData>  handData)
    {
        return handData.OrderBy(x => x.NetworkTimeStamp).ToList();
    }

    public static HandData GetMostRecentHandDataFromTime(List<HandData>  handData,double time)
    {
        List<HandData> handDatas = GetOrderedList(handData);
        foreach (HandData hd in handDatas)
        {
            if (hd.NetworkTimeStamp > time) return hd;
        }

        return null;
    }

}
