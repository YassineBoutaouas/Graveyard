using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

public class SetCharacterConstraints : StateMachineBehaviour
{
    protected CharacterHandler characterHandler;


    [Header("On Enter")]
    [Space(5)]
    public bool Enter_CanRotate = true;
    public bool Enter_CanMove = true;

    [Header("On Exit")]
    public bool Exit_Rotate = true;
    public bool Exit_Move = true;

    public float ExitPoint = 0.44f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        CharacterHandler character = animator.GetComponent<CharacterHandler>();

        character.CanRotate = Enter_CanRotate;
        character.CanMove = Enter_CanMove;

        character.StartCoroutine(ExitConstraint(stateInfo, character));
    }

    private IEnumerator ExitConstraint(AnimatorStateInfo stateInfo, CharacterHandler character)
    {
        float t = stateInfo.normalizedTime;
        while (t <= ExitPoint)
        {
            yield return null;
            t += Time.deltaTime;
        }

        character.CanRotate = Exit_Rotate;
        character.CanMove = Exit_Move;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}