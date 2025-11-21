using System.Collections.Generic;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.UX.Experimental;
using TMPro;
using UnityEngine;

public class MainStateHandler : MonoBehaviour
{
    public GameObject mainMenu;
    public VirtualizedScrollRectList list;
    public MessageMenu messageMenu;
    public GameObject mainCamera;
    
    [Header("Calibration menu")]
    public GameObject calibrationMenu;
    public PressableButton calibrateButton;
    
    [Header("Hand menu buttons")]
    public PressableButton startButton;
    public PressableButton addNodeButton;
    public PressableButton endRoutineButton;
    public PressableButton previewButton;
    public PressableButton publishButton;
    public PressableButton tweakButton;
    public PressableButton cancelButton;
    
    [Header("Paths Drawing")]
    public GameObject nodeMenuPrefab;
    public GameObject linesParent;
    public Material pathsMaterial;
    public float lastStepDistance = 0.05f;
    public float planeSize = 0.05f;

    [Header("Routine Visualization")]
    public GameObject pepperRobot;

    private readonly PepperVisualization _pepperVisualization = new();
    private readonly RoutineBuilder _routineBuilder = new();
    private TextToSpeechSubsystem _tts;
    
    // Main menu variables
    private bool _isMovingMenuToCenter;
    
    private List<ApiService.Routine> _routines = new();
    private ApiService.Routine _loadedRoutine;
    
    void Start()
    {
        list.SetItemCount(0);
        list.OnVisible = (o, i) =>
        {
            o.GetComponentInChildren<TextMeshProUGUI>().text = _routines[i].routine_name;
            o.GetComponent<PressableButton>().OnClicked.AddListener(() => LoadRoutine(i));
        };

        list.OnInvisible = (o, i) =>
        {
            o.GetComponent<PressableButton>().OnClicked.RemoveAllListeners();
        };

        ApiService.OnRoutinesReceived += (routines) =>
        {
            _routines = routines;
            list.SetItemCount(routines.Count);
        };

        ApiService.OnRequestError += (error) =>
        {
            messageMenu.SetMessage(error);
            messageMenu.gameObject.SetActive(true);
        };

        StartCoroutine(ApiService.GetRoutines());
        
        // Initializations
        pepperRobot.transform.position = mainCamera.transform.position;
        pepperRobot.transform.rotation = mainCamera.transform.rotation;
        pepperRobot.SetActive(false);
        PathBuilder.SetUp(mainCamera, pathsMaterial, planeSize, linesParent, lastStepDistance, nodeMenuPrefab);
        
        mainMenu.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
        mainMenu.SetActive(true); // Show main menu in Idle state
        
        // Hand Menu Buttons
        _routineBuilder.AssignButton(RoutineBuilder.Operation.StartRoutine, startButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.Calibrate, calibrateButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.AddNode, addNodeButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.EndRoutine, endRoutineButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.Preview, previewButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.Publish, publishButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.Tweak, tweakButton);
        _routineBuilder.AssignButton(RoutineBuilder.Operation.CancelRoutine, cancelButton);
        
        // Operation Handlers
        _routineBuilder.OnDeltaFunction += HandleOperation;
    }
    
    void OnDestroy()
    {
        // Clean up Hand Menu listeners to avoid memory leaks
        _routineBuilder.Clean();
    }


    // Update is called once per frame
    void Update()
    {
        PathBuilder.Update();
        
        // Update menu position to follow camera when visible
        if (mainMenu.activeSelf)
        {
            UpdateMenuPosition(mainMenu);
        }
        
        if (calibrationMenu.activeSelf)
        {
            UpdateMenuPosition(calibrationMenu);
        }
    }

    void LoadRoutine(int index)
    {
        if (index < 0 || index >= _routines.Count) return;
        
        var routine = _routines[index];
        
        // Ensure clean state before loading
        PathBuilder.Restore();
        _pepperVisualization.StopVisualization();
        pepperRobot.SetActive(false);
        
        // Hide main menu and show calibration menu
        mainMenu.SetActive(false);
        calibrationMenu.SetActive(true);
        
        // Store the routine to load after calibration
        _loadedRoutine = routine;
        
        // Transition to Loading state - this will show only Calibrate and Cancel buttons
        _routineBuilder.TransitionToDirectly(RoutineBuilder.State.Loading);
    }

    void HandleOperation(RoutineBuilder.Operation operation)
    {
        switch (operation)
        {
            case RoutineBuilder.Operation.StartRoutine:
                StartRoutineHandler();
                break;
            case RoutineBuilder.Operation.Calibrate:
                CalibrateHandler();
                break;
            case RoutineBuilder.Operation.AddNode:
                AddNodeHandler();
                break;
            case RoutineBuilder.Operation.EndRoutine:
                EndRoutineHandler();
                break;
            case RoutineBuilder.Operation.Preview:
                PreviewHandler();
                break;
            case RoutineBuilder.Operation.Tweak:
                TweakHandler();
                break;
            case RoutineBuilder.Operation.Publish:
                PublishHandler();
                break;
            case RoutineBuilder.Operation.CancelRoutine:
                CancelRoutineHandler();
                break;
        }
    }

    void StartRoutineHandler()
    {
        // Ensure clean state before starting a new routine
        PathBuilder.Restore();
        
        mainMenu.SetActive(false);
        _routineBuilder.TransitionTo(RoutineBuilder.State.Calibrating, RoutineBuilder.Operation.StartRoutine);
        calibrationMenu.SetActive(true);
    }

    void CalibrateHandler()
    {
        PathBuilder.SetReference(mainCamera.transform.position, mainCamera.transform.rotation);
        _pepperVisualization.SetReference(mainCamera.transform.position, mainCamera.transform.rotation);
        calibrationMenu.SetActive(false);
        
        // Check if we're loading an existing routine or creating a new one
        if (_loadedRoutine != null)
        {
            Debug.Log($"Loading routine: {_loadedRoutine.routine_name} with {_loadedRoutine.line?.Count ?? 0} points and {_loadedRoutine.nodes?.Count ?? 0} nodes");
            
            // Load the routine with the calibrated reference
            PathBuilder.LoadRoutine(_loadedRoutine.line, _loadedRoutine.nodes);
            _loadedRoutine = null; // Clear the loaded routine
            
            // Transition to Tweaking state (Cancel, Preview, Publish available)
            _routineBuilder.TransitionTo(RoutineBuilder.State.Tweaking, RoutineBuilder.Operation.Calibrate);
        }
        else
        {
            // Start recording a new routine
            PathBuilder.Activate();
            PathBuilder.AddNode(1.5f);
            _routineBuilder.TransitionTo(RoutineBuilder.State.Recording, RoutineBuilder.Operation.Calibrate);
        }
    }

    void AddNodeHandler()
    {
        PathBuilder.AddNode();
    }


    void EndRoutineHandler()
    {
        PathBuilder.AddNode(1.3f);
        PathBuilder.Deactivate();
        _routineBuilder.TransitionTo(RoutineBuilder.State.Tweaking, RoutineBuilder.Operation.EndRoutine);
    }
    
    void PreviewHandler()
    {
        pepperRobot.SetActive(true);
        _pepperVisualization.StartVisualization(PathBuilder.CurrentPath().Item1, pepperRobot);
        _routineBuilder.TransitionTo(RoutineBuilder.State.Previewing, RoutineBuilder.Operation.Preview);
    }
    
    void TweakHandler()
    {
        _pepperVisualization.StopVisualization();
        pepperRobot.SetActive(false);
        _routineBuilder.TransitionTo(RoutineBuilder.State.Tweaking, RoutineBuilder.Operation.Tweak);
    }
    
    void PublishHandler()
    {
        pepperRobot.SetActive(false);
        _pepperVisualization.StopVisualization();
        var (points, nodes) = PathBuilder.CurrentPath();
        var path2 = "";
        foreach (var point in points)
        {
            path2 += point + " | ";
        }
        Debug.Log(path2);
        
        ApiService.Routine routine = new  ApiService.Routine
        {
            nodes = nodes,
            line = points
        };
        
        StartCoroutine(ApiService.PostRoutine(routine));
        
        // Transition to Publishing state, keeping the routine visible
        _routineBuilder.TransitionTo(RoutineBuilder.State.Publishing, RoutineBuilder.Operation.Publish);
    }

    void CancelRoutineHandler()
    {
        // Stop any ongoing visualization
        _pepperVisualization.StopVisualization();
        pepperRobot.SetActive(false);
        
        // Clean up all path data and reset PathBuilder
        PathBuilder.Restore();
        
        // Clear any loaded routine
        _loadedRoutine = null;
        
        // Transition back to Idle state
        _routineBuilder.TransitionTo(RoutineBuilder.State.Idle, RoutineBuilder.Operation.CancelRoutine);
        StartCoroutine(ApiService.GetRoutines());
        
        // Show main menu
        mainMenu.SetActive(true);
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
