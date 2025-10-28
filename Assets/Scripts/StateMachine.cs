using System;
using UnityEngine;

public static class StateMachine
{
    public enum TutorialStage
    {
        OpeningHandMenu,
        PressingStartButton,
        Walking,
        AddingNode,
        Calibrating,
        FinalWalking,
        Previewing,
        Publishing
    }

    public enum RoutineState
    {
        Idle,
        Calibrating,
        Recording,
        Previewing,
        Publishing
    }

    // State variables
    private static TutorialStage _currentTutorialStage = TutorialStage.OpeningHandMenu;
    private static RoutineState _currentRoutineState = RoutineState.Idle;
    private static bool _doingTutorial;

    // Events for state changes
    public static event Action<TutorialStage> OnTutorialStageChanged;
    public static event Action<RoutineState> OnRoutineStateChanged;
    public static event Action<bool> OnTutorialStatusChanged;

    // Properties
    public static TutorialStage CurrentTutorialStage
    {
        get => _currentTutorialStage;
        private set
        {
            if (_currentTutorialStage != value)
            {
                _currentTutorialStage = value;
                Debug.Log($"Tutorial Stage Changed: {value}");
                OnTutorialStageChanged?.Invoke(value);
            }
        }
    }

    public static RoutineState CurrentRoutineState
    {
        get => _currentRoutineState;
        private set
        {
            if (_currentRoutineState != value)
            {
                _currentRoutineState = value;
                Debug.Log($"Routine State Changed: {value}");
                OnRoutineStateChanged?.Invoke(value);
            }
        }
    }

    public static bool DoingTutorial
    {
        get => _doingTutorial;
        private set
        {
            if (_doingTutorial != value)
            {
                _doingTutorial = value;
                Debug.Log($"Tutorial Status Changed: {value}");
                OnTutorialStatusChanged?.Invoke(value);
            }
        }
    }

    // Tutorial state transitions
    public static void StartTutorial()
    {
        DoingTutorial = true;
        CurrentTutorialStage = TutorialStage.OpeningHandMenu;
        CurrentRoutineState = RoutineState.Idle;
    }

    public static void EndTutorial()
    {
        DoingTutorial = false;
        CurrentTutorialStage = TutorialStage.OpeningHandMenu;
    }

    public static void TransitionToTutorialStage(TutorialStage newStage)
    {
        if (!DoingTutorial)
        {
            Debug.LogWarning("Cannot transition tutorial stage when not doing tutorial.");
            return;
        }

        CurrentTutorialStage = newStage;
    }

    public static bool IsTutorialStage(TutorialStage stage)
    {
        return DoingTutorial && CurrentTutorialStage == stage;
    }

    // Routine state transitions
    public static void StartCalibration()
    {
        if (CurrentRoutineState != RoutineState.Idle)
        {
            Debug.LogWarning("Cannot start calibration from current state: " + CurrentRoutineState);
            return;
        }

        CurrentRoutineState = RoutineState.Calibrating;

        if (DoingTutorial && CurrentTutorialStage == TutorialStage.PressingStartButton)
        {
            CurrentTutorialStage = TutorialStage.Calibrating;
        }
    }

    public static void StartRecording()
    {
        if (CurrentRoutineState != RoutineState.Calibrating)
        {
            Debug.LogWarning("Cannot start recording from current state: " + CurrentRoutineState);
            return;
        }

        CurrentRoutineState = RoutineState.Recording;

        if (DoingTutorial && CurrentTutorialStage == TutorialStage.Calibrating)
        {
            CurrentTutorialStage = TutorialStage.Walking;
        }
    }

    public static void StartPreviewing()
    {
        if (CurrentRoutineState != RoutineState.Recording)
        {
            Debug.LogWarning("Cannot start previewing from current state: " + CurrentRoutineState);
            return;
        }

        CurrentRoutineState = RoutineState.Previewing;

        if (DoingTutorial && CurrentTutorialStage == TutorialStage.FinalWalking)
        {
            CurrentTutorialStage = TutorialStage.Previewing;
        }
    }

    public static void StartPublishing()
    {
        if (CurrentRoutineState != RoutineState.Previewing)
        {
            Debug.LogWarning("Cannot start publishing from current state: " + CurrentRoutineState);
            return;
        }

        CurrentRoutineState = RoutineState.Publishing;

        if (DoingTutorial && CurrentTutorialStage == TutorialStage.Previewing)
        {
            CurrentTutorialStage = TutorialStage.Publishing;
        }
    }

    public static void RestoreToIdle()
    {
        if (CurrentRoutineState != RoutineState.Publishing)
        {
            Debug.LogWarning("Cannot restore to idle from current state: " + CurrentRoutineState);
            return;
        }

        CurrentRoutineState = RoutineState.Idle;
    }

    // Tutorial stage specific transitions
    public static void OnHandMenuOpened()
    {
        if (DoingTutorial && CurrentTutorialStage == TutorialStage.OpeningHandMenu)
        {
            CurrentTutorialStage = TutorialStage.PressingStartButton;
        }
    }

    public static void OnNodeAdded()
    {
        if (DoingTutorial && CurrentTutorialStage == TutorialStage.AddingNode)
        {
            CurrentTutorialStage = TutorialStage.FinalWalking;
        }
    }

    // State validation
    public static bool CanStartCalibration()
    {
        return CurrentRoutineState == RoutineState.Idle;
    }

    public static bool CanCalibrate()
    {
        return CurrentRoutineState == RoutineState.Calibrating;
    }

    public static bool CanRecord()
    {
        return CurrentRoutineState == RoutineState.Recording;
    }

    public static bool CanPreview()
    {
        return CurrentRoutineState == RoutineState.Previewing;
    }

    public static bool CanPublish()
    {
        return CurrentRoutineState == RoutineState.Publishing;
    }

    // Reset the entire state machine
    public static void Reset()
    {
        DoingTutorial = false;
        CurrentTutorialStage = TutorialStage.OpeningHandMenu;
        CurrentRoutineState = RoutineState.Idle;
    }
}

