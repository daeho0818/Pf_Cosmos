using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanceManager : MonoBehaviour
{
    public static string manager_name { get; } = "ChanceManager (DDOL)";

    public int chance_count { get; private set; } = 1;
    public int chance_timer { get; private set; } = 60;
    public bool chance_up { get; set; } = false;

    System.DateTime now_time;
    void Awake()
    {
        GameObject changeManager = GameObject.Find(manager_name);
        if (changeManager && name != manager_name)
        {
            Destroy(gameObject);
        }
        else if (!changeManager)
        {
            DontDestroyOnLoad(gameObject);
            name = manager_name;

            chance_count = PlayerPrefs.GetInt("Chance Count", 1);
            chance_timer = PlayerPrefs.GetInt("Chance Timer", 60);
        }
    }

    private void Start()
    {
        // SetChanceCount();

        InvokeRepeating(nameof(CountDown), 1, 1);
    }

    void Update()
    {
        if (chance_count >= 5) chance_count = 5;
    }

    void SetChanceCount()
    {
        now_time = System.DateTime.Now;

        if (now_time.Year > PlayerPrefs.GetInt("Quit Year", 2021))
            chance_count = 5;
        else if (now_time.Month > PlayerPrefs.GetInt("Quit Month", 12))
            chance_count = 5;
        else if (now_time.Day > PlayerPrefs.GetInt("Quit Day", 16))
            chance_count = 5;
        else if (now_time.Hour > PlayerPrefs.GetInt("Quit Hour", 6))
            chance_count = 5;
        else
        {
            int second = PlayerPrefs.GetInt("Quit Second");
            int minute = PlayerPrefs.GetInt("Quit Minute");

            if (now_time.Minute > minute && now_time.Second >= second)
                chance_count += now_time.Minute - minute;
            else if (now_time.Minute > minute)
                chance_count += now_time.Minute - minute - 1;

        }

        if (chance_count >= 5) chance_count = 5;
    }

    void CountDown()
    {
        if (chance_timer <= 0)
        {
            chance_count++;
            chance_timer = 60;
            chance_up = true;
            if (chance_count >= 5)
                CancelInvoke(nameof(CountDown));
            return;
        }

        chance_timer--;
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("Chance Count", chance_count);
        PlayerPrefs.SetInt("Chance Timer", chance_timer);

        // now_time = System.DateTime.Now;
        // 
        // PlayerPrefs.SetInt("Quit Year", now_time.Year);
        // PlayerPrefs.SetInt("Quit Month", now_time.Month);
        // PlayerPrefs.SetInt("Quit Day", now_time.Day);
        // PlayerPrefs.SetInt("Quit Hour", now_time.Hour);
        // PlayerPrefs.SetInt("Quit Minute", now_time.Minute);
        // PlayerPrefs.SetInt("Quit Second", now_time.Second);
    }

    public void MinusCount()
    {
        if (chance_count > 0)
            chance_count--;
    }
}