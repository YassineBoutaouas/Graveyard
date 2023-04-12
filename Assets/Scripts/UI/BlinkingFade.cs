using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkingFade : MonoBehaviour
{
    [Range(0.5f, 5)]
    public float speed = 1f;
    [Range(0, 1)]
    public float opacityMin = 0f;
    [Range(0, 1)]
    public float opacityMax = 1f;

    TextMeshProUGUI ownText;

    void Start()
    {
        ownText = gameObject.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        ownText.alpha = (Mathf.Sin(Time.time * speed) * opacityMax) /2 + 0.5f + opacityMin ;
    }
}
