using UnityEngine;
using Lulasz.SuperFSM;

public class SuperFSMExample : MonoBehaviour
{
    private void Start()
    {
        FSM<string> _fsm = new FSM<string>(this);

        _fsm.AddState("state 1",
        OnUpdate: () =>
        {
            Debug.Log($"{_fsm.CurrentState.ID} : update");
        })
        .AddTransition(to: "state 2",
        Condition: () =>
        {
            return true;
        });

        _fsm.AddState("state 2",
        OnEntry: () =>
        {
            Debug.Log($"{_fsm.CurrentState.ID} : entered");
        });

        _fsm.SetState("state 1");
        _fsm.Start(TimeScale.Scaled);
    }

}
