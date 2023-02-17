using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperFSM
{
    public class Transition
    {
        public string Name;
        public string To;
        public Func<bool> Condition;
    }

    public class State
    {
        private string _name;
        private Action _update;
        private Action _entry;
        private Action _exit;
        private List<Transition> _transitions = new List<Transition>();
        internal State(string name, Action entry, Action update, Action exit)
        {
            this._name = name;
            this._entry = entry;
            this._update = update;
            this._exit = exit;
        }

        /// <summary>
        /// Adds a transition between states
        /// </summary>
        /// <param name="name">Name of the transition</param>
        /// <param name="to">State to move to</param>
        /// <param name="Condition">A condition when the transition can be executed</param>
        public State AddTransition(string name, string to, Func<bool> Condition)
        {
            Transition transition = new Transition();
            transition.Name = name;
            transition.To = to;
            transition.Condition = Condition;
            _transitions.Add(transition);

            return this;
        }

        /// <summary>
        /// Gets the name of the state
        /// </summary>
        public string Name { get { return _name; } }

        public Action OnUpdate { get { return _update; } }
        public Action OnEntry { get { return _entry; } }
        public Action OnExit { get { return _exit; } }

        /// <summary>
        /// Gets all transitions of this state
        /// </summary>
        public List<Transition> Transitions { get { return _transitions; } }
    }

    public class FSM
    {
        public enum TimeScale
        {
            Scaled,
            Unscaled
        }

        private MonoBehaviour _mono;
        private List<State> _states = new List<State>();
        private State _previousState;
        private State _currentState;

        /// <summary>
        /// Creates new Finite State Machine
        /// </summary>
        /// <param name="mono">The Finite State Machine needs a reference to the MonoBehaviour to run. You can put "this" here.</param>
        public FSM(MonoBehaviour mono) => _mono = mono;

        /// <summary>
        /// Adds a state to the Finite State Machine
        /// </summary>
        /// <param name="Name">The name of the state</param>
        /// <param name="OnUpdate">Executes every frame</param>
        /// <param name="OnEntry">Executes on entering the state</param>
        /// <param name="OnExit">Executes on leaving the state</param>
        /// <returns></returns>
        public State AddState(string Name, Action OnUpdate = null, Action OnEntry = null, Action OnExit = null)
        {
            State state = new State(Name, OnEntry, OnUpdate, OnExit);
            _states.Add(state);
            return state;
        }

        /// <summary>
        /// Starts the Finite State Machine
        /// </summary>
        public void Start(TimeScale scale = TimeScale.Scaled) => _mono.StartCoroutine(this.Execute(scale));

        /// <summary>
        /// Stops the Finite State Machine
        /// </summary>
        public void Stop() => _mono.StopCoroutine(this.Execute(0));

        /// <summary>
        /// Sets the current state
        /// </summary>
        /// <param name="name">Name of the state</param>
        public void SetState(string name) => _currentState = _states.Find((x) => { return x.Name == name; });

        private IEnumerator Execute(TimeScale scale)
        {
            if (_currentState == null)
                throw new Exception($"{this} has no starting State set! Set using SetState('NAME') before Start()");

            bool entry = false;
            while (_states.Count > 0)
            {
                // Execute Actions
                if (_currentState != null)
                {
                    // Execute OnEnter state
                    if (!entry)
                    {
                        _currentState.OnEntry?.Invoke();
                        entry = true;
                    }

                    // Execute OnUpdate state
                    _currentState.OnUpdate?.Invoke();

                    // Check for open transition of current state
                    if (_currentState.Transitions != null && _currentState.Transitions.Count > 0)
                    {
                        foreach (var transition in _currentState.Transitions)
                        {
                            // If a transition is open, switch to the new state
                            if (transition.Condition != null)
                            {
                                bool ok = transition.Condition.Invoke();
                                if (ok)
                                {
                                    _previousState = _currentState;
                                    _previousState.OnExit?.Invoke();
                                    // Reset
                                    entry = false;
                                    _currentState = _states.Find((x) => { return x.Name == transition.To; });
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Stop();
                        yield return null;
                    }
                }

                yield return new WaitForSeconds((scale == TimeScale.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// Returns the current state
        /// </summary>
        public State CurrentState { get { return _currentState; } }

        /// <summary>
        /// Returns the previous state
        /// </summary>
        public State PreviousState { get { return _previousState; } }
    }
}
