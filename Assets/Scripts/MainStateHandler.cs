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
    
    [Header("Calibration menu")]
    public GameObject calibrationMenu;

    public PressableButton calibrateButton;
    
    [Header("Hand menu buttons")]
    public PressableButton startButton;
    public PressableButton nodeButton;
    public PressableButton saveButton;
    public PressableButton publishButton;
    public PressableButton restoreButton;
    
    [Header("Paths Drawing")]
    public GameObject nodeMenuPrefab;
    public GameObject linesParent;
    public Material pathsMaterial;
    public float lastStepDistance = 0.05f;
    public float planeSize = 0.05f;

    private TextToSpeechSubsystem _tts;
    
    // Main menu variables
    private bool _isMovingMenuToCenter;
    
    void Start()
    {
        PathsController.SetUp(mainCamera, pathsMaterial, planeSize, linesParent, lastStepDistance, nodeMenuPrefab);
        
        mainMenu.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
        _tts = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();
        
        if (startButton != null)
        {
            startButton.OnClicked.AddListener(StartButtonPressed);
            startButton.enabled = true;
        }
        if (nodeButton != null)
        {
            nodeButton.OnClicked.AddListener(AddNodeButtonPressed);
            nodeButton.enabled = false;
        }
        if (saveButton != null)
        {
            saveButton.OnClicked.AddListener(SaveButtonPressed);
            saveButton.enabled = false;
        }
        if (publishButton != null)
        {
            publishButton.OnClicked.AddListener(PublishButtonPressed);
            publishButton.gameObject.SetActive(false);
        }
        if (restoreButton != null)
        {
            restoreButton.OnClicked.AddListener(RestoreButtonPressed);
            restoreButton.gameObject.SetActive(false);
        }
        if (calibrateButton != null) calibrateButton.OnClicked.AddListener(CalibrateButtonPressed);
    }
    
    void OnDestroy()
    {
        if (startButton != null) startButton.OnClicked.RemoveListener(StartButtonPressed);
        if (nodeButton != null) nodeButton.OnClicked.RemoveListener(AddNodeButtonPressed);
        if (saveButton != null) saveButton.OnClicked.RemoveListener(SaveButtonPressed);
        if (calibrateButton != null) calibrateButton.OnClicked.RemoveListener(CalibrateButtonPressed);
        if (publishButton != null) publishButton.OnClicked.RemoveListener(PublishButtonPressed);
        if (restoreButton != null) restoreButton.OnClicked.RemoveListener(RestoreButtonPressed);
    }


    // Update is called once per frame
    void Update()
    {
        PathsController.Compute();
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

    IEnumerator WaitForSendToRobot()
    {
        Speak("Now you can walk to each node and configure the animation and speech that you want the robot to say.");
        yield return WaitSpeaking();
        Speak("Once you are done, move the Pepper robot to the start position looking into the right direction.");
        yield return WaitSpeaking();
        Speak("Then click the publish button.");
        yield return WaitSpeaking();
        publishButton.enabled = true;
        publishButton.gameObject.SetActive(true);
        while (true)
        {
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Publishing) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Publishing, 20);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Publishing) break;
            Speak("Remember to click the Publish button in the menu in order to complete the tutorial.");
            yield return WaitSpeaking();
        }
        Speak("Great! Now you have sent your first routine to the robot.");
        yield return WaitSpeaking();
        Speak("You have completed the tutorial. You can now create more routines by clicking the Clear button in the menu.");
        yield return WaitSpeaking();

        StateMachine.EndTutorial();
    }

    IEnumerator WaitForCompleteRoutineButton()
    {
        while (true)
        {
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Previewing) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Previewing, 20);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Previewing) break;
            Speak("Remember to click the Save button when you are ready in the menu in order proceed with the tutorial.");
            yield return WaitSpeaking();
        }
        Speak("Great! Now you have completed your first routine.");
        yield return WaitSpeaking();
        StartCoroutine(WaitForSendToRobot());
    }

    IEnumerator WaitForAddNodeButton()
    {
        Speak("Now you walked enough distance. In the hand menu, press the Add node button.");
        yield return WaitSpeaking();
        nodeButton.enabled = true;
        Speak("You can continue drawing your path by walking if you want to.");
        while (true)
        {
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.FinalWalking) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.FinalWalking, 15);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.FinalWalking) break;
            Speak("Remember to click the Add node button in the menu in order to proceed with the tutorial.");
            yield return WaitSpeaking();
        }
        Speak("Perfect! Now you have created the first node in your path.");
        yield return WaitSpeaking();
        Speak("In each node you will be able to configure the animation and the speech that you want the robot to say.");
        yield return WaitSpeaking();
        Speak("The options will appear on each node once you decide to complete the routine.");
        yield return WaitSpeaking();
        Speak("Now you can keep walking and adding new nodes or you can save the routine by clicking the save routine button.");
        yield return WaitSpeaking();
        StartCoroutine(WaitForCompleteRoutineButton());
    }


    IEnumerator WaitForLongPath()
    {
        Speak("Walk in your room by creating a long enough path. The path is being drawn in your feet.");
        yield return WaitSpeaking();
        
        while (true)
        {
            if (PathsController.CurrentPathDistance(3.0f) >= 3.0f) break;
            yield return WaitUntilOrTimeout(() => PathsController.CurrentPathDistance(3.0f) >= 3.0f, 10);
            if (PathsController.CurrentPathDistance(3.0f) >= 3.0f) break;
            Speak("Walk in your room by creating a long enough path. The path is being drawn in your feet.");
            yield return WaitSpeaking();
        }
        StateMachine.TransitionToTutorialStage(StateMachine.TutorialStage.AddingNode);
        StartCoroutine(WaitForAddNodeButton());
    }

    IEnumerator WaitForCalibration()
    {
        Speak("Please, walk to the position in which the robot will start, and look at the direction in which the robot will start looking at. Then, click the calibrate button.");
        yield return WaitSpeaking();
        while (true)
        {
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Walking) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Walking, 10);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Walking) break;
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
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Calibrating) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Calibrating, 5);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.Calibrating) break;
            Speak("Click the start button");
            yield return WaitSpeaking();
        }
        startButton.enabled = false;
        Speak("The start position is the most important, this application assumes that the point in which you start the routine, in the same direction that you are, is the same position and direction in which the robot will start moving.");
        yield return WaitSpeaking();
        calibrationMenu.SetActive(true);
        StartCoroutine(WaitForCalibration());
    }

    IEnumerator WaitForLookHand()
    {
        while (true)
        {
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.PressingStartButton) break;
            yield return WaitUntilOrTimeout(() => StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.PressingStartButton, 5);
            if (StateMachine.CurrentTutorialStage == StateMachine.TutorialStage.PressingStartButton) break;
            Speak("To access the menu, look at your hand's palm.");
            yield return WaitSpeaking();
        }
        Speak("In this menu you have three options. Let's start with the first one.");
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
        StateMachine.StartTutorial();
        startButton.enabled = false;
        nodeButton.enabled = false;
        saveButton.enabled = false;
        StartCoroutine(TutorialWelcomeCoroutine());
    }

    public void LookingAtHand()
    {
        if (!StateMachine.DoingTutorial) return;
        StateMachine.OnHandMenuOpened();
    }


    void StartButtonPressed()
    {
        if (!StateMachine.CanStartCalibration()) return;
        
        StateMachine.StartCalibration();
        calibrationMenu.SetActive(true);
    }

    void CalibrateButtonPressed()
    {
        if (!StateMachine.CanCalibrate()) return;
        
        StateMachine.StartRecording();
        
        PathsController.SetReference(mainCamera.transform.position, mainCamera.transform.rotation);
        
        calibrationMenu.SetActive(false);
        
        saveButton.enabled = true;
        
        PathsController.Activate();
        PathsController.AddNode(1.5f);
        
        if (!StateMachine.DoingTutorial)
        {
            nodeButton.enabled = true;
        }
    }

    void AddNodeButtonPressed()
    {
        Debug.Log("Added node ... " + StateMachine.CurrentRoutineState);
        if (!StateMachine.CanRecord()) return;
        
        PathsController.AddNode();
        
        if (StateMachine.DoingTutorial)
        {
            StateMachine.OnNodeAdded();
        }
    }


    void SaveButtonPressed()
    {
        if (!StateMachine.CanRecord()) return;
        
        PathsController.AddNode(1.3f);
        PathsController.Deactivate();
        saveButton.enabled = false;
        saveButton.gameObject.SetActive(false);
        
        StateMachine.StartPreviewing();
        
        if (!StateMachine.DoingTutorial)
        {
            publishButton.enabled = true;
            publishButton.gameObject.SetActive(true);
        }
    }


    void PublishButtonPressed()
    {
        if (!StateMachine.CanPreview()) return;
        
        var path = PathsController.CurrentPath();
        var path2 = "";
        for (int i = 0; i < path.Item1.Count; i++)
        {
            path2 += path.Item1[i].ToString() + " | ";
        }
        Debug.Log(path2);
        
        ApiService.Routine routine = new  ApiService.Routine
        {
            nodes = path.Item2,
            line = path.Item1
        };
        StartCoroutine(ApiService.PostRoutine(routine));
        
        StateMachine.StartPublishing();
        
        restoreButton.enabled = true;
        restoreButton.gameObject.SetActive(true);
    }

    void RestoreButtonPressed()
    {
        if (!StateMachine.CanPublish()) return;

        StateMachine.RestoreToIdle();
        restoreButton.enabled = false;
        restoreButton.gameObject.SetActive(false);
        publishButton.enabled = false;
        publishButton.gameObject.SetActive(false);
        saveButton.enabled = false;
        nodeButton.enabled = false;
        startButton.enabled = true;
        PathsController.Restore();
    }


    private void UpdateMenuPosition(GameObject menu)
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward;
        Quaternion targetRotation = mainCamera.transform.rotation;
        
        float easeSpeed = 2f;
        menu.transform.position = Vector3.Lerp(menu.transform.position, targetPosition, Time.deltaTime * easeSpeed);
        menu.transform.rotation = Quaternion.Slerp(menu.transform.rotation, targetRotation, Time.deltaTime * easeSpeed);
    }
}
