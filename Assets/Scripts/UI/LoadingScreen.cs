using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{

    public float WaitUntilStart;
    void Start()
    {
        StartCoroutine("LoadScreen");
        Time.timeScale = 1f;
    }

    IEnumerator LoadScreen()
    {
        yield return new WaitForSecondsRealtime(WaitUntilStart);
        SceneManager.LoadSceneAsync(2);
    }
}
