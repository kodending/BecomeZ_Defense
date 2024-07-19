using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWeight
{
    public static Dictionary<string, object> RandomItem (List<Dictionary<string, object>> dataList)
    {
        //만약 항목이 없으면 기본값 반환
        if (dataList.Count == 0)
            return null;

        // 모든 가중치를 더하여 총합을 구함
        float totalWeight = 0f;
        foreach(var item in dataList)
        {
            totalWeight += float.Parse(item["WEIGHT"].ToString());
        }

        float randomValue = UnityEngine.Random.value * totalWeight;

        foreach( var item in dataList)
        {
            randomValue -= float.Parse(item["WEIGHT"].ToString());
            if (randomValue < 0f)
                return item;
        }

        return null;
    }
}
