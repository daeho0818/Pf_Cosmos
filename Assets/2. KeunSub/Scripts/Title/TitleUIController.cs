using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUIController : Singleton<TitleUIController>
{

    public Slider soundEffectSlider;
    public Slider backgroundSoundSlider;

    public float soundEffectVolume;
    public float backgroundVolume;

    public Image Setting;
    Animator settingAnim;

    public string dataPath;

    Status SaveStatus = new Status();

    protected override void Awake()
    {
        dataPath = Application.persistentDataPath + "./Status.json";
    }

    void Start()
    {

        SaveStatus = SaveStatus.LoadData(dataPath);
        if(File.Exists(dataPath))
        {
            soundEffectVolume = SaveStatus.soundEffectVolume;
            backgroundVolume = SaveStatus.backgroundVolume;

            soundEffectSlider.value = soundEffectVolume;
            backgroundSoundSlider.value = backgroundVolume;
        }

        settingAnim = Setting.gameObject.GetComponent<Animator>();
        Setting.gameObject.SetActive(false);
    }

    void Update()
    {
        soundEffectVolume = soundEffectSlider.value;
        backgroundVolume = backgroundSoundSlider.value;

        SoundManager.Instance.backgroundVolume = backgroundVolume;
        SoundManager.Instance.effectVolume = soundEffectVolume;
    }

    public void SettingOn()
    {
        Setting.gameObject.SetActive(true);
        settingAnim.SetInteger("State", 1);
    }

    public void SettingOff()
    {
        settingAnim.SetInteger("State", 2);
    }

    public void SettingSetactive()
    {
        settingAnim.SetInteger("State", 0);
        Setting.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        SaveStatus.SetValue(soundEffectVolume, backgroundVolume, dataPath);
        SaveStatus.SaveData(SaveStatus);
    }
}

[System.Serializable]
public class Status
{
    public float soundEffectVolume;
    public float backgroundVolume;

    public string dataPath;

    public void SetValue(float _soundVolume, float _backgroundVolume, string _dataPath)
    {
        soundEffectVolume = _soundVolume;
        backgroundVolume = _backgroundVolume;
        dataPath = _dataPath;
    }

    public void SetValue(float _soundVolume, float _backgroundVolume)
    {
        soundEffectVolume = _soundVolume;
        backgroundVolume = _backgroundVolume;
    }

    public void SetValue(string _dataPath)
    {
        dataPath = _dataPath;
    }

    public void SaveData(object _object)
    {
        string jsonData = JsonUtility.ToJson(_object, true);
        File.WriteAllText(dataPath, jsonData);
    }

    public Status LoadData(string _dataPath)
    {
        Status retObj = new Status();

        if(File.Exists(_dataPath))
        {
            string jsonData = File.ReadAllText(_dataPath);
            retObj = JsonUtility.FromJson<Status>(jsonData);
        }
        else
        {
            Debug.Log("no data exists: " + _dataPath);
        }

        return retObj;
    }
}