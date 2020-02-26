using UnityEngine;
using MLAgents;

public class CylinderAgent : Agent
{
    public bool menu;
    public string team;

    public GameObject friendlyWood;
    public GameObject friendlyFire;
    public GameObject friendlyEarth;
    public GameObject friendlyMetal;
    public GameObject friendlyWater;

    public GameObject enemy;

    public GameObject enemyWood;
    public GameObject enemyFire;
    public GameObject enemyEarth;
    public GameObject enemyMetal;
    public GameObject enemyWater;

    public float speed = 80f;
    private Rigidbody rigidBody;
    public Vector3 startingPos;

    public GameController gameController;

    public override void InitializeAgent()
    {
        menu = gameController.menu;
        rigidBody = GetComponent<Rigidbody>();
    }

    public override void CollectObservations()
    {
        // So the agent knows how fast it is moving
        AddVectorObs(rigidBody.velocity.x);
        AddVectorObs(rigidBody.velocity.z);

        // Observation of all other spheres in the scene

        // Enemy Team

        Observe(enemy);

        Observe(enemyEarth, true, true);

        Observe(enemyFire, true, true);

        Observe(enemyMetal, true, true);

        Observe(enemyWater, true, true);

        Observe(enemyWood, true, true);

        // Teammates

        Observe(friendlyEarth, true);

        Observe(friendlyFire, true);

        Observe(friendlyMetal, true);

        Observe(friendlyWater, true);

        Observe(friendlyWood, true);

        // Up to 80 Observations
    }

    // 13 observations when "size" = false
    // If "multidistance" and "size" = true, 9 total observations
    private void Observe(GameObject obj, bool size = false, bool multiDistance = false)
    {
        if (obj.GetComponent<Rigidbody>().useGravity)
        {
            Vector3 directionToObject = (obj.transform.localPosition - transform.localPosition).normalized;

            AddVectorObs(directionToObject.x);
            AddVectorObs(directionToObject.z);
            ObserveDistance(obj);
            
            if (size)
            {
                AddVectorObs(obj.transform.localScale.x);
            }
            // For tracking the distance between the enemy player and all other agents
            else
            {
                CylinderAgent otherAgent = obj.GetComponent<CylinderAgent>();
                ObserveDistance(otherAgent.friendlyWood);
                ObserveDistance(otherAgent.friendlyWater);
                ObserveDistance(otherAgent.friendlyMetal);
                ObserveDistance(otherAgent.friendlyFire);
                ObserveDistance(otherAgent.friendlyEarth);

                ObserveDistance(otherAgent.enemyWood);
                ObserveDistance(otherAgent.enemyWater);
                ObserveDistance(otherAgent.enemyMetal);
                ObserveDistance(otherAgent.enemyFire);
                ObserveDistance(otherAgent.enemyEarth);
            }
            // For tracking the distance between the object and its various targets
            if (multiDistance)
            {
                ObserveDistance(obj, "absorb");
                ObserveDistance(obj, "destroy");
                ObserveDistance(obj, "predator");
                ObserveDistance(obj, "destroyer");
                ObserveDistance(obj, "mirror");
            }
        } // For padding observations that are no longer present
        else
        {
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);

            if (size)
            {
                AddVectorObs(0);
            }
            else
            {
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);

                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
            }

            if (multiDistance)
            {
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);
            }
        }
    }

    private void ObserveDistance(GameObject obj, string other = null)
    {
        Vector3 pointA;
        Vector3 pointB;
        BallAgent objectAgent = obj.GetComponent<BallAgent>();
        Collider objectCollider = obj.GetComponent<Collider>();
        
        if (other == "absorb")
        {
            pointA = objectAgent.absorbTarget.GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(objectAgent.absorbTarget.transform.localPosition);
        }
        else if (other == "destroy")
        {
            pointA = objectAgent.destroyTarget.GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(objectAgent.destroyTarget.transform.localPosition);
        }
        else if (other == "predator")
        {
            pointA = objectAgent.predator.GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(objectAgent.predator.transform.localPosition);
        }
        else if (other == "destroyer")
        {
            pointA = objectAgent.destroyer.GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(objectAgent.destroyer.transform.localPosition);
        }
        else if (other == "mirror")
        {
            pointA = objectAgent.enemyMirror.GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(objectAgent.enemyMirror.transform.localPosition);
        }
        else 
        {
            pointA = GetComponent<Collider>().ClosestPointOnBounds(obj.transform.localPosition);
            pointB = objectCollider.ClosestPointOnBounds(transform.localPosition);
        }

        // Finds the absolute distance between the two points
        float distanceFromObject = Vector3.Distance(pointA, pointB);

        AddVectorObs(distanceFromObject);
    }

    public override void AgentAction(float[] vectorAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        rigidBody.AddForce(controlSignal * speed * 10);

        // Reset agent's position if it somehow ends up out of bounds due to physics/collider bugs
        if (transform.localPosition.x > 65 | transform.localPosition.x < -40 |
            transform.localPosition.z > 90 | transform.localPosition.z < -95 |
            transform.localPosition.y < 5 | transform.localPosition.y > 100)
        {
            ResetPosition();
        }
    }

    public override void AgentReset()
    {
        ResetPosition();
    }

    private void ResetPosition()
    {
        rigidBody.velocity = Vector3.zero;
        transform.localPosition = startingPos;
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}