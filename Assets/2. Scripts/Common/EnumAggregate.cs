using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//각종 enum문 정의한 곳
public enum UISTATE
{
    LOAD,
    MAIN,
    FIELD,
    COSTUME,
    READY,
    ENTERING_FIELD,
    ENTERING_LOGIN,
    ENTERING_READY,
    INGAME,
    LOBBY,
    _MAX_
}

public enum GMSTATE
{
    MAIN,
    FIELD,
    PHASE_READY,
    PHASE_START,
    RESULT,
    LOBBY,
    _MAX_
}

public enum PLAYERSTATE
{
    IDLE,
    MOVE,
    ATTACK,
    JUMP,
    FALLING,
    LANDING,
    _MAX_
}

public enum UNITSTATE
{
    ENTRY,
    IDLE,
    ATTACK,
    _MAX_
}

public enum ENEMYSTATE
{
    RUN,
    DEATH,
    _MAX_
}



public enum COSTUMETYPE
{
    Hat,
    Hair,
    Eyebrow,
    Glasses,
    Mustache,
    Backpack,
    Outerwear,
    Glove,
    Pants,
    Shoe,
    FullBody,
    Body,
    _MAX_
}

public enum OPTIONINFO
{
    SAVED,
    MAIL,
    _MAX_
}


public enum LOCALDATALOADTYPE
{
    COSTUMEINFO,
    OPTIONINFO,
    ENEMYINFO,
    UNITINFO,
    CARDINFO,
    GAMBLEINFO,
    COSTINFO,
    GAMEROUNDINFO,
    _MAX_
}

public enum UIDIRECTION
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum SAVELOADMODE
{
    SAVE,
    LOAD,
    NONE
}

public enum ENEMYSPAWNSPOT
{
    LeftFlags,
    RightFlags,
    CenterFlags,
    LeftAirFlags,
    RightAirFlags,
    _MAX_
}

public enum POOLTYPE
{
    PLAYER,
    UNIT,
    ENEMY,
    UNITBULLET,
    GAMBLEBOX,
    _MAX_
}

public enum UNITTYPE
{
    MELEE_SINGLE = 1,
    MELEE_MULTI,
    RANGED_SINGLE,
    RANGED_MULTI,
    BUFFER,
    SPECIAL_MELEE,
    SPECIAL_RANGED,
    SPECIAL_BUFFER,
    _MAX_
}

public enum UNITRANK
{
    COMMON = 1,
    UNCOMMON,
    RARE,
    EPIC,
    SUPER_EPIC,
    UNIQUE,
    LEGEND,
    _MAX_
}

public enum USERROOMSTATE
{
    WAIT,
    READY,
    _MAX_
}

public enum UNITBULLETTYPE
{
    SINGLE_NORMAL = 1,
    SINGLE_EPIC,
    MULTI_NORMAL,
    MULTI_EPIC,
    _MAX_
}

public enum EFFECTTYPE
{
    UNITATTACK,
    PLAYERATTACK,
    _MAX_
}

public enum PLAYERTYPE
{
    COMMONS,
    BOXER,
    CHEERLEADER,
    _MAX_
}

public enum CARDTYPE
{
    BOOM,
    ATTACKPOWER,
    ATTACKSPEED,
    ATTACKRANGE,
    MOVESPEED,
    TARGETCOUNT,
    _MAX_
}

public enum CARDRANK
{
    NORMAL = 1,
    RARE,
    EPIC,
    _MAX_
}

public enum GAMBLERANK
{ 
    BOOM,
    NORMAL,
    RARE,
    EPIC,
    UNIQUE,
    _MAX_
}

public enum SFX
{
    LOGO,
    BUTTON,
    BOXER_HIT_JAB,
    BOXER_HIT_RIGHT,
    UNIT_RANGED,
    UNIT_RANGED_HIT,
    UNIT_RANGED_SPECIAL_HIT,
    UNIT_MELEE,
    UNIT_MELEE_SPECIAL,
    UNIT_BUFFER,
    ENEMY_DIE,
    LIFE_HIT,
    UPGRADE,
    EVOLUTION,
    GAMBLE_OPEN,
    GAMBLE_SUCCESS,
    CARD_SELECT,
    CARD_ROTATION,
    GAMESTART,
    GAMEOVER,
    GAMEVICTORY,
    _MAX_
}

public enum BGM
{
    MAIN,
    LOBBY,
    INGAME,
    _MAX_
}

public enum CHEERBGM
{
    BilliJEANS,
    UPTOWNPUNK,
    _MAX_
}

public enum GAMERESULT
{
    VICTORY,
    GAMEOVER,
    _MAX_
}
















