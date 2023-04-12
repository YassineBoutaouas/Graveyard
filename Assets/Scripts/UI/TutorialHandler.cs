using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialHandler : HUDElementController
{
    public int PageCount = 6;

    private InputManager _inputManager;
    private HUDElementController _currentTutorialPage;
    private List<HUDElementController> _tutorialPages = new List<HUDElementController>();

    public override void OnEnable()
    {
        #region Disable all menus
        GlobalHUDManager.Instance.EnableHUDElement("PauseMenuMain", false);
        GlobalHUDManager.Instance.EnableHUDElement("SettingsMain", false);
        GlobalHUDManager.Instance.EnableHUDElement("CameraSettings", false);
        GlobalHUDManager.Instance.EnableHUDElement("AudioSettings", false);
        GlobalHUDManager.Instance.EnableHUDElement("ControlSettings", false);
        GlobalHUDManager.Instance.EnableHUDElement("Quit?", false);
        #endregion

        base.OnEnable();

        for (int i = 0; i < PageCount; i++)
            _tutorialPages.Add(GlobalHUDManager.Instance.GetHUDElement("TutorialPage0" + (i+1).ToString()));

        _currentTutorialPage = GlobalHUDManager.Instance.GetHUDElement("TutorialPage01");
        GlobalHUDManager.Instance.EnableHUDElement("TutorialPage01", true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ButtonElements["NextButton"].gameObject);

        ButtonElements["NextButton"].onClick.AddListener(() => SwitchToNextPage());
        ButtonElements["PreviousButton"].onClick.AddListener(() => SwitchToPreviousPage());

        GlobalHUDManager.Instance.GetHUDElement("TutorialPage07").ButtonElements["SnappingButton"].onClick.AddListener(() => { GameManager.Instance.Settings.AutoTargeting = true; DisableElement(); });
        GlobalHUDManager.Instance.GetHUDElement("TutorialPage07").ButtonElements["NoSnappingButton"].onClick.AddListener(() => { GameManager.Instance.Settings.AutoTargeting = false; DisableElement(); });

        GlobalHUDManager.Instance.ChangeHUDState(GlobalHUDManager.HUDStates.Tutorial);

        _inputManager = InputManager.GetInstance();
        _inputManager.SwitchActionMap(_inputManager.inputActions.UI, _inputManager.inputActions.InGame);

        ButtonElements["PreviousButton"].gameObject.SetActive(_currentTutorialPage != GlobalHUDManager.Instance.GetHUDElement("TutorialPage01"));
    }

    public void SwitchToNextPage()
    {
        int index = _tutorialPages.IndexOf(_currentTutorialPage) + 1;
        if (!index.IsInRange(0, PageCount)) return;

        GlobalHUDManager.Instance.EnableHUDElement(_currentTutorialPage.ElementName, false);
        _currentTutorialPage = _tutorialPages[index];
        GlobalHUDManager.Instance.EnableHUDElement(_currentTutorialPage.ElementName, true);

        ButtonElements["NextButton"].gameObject.SetActive(_currentTutorialPage != GlobalHUDManager.Instance.GetHUDElement("TutorialPage07"));
        ButtonElements["PreviousButton"].gameObject.SetActive(_currentTutorialPage != GlobalHUDManager.Instance.GetHUDElement("TutorialPage01"));

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ButtonElements["NextButton"].gameObject.activeSelf ? ButtonElements["NextButton"].gameObject : ButtonElements["PreviousButton"].gameObject);
    }

    public void SwitchToPreviousPage()
    {
        int index = _tutorialPages.IndexOf(_currentTutorialPage) - 1;
        if (!index.IsInRange(-1, PageCount)) return;

        GlobalHUDManager.Instance.EnableHUDElement(_currentTutorialPage.ElementName, false);
        _currentTutorialPage = _tutorialPages[index];
        GlobalHUDManager.Instance.EnableHUDElement(_currentTutorialPage.ElementName, true);

        ButtonElements["NextButton"].gameObject.SetActive(_currentTutorialPage != GlobalHUDManager.Instance.GetHUDElement("TutorialPage07"));
        ButtonElements["PreviousButton"].gameObject.SetActive(_currentTutorialPage != GlobalHUDManager.Instance.GetHUDElement("TutorialPage01"));

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ButtonElements["NextButton"].gameObject.activeSelf ? ButtonElements["NextButton"].gameObject : ButtonElements["PreviousButton"].gameObject);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        foreach (HUDElementController controller in _tutorialPages)
            controller.gameObject.SetActive(false);

        _tutorialPages.Clear();

        ButtonElements["NextButton"].onClick.RemoveAllListeners();
        ButtonElements["PreviousButton"].onClick.RemoveAllListeners();

        _currentTutorialPage = null;

        if(_inputManager != null)
            _inputManager.SwitchActionMap(_inputManager.inputActions.InGame, _inputManager.inputActions.UI);

        GlobalHUDManager.Instance.EnableHUDElement("PauseMenu", false);

        GlobalHUDManager.Instance.ChangeHUDState(GlobalHUDManager.HUDStates.None);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
