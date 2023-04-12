using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    private static InputManager Instance;
    [Header("Action Maps")]
    public InputActions inputActions;

    public Vector2 MoveInput;
    public Vector2 LookInput;

    private bool _initialized;
    public bool Initialized { get { return _initialized; } set { _initialized = value; } }

    /// <summary>
    /// Get the instance of this input manager
    /// </summary>
    /// <returns></returns>
    public static InputManager GetInstance()
    {
        if (Instance == null) Instance = new InputManager();
        return Instance;
    }

    /// <summary>
    /// Enable a certain input action map
    /// </summary>
    /// <param name="map"></param>
    public void EnableActionMap(InputActionMap map)
    {
        map.Enable();
    }

    /// <summary>
    /// Disable a certain action map
    /// </summary>
    /// <param name="map"></param>
    public void DisableActionMap(InputActionMap map)
    {
        map.Disable();
    }

    /// <summary>
    /// Switch between two action maps
    /// </summary>
    /// <param name="enabledMap"> Map that will be enabled</param>
    /// <param name="disabledMap"> Map that will be disabled</param>
    public void SwitchActionMap(InputActionMap enabledMap, InputActionMap disabledMap)
    {
        DisableActionMap(disabledMap);
        EnableActionMap(enabledMap);
    }

    public void Initialize()
    {
        if (_initialized) return;

        inputActions = new InputActions();
        EnableActionMap(inputActions.InGame);

        inputActions.InGame.Look.performed += ctx => ReadVectorInput(ctx, out LookInput);
        inputActions.InGame.Move.performed += ctx => ReadVectorInput(ctx, out MoveInput);

        _initialized = true;
    }

    private void ReadVectorInput(InputAction.CallbackContext ctx, out Vector2 targetVector)
    {
        targetVector = ctx.ReadValue<Vector2>();
    }

    public void DisposeAllActions(InputActionMap map)
    {
        foreach (InputAction action in map.actions)
        {
            action.Dispose();
        }
    }
}
