using UnityEngine;
using MLAgents;

public class BallAgent : Agent
{
    private bool menu;
    public bool rewards;
    public bool playerRewards;
    public string team;
    public string absorb;
    public string destroy;
    public string sameElement;

    public CylinderAgent friendlyPlayer;
    public CylinderAgent enemyPlayer;

    private GameObject self;
    public GameObject friendlyWood;
    public GameObject friendlyFire;
    public GameObject friendlyEarth;
    public GameObject friendlyMetal;
    public GameObject friendlyWater;

    public GameObject absorbTarget;
    public GameObject destroyTarget;
    public GameObject enemyMirror;

    public GameObject predator;
    public GameObject destroyer;

    public float speed = 4f;
    private Rigidbody rigidBody;
    private MeshRenderer mesh;
    private Vector3 thisSurfaceToTargetSurface;
    private Vector3 targetSurfaceToThisSurface;
    private float nearestTargetDistance;

    private readonly float FORCE = 0.25f;
    public GameController gameController;
    public Vector3 startingPos;
    private Vector3 startingSize = new Vector3(4, 4, 4);
    private float startingMass = 4f;
    private float startingSpeed = 4f;

    public override void InitializeAgent()
    {
        menu = gameController.menu;
        rewards = gameController.rewards;
        playerRewards = gameController.playerRewards;
        self = gameObject;
        rigidBody = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        string otherName = collision.gameObject.name;
        BallAgent otherAgent = collision.gameObject.GetComponent<BallAgent>();

        // Sets a force multiplier that is a fraction of the magnitude of the relative velocity of the collision
        float forceMultiplier = 1 + collision.relativeVelocity.magnitude / 3;

        // Then store a variable that is the force multiplied by the force multiplier
        float energy = FORCE * forceMultiplier;

        // If colliding with an object whose name contains the string stored within "absorb"
        if (otherName.Contains(absorb))
        {
            Absorb(otherAgent, energy, 0.6f, 0.4f, 0.1f);
        }

        // If colliding with an object whose name contains the string stored within "destroy"
        else if (otherName.Contains(destroy))
        {
            Destroy(otherAgent, energy, 1.05f, 0.05f, 0.1f);
        }

        // If colliding with a smaller object whose name contains the string stored within "sameElement"
        else if (otherName.Contains(sameElement) && (otherAgent.gameObject.transform.localScale.x < transform.localScale.x))
        {
            Absorb(otherAgent, energy, 0.40f, 0.35f, 0.075f);
        }
    }

    private void Absorb(BallAgent otherAgent, float energy, float growthMultiplier, float shrinkMultiplier, float reward)
    {
        if (rewards)
        {
            if (otherAgent.team != team)
            {
                // Rewarding the agents for absorbing an enemy
                AddReward(reward);
                otherAgent.AddReward(-reward);

                if (playerRewards)
                {
                    friendlyPlayer.AddReward(reward);
                    // Penalizing the other agents for being absorbed by an enemy
                    enemyPlayer.AddReward(-reward);
                }
            }
            else
            {
                // Lesser reward for the agents involved in size balancing among teammates
                if (otherAgent.transform.localScale.x > transform.localScale.x)
                {
                    float halfReward = reward / 2;

                    AddReward(halfReward);
                    otherAgent.AddReward(halfReward);

                    if (playerRewards)
                    {
                        friendlyPlayer.AddReward(halfReward);

                        // Penalty for enemy player agent
                        enemyPlayer.AddReward(-halfReward);
                    }
                }
                // Penalizes the agent and player for feeding upon a smaller teammate
                else if (otherAgent.transform.localScale.x < transform.localScale.x)
                {
                    AddReward(-reward);
                    otherAgent.AddReward(-reward);
                    friendlyPlayer.AddReward(-reward);
                    enemyPlayer.AddReward(reward);
                }
            }
        }
        
        float growth = energy * growthMultiplier;
        float damage = energy * shrinkMultiplier;

        transform.localScale += new Vector3(growth, growth, growth);
        rigidBody.mass += growth;
        speed += growth;

        otherAgent.gameObject.transform.localScale -= new Vector3(damage, damage, damage);
        otherAgent.gameObject.GetComponent<Rigidbody>().mass -= damage;
        otherAgent.speed -= damage;

        SizeCheck(otherAgent);
    }

    private void Destroy(BallAgent otherAgent, float energy, float damageMultiplier, float shrinkMultiplier, float reward)
    {
        if (rewards)
        {
            // Penalizing the other agent for taking damage
            otherAgent.AddReward(-reward);

            // Checking for friendly fire, then the agent causing the collision is penalized
            // And the enemy player is rewarded
            if (otherAgent.team == team)
            {
                AddReward(-reward);
                
                if (playerRewards)
                {
                    friendlyPlayer.AddReward(-reward);
                    enemyPlayer.AddReward(reward);
                }
            }
            // Otherwise this agent is rewarded and the player of the other agent is penalized
            else
            {
                AddReward(reward);
                
                if (playerRewards)
                {
                    friendlyPlayer.AddReward(reward);
                    otherAgent.friendlyPlayer.AddReward(-reward);
                }
            }
        }

        float shrinkage = energy * shrinkMultiplier;
        float damage = energy * damageMultiplier;

        transform.localScale -= new Vector3(shrinkage, shrinkage, shrinkage);
        rigidBody.mass -= shrinkage;
        speed -= shrinkage;

        otherAgent.gameObject.transform.localScale -= new Vector3(damage, damage, damage);
        otherAgent.gameObject.GetComponent<Rigidbody>().mass -= damage;
        otherAgent.speed -= damage;

        SizeCheck(otherAgent);
    }

    private void SizeCheck(BallAgent agent)
    {
        if (agent.transform.localScale.x <= 1f)
        {
            if (rewards)
            {
                // Penalty if killing a teammate
                if (agent.team == team)
                {
                    AddReward(-0.3f);
                    
                    if (playerRewards)
                    {
                        friendlyPlayer.AddReward(-0.3f);
                        enemyPlayer.AddReward(0.3f);
                    }
                }
                // Reward for killing an enemy
                else
                {
                    AddReward(0.3f);

                    if (playerRewards)
                    {
                        friendlyPlayer.AddReward(0.3f);
                    }
                }
            }   

            // Checks which team the agent is on, and decrements the team counter accordingly

            if (agent.team == "Yin")
            {
                gameController.yinCount--;
            }
            else
            {
                gameController.yangCount--;
            }

            // Sets agent as inactive
            agent.Death();
        }
    }

    // Called upon by other agents
    private void Death()
    {
        mesh.enabled = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = Vector3.zero;
        gameObject.GetComponent<SphereCollider>().enabled = false;
    }

    public override void CollectObservations()
    {
        if (rigidBody.useGravity)
        {
            // So the agent knows how fast it is moving

            AddVectorObs(rigidBody.velocity.x);
            AddVectorObs(rigidBody.velocity.z);

            // Enemies

            ObserveEnemy(absorbTarget, "pursue", true);

            ObserveEnemy(destroyTarget, "pursue", true);

            ObserveEnemy(enemyMirror, "mirror", true);

            if (playerRewards)
            {
                friendlyPlayer.AddReward(-nearestTargetDistance / 1000000000);
            }

            ObserveEnemy(destroyer, "avoid");

            ObserveEnemy(predator, "avoid");

            // Teammates

            ObserveTeammate(friendlyWood);

            ObserveTeammate(friendlyFire);

            ObserveTeammate(friendlyEarth);

            ObserveTeammate(friendlyMetal);

            ObserveTeammate(friendlyWater);

        }// Up to 34 maximum observations
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

            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);
        }
    }

    // Observes enemy vector (x, z) position, and x scale if "size" parameter is set to "true"
    private void ObserveEnemy(GameObject enemy, string mode, bool size = false)
    {
        if (enemy.GetComponent<Rigidbody>().useGravity)
        {
            thisSurfaceToTargetSurface = GetComponent<Collider>().ClosestPointOnBounds(enemy.transform.localPosition);
            targetSurfaceToThisSurface = enemy.GetComponent<Collider>().ClosestPointOnBounds(transform.localPosition);

            float distanceToEnemy = Vector3.Distance(thisSurfaceToTargetSurface, targetSurfaceToThisSurface);
            Vector3 directionToEnemy = (enemy.transform.localPosition - transform.localPosition).normalized;

            AddVectorObs(distanceToEnemy);
            AddVectorObs(directionToEnemy.x);
            AddVectorObs(directionToEnemy.z);

            if (playerRewards)
            {
                if ((mode == "pursue") && (distanceToEnemy <= nearestTargetDistance))
                {
                    nearestTargetDistance = distanceToEnemy;
                }
                else if (mode == "avoid")
                {
                    friendlyPlayer.AddReward(distanceToEnemy / 1000000000);
                }
                else if (mode == "mirror")
                {
                    if (enemy.transform.localScale.x >= transform.localScale.x)
                    {
                        friendlyPlayer.AddReward(distanceToEnemy / 1000000000);
                    }
                    else if (distanceToEnemy <= nearestTargetDistance)
                    {
                        nearestTargetDistance = distanceToEnemy;
                    }
                }
            }
            
            if (size)
            {
                AddVectorObs(enemy.transform.localScale.x - transform.localScale.x);
            }
        }
        else
        {
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);

            if (size)
            {
                AddVectorObs(0);
            }
        }
    }

    //Observes teammate vector (x, z) position, and x scale if either "elementA" or "elementB" conditions are true
    private void ObserveTeammate(GameObject teammate)
    {
        if (self != teammate)
        {
            if (teammate.GetComponent<Rigidbody>().useGravity)
            {
                thisSurfaceToTargetSurface = GetComponent<Collider>().ClosestPointOnBounds(teammate.transform.localPosition);
                targetSurfaceToThisSurface = teammate.GetComponent<Collider>().ClosestPointOnBounds(transform.localPosition);

                float distanceToTeammate = Vector3.Distance(thisSurfaceToTargetSurface, targetSurfaceToThisSurface);
                Vector3 directionToTeammate = (teammate.transform.localPosition - transform.localPosition).normalized;

                AddVectorObs(distanceToTeammate);
                AddVectorObs(directionToTeammate.x);
                AddVectorObs(directionToTeammate.z);

                BallAgent teamAgent = teammate.GetComponent<BallAgent>();

                // Checks if the agent can absorb or be absorbed by the teammate
                // Also used to reward the agent's player for maximizing or minimizing distance
                if (name.Contains(teamAgent.absorb) | teammate.name.Contains(absorb))
                {
                    float teamMateScale = teammate.transform.localScale.x;
                    float thisScale = transform.localScale.x;

                    AddVectorObs(teamMateScale - thisScale);

                    // If the teammate can absorb this agent
                    if (name.Contains(teamAgent.absorb))
                    {
                        // Checks if getting too big, then will share size equally with teammate
                        Overflow(teamAgent);

                        if (playerRewards)
                        {
                            // If the teammate is smaller than this agent
                            if (teamMateScale < thisScale)
                            {
                                // Very small constant negative reward for keeping away based off distance
                                friendlyPlayer.AddReward(-distanceToTeammate / 1000000000);
                            }
                            else
                            {
                                friendlyPlayer.AddReward(distanceToTeammate / 1000000000);
                            }
                        }
                        
                    } // If the agent is able to absorb the teammate
                    else
                    {
                        if (playerRewards)
                        {
                            // If the teammate is larger than this agent
                            if (teamMateScale > thisScale)
                            {
                                // Very small negative reward for keeping away based on distance
                                friendlyPlayer.AddReward(-distanceToTeammate / 1000000000);
                            }
                            else
                            {
                                friendlyPlayer.AddReward(distanceToTeammate / 1000000000);
                            }
                        }
                    }
                } // If the agent can destroy, or be destroyed by the teammate
                else if (playerRewards && (name.Contains(teamAgent.destroy) | teammate.name.Contains(destroy)))
                {
                    // Very small constant reward for keeping away based on distance
                    friendlyPlayer.AddReward(distanceToTeammate / 1000000000);
                }
            }
            else
            {
                AddVectorObs(0);
                AddVectorObs(0);
                AddVectorObs(0);

                BallAgent teamAgent = teammate.GetComponent<BallAgent>();

                if (name.Contains(teamAgent.absorb) | teamAgent.name.Contains(absorb))
                {
                    AddVectorObs(0);

                    Overflow(teamAgent, true);
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction)
    {
        if (rigidBody.useGravity)
        {
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];

            rigidBody.AddForce(controlSignal * speed * 6);

            if (team == "Yin")
            {
                if (gameController.yangCount <= 3)
                {
                    DominationCheck(team);
                }
            }
            else
            {
                if (gameController.yinCount <= 3)
                {
                    DominationCheck(team);
                }
            }

            // Reset agent's position if it somehow ends up out of bounds due to physics/collider bugs
            if (transform.localPosition.x > 65 | transform.localPosition.x < -40 |
                transform.localPosition.z > 90 | transform.localPosition.z < -95 |
                transform.localPosition.y < 5 | transform.localPosition.y > 100)
            {
                ResetPosition();
            }
        }
    }

    private void DominationCheck(string winningTeam)
    {
        // If the agent has no predator and destroyer and is larger than the other orb of its same type
        if (!predator.GetComponent<Rigidbody>().useGravity &&
            !destroyer.GetComponent<Rigidbody>().useGravity &&
            (transform.localScale.x > enemyMirror.transform.localScale.x))
        {
            gameController.Win(winningTeam);
        }
    }

    private void Overflow(BallAgent teamAgent, bool resurrection = false)
    {
        if (transform.localScale.x > 8)
        {
            Vector3 share = transform.localScale / 4;
            
            transform.localScale -= share;
            speed -= share.x;
            rigidBody.mass -= share.x;
            
            teamAgent.transform.localScale += share;
            teamAgent.speed += share.x;
            teamAgent.rigidBody.mass += share.x;
            
            if (resurrection)
            {
                if (team == "Yin")
                {
                    gameController.yinCount++;
                }
                else
                {
                    gameController.yangCount++;
                }
                
                teamAgent.transform.localPosition = new Vector3(0, 19, 0);
                teamAgent.Resurrect();
            }
        }
    }

    public override void AgentReset()
    {
        Resurrect();
        speed = startingSpeed;
        rigidBody.mass = startingMass;
        transform.localScale = startingSize;
        ResetPosition();
    }

    private void ResetPosition()
    {
        rigidBody.velocity = Vector3.zero;
        transform.localPosition = startingPos;
    }

    public void Resurrect()
    {
        mesh.enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = true;
        rigidBody.useGravity = true;
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}