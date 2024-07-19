using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//각종 구조체 정의한 곳
public struct ENEMYINFO
{
    public int idx;
    public int hp;
    public int curHp;
    public int def;
    public float spd;
    public float accel;
    public int atk;
    public int gold;
}

public struct UNITINFO
{
    public UNITTYPE eType;
    public UNITRANK eRank;
    public float animLength;
    public int atk;
    public float atkSpeed;
    public float atkRange;
    public int curLV;
    public int LvAtk;
}

public struct ATTACKINFO
{
    public EnemyFSM enemyFSM;
    public string strAttackerName;
    public int iDamage;
}

public struct BUFFINFO
{
    public int buffAtk;
    public float buffAtkSpd;
    public float buffAtkRange;
}

public struct PLAYERKILLINFO
{
    public string strName;
    public int iKillCount;
}

public struct PLAYERINFO
{
    public int atk;
    public int targetCnt;
    public float atkRange;
    public float atkSpeed;
    public float runSpeed;
}

public struct GAMEROUNDINFO
{
    public int enemyIndex;
    public int amount;
    public float interval;
    public bool isRandSpawn;
    public ENEMYSPAWNSPOT eSpot;
}


