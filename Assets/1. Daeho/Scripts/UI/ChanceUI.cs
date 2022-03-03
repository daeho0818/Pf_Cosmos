using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChanceUI : MonoBehaviour
{
    ChanceManager chance_manager;

    [SerializeField] RectTransform rocket_parent;
    [SerializeField] TextMeshProUGUI chance_timer_text;
    void Start()
    {
        chance_manager = GameObject.Find(ChanceManager.manager_name).GetComponent<ChanceManager>();

        foreach (RectTransform rocket in rocket_parent)
        {
            rocket.gameObject.SetActive(false);
        }

        // 남은 플레이 횟수
        for (int i = 0; i < chance_manager.chance_count; i++)
        {
            rocket_parent.GetChild(i).gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (chance_manager.chance_up)
        {
            rocket_parent.GetChild(chance_manager.chance_count - 1).gameObject.SetActive(true);
            chance_manager.chance_up = false;
        }

        chance_timer_text.text = $"{chance_manager.chance_timer / 60} : {chance_manager.chance_timer % 60:00.##}";
    }
}
