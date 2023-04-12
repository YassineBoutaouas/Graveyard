using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ExtensionMethods
{
    public static float ConvertBeatsToMilliseconds(this int Beats, int BeatsPerMinute)
    {
        float totalMS = 60000 / BeatsPerMinute;
        return totalMS * Beats;
    }

    public static float ConvertBeatsToSeconds(this int Beats, int BeatsPerMinute)
    {
        return Beats.ConvertBeatsToMilliseconds(BeatsPerMinute) / 1000;
    }

    public static List<string> GetMethodsList(this Behaviour b, out List<string> methodList)
    {
        Type type = b.GetType();
        MethodInfo[] methodsInfo = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(m => m.DeclaringType == type).OrderBy(x => x.Name).ToArray();
        methodList = new List<string>();

        foreach (MethodInfo methodInfo in methodsInfo)
            methodList.Add(methodInfo.Name);

        return methodList;
    }

    public static bool Contains<T>(this T[] array, T item) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == item)
                return true;
        }

        return false;
    }

    public static List<T> ShuffleList<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = UnityEngine.Random.Range(0, i);
            T temp = list[i];
            list[i] = list[random];
            list[random] = temp;
        }

        return list;
    }

    public static bool LayerMaskContainsLayer(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }

    public static bool LayerMaskContainsLayer(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }

    public static List<T> ToList<T>(this T[] array)
    {
        List<T> output = new List<T>();
        output.AddRange(array);
        return output;
    }

    public static GameObject[] GetChildren(this Transform transform)
    {
        GameObject[] children = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i).gameObject;
        }

        return children;
    }

    public static void ClearLog()
    {
#if UNITY_EDITOR
        Assembly assembly = Assembly.GetAssembly(typeof(Editor));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }

    public static void DrawArrow(Vector3 pos, Quaternion rot, float size, Color color, int id = 0)
    {
#if UNITY_EDITOR
        Handles.color = color;
        Handles.ArrowHandleCap(id, pos, rot, HandleUtility.GetHandleSize(pos) + size, EventType.Repaint);
#endif
    }

#if UNITY_EDITOR
    public static void SetSerializedProperty(this SerializedObject serializedObject, out SerializedProperty property, string name)
    {
        property = serializedObject.FindProperty(name);
    }

    public static void DrawScriptField<T>(this Editor editor) where T : MonoBehaviour
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script ", MonoScript.FromMonoBehaviour((MonoBehaviour)editor.target), typeof(T), false);
        GUI.enabled = true;
    }
#endif

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static bool IsInRange(this float value, float lowerBounds, float upperBounds)
    {
        return value > lowerBounds && value < upperBounds;
    }

    public static bool IsInRange(this int value, int lowerBounds, int upperBounds)
    {
        return value > lowerBounds && value < upperBounds;
    }

    public static void SetAgentDestination(this NavMeshAgent agent, Vector3 destination)
    {
        if (!agent.enabled) return;
            agent.SetDestination(destination);
    }

    public static void SetAgentValues(this NavMeshAgent agent, float stoppingDistance, float speed, float acceleration)
    {
        agent.stoppingDistance = stoppingDistance;
        agent.speed = speed;
        agent.acceleration = acceleration;
    }

    #region Animator methods
    public static bool CheckAnimationIsPlaying(this Animator animator, string name, string layerName)
    {
        int layer = animator.GetLayerIndex(layerName);
        return animator.CheckAnimationIsPlaying(name, layer);
    }

    public static bool CheckAnimationIsPlaying(this Animator animator, string name, int layer)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).IsName(name) && animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1;
    }

    public static bool CheckTransitionIsPlaying(this Animator animator, int layerIndex, string sourceState, string targetState)
    {
        return animator.GetAnimatorTransitionInfo(layerIndex).IsName(sourceState + " -> " + targetState);
    }

    public static bool CheckTransitionIsPlaying(this Animator animator, string layerName, string source, string target)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        return animator.CheckTransitionIsPlaying(layerIndex, source, target);
    }

    public static bool CheckTransitionIsPlaying(this Animator animator, int layerIndex, string transitionName)
    {
        return animator.GetAnimatorTransitionInfo(layerIndex).IsName(transitionName);
    }
    #endregion
}