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
    public SystemLanguage voiceLanguage;

    private readonly RoutineBuilder _routineBuilder = new();
    private TextToSpeechSubsystem _tts;
    
    // Main menu variables
    private bool _isMovingMenuToCenter;
    
    void Start()
    {
        PathBuilder.SetUp(mainCamera, pathsMaterial, planeSize, linesParent, lastStepDistance, nodeMenuPrefab);
        
        mainMenu.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
        mainMenu.transform.rotation = mainCamera.transform.rotation;
        
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
        _routineBuilder.OnDeltaFunction += StartRoutineHandler;
        _routineBuilder.OnDeltaFunction += CalibrateHandler;
        _routineBuilder.OnDeltaFunction += AddNodeHandler;
        _routineBuilder.OnDeltaFunction += EndRoutineHandler;
        _routineBuilder.OnDeltaFunction += PreviewHandler;
        _routineBuilder.OnDeltaFunction += TweakHandler;
        _routineBuilder.OnDeltaFunction += PublishHandler;
        _routineBuilder.OnDeltaFunction += CancelRoutineHandler;
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
    }

    private void FixedUpdate()
    {
        if (mainMenu.activeSelf) UpdateMenuPosition(mainMenu);
        if (calibrationMenu.activeSelf) UpdateMenuPosition(calibrationMenu);
    }

    void StartRoutineHandler()
    {
        _routineBuilder.TransitionTo(RoutineBuilder.State.Calibrating, RoutineBuilder.Operation.StartRoutine);
        calibrationMenu.SetActive(true);
    }

    void CalibrateHandler()
    {
        PathBuilder.SetReference(mainCamera.transform.position, mainCamera.transform.rotation);
        calibrationMenu.SetActive(false);
        PathBuilder.Activate();
        PathBuilder.AddNode(1.5f);
        _routineBuilder.TransitionTo(RoutineBuilder.State.Recording, RoutineBuilder.Operation.Calibrate);
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
        _routineBuilder.TransitionTo(RoutineBuilder.State.Previewing, RoutineBuilder.Operation.Preview);
    }
    
    void TweakHandler()
    {
        _routineBuilder.TransitionTo(RoutineBuilder.State.Tweaking, RoutineBuilder.Operation.Tweak);
    }
    
    void PublishHandler()
    {
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
        _routineBuilder.Finish();
    }

    void CancelRoutineHandler()
    {
        PathBuilder.Restore();
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
