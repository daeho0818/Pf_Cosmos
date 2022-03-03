using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{

    public List<AudioClip> backgroundAudioList = new List<AudioClip>();
    public List<AudioClip> effectAudioList = new List<AudioClip>();
    public List<AudioClip> generalAudioList = new List<AudioClip>();

    Dictionary<string, AudioClip> backgroundAudio = new Dictionary<string, AudioClip>();
    Dictionary<string, AudioClip> effectAudio = new Dictionary<string, AudioClip>();
    Dictionary<string, AudioClip> generalAudio = new Dictionary<string, AudioClip>();

    AudioSource backgroundAudioSouce;

    public float backgroundVolume;
    public float effectVolume;

    void Start()
    {
        backgroundAudioSouce = GetComponent<AudioSource>();
        InitialDictionary();

        PlayAudio("Background", 0);
    }

    void Update()
    {
        backgroundAudioSouce.volume = backgroundVolume;
    }

    void InitialDictionary()
    {
        foreach (var item in backgroundAudioList)
        {
            backgroundAudio.Add(item.name, item);
        }

        foreach (var item in effectAudioList)
        {
            effectAudio.Add(item.name, item);
        }

        foreach (var item in generalAudioList)
        {
            generalAudio.Add(item.name, item);
        }
    }

    /// <summary>
    /// play audio by clip
    /// </summary>
    /// <param name="_clip"></param>
    public void PlayAudio(AudioClip _clip)
    {
        GameObject clipObj = new GameObject(_clip.name);
        AudioSource _audio = clipObj.AddComponent<AudioSource>();
        _audio.clip = _clip;
        _audio.Play();
        Destroy(clipObj, _clip.length);
    }

    /// <summary>
    /// play audio by clip
    /// type = 0: background || 1: effect || 2: general 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    public void PlayAudio(string key, int type)
    {
        GameObject audioObj = new GameObject(key);
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.volume = effectVolume;

        switch (type)
        {
            case 0:
                backgroundAudioSouce.clip = backgroundAudio[key];
                backgroundAudioSouce.Play();
                break;
            case 1:
                audioSource.clip = effectAudio[key];
                audioSource.Play();
                Destroy(audioObj, effectAudio[key].length);
                break;
            case 2:
                audioSource.clip = generalAudio[key];
                audioSource.Play();
                Destroy(audioObj, generalAudio[key].length);
                break;
            default:
                break;
        }
    }

    public void ButtonPress(bool cancel = false)
    {
        PlayAudio(cancel ? "Button_Cancel" : "Button_2", 1);
    }
}
