using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScreen : HUDElementController
{
    public static string MainMenuSceneName = "01_MainMenu";

    private InputManager _inputManager;
    private SoundHandler _soundHandler;

    private Animator _animator;

    private Sound _losingSound;

    public override void OnEnable()
    {
        base.OnEnable();
        _animator = GetComponent<Animator>();
        _animator.Play("GameOverScreen");

        GlobalHUDManager.Instance.ChangeHUDState(GlobalHUDManager.HUDStates.GameOver);
        GlobalHUDManager.Instance.EnableHUDElement("HUD_FX", false);

        if (_soundHandler == null)
        {
            _soundHandler = GetComponent<SoundHandler>();
            _soundHandler.PlaySound("LoosingSound");
        }

        _soundHandler = GetComponent<SoundHandler>();
        _soundHandler.GetSound("LoosingSound", out _losingSound);
        StartCoroutine(WaitForSound());
    }

    private IEnumerator WaitForSound()
    {
        yield return new WaitUntil(() => _losingSound.Source != null);
        _soundHandler.PlaySound("LoosingSound");
    }

    public override void Start()
    {
        ButtonElements["StartAgain_Button"].onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); });
        ButtonElements["BackToMenu_Button"].onClick.AddListener(() => { GlobalHUDManager.Instance.CurrentHUDState = GlobalHUDManager.HUDStates.None; Time.timeScale = 1f; AudioManager.Instance.OnChangedVolume(); SceneManager.LoadScene(0); });

        _inputManager = InputManager.GetInstance();
        _inputManager.SwitchActionMap(_inputManager.inputActions.UI, _inputManager.inputActions.InGame);

        GameManager.Instance.PlayerController.Velocity = Vector3.zero;
        GameManager.Instance.PlayerController.CanMove = false;
        GameManager.Instance.PlayerController.CanRotate = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(ButtonElements["StartAgain_Button"].gameObject);
    }
}