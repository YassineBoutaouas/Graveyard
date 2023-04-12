using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimatorParameters : StateMachineBehaviour
{
    public AnimatorParameter[] AnimatorParameters;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        foreach (AnimatorParameter parameter in AnimatorParameters)
        {
            if (!parameter.ModifyOnEnter) continue;

            AnimatorControllerParameterType parameterType = animator.GetParameter(parameter.ParameterIndex).type;

            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.ParameterName, parameter.Value_OnEnter);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.ParameterName, (int)parameter.Value_OnEnter);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.ParameterName, parameter.Value_OnEnter != 0);
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        foreach (AnimatorParameter parameter in AnimatorParameters)
        {
            if (!parameter.ModifyOnExit) continue;

            AnimatorControllerParameterType parameterType = animator.GetParameter(parameter.ParameterIndex).type;

            Debug.Log(animator.GetParameter(parameter.ParameterIndex).name);

            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.ParameterName, parameter.Value_OnExit);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.ParameterName, (int)parameter.Value_OnExit);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.ParameterName, parameter.Value_OnExit != 0);
                    break;
                default:
                    break;
            }
        }
    }
}

[System.Serializable]
public class AnimatorParameter
{
    public int ParameterIndex;
    public string ParameterName;
    [Space(5)]
    public bool ModifyOnEnter;
    public float Value_OnEnter;
    [Space(5)]
    public bool ModifyOnExit;
    public float Value_OnExit;
}
