using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameReady : MonoBehaviour
{
    [SerializeField] GameObject ingame_scene;
    [SerializeField] GameObject ready_canvas;
    [SerializeField] GameObject ingame_canvas;

    [SerializeField] TextMeshProUGUI best_score_text;

    [SerializeField] Transform rocket_obj;
    private SpriteRenderer rocket_renderer;

    private Graphic[] uis;

    bool press_start = false;
    void Start()
    {
        rocket_renderer = rocket_obj.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (press_start && rocket_obj)
        {
            rocket_obj.position = Vector2.Lerp(rocket_obj.position, new Vector2(3, 12), 0.003f);
            rocket_renderer.color = Color.Lerp(rocket_renderer.color, new Color(1, 1, 1, 0), 0.05f);

            foreach (var ui in uis)
            {
                ui.color = Color.Lerp(ui.color, new Color(1, 1, 1, 0), 0.05f);
            }

            if (rocket_renderer.color.a <= 0.01f)
            {
                Destroy(rocket_obj.gameObject);

                Invoke(nameof(ShowIngame), 0.3f);
            }
        }

        best_score_text.text = $"{PlayerPrefs.GetInt("BestScore", 0):#,0}";
    }

    public void StartGame()
    {
        press_start = true;
        uis = FindObjectsOfType<Graphic>();

        GameObject.Find(ChanceManager.manager_name).GetComponent<ChanceManager>().MinusCount();
    }

    void ShowIngame()
    {
        gameObject.SetActive(false);
        ready_canvas.SetActive(false);

        ingame_scene.SetActive(true);
        ingame_canvas.SetActive(true);
    }
}
