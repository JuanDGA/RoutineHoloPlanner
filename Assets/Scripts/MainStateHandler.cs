using System;
using System.Collections;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class MainStateHandler : MonoBehaviour
{
    private enum TutorialStage 
    {
        OpeningHandMenu,
        PressingStartButton,
        Walking,
        AddingNode,
        Calibrating,
        FinalWalking
    }
    
    
    public GameObject mainMenu;
    public GameObject mainCamera;
    public AudioSource audioSource;
    
    [Header("Calibration menu")]
    public GameObject calibrationMenu;

    public PressableButton calibrateButton;
    
    [Header("Hand menu buttons")]
    public PressableButton startButton;
    public PressableButton nodeButton;
    public PressableButton saveButton;
    public PressableButton cancelButton;
    public PressableButton continueButton;
    
    [Header("Paths Drawing")]
    public GameObject linesParent;
    public Material pathsMaterial;
    public float lastStepDistance = 0.05f;
    public float planeSize = 0.05f;
    public float smoothFactor = 0.1f;

    private TextToSpeechSubsystem _tts;
    
    // Tutorial variables
    public static bool DoingTutorial;
    private TutorialStage _currentTutorialStage = TutorialStage.OpeningHandMenu;
    
    // Main menu variables
    private bool _isMovingMenuToCenter;

    
    // Start is called before the first frame update
    void Start()
    {
        mainMenu.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
        _tts = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();
        
        if (startButton != null) startButton.OnClicked.AddListener(StartButtonPressed);
        if (calibrateButton != null) calibrateButton.OnClicked.AddListener(CalibrateButtonPressed);
        if (nodeButton != null) nodeButton.OnClicked.AddListener(AddNodeButtonPressed);
        if (saveButton != null) saveButton.OnClicked.AddListener(SaveButtonPressed);
    }
    
    void OnDestroy()
    {
        if (startButton != null) startButton.OnClicked.RemoveListener(StartButtonPressed);
    }


    // Update is called once per frame
    void Update()
    {
        PathsController.Compute(mainCamera, smoothFactor, lastStepDistance);
    }

    private void FixedUpdate()
    {
        if (mainMenu.activeSelf) UpdateMenuPosition(mainMenu);
        if (calibrationMenu.activeSelf) UpdateMenuPosition(calibrationMenu);
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

    IEnumerator WaitForCompleteRoutineButton()
    {
        
    }

    IEnumerator WaitForAddNodeButton()
    {
        Speak("Now you walked enough distance. In the hand menu, press the Add node button.");
        yield return WaitSpeaking();
        nodeButton.enabled = true;
        Speak("You can continue drawing your path by walking if you want to.");
        while (true)
        {
            if (_currentTutorialStage == TutorialStage.FinalWalking) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == TutorialStage.FinalWalking, 20);
            if (_currentTutorialStage == TutorialStage.FinalWalking) break;
            Speak("Remember to click the Add node button in the menu in order proceed with the tutorial.");
            yield return WaitSpeaking();
        }
        PathsController.Deactivate(mainCamera, false);
        Speak("Perfect! Now you have created the first node in your path.");
        yield return WaitSpeaking();
        Speak("In each node you will be able to configure the animation and the speech that you want the robot to say.");
        yield return WaitSpeaking();
        Speak("The options will appear on each node once you decide to complete the routine.");
        yield return WaitSpeaking();
        Speak("Now you can keep walking and adding new nodes or you can save the routine by clicking the save routine button.");
        yield return WaitSpeaking();
        PathsController.Activate(mainCamera, pathsMaterial, planeSize, linesParent);
        StartCoroutine(WaitForCompleteRoutineButton());
    }


    IEnumerator WaitForLongPath()
    {
        Speak("Walk in your room by creating a long enough path. The path is being drawn in your feet.");
        yield return WaitSpeaking();
        PathsController.Activate(mainCamera, pathsMaterial, planeSize, linesParent);
        
        while (true)
        {
            if (PathsController.CurrentPathDistance(3.0f) >= 3.0f) break;
            yield return WaitUntilOrTimeout(() => PathsController.CurrentPathDistance(3.0f) >= 3.0f, 10);
            if (PathsController.CurrentPathDistance(3.0f) >= 3.0f) break;
            Speak("Walk in your room by creating a long enough path. The path is being drawn in your feet.");
            yield return WaitSpeaking();
        }
        _currentTutorialStage = TutorialStage.AddingNode;
        StartCoroutine(WaitForAddNodeButton());
    }

    IEnumerator WaitForCalibration()
    {
        Speak("Please, walk to the position in which the robot will start, and look at the direction in which the robot will start looking at. Then, click the calibrate button.");
        yield return WaitSpeaking();
        while (true)
        {
            if (_currentTutorialStage == TutorialStage.Walking) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == TutorialStage.Walking, 10);
            if (_currentTutorialStage == TutorialStage.Walking) break;
            Speak("Please, walk to the position in which the robot will start, and look at the direction in which the robot will start looking at. Then, click the calibrate button.");
            yield return WaitSpeaking();
        }
        StartCoroutine(WaitForLongPath());
    }


    IEnumerator WaitForStartButton()
    {
        Speak("The first option is the one that let's you walk in your room while recording the positions you visit.");
        yield return WaitSpeaking();
        Speak("Click the start button");
        yield return WaitSpeaking();
        startButton.enabled = true;
        while (true)
        {
            if (_currentTutorialStage == TutorialStage.Calibrating) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == TutorialStage.Calibrating, 5);
            if (_currentTutorialStage == TutorialStage.Calibrating) break;
            Speak("Click the start button");
            yield return WaitSpeaking();
        }
        startButton.enabled = false;
        startButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        continueButton.enabled = false;
        Speak("The start position is the most important, this application assumes that the point in which you start the routine, in the same direction that you are, is the same position and direction in which the robot will start moving.");
        yield return WaitSpeaking();
        calibrationMenu.SetActive(true);
        StartCoroutine(WaitForCalibration());
    }

    IEnumerator WaitForLookHand()
    {
        while (true)
        {
            if (_currentTutorialStage == TutorialStage.OpeningHandMenu) break;
            yield return WaitUntilOrTimeout(() => _currentTutorialStage == TutorialStage.OpeningHandMenu, 5);
            if (_currentTutorialStage == TutorialStage.OpeningHandMenu) break;
            Speak("To access the menu, look at your hand's palm.");
            yield return WaitSpeaking();
        }
        Speak("In this menu you have four options. Let's start with the first one.");
        yield return WaitSpeaking();
        StartCoroutine(WaitForStartButton());
    }

    IEnumerator TutorialWelcomeCoroutine()
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
        cancelButton.gameObject.SetActive(false);
        StartCoroutine(TutorialWelcomeCoroutine());
    }

    public void LookingAtHand()
    {
        if (!DoingTutorial) return;
        if (_currentTutorialStage != TutorialStage.OpeningHandMenu) return;
        _currentTutorialStage = TutorialStage.PressingStartButton;
    }


    void StartButtonPressed()
    {
        if (!DoingTutorial || _currentTutorialStage != TutorialStage.PressingStartButton) return;
        if (DoingTutorial) _currentTutorialStage = TutorialStage.Calibrating;
    }

    void CalibrateButtonPressed()
    {
        PathsController.SetReference(mainCamera.transform.position, mainCamera.transform.rotation);
        if (!DoingTutorial || _currentTutorialStage != TutorialStage.Calibrating) return;
        if (DoingTutorial) _currentTutorialStage = TutorialStage.Walking;
    }


    void AddNodeButtonPressed()
    {
        if (!DoingTutorial || _currentTutorialStage != TutorialStage.PressingStartButton) return;
        if (DoingTutorial) _currentTutorialStage = TutorialStage.Calibrating;
    }


    private void UpdateMenuPosition(GameObject menu)
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward;
        Quaternion targetRotation = mainCamera.transform.rotation;
        
        Vector3 directionToMenu = (menu.transform.position - mainCamera.transform.position).normalized;
        float angleToMenu = Vector3.Angle(mainCamera.transform.forward, directionToMenu);
        
        bool isInFieldOfView = angleToMenu < 40f;
            
        if (!isInFieldOfView && !_isMovingMenuToCenter)
        {
            _isMovingMenuToCenter = true;
        }
        
        if (_isMovingMenuToCenter)
        {
            float easeSpeed = 2f;
            menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * easeSpeed);
            menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * easeSpeed);
            
            if (Vector3.Distance(menu.transform.position, targetPosition) < 0.01f)
            {
                _isMovingMenuToCenter = false;
            }
        }
    }
}
