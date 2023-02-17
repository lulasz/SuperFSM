using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperFSM;

public class SuperFSMExample : MonoBehaviour
{
    private void Start()
    {
        FSM _fsm = new FSM(this);

        _fsm.AddState("state 1",
        OnUpdate: () =>
        {
            Debug.Log($"{_fsm.CurrentState.Name} : update");
        })
        .AddTransition("can change to 2", "state 2",
        Condition: () =>
        {
            return true;
        });

        _fsm.AddState("state 2",
        OnEntry: () =>
        {
            Debug.Log($"{_fsm.CurrentState.Name} : entered");
        });

        _fsm.SetState("state 1");
        _fsm.Start();
    }

}
