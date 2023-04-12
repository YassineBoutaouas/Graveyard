using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDElementController : MonoBehaviour, IHUDElement
{
    public string ElementName = "New HUD-Element";
    public float TimeScale;
    public bool SlowTimeOnEnable;

    public Dictionary<string, Image> ImageElements = new Dictionary<string, Image>();
    public Dictionary<string, TMPro.TextMeshProUGUI> TextElements = new Dictionary<string, TMPro.TextMeshProUGUI>();
    public Dictionary<string, Button> ButtonElements = new Dictionary<string, Button>();
    public Dictionary<string, Toggle> ToggleElements = new Dictionary<string, Toggle>();
    public Dictionary<string, Slider> SliderElements = new Dictionary<string, Slider>();

    private HUDManager _hudManager;

    private float _initialTimeScale;

    public void Initialize(HUDManager hudManager)
    {
        _hudManager = hudManager;
        _hudManager.OnHudElementEnabled += Enable;

        FetchElementsOfType(ImageElements);
        FetchElementsOfType(TextElements);
        FetchElementsOfType(ButtonElements);
        FetchElementsOfType(ToggleElements);
        FetchElementsOfType(SliderElements);
    }

    private void FetchElementsOfType<T>(Dictionary<string, T> elements) where T : Behaviour
    {
        foreach (T t in transform.GetComponentsInChildren<T>(true))
            elements.Add(t.gameObject.name, t);
    }

    public virtual void OnEnable()
    {
        _initialTimeScale = Time.timeScale;
        if (SlowTimeOnEnable) Time.timeScale = TimeScale;
    }

    public virtual void Start() { }

    public void Enable(string controllerName, bool enabled)
    {
        if (controllerName != ElementName) return;
        gameObject.SetActive(enabled);
    }

    public virtual void DisableElement()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnDisable()
    {
        if(SlowTimeOnEnable) Time.timeScale = _initialTimeScale;
    }
}
