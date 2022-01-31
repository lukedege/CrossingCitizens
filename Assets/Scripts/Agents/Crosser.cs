using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Crosser : MonoBehaviour
{
    public float reactionTime = .2f;

    public Transform base_destination;
    private Vector3 destination;

    private Vector3 prev_destination;
    private Vector3 start_position;
    private Vector3 crossing_checkpoint;

    private SeekBehaviour seekBhvr;
    private DelegatedSteering steering;

    private GameObject[] semaphores;
    private GameObject crossing;

    private DecisionTree dt;

    void Start()
    {
        start_position = transform.position;

        seekBhvr = GetComponent<SeekBehaviour>();
        steering = GetComponent<DelegatedSteering>();

        NewDestination();

        semaphores = GameObject.FindGameObjectsWithTag("Semaphore");
        crossing = GameObject.FindGameObjectWithTag("Crossing");
        crossing_checkpoint = crossing.transform.GetChild(0).GetComponent<Collider>().bounds.ClosestPoint(start_position);

        InitDT();
        StartCoroutine(Wander());
    }

    /*
     * DECISION TREE 
     */

    void InitDT()
    {
        // Define actions
        DTAction walk = new DTAction(Walk);
        DTAction hasten = new DTAction(Hasten);
        DTAction run = new DTAction(Run);
        DTAction wait = new DTAction(Wait);
        DTAction despawn = new DTAction(Despawn);
        DTAction resume = new DTAction(Resume);

        // Define decisions
        DTDecision d1 = new DTDecision(IsNearCrossing);
        DTDecision d2 = new DTDecision(IsGoalReached);
        DTDecision d3 = new DTDecision(IsSemaphoreGreen);
        DTDecision d4 = new DTDecision(IsSemaphoreYellow);
        DTDecision d5 = new DTDecision(IsHalfCrossingPassed);


        // Link action with decisions
        d1.AddLink(true, d3);
        d1.AddLink(false, d2);

        d2.AddLink(true, despawn);
        d2.AddLink(false, resume);

        d3.AddLink(true, resume);
        d3.AddLink(false, d4);

        d4.AddLink(true, hasten);
        d4.AddLink(false, d5);

        d5.AddLink(true, run);
        d5.AddLink(false, wait);

        dt = new DecisionTree(d1);
    }

    // ACTIONS
    public object Resume(object o)
    {
        seekBhvr.destination = destination;
        steering.maxLinearSpeed = 2f;
        steering.ChangeBehaviourWeight<AvoidBehaviourVolume>(1f);
        steering.ChangeBehaviourWeight<SeparationBehaviour>(1f);
        return null;
    }

    public object Walk(object o)
    {
        seekBhvr.destination = destination;
        steering.maxLinearSpeed = 2f;
        return null;
    }

    public object Hasten(object o)
    {
        steering.maxLinearSpeed = 3f;
        return null;
    }

    public object Run(object o)
    {
        steering.maxLinearSpeed = 4f;
        return null;
    }

    public object Wait(object o)
    {
        // TODO modella il tornare indietro
        seekBhvr.destination = crossing_checkpoint;
        steering.ChangeBehaviourWeight<AvoidBehaviourVolume>(0f);
        steering.ChangeBehaviourWeight<SeparationBehaviour>(0f);
        return null;
    }

    public object Despawn(object o)
    {
        Vector3 respawnPosition = Utilities.GenerateValidPosition(start_position, 2f, transform.localScale.y, transform.localScale);
        transform.position = respawnPosition;

        NewDestination();
        return null;
    }

    // DECISIONS
    public object IsGoalReached(object o)
    {
        return (transform.position - destination).magnitude <= seekBhvr.stopAt + 2; // TODO parametrizza questo valore (1f)
    }

    public object IsNearCrossing(object o)
    {
        return crossing.transform.GetChild(0).GetComponent<Collider>().bounds.SqrDistance(transform.position) < 0.75f; // TODO parametrizza questo valore (1f)
    }

    public object IsSemaphoreGreen(object o)
    {
        return CheckSemaphore(SimpleTrafficLight.ColorState.green);
    }

    public object IsSemaphoreYellow(object o)
    {
        return CheckSemaphore(SimpleTrafficLight.ColorState.yellow);
    }

    public object IsHalfCrossingPassed(object o)
    {
        return Vector3.Dot((crossing.transform.position - transform.position), transform.forward) < 0;
    }

    // HELPER METHODS

    bool CheckSemaphore(SimpleTrafficLight.ColorState color)
    {
        foreach (var s in semaphores)
        {
            if (s.GetComponent<SimpleTrafficLight>().currentState == color)
                return true;
        }

        return false;
    }

    public void NewDestination()
    {
        SetDestination(base_destination.position);// + new Vector3(0, 0, Random.Range(-7, 7)));
    }

    public void SetDestination(Vector3 newDestination)
    {
        prev_destination = destination;
        destination = newDestination;
        seekBhvr.destination = destination;
    }

    // TODO implement decision tree
    IEnumerator Wander()
    {
        while (true)
        {
            dt.walk();
            yield return new WaitForSeconds(reactionTime);
        }
    }

}
