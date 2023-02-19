## SuperFSM
Is a compact form of a generic Finite State Machine for Unity3D

### Usage
```C#
using SuperFSM;

public class Example : MonoBehaviour
{
    private void Start() // You need to set it only once, nothing on Unity's Update etc
    {
        FSM<string> _fsm = new FSM<string>(this); // You need to pass this MonoBehaviour

        _fsm.AddState("STATE 1",
        OnUpdate: () => // You can set some functions on Entry, Update and Exit of a state
        {
            // DO STUFF ON LOOP
        })
        .AddTransition(to: "STATE 2", // Transitions check constantly if they can transition to another state
        Condition: () =>
        {
            return true;
        });

        _fsm.AddState("STATE 2",
        OnEntry: () =>
        {
            // DO STUFF WHEN ENTERED
        });

        _fsm.SetState("STATE 1"); // You need to select the first state
        _fsm.Start(TimeScale.Scaled); // You need to start it at the end with time scale, basically put TimeScale.Scaled or TimeScale.Unscaled
    }

}

```

## Installing SuperFSM using Git
To install SuperFSM in your project use the Unity Package Manager.  
To open the Unity Package Manager, inside unity, go to `Window > Package Manager` and to install SuperFSM click the `+` icon in the upper left corner of the window, then click on `Add package from git URL...` and paste in `https://github.com/lulasz/SuperFSM.git`
