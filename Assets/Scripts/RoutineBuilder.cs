using System;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

// An State Machine to manage the Routine Building process
    public class RoutineBuilder
    {
        public enum State { Idle, Calibrating, Recording, Tweaking, Previewing, Publishing }
        public enum Operation { StartRoutine, Calibrate, EndRoutine, CancelRoutine, AddNode, Preview, Tweak, Publish }
        
    
        public State CurrentState { get; private set; } = State.Idle;
        public event Action OnDeltaFunction;
        private Dictionary<Operation, PressableButton> _buttonsByOperation = new();
        private bool _validated;
    
        public bool CanTransitionTo(State newState, Operation by)
        {
            if (!_validated) 
                throw new InvalidOperationException("State machine transitions have not been validated yet.");
            return ValidTransitions[CurrentState].Contains(newState) && ValidOperations[CurrentState].Contains(by);
        }
        
        public bool CanPerform(Operation by)
        {
            if (!_validated) 
                throw new InvalidOperationException("State machine transitions have not been validated yet.");
            return ValidOperations[CurrentState].Contains(by);
        }
        
        public void CallDelta(Operation by)
        {
            if (!_validated) 
                throw new InvalidOperationException("State machine transitions have not been validated yet.");
            
            if (!CanPerform(by)) 
                throw new InvalidOperationException($"Cannot perform operation {by} from state {CurrentState}");
            
            OnDeltaFunction?.Invoke();
        }
    
        public void TransitionTo(State newState, Operation by)
        {
            if (!_validated) 
                throw new InvalidOperationException("State machine transitions have not been validated yet.");
            Debug.Log($"Transitioning to {newState} from {CurrentState} by {by}");
            if (!CanTransitionTo(newState, by)) 
                throw new InvalidOperationException($"Cannot transition from {CurrentState} to {newState}");
            
            CurrentState = newState;
            _SwitchButtons();
        }
        
        public void AssignButton(Operation operation, PressableButton button)
        {
            if (button == null)
                throw new ArgumentNullException(nameof(button), "Button cannot be null.");
            bool added = _buttonsByOperation.TryAdd(operation, button);
            if (!added) throw new InvalidOperationException($"Button for operation {operation} is already assigned.");
            
            // We listen to the button's onClick event to trigger the corresponding operation 
            button.OnClicked.AddListener(() => CallDelta(operation));
            
            _validated = _buttonsByOperation.Count == Enum.GetValues(typeof(Operation)).Length;
            
            if (_validated) _SwitchButtons();
        }

        public void Finish()
        {
            if (CurrentState != State.Publishing)
                throw new InvalidOperationException("Can only finish routine from Publishing state.");
            CurrentState = State.Idle;
            _SwitchButtons();
        }

        public void Clean()
        {
            // Remove all listeners to avoid memory leaks
            foreach (var button in _buttonsByOperation.Values)
            {
                button.OnClicked.RemoveAllListeners();
            }
            OnDeltaFunction = null;
        }
        
        private void _SwitchButtons()
        {
            if (!_validated) 
                throw new InvalidOperationException("State machine transitions have not been validated yet.");
            foreach (var (operation, button) in _buttonsByOperation)
            {
                button.gameObject.SetActive(ValidOperations[CurrentState].Contains(operation));
            }
        }
    
        private static readonly Dictionary<State, HashSet<State>> ValidTransitions = new()
        {
            { State.Idle, new HashSet<State> { State.Calibrating } },
            { State.Calibrating, new HashSet<State> { State.Recording, State.Idle } },
            { State.Recording, new HashSet<State> { State.Tweaking, State.Idle } },
            { State.Tweaking, new HashSet<State> { State.Previewing, State.Publishing, State.Idle } },
            { State.Previewing, new HashSet<State> { State.Tweaking, State.Publishing, State.Idle } },
            { State.Publishing, new HashSet<State> { State.Idle } }
        };
        
        private static readonly Dictionary<State, HashSet<Operation>> ValidOperations = new()
        {
            { State.Idle, new HashSet<Operation> { Operation.StartRoutine } },
            { State.Calibrating, new HashSet<Operation> { Operation.Calibrate, Operation.CancelRoutine } },
            { State.Recording, new HashSet<Operation> { Operation.AddNode, Operation.EndRoutine, Operation.CancelRoutine } },
            { State.Tweaking, new HashSet<Operation> { Operation.Preview, Operation.Publish, Operation.CancelRoutine } },
            { State.Previewing, new HashSet<Operation> { Operation.Tweak, Operation.Publish, Operation.CancelRoutine } },
        };
    }
    