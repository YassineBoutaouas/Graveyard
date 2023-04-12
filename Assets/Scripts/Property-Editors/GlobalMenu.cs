using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class GlobalMenu :MonoBehaviour
{
    //[MenuItem("Team Tromboners/Rename particles")]
    //public static void RenameAsset()
    //{
    //    string name = Selection.activeObject.name;
    //    if (Selection.activeObject.name.Contains("pf_vfx-ult_demo_psys_loop_"))
    //    {
    //        name.Replace("pf_vfx-ult_demo_psys_loop_", "");
    //        Selection.activeObject.name = AssetDatabase.RenameAsset(Selection.activeObject.name, name);
    //    }
    //}

    [MenuItem("GameObject/UI/Descriptive Slider", false)]
    public static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject slider = Instantiate(Resources.Load<GameObject>("DescriptiveSlider"));
        slider.name.Remove("DescriptiveSlider".Length, "(Clone)".Length);
        slider.transform.SetParent(Selection.activeTransform);
        slider.transform.position = Vector3.zero;
        slider.transform.localScale = Vector3.one;

        Selection.activeGameObject = slider;
    }

    [MenuItem("Edit/UI/Rename Slider", false)]
    public static void RenameSlider(MenuCommand command)
    {
        GameObject slider = Selection.activeGameObject;
        RenameHierarchy(slider);
    }

    public static void RenameHierarchy(GameObject slider)
    {
        for (int i = 0; i < slider.transform.childCount; i++)
        {
            slider.transform.GetChild(i).gameObject.name = slider.transform.GetChild(i).gameObject.name.Replace("Name", slider.name);
            RenameHierarchy(slider.transform.GetChild(i).gameObject);
        }
    }
}
#endif