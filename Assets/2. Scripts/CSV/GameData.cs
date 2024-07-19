using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] //인스펙터 창에 클래스나 구조체 정보 노출하기 위함

public class GameData
{
    //게임 데이터 저장관리하는 곳

    //게임 데이터 리스트화

    public List<Dictionary<string, object>> recordDataList = new List<Dictionary<string, object>>();

    //게임 데이터 카테고리 타이틀 저장
    public List<Dictionary<int, object>> recordTitleList = new List<Dictionary<int, object>>();
}
