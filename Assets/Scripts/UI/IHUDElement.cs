using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHUDElement
{
    public void Initialize(HUDManager hudManager);
    public void OnEnable();
    public void OnDisable();
    public void Enable(string controller, bool enabled);
}
