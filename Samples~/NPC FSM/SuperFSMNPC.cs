using UnityEngine;
using Lulasz.SuperFSM;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SuperFSMNPC : MonoBehaviour
{
    private enum States
    {
        Wait,
        Move
    }

    private FSM<States> _fsm;
    private NavMeshAgent _agent;
    private float _walkRadius = 10f;

    private float _waitTime;
    private Vector3 _destination;

    private TextMesh _textMesh;

    private void Start()
    {
        _textMesh = GetComponentInChildren<TextMesh>();
        _agent = gameObject.GetComponent<NavMeshAgent>();


        _fsm = new FSM<States>(this);

        _fsm.AddState(States.Wait,
            OnEntry: () =>
            {
                _waitTime = Time.time + 1f;
            })
        .AddTransition(to: States.Move, () => Time.time > _waitTime);

        _fsm.AddState(States.Move,
            OnEntry: () =>
            {
                Vector3 randomDirection = Random.insideUnitSphere * _walkRadius;
                randomDirection += transform.position;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, _walkRadius, 1);
                _destination = hit.position;
                _agent.SetDestination(_destination);
            })
        .AddTransition(to: States.Wait, () => DestinationReached());

        _fsm.SetState(States.Wait);
        _fsm.Start(Time.deltaTime);

        // Subscribe to changed state
        _fsm.onStateChanged += (p, n) =>
        {
            _textMesh.text = $"{p.ID}->{n.ID}";
        };
    }

    private bool DestinationReached()
    {
        if (!_agent.pathPending)
            if (_agent.remainingDistance <= _agent.stoppingDistance)
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    return true;
        return false;
    }
}
