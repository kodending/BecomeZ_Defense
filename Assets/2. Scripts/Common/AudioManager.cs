using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviourPunCallbacks
{
    public static AudioManager am;

    public Slider sdVolume;

    [Header("BGM")]
    public AudioClip[] bgmClips;
    public float bgmVolume;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect;

    [Header("SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    AudioSource[] sfxPlayers;
    int channelIndex;

    [Header("CHEER_SFX")]
    public AudioClip[] cheerClips;
    public float cheerVolume;
    public AudioSource[] cheerSources;
    int cheerChannelIndex;
    //public List<PlayerController> listPC = new List<PlayerController>();
    //AudioSource[] cheerPlayers;

    Dictionary<PlayerController, AudioSource> dicPlayers = new Dictionary<PlayerController, AudioSource>();

    private void Awake()
    {
        am = this;
        DontDestroyOnLoad(am);
        Init();
    }

    void Init()
    {
        //배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();

        //효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.SetParent(transform);
        sfxPlayers = new AudioSource[channels];

        for (int idx = 0; idx< sfxPlayers.Length; idx++)
        {
            sfxPlayers[idx] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[idx].playOnAwake = false;
            sfxPlayers[idx].bypassListenerEffects = true;
            sfxPlayers[idx].volume = sfxVolume;
        }

        //치어리더 효과음 플레이어 초기화
        GameObject cheerObject = new GameObject("CheerPlayer");
        cheerObject.transform.SetParent(transform);
        //cheerPlayers = new AudioSource[userNum];

        //for (int idx = 0; idx < cheerPlayers.Length; idx++)
        //{
        //    cheerPlayers[idx] = cheerObject.AddComponent<AudioSource>();
        //    cheerPlayers[idx].playOnAwake = false;
        //    cheerPlayers[idx].bypassListenerEffects = true;
        //    cheerPlayers[idx].volume = cheerVolme;
        //    listPC[idx] = null;
        //}
    }

    static public void PlayBGM(BGM eBgm, bool isPlay)
    {
        am.bgmPlayer.clip = am.bgmClips[(int)eBgm];

        if(isPlay) am.bgmPlayer.Play();
        else am.bgmPlayer.Stop();
    }

    static public void EffectBGM(bool isPlay) => am.bgmEffect.enabled = isPlay;

    static public void PlaySfx(SFX eSfx)
    {
        for (int idx = 0; idx < am.sfxPlayers.Length; idx++)
        {
            int loopIdx = (idx + am.channelIndex) % am.sfxPlayers.Length;

            if (am.sfxPlayers[loopIdx].isPlaying) continue;

            am.channelIndex = loopIdx;
            am.sfxPlayers[loopIdx].clip = am.sfxClips[(int)eSfx];
            am.sfxPlayers[loopIdx].Play();
            break;
        }
    }

    static public void PlayCheerSfx(CHEERBGM eCheerBgm, PlayerController pc, bool isPlay)
    {
        if (!pc.m_pv.IsMine) return;

        //처음 튼거면 PC 리스트에 넣어주고 
        if (!am.dicPlayers.ContainsKey(pc))
        {
            AudioSource audio = am.cheerSources[am.cheerChannelIndex];
            audio.playOnAwake = false;
            audio.bypassListenerEffects = true;
            audio.volume = am.cheerVolume;
            am.dicPlayers.Add(pc, audio);
            am.cheerChannelIndex++;
        }

        am.dicPlayers[pc].clip = am.cheerClips[(int)eCheerBgm];

        if (isPlay) am.dicPlayers[pc].Play();
        else am.dicPlayers[pc].Stop();
    }

    static public void ClearCheerSfx()
    {
        if (!GameManager.gm.m_pcLocal.m_pv.IsMine) return;

        foreach (var sfx in am.cheerSources)
        {
            sfx.Stop();
        }

        am.cheerChannelIndex = 0;
    }

    static public void ChangeVolume()
    {
        foreach (var sfx in am.cheerSources)
        {
            sfx.volume = am.cheerVolume * (am.sdVolume.value / 100);
        }

        foreach (var sfx in am.sfxPlayers)
        {
            sfx.volume = am.sfxVolume * (am.sdVolume.value / 100);
        }

        am.bgmPlayer.volume = am.bgmVolume * (am.sdVolume.value / 100);

        List<Dictionary<string, object>> optionInfo = CSVManager.instance.m_dicData[LOCALDATALOADTYPE.OPTIONINFO].recordDataList;

        optionInfo[0]["SOUND"] = am.sdVolume.value;

        CSVManager.instance.SaveFile(LOCALDATALOADTYPE.OPTIONINFO, optionInfo);
    }
}
