using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioTimeLine : MonoBehaviour
{
    //public Color[] ColorIndicators = new Color[System.Enum.GetValues(typeof(AudioSpectrumManager.BeatEvaluation)).Length];
    public int Segments;
    public GameObject BoneObject;

    public AnimationCurve MoveInterpolation;
    public AnimationCurve FadeInCurve;
    public AnimationCurve FadeOutCurve;

    private List<BoneBarBone> _leftBoneBarBones = new List<BoneBarBone>();
    private List<BoneBarBone> _rightBoneBarBones = new List<BoneBarBone>();

    private RectTransform _rectTransform;
    private float _timeToBeat;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _timeToBeat = 1.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);

        for (int i = 0; i < Segments - 1; i++)
            CreateBone("_Left", i, new Vector3((_rectTransform.anchoredPosition.x - (_rectTransform.rect.width / 2)) + ((_rectTransform.rect.width / 2) / (Segments - 1) * i), 0f, 0f), 1, _leftBoneBarBones);

        for (int i = 0; i < Segments - 1; i++)
            CreateBone("_Right", i, new Vector3((_rectTransform.anchoredPosition.x + (_rectTransform.rect.width / 2)) - ((_rectTransform.rect.width / 2) / (Segments - 1) * i), 0f, 0f), -1, _rightBoneBarBones);

        StartCoroutine(StartBoneTimeLine());
    }

    private IEnumerator StartBoneTimeLine()
    {
        float t = 0;

        Image firstBone_left = _leftBoneBarBones[0].BoneImage;
        Image lastBone_left = _leftBoneBarBones[_leftBoneBarBones.Count - 1].BoneImage;
        Image firstBone_right = _rightBoneBarBones[0].BoneImage;
        Image lastBone_right = _rightBoneBarBones[_rightBoneBarBones.Count - 1].BoneImage;

        while (enabled)
        {
            yield return null;

            while (t < _timeToBeat)
            {
                yield return null;
                t += Time.deltaTime;

                for (int i = 0; i < _leftBoneBarBones.Count; i++)
                {
                    Vector2 leftTarget = _leftBoneBarBones[i].Position + (Vector3.right * ((_rectTransform.rect.width / 2) / (Segments - 1)));
                    Vector2 rightTarget = _rightBoneBarBones[i].Position - (Vector3.right * ((_rectTransform.rect.width / 2) / (Segments - 1)));

                    _leftBoneBarBones[i].BoneTransform.anchoredPosition = Vector3.Lerp(_leftBoneBarBones[i].Position, leftTarget, MoveInterpolation.Evaluate(t / _timeToBeat));
                    _rightBoneBarBones[i].BoneTransform.anchoredPosition = Vector3.Lerp(_rightBoneBarBones[i].Position, rightTarget, MoveInterpolation.Evaluate(t / _timeToBeat));

                    ChangeBoneColor(_leftBoneBarBones[i].BoneImage, (100 / (Segments - 1)) * i, (100 / (Segments - 1)) * (1 + i),t, FadeInCurve);
                    ChangeBoneColor(_rightBoneBarBones[i].BoneImage, (100 / (Segments - 1)) * i, (100 / (Segments - 1)) * (1 + i), t, FadeInCurve);
                }

                 ChangeBoneColor(lastBone_left, 100, 0, t, FadeOutCurve);
                 ChangeBoneColor(lastBone_right, 100, 0, t, FadeOutCurve);
            }

            yield return null;
            t = 0;
        }
    }

    private void CreateBone(string name, int index, Vector3 position, int orientation, List<BoneBarBone> bonesList)
    {
        GameObject bone = Instantiate(BoneObject);

        bone.name = "Bone(" + index + ")" + name;
        bone.transform.SetParent(transform);

        RectTransform boneRect = bone.GetComponent<RectTransform>();
        boneRect.anchoredPosition3D = position;
        boneRect.localScale = new Vector3(orientation * 1.6f, 1, 1) * 0.5f;
        bone.SetActive(true);

        bonesList.Add(new BoneBarBone(boneRect, boneRect.anchoredPosition3D, bone.GetComponent<Image>()));
    }

    
    private void ChangeBoneColor(Image img, float startValue, float endValue, float t, AnimationCurve curve)
    {
       img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(startValue/100, endValue/100, curve.Evaluate(t / _timeToBeat)));
    }

  
}

[System.Serializable]
public class BoneBarBone
{
    public RectTransform BoneTransform;
    public Vector3 Position;
    public Image BoneImage;

    public BoneBarBone(RectTransform g, Vector3 position, Image image)
    {
        BoneTransform = g;
        Position = position;
        BoneImage = image;
    }
}