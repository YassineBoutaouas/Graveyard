using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.AI
{
    public class StateMachine
    {
        public IState CurrentState { get { return _currentState; } }

        private IState _currentState;

        public string CurrentStateName;
        public string LastStateName;

        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();

        private static List<Transition> EmptyTransitions = new List<Transition>();

        public void Update()
        {
            Transition transition = GetTransition();
            if (transition != null)
                SetState(transition.TargetState);

            _currentState?.OnStateUpdate();
        }

        private Transition GetTransition()
        {
            foreach (Transition anyTransition in _anyTransitions)
                if (anyTransition.Condition())
                    return anyTransition;

            foreach (Transition currentTransition in _currentTransitions)
                if (currentTransition.Condition())
                    return currentTransition;

            return null;
        }

        public void SetState(IState state)
        {
            if (state == _currentState) return;

            _currentState?.OnStateExit();
            LastStateName = _currentState?.GetType().Name;
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            CurrentStateName = _currentState.GetType().Name;

            _currentState.OnStateEnter();
        }

        public void AddTransition(IState originState, IState targetState, Func<bool> condition)
        {
            if (_transitions.TryGetValue(originState.GetType(), out List<Transition> transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[originState.GetType()] = transitions;
            }

            transitions.Add(new Transition(targetState, condition));
        }

        public void AddAnyTransition(IState targetState, Func<bool> condition)
        {
            _anyTransitions.Add(new Transition(targetState, condition));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState TargetState { get; }

            public Transition(IState targetState, Func<bool> condition)
            {
                TargetState = targetState;
                Condition = condition;
            }
        }
    }
}