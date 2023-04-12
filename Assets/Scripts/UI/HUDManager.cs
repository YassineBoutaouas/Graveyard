using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDManager : MonoBehaviour
{
    public List<HUDElementController> HUDElements = new List<HUDElementController>();

    public event Action<string, bool> OnHudElementEnabled;

    public void EnableHUDElement(string name, bool active) { OnHudElementEnabled?.Invoke(name, active); }

    public virtual void Awake()
    {
        foreach (HUDElementController HUDElement in transform.GetComponentsInChildren<HUDElementController>(true))
        {
            HUDElement.Initialize(this);
            HUDElements.Add(HUDElement);
        }
    }

    public HUDElementController GetHUDElement(string name)
    {
        return this.HUDElements.Find(e => e.ElementName == name);
    }
}
