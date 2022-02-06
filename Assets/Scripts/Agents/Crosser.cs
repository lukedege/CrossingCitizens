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
    private Vector3 first_crossing_checkpoint;
    private Vector3 second_crossing_checkpoint;

    private SeekBehaviour seekBhvr;
    private DelegatedSteering steering;

    private GameObject[] semaphores;
    private Collider crossing;

    private DecisionTree dt;

    private Animator animator;

    void Start()
    {
        start_position = transform.position;

        seekBhvr = GetComponent<SeekBehaviour>();
        steering = GetComponent<DelegatedSteering>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        

        semaphores = GameObject.FindGameObjectsWithTag("Semaphore");
        crossing = GameObject.FindGameObjectWithTag("Crossing").transform.GetChild(0).GetComponent<Collider>();

        //based on spawn
        //first_crossing_checkpoint = crossing.bounds.ClosestPoint(start_position);
        //centered
        first_crossing_checkpoint = Vector3.Scale(crossing.bounds.ClosestPoint(start_position), new Vector3(1, 1, 0.5f));

        NewDestination();
        

        InitDT();
        StartCoroutine(Wander());
    }

    /*
     * DECISION TREE 
     */

    void InitDT()
    {
        // Define actions
        DTAction despawn = new DTAction(Despawn);
        DTAction arrive = new DTAction(Arrive);
        DTAction approach = new DTAction(Approach);
        DTAction cross = new DTAction(Cross);
        DTAction hasten = new DTAction(Hasten);
        DTAction jog = new DTAction(Jog);
        DTAction turn = new DTAction(Turn);
        DTAction wait = new DTAction(Wait);

        // Define decisions
        DTDecision hasCrossed = new DTDecision(HasCrossed);
        DTDecision isGoalReached = new DTDecision(IsGoalReached);
        DTDecision isNearCrossing = new DTDecision(IsNearCrossing);
        DTDecision isGreen = new DTDecision(IsSemaphoreGreen);
        DTDecision isYellow = new DTDecision(IsSemaphoreYellow);
        DTDecision isHalfPassed = new DTDecision(IsHalfCrossingPassed);
        DTDecision isBeforeCrossing = new DTDecision(IsBeforeCrossing);


        // Link action with decisions
        hasCrossed.AddLink(true, isGoalReached);
        hasCrossed.AddLink(false, isNearCrossing);

        isGoalReached.AddLink(true, despawn);
        isGoalReached.AddLink(false, arrive);

        isNearCrossing.AddLink(true, isGreen);
        isNearCrossing.AddLink(false, approach);

        isGreen.AddLink(true, cross);
        isGreen.AddLink(false, isYellow);

        isYellow.AddLink(true, hasten);
        isYellow.AddLink(false, isHalfPassed);

        isHalfPassed.AddLink(true, jog);
        isHalfPassed.AddLink(false, isBeforeCrossing);

        isBeforeCrossing.AddLink(true, wait);
        isBeforeCrossing.AddLink(false, turn);

        dt = new DecisionTree(hasCrossed);
    }

    // ACTIONS
    public object Despawn(object o)
    {

        Debug.Log("Despawn");
        Vector3 respawnPosition = Utilities.GenerateValidPosition(start_position, 2f, transform.localScale.y, transform.localScale);
        transform.position = respawnPosition;

        NewDestination();
        return null;
    }

    public object Arrive(object o)
    {
        Debug.Log("Arrive");
        seekBhvr.destination = destination;
        StartWalking();
        return null;
    }

    public object Approach(object o)
    {
        Debug.Log("Approach");
        seekBhvr.destination = first_crossing_checkpoint;
        StartWalking();
        return null;
    }

    public object Cross(object o)
    {
        Debug.Log("Cross");
        seekBhvr.destination = second_crossing_checkpoint;
        StartWalking();
        return null;
    }

    public object Hasten(object o)
    {
        Debug.Log("Haste");
        seekBhvr.destination = second_crossing_checkpoint;
        StartHastening();
        return null;
    }

    public object Jog(object o)
    {
        Debug.Log("Jog");
        seekBhvr.destination = second_crossing_checkpoint;
        StartJogging();
        return null;
    }

    public object Turn(object o)
    {
        Debug.Log("Turn");
        seekBhvr.destination = first_crossing_checkpoint;
        StartJogging();
        return null;
    }

    public object Wait(object o)
    {
        Debug.Log("Wait");
        seekBhvr.destination = second_crossing_checkpoint;
        StartIdling();
        //steering.ChangeBehaviourWeight<AvoidBehaviourVolumeSingle>(0f);
        return null;
    }

    

    // DECISIONS
    public object HasCrossed(object o)
    {
        Vector3 second_checkpoint_forward = Vector3.Scale((crossing.transform.position - second_crossing_checkpoint), crossing.transform.right).normalized;

        return Vector3.Dot(second_checkpoint_forward, second_crossing_checkpoint - transform.position) > 0;
    }

    public object IsGoalReached(object o)
    {
        return (destination - transform.position).magnitude <= 1f; // TODO parametrizza questo valore e usa sqrmagnitud e usa seekbhvr maybe
    }

    public object IsNearCrossing(object o)
    {
        return crossing.bounds.SqrDistance(transform.position) < 2f; // TODO parametrizza questo valore
    }

    public object IsSemaphoreGreen(object o)
    {
        return CheckSemaphore(SimpleTrafficLight.ColorState.green); // TODO fix this ugly getter you can do better
    }

    public object IsSemaphoreYellow(object o)
    {
        return CheckSemaphore(SimpleTrafficLight.ColorState.yellow); // TODO fix this ugly getter you can do better
    }

    public object IsHalfCrossingPassed(object o)
    {
        return Vector3.Dot((crossing.transform.position - transform.position), (destination - transform.position)) < 0;
    }

    public object IsBeforeCrossing(object o)
    {
        return (first_crossing_checkpoint - transform.position).magnitude <= 1.5f;
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

    // HELPER FUNCTIONS
    public void NewDestination()
    {
        SetDestination(base_destination.position + new Vector3(0, 0, Random.Range(-14, 14)));
        second_crossing_checkpoint = crossing.bounds.ClosestPoint(destination);
        second_crossing_checkpoint -= (destination - second_crossing_checkpoint) * 0.25f;
    }

    public void SetDestination(Vector3 newDestination)
    {
        prev_destination = destination;
        destination = newDestination;
        seekBhvr.destination = destination;
    }

    public void StartWalking()
    {
        steering.maxLinearSpeed = 2f;
        animator.Play("Base Layer.Walking");
        animator.speed = 1f;
    }

    public void StartHastening()
    {
        steering.maxLinearSpeed = 2.5f;
        animator.speed = 1.25f;
    }

    public void StartJogging()
    {
        steering.maxLinearSpeed = 3f;
        animator.Play("Base Layer.Jogging");
        animator.speed = 1f;
    }

    public void StartIdling()
    {
        steering.maxLinearSpeed = 0f;
        animator.Play("Base Layer.Idling");
        animator.speed = 1f;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(first_crossing_checkpoint, 0.1f);
        Gizmos.DrawSphere(second_crossing_checkpoint, 0.1f);
        Gizmos.DrawSphere(destination, 0.1f);
    }
}
