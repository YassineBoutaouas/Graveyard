using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WinningScreen : HUDElementController
{
    public float DelayTime = 5;

    private Animator _menuAnimator;
    private InputManager _inputManager;
    private SoundHandler _soundHandler;

    private Sound _winningSound;

    public override void OnEnable()
    {
        base.OnEnable();
        TryGetComponent(out _menuAnimator);
        _menuAnimator.Play("WinningScreen");
        GlobalHUDManager.Instance.EnableHUDElement("HUD_FX", false);
     
        _soundHandler = GetComponent<SoundHandler>();
        _soundHandler.GetSound("WinningSound", out _winningSound);
        StartCoroutine(WaitForSound());
    }

    private IEnumerator WaitForSound()
    {
        yield return new WaitUntil(() => _winningSound.Source != null);
        _soundHandler.PlaySound("WinningSound");
    }

    public void OnScreenEnabled()
    {
        Time.timeScale = 0f;

        ButtonElements["Restart"].onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); });
        ButtonElements["BackToMenu"].onClick.AddListener(() => { GlobalHUDManager.Instance.CurrentHUDState = GlobalHUDManager.HUDStates.None; Time.timeScale = 1f; AudioManager.Instance.OnChangedVolume(); SceneManager.LoadScene(0); });

        TryGetComponent(out _menuAnimator);

        _inputManager = InputManager.GetInstance();
        _inputManager.SwitchActionMap(_inputManager.inputActions.UI, _inputManager.inputActions.InGame);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(ButtonElements["Restart"].gameObject);

    }
}