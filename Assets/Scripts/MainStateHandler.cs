using System;
using System.Collections;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MainStateHandler : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject mainCamera;
    public AudioSource audioSource;
    
    [Header("Hand menu buttons")]
    public PressableButton startButton;
    public PressableButton nodeButton;
    public PressableButton saveButton;
    public PressableButton cancelButton;

    private TextToSpeechSubsystem _tts;
    
    public static bool DoingTutorial;
    private bool _isMovingMenuToCenter;
    private RoutineController _routineController;

    private int _currentTutorialStage;

    
    // Start is called before the first frame update
    void Start()
    {
        mainMenu.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
        _tts = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();
        
        if (startButton != null) startButton.OnClicked.AddListener(StartButtonPressed);
    }
    
    void OnDestroy()
    {
        if (startButton != null) startButton.OnClicked.RemoveListener(StartButtonPressed);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (mainMenu.activeSelf) UpdateMainMenuPosition();
    }
    
    void Speak(string text)
    {
        if (_tts != null && audioSource != null) _tts.TrySpeak(text, audioSource);
    }

    IEnumerator WaitSpeaking()
    {
        yield return new WaitUntil(() => audioSource.isPlaying);
        yield return new WaitUntil(() => !audioSource.isPlaying);
    }

    IEnumerator WaitUntilOrTimeout(Func<bool> condition, float timeoutSeconds)
    {
        float timer = 0f;
        while (timer < timeoutSeconds && !condition())
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }


    IEnumerator WaitForFirstButton()
    {
        Speak("The first option is the one that let's you walk in your room while recording the positions you visit.");
        yield return WaitSpeaking();
        Speak("Click the start button");
        yield return WaitSpeaking();
        startButton.enabled = true;
        while (true)
        {
            if (_currentTutorialStage == 2) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == 2, 5);
            if (_currentTutorialStage == 2) break;
            Speak("Click the start button");
            yield return WaitSpeaking();
        }
        startButton.enabled = false;
        Speak("Walk in your room by creating a long enough path. The path is being drawn in your feet.");
        yield return WaitSpeaking();
    }

    IEnumerator WaitForLookHand()
    {
        while (true)
        {
            if (_currentTutorialStage == 1) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == 1, 5);
            if (_currentTutorialStage == 1) break;
            Speak("To access the menu, look at your hand's palm.");
            yield return WaitSpeaking();
        }
        Speak("In this menu you have four options. Let's start with the first one.");
        yield return WaitSpeaking();
        StartCoroutine(WaitForFirstButton());
    }

    IEnumerator TutorialWelcomeCorountine()
    {
        Speak("Welcome to the Pepper Robot Routine Planner");
        yield return WaitSpeaking();
        Speak("Every option in this application must be accessed from the menu.");
        yield return WaitSpeaking();
        Speak("To access the menu, look at your hand's palm.");
        yield return WaitSpeaking();
        StartCoroutine(WaitForLookHand());
    }

    public void StartTutorial()
    {
        DoingTutorial = true;
        startButton.enabled = false;
        nodeButton.enabled = false;
        saveButton.enabled = false;
        cancelButton.enabled = false;
        StartCoroutine(TutorialWelcomeCorountine());
    }

    public void LookingAtHand()
    {
        if (!DoingTutorial) return;
        if (_currentTutorialStage >= 1) return;
        _currentTutorialStage = 1;
    }


    void StartButtonPressed()
    {
        if (_currentTutorialStage >= 2) return;
        _currentTutorialStage = 2;
    }


    private void UpdateMainMenuPosition()
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward;
        Quaternion targetRotation = mainCamera.transform.rotation;
        
        Vector3 directionToMenu = (mainMenu.transform.position - mainCamera.transform.position).normalized;
        float angleToMenu = Vector3.Angle(mainCamera.transform.forward, directionToMenu);
        
        bool isInFieldOfView = angleToMenu < 100f;
            
        if (!isInFieldOfView && !_isMovingMenuToCenter)
        {
            _isMovingMenuToCenter = true;
        }
        
        if (_isMovingMenuToCenter)
        {
            float easeSpeed = 2f;
            mainMenu.transform.position = Vector3.Lerp(mainMenu.transform.position, targetPosition, Time.deltaTime * easeSpeed);
            mainMenu.transform.rotation = Quaternion.Slerp(mainMenu.transform.rotation, targetRotation, Time.deltaTime * easeSpeed);
            
            if (Vector3.Distance(mainMenu.transform.position, targetPosition) < 0.1f)
            {
                _isMovingMenuToCenter = false;
            }
        }
    }
}
