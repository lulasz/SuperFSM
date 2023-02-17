using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lulasz.SuperFSM
{
    public class Transition<T> where T : IComparable
    {
        public T To;
        public Func<bool> Condition;
    }

    public class State<T> where T : IComparable
    {
        private T _id;
        private Action _update;
        private Action _entry;
        private Action _exit;
        private List<Transition<T>> _transitions = new List<Transition<T>>();
        internal State(T id, Action entry, Action update, Action exit)
        {
            this._id = id;
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
        public State<T> AddTransition(T to, Func<bool> Condition)
        {
            Transition<T> transition = new Transition<T>();
            transition.To = to;
            transition.Condition = Condition;
            _transitions.Add(transition);

            return this;
        }

        /// <summary>
        /// Gets the name of the state
        /// </summary>
        public T ID { get { return _id; } }

        public Action OnUpdate { get { return _update; } }
        public Action OnEntry { get { return _entry; } }
        public Action OnExit { get { return _exit; } }

        /// <summary>
        /// Gets all transitions of this state
        /// </summary>
        public List<Transition<T>> Transitions { get { return _transitions; } }
    }

    public class FSM<T> where T : IComparable
    {
        private MonoBehaviour _mono;
        private List<State<T>> _states = new List<State<T>>();
        private State<T> _previousState;
        private State<T> _currentState;

        /// <summary>
        /// Creates new Finite State Machine
        /// </summary>
        /// <param name="mono">The Finite State Machine needs a reference to the MonoBehaviour to run. You can put "this" here.</param>
        public FSM(MonoBehaviour mono) => _mono = mono;

        /// <summary>
        /// Adds a state to the Finite State Machine
        /// </summary>
        /// <param name="ID">The ID of the state</param>
        /// <param name="OnUpdate">Executes every frame</param>
        /// <param name="OnEntry">Executes on entering the state</param>
        /// <param name="OnExit">Executes on leaving the state</param>
        /// <returns></returns>
        public State<T> AddState(T ID, Action OnUpdate = null, Action OnEntry = null, Action OnExit = null)
        {
            State<T> state = new State<T>(ID, OnEntry, OnUpdate, OnExit);
            _states.Add(state);
            return state;
        }

        /// <summary>
        /// Starts the Finite State Machine
        /// <param name="time">Delay to execute. Put here Time.deltaTime to execute like it would run on Update.</param>
        /// </summary>
        public void Start(float time) => _mono.StartCoroutine(this.Execute(time));

        /// <summary>
        /// Stops the Finite State Machine
        /// </summary>
        public void Stop() => _mono.StopCoroutine(this.Execute(0));

        /// <summary>
        /// Sets the current state
        /// </summary>
        /// <param name="id">Name of the state</param>
        public void SetState(T id) => _currentState = _states.Find((x) => { return Compare(x.ID, id); });

        private IEnumerator Execute(float time)
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

                    // Wait for one frame to execute OnUpdate fully
                    yield return new WaitForEndOfFrame();

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
                                    _currentState = _states.Find((x) => { return Compare(x.ID, transition.To); });
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

                yield return new WaitForSeconds((time <= 0f) ? Time.deltaTime : time);
            }
        }

        private bool Compare(T value1, T value2) => Comparer<T>.Default.Compare(value1, value2) == 0;

        /// <summary>
        /// Returns the current state
        /// </summary>
        public State<T> CurrentState { get { return _currentState; } }

        /// <summary>
        /// Returns the previous state
        /// </summary>
        public State<T> PreviousState { get { return _previousState; } }
    }
}
