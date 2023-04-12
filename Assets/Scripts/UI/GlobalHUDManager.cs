using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalHUDManager : HUDManager
{
    public static GlobalHUDManager Instance;

    public event Action<HUDStates> OnHUDStateChanged;
    public void ChangeHUDState(HUDStates state) { CurrentHUDState = state; OnHUDStateChanged?.Invoke(state); }

    public enum HUDStates { None, PauseMenu, GameOver, WinningScreen, Tutorial };
    public HUDStates CurrentHUDState = HUDStates.None;

    public bool LockCursor = false;
    private InputManager _inputManager;

    public override void Awake()
    {
        base.Awake();
        Instance = this;

        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        _inputManager = InputManager.GetInstance();
        _inputManager.Initialize();

        _inputManager.inputActions.InGame.Pause.performed += EnableMenu;
        _inputManager.inputActions.UI.Pause.performed += DisableMenu;
    }

    public void OnDestroy()
    {
        _inputManager.inputActions.InGame.Pause.performed -= EnableMenu;
        _inputManager.inputActions.UI.Pause.performed -= DisableMenu;
    }

    public void DisableMenu(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (CurrentHUDState == HUDStates.PauseMenu) EnableHUDElement("PauseMenu", false);
    }

    public void EnableMenu(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (CurrentHUDState == HUDStates.None) EnableHUDElement("PauseMenu", true);
    }
}