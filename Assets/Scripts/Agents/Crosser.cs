using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DelegatedSteering), typeof(SeekBehaviour))]
public class Crosser : MonoBehaviour
{
    public float reactionTime = .2f;            // reaction time of the crosser
    public float waitingDistance = 1.5f;        // min distance for waiting in front of the crossing
    public float arriveDistance = 1f;           // min distance to consider the destination reached
    public float nearCrossingDistance = 1.4f;   // min distance to consider close to the crossing

    public float walkSpeed = 2f;                // maximum linear speed while walking
    public float hasteSpeed = 2.5f;             // maximum linear speed while hastening
    public float jogSpeed = 3f;                 // maximum linear speed while jogging

    public Transform baseDestination;           // base position to reach

    private Vector3 startPosition;              // position in which the crosser was spawned
    private Vector3 firstCrossingCheckpoint;    // position to reach before the crossing
    private Vector3 secondCrossingCheckpoint;   // position to reach while the crossing
    private Vector3 destination;                // position to reach after crossing

    private Vector3 halfCrossingTowards;        // vector used to test if we're past the half of the crossing
    private Vector3 secondCheckpointTowards;    // vector used to test if we're past the second checkpoint

    private DelegatedSteering steering;         // steering behaviour to control crosser movement
    private SeekBehaviour seekBhvr;             // seek behaviour to assign intermediate destinations

    private SimpleTrafficLight semaphore;       // semaphore to look at 
    private Collider crossing;                  // crossing boundaries

    private DecisionTree dt;                    // decision tree

    private Animator animator;                  // animator to control the crosser reaction to semaphore states

    void Start()
    {
        startPosition = transform.position;

        seekBhvr = GetComponent<SeekBehaviour>();
        steering = GetComponent<DelegatedSteering>();
        animator = transform.GetChild(0).GetComponent<Animator>();

        semaphore = FindClosestSemaphore();
        crossing = GameObject.FindGameObjectWithTag("Crossing").transform.GetChild(0).GetComponent<Collider>();
        firstCrossingCheckpoint = Vector3.Scale(crossing.bounds.ClosestPoint(startPosition), new Vector3(1, 1, 0.3f));

        NewDestination();

        // vector towards first crossing checkpoint starting from half crossing
        halfCrossingTowards = Vector3.Scale((firstCrossingCheckpoint - crossing.transform.position), crossing.transform.right).normalized;
        // vector towards first crossing checkpoint starting from second checkpoint (end crossing)
        secondCheckpointTowards = Vector3.Scale((crossing.transform.position - secondCrossingCheckpoint), crossing.transform.right).normalized;

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
        //Debug.Log("Despawn");
        Vector3 respawnPosition = Utilities.GenerateValidPosition(startPosition, 2f, transform.localScale.y, transform.localScale);
        transform.position = respawnPosition;

        NewDestination();
        return null;
    }

    public object Arrive(object o)
    {
        //Debug.Log("Arrive");
        seekBhvr.destination = destination;
        StartWalking();
        return null;
    }

    public object Approach(object o)
    {
        //Debug.Log("Approach");
        seekBhvr.destination = crossing.bounds.ClosestPoint(transform.position);
        StartWalking();
        return null;
    }

    public object Cross(object o)
    {
        //Debug.Log("Cross");
        seekBhvr.destination = secondCrossingCheckpoint;
        StartWalking();
        return null;
    }

    public object Hasten(object o)
    {
        //Debug.Log("Haste");
        seekBhvr.destination = secondCrossingCheckpoint;
        StartHastening();
        return null;
    }

    public object Jog(object o)
    {
        //Debug.Log("Jog");
        seekBhvr.destination = secondCrossingCheckpoint;
        StartJogging();
        return null;
    }

    public object Turn(object o)
    {
        //Debug.Log("Turn");
        seekBhvr.destination = firstCrossingCheckpoint;
        StartJogging();
        return null;
    }

    public object Wait(object o)
    {
        //Debug.Log("Wait");
        seekBhvr.destination = secondCrossingCheckpoint;
        StartIdling();
        return null;
    }

    // DECISIONS
    public object HasCrossed(object o)
    {
        return Vector3.Dot(secondCheckpointTowards, secondCrossingCheckpoint - transform.position) > 0;
    }

    public object IsGoalReached(object o)
    {
        return (destination - transform.position).sqrMagnitude <= arriveDistance * arriveDistance; 
    }

    public object IsNearCrossing(object o)
    {
        return crossing.bounds.SqrDistance(transform.position) < nearCrossingDistance * nearCrossingDistance; 
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
        return Vector3.Dot(halfCrossingTowards, crossing.transform.position - transform.position) > 0;
    }

    public object IsBeforeCrossing(object o)
    {
        return (firstCrossingCheckpoint - transform.position).sqrMagnitude <= waitingDistance * waitingDistance;
    }

    // HELPER METHODS
    SimpleTrafficLight FindClosestSemaphore()
    {
        GameObject closest = null;
        GameObject[] semaphores = GameObject.FindGameObjectsWithTag("Semaphore");
        if (semaphores.Length > 0)
        {
            closest = semaphores[0];

            for (int i = 1; i < semaphores.Length; i++)
            {
                // if its closer to crosser than the current closest, choose it as new closest
                if ((semaphores[i].transform.position - startPosition).sqrMagnitude < (closest.transform.position - startPosition).sqrMagnitude)
                    closest = semaphores[i];
            }
        }

        return closest.GetComponent<SimpleTrafficLight>();
    }

    bool CheckSemaphore(SimpleTrafficLight.ColorState color)
    {
        return semaphore.currentState == color;
    }

    // HELPER FUNCTIONS
    public void NewDestination()
    {
        SetDestination(baseDestination.position + new Vector3(0, 0, Random.Range(-14, 14)));
        secondCrossingCheckpoint = crossing.bounds.ClosestPoint(destination);
        secondCrossingCheckpoint -= (destination - secondCrossingCheckpoint) * 0.25f;
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        seekBhvr.destination = destination;
    }

    public void StartWalking()
    {
        steering.maxLinearSpeed = walkSpeed;
        animator.Play("Base Layer.Walking");
        animator.speed = 1f;
        steering.ChangeBehaviourWeight<AvoidBehaviourVolumeAdaptive>(1f);
    }

    public void StartHastening()
    {
        steering.maxLinearSpeed = hasteSpeed;
        animator.speed = 1.25f;
    }

    public void StartJogging()
    {
        steering.maxLinearSpeed = jogSpeed;
        animator.Play("Base Layer.Jogging");
        animator.speed = 1f;
    }

    public void StartIdling()
    {
        steering.maxLinearSpeed = 0f;
        animator.Play("Base Layer.Idling");
        animator.speed = 1f;
        steering.ChangeBehaviourWeight<AvoidBehaviourVolumeAdaptive>(0f);
    }

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

        Gizmos.DrawSphere(startPosition, 0.1f);
        Gizmos.DrawSphere(firstCrossingCheckpoint, 0.1f);
        Gizmos.DrawSphere(secondCrossingCheckpoint, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.1f);
    }
}
