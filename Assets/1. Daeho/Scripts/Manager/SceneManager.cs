using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    Coroutine change_scene;
    Image fade_image_prefab;

    private void Awake()
    {
        name = nameof(SceneManager);
    }

    void Start()
    {
        fade_image_prefab = Resources.Load<Image>("fade image");

        StartCoroutine(FadingScreen(0));
    }

    void Update()
    {

    }

    public void ChangeScene(string scene_name)
    {
        if (change_scene != null) return;
        change_scene = StartCoroutine(_ChangeScene(scene_name));
    }

    IEnumerator _ChangeScene(string scene_name)
    {
        yield return StartCoroutine(FadingScreen(1));

        LoadScene(scene_name);
    }

    IEnumerator FadingScreen(float alpha)
    {
        Image fade_image = Instantiate(fade_image_prefab, FindObjectOfType<Canvas>().transform);
        fade_image.color = new Color(1, 1, 1, Mathf.Abs(1 - alpha));

        while (Mathf.Abs(alpha - fade_image.color.a) > 0.01f)
        {
            fade_image.color = Color.Lerp(fade_image.color, new Color(1, 1, 1, alpha), 0.1f);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
