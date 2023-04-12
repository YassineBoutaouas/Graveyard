using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneBarEvents : MonoBehaviour
{
    private Animator _animator;
    public GameObject VFX_success;

    private ParticleSystem _timeLineVFX;
    private Animator _buttonAnimator;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetAttackHandler());
        _animator = GetComponent<Animator>();
        _buttonAnimator = GlobalHUDManager.Instance.GetHUDElement("BeatBarProcedural").ImageElements["BeatButtonInput"].GetComponent<Animator>();

        _timeLineVFX = Instantiate(VFX_success, transform).GetComponent<ParticleSystem>();
    }

    void TriggerBoneBarEvents(float f, AudioSpectrumManager.BeatEvaluation b)
    {
        switch (b)
        {
            case AudioSpectrumManager.BeatEvaluation.Perfect:
                _animator.SetInteger("BeatValue", 2);
                StartCoroutine(ResetTrigger());
                _timeLineVFX.Play();

                break;
            case AudioSpectrumManager.BeatEvaluation.Good:
                _animator.SetInteger("BeatValue", 1);
                StartCoroutine(ResetTrigger());
                _timeLineVFX.Play();

                break;
            case AudioSpectrumManager.BeatEvaluation.Bad:
                _animator.SetInteger("BeatValue", -1);
                StartCoroutine(ResetTrigger());

                break;
            default:
                break;

        }

        //StartCoroutine("ResetAnimationTrigger");
    }

    void BeatPulsating()
    {
        Vector3 oldScale = transform.localScale;
        transform.rotation = new Quaternion(90f, 0, 0,1);
        StartCoroutine(BeatPulsatingTime(oldScale.x));
    }
    
    IEnumerator BeatPulsatingTime(float oldScale)
    {
        yield return new WaitForSeconds(0.1f);
        transform.rotation = new Quaternion(0, 0, 0, 1);
    }

    IEnumerator GetAttackHandler()
    {
        yield return new WaitForSeconds(1);
        GameManager.Instance.PlayerController.CharacterAttackHandler.OnBeatAttack += TriggerBoneBarEvents;
        GameManager.Instance.PlayerController.CharacterAttackHandler.OnBeatInput += (arg1, arg2) => _buttonAnimator.Play("BeatButton_Pressed");
        _buttonAnimator.Play("BeatButton_Idle");

        AudioSpectrumManager.Instance.OnBeatPlay += BeatPulsating;
    }

    IEnumerator ResetTrigger()
    {
        int x = 0;

        while (x <1)
        {
            x++;
            yield return new WaitForSeconds(0.09f);
        }

       _animator.SetInteger("BeatValue", 0);

    }
}
