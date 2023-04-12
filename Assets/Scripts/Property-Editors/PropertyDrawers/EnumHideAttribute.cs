using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class EnumHideAttribute : PropertyAttribute
{
    public string ConditionalSource = "";
    public int ShowValue = 0;
    public bool HideInInspector = false;

    public EnumHideAttribute(string conditionalSource, int showValue, bool hide = false)
    {
        ConditionalSource = conditionalSource;
        ShowValue = showValue;
        HideInInspector = hide;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumHideAttribute hideAttribute = (EnumHideAttribute)attribute;
        bool enable = GetConditionalHideResult(hideAttribute, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enable;

        if (!hideAttribute.HideInInspector || enable)
            EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnumHideAttribute hideAttribute = (EnumHideAttribute)attribute;
        bool enable = GetConditionalHideResult(hideAttribute, property);

        if (!hideAttribute.HideInInspector || enable)
            return EditorGUI.GetPropertyHeight(property, label);
        else
            return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool GetConditionalHideResult(EnumHideAttribute hideAttribute, SerializedProperty property)
    {
        bool enabled = hideAttribute.HideInInspector;

        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, hideAttribute.ConditionalSource);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
            enabled = sourcePropertyValue.enumValueIndex == hideAttribute.ShowValue;
        else
            Debug.LogWarning("Attempting to use conditional hide without valid source property: " + hideAttribute.ConditionalSource);

        return enabled;
    }
}
#endif