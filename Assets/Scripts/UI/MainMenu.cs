using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    public GameObject StartScreen;
    public GameObject Menu;
    public GameObject Credits;
    private Animator m_animator;
    private GameObject CreditsButton;
    public GameObject pressToStart;
    SoundHandler m_soundhandler;
    public GameObject FlippingRaver;

    public Animator DoorAnimator;

    // Start is called before the first frame update
    void Start()
    {
        StartScreen.SetActive(true);
        Menu.SetActive(false);
        m_animator = GetComponent<Animator>();
        CreditsButton = Credits.GetComponentInChildren<Button>().gameObject;
        m_soundhandler = GetComponent<SoundHandler>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        //checking if any keyboard button or - if a gamepad is available - any gamepad button is pressed PLUS checking if the startscreen is still there
        if ((Keyboard.current.anyKey.isPressed || (Mouse.current != null && Mouse.current.leftButton.IsPressed()) || (Gamepad.current != null && Gamepad.current.allControls.Any(x => x is ButtonControl button && x.IsPressed() && !x.synthetic)))
             && StartScreen.activeSelf && pressToStart.activeSelf)
        {
            m_animator.SetTrigger("FadeOut");
            Camera.main.GetComponent<Animator>().SetTrigger("isStarted");
            Menu.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            m_soundhandler.PlaySound("StartGame");
            DoorAnimator.SetBool("isDoorOpen", true);
        }

    }

    public void StartGame()
    {
        //loading loading screen
        SceneManager.LoadSceneAsync(1);
        m_soundhandler.PlaySound("StartRealGame");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("quit game!");
    }

    public void ToggleCredits()
    {
        bool oldState = Credits.activeSelf;
        float timer = 0;
        SetCameraPan(!oldState);

        //if going back to main menu..
        if (oldState)

        {   m_soundhandler.PlaySound("DirtOut");
            timer = 0.7f;
            Credits.GetComponent<Animator>().SetTrigger("FadeOut");
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);

        }
        else
        {
            m_soundhandler.PlaySound("DirtIn");
        }

        StartCoroutine(SetCredits(timer, oldState));
    }



    public IEnumerator SetCredits(float timer, bool oldState)
    {
       
        yield return new WaitForSeconds(timer);
        Credits.SetActive(!oldState);

        if (!oldState)
        {
           
            yield return new WaitForSeconds(0.7f);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(CreditsButton);
        }
    }

    public void CameraShake()
    {
        Camera.main.GetComponent<Animator>().SetTrigger("CameraShake");
    }

    public void SetCameraPan(bool isCamUnderground)
    {
        Camera.main.GetComponent<Animator>().SetBool("isMenuSwitched", isCamUnderground);
    }

    public void SetFlippingRaver()
    {
        FlippingRaver.SetActive(true);
    }
}
