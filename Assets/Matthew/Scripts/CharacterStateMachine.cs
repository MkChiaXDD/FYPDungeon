using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    // Animator reference and state tracking
    private Animator _animator;
    private BaseState _currentState;
    private Dictionary<StateType, BaseState> _states = new Dictionary<StateType, BaseState>();

    // State definitions
    public enum StateType { Idle, Walk }

    void Start()
    {
        _animator = GetComponent<Animator>();
        InitializeStates();
        TransitionToState(StateType.Idle);
    }

    void Update() => _currentState?.UpdateState();

    // State management
    public void TransitionToState(StateType newStateType)
    {
        _currentState?.ExitState();
        _currentState = _states[newStateType];
        _currentState.EnterState();
    }

    private void InitializeStates()
    {
        // Register states
        _states.Add(StateType.Idle, new IdleState(this, _animator));
        _states.Add(StateType.Walk, new WalkState(this, _animator));
    }

    // Base state template
    public abstract class BaseState
    {
        protected CharacterStateMachine _machine;
        protected Animator _animator;

        public BaseState(CharacterStateMachine machine, Animator animator)
        {
            _machine = machine;
            _animator = animator;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }

    // Concrete state implementations
    private class IdleState : BaseState
    {
        public IdleState(CharacterStateMachine machine, Animator animator) : base(machine, animator) { }

        public override void EnterState()
        {
            _animator.SetBool("IsWalking", false);
        }

        public override void UpdateState()
        {
            // State-specific logic can be added here
        }

        public override void ExitState()
        {
            // Cleanup logic when exiting state
        }
    }

    private class WalkState : BaseState
    {
        public WalkState(CharacterStateMachine machine, Animator animator) : base(machine, animator) { }

        public override void EnterState()
        {
            _animator.SetBool("IsWalking", true);
        }

        public override void UpdateState()
        {
            // Movement logic would typically go here
        }

        public override void ExitState()
        {
            // Cleanup logic when exiting state
        }
    }
}