using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleDirector : MonoBehaviour
{
    [SerializeField] RectTransform logo_transform;
    //[SerializeField] TextMeshProUGUI start_text;
    void Start()
    {

    }

    float sin_value = 0.5f;
    void Update()
    {
        // sin_value -= Time.deltaTime;
        // start_text.color = new Color(1, 1, 1, Mathf.Sin(sin_value) / 2 + 0.5f);

        sin_value += Time.deltaTime * 150;

        logo_transform.position += new Vector3(0, Mathf.Sin(sin_value * Mathf.Deg2Rad) / 2);
    }
}
