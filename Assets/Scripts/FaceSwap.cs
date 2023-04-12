using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSwap : MonoBehaviour
{

        //Set these Textures in the Inspector
        public Texture m_idle, m_shortScream, m_longScream, m_choir01, m_choir02,m_choir03,m_stunned,m_angry;
        SkinnedMeshRenderer m_Renderer;
        //public bool isChange;
        public bool isChoir;
        public float currenttimer;
        float faceCounter = 1;
        public float faceSwapFrequency;
        
        public enum Emotion{
            idle,
            shortScream,
            longScream,
            choir,
            stunned,
            angry,
        }
        public Emotion currentEmotion;

        // Use this for initialization
        void Awake()
        {

            m_Renderer = GetComponent<SkinnedMeshRenderer>();

        }
    
        private void Update()
        {
            /*if (isChange)
            {
                SetEmotion(currentEmotion);
                isChange = false;
            }*/

            //might be perfomance heavy? --> put it in state
             if (isChoir)
             {
                if (Time.time > currenttimer)
                {
                    currenttimer = Time.time + faceSwapFrequency;
                    faceCounter++;

                    if (faceCounter == 1)
                    {
                         m_Renderer.material.SetTexture("_MainTex", m_choir01);
                    }
                    else if(faceCounter ==2)
                    {
                        m_Renderer.material.SetTexture("_MainTex", m_choir02);
                        faceCounter = 0;
                }
                    else
                    {
                        m_Renderer.material.SetTexture("_MainTex", m_choir03);
                        faceCounter = 0;
                    }

                }
             }
        }

    public void SetEmotion(Emotion newEmotion)
    {
        //just for debugging face; can be cut out later
        isChoir = false;

    
        //changing and checking new emotion texture
        if (newEmotion == Emotion.idle)
        {
            m_Renderer.material.SetTexture("_MainTex", m_idle);
        }
        else if (newEmotion == Emotion.shortScream)
        {
            StartCoroutine(shortScream());
        }
        else if (newEmotion == Emotion.choir)
        {
            isChoir = true;
            currenttimer = Time.time + 3f;
        }
        else if (newEmotion == Emotion.longScream)
        {
            m_Renderer.material.SetTexture("_MainTex", m_longScream);
        }
        else if (newEmotion == Emotion.angry)
        {
            m_Renderer.material.SetTexture("_MainTex", m_angry);
        }
        else if(newEmotion == Emotion.stunned)
        {
            m_Renderer.material.SetTexture("_MainTex", m_stunned);
        }
    }

    public IEnumerator shortScream()
    {
        m_Renderer.material.SetTexture("_MainTex", m_shortScream);
        yield return new WaitForSeconds(0.5f);
        SetEmotion(Emotion.idle);
    }
    
}
