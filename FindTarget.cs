using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTarget : MonoBehaviour
{
    public string enemySameElement;
    public string primaryTargetType;
    public string secondaryTargetType;
    public string primaryNemesisType;
    public string secondaryNemesisType;

    public float speed = 8.0f;
    private Rigidbody rigidBody;

    private GameObject primaryTarget;
    private GameObject secondaryTarget;
    private GameObject primaryNemesis;
    private GameObject secondaryNemesis;
    private GameObject sameElement;
    

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        primaryTarget = GameObject.FindGameObjectWithTag(primaryTargetType);
        secondaryTarget = GameObject.FindGameObjectWithTag(secondaryTargetType);
        primaryNemesis = GameObject.FindGameObjectWithTag(primaryNemesisType);
        secondaryNemesis = GameObject.FindGameObjectWithTag(secondaryNemesisType);
        sameElement = GameObject.FindGameObjectWithTag(enemySameElement);
    }

    // Update is called once per frame
    void Update()
    {   
        if (primaryTarget | secondaryTarget)
        {
            if (primaryTarget)
            {
                Pursue(primaryTarget);
            }
            else
            {
                if (GameObject.FindGameObjectWithTag(primaryTargetType))
                {
                    primaryTarget = GameObject.FindGameObjectWithTag(primaryTargetType);
                    Pursue(primaryTarget);
                }
                if (secondaryTarget)
                {
                    Pursue(secondaryTarget);
                }
                else if (GameObject.FindGameObjectWithTag(secondaryTargetType))
                {
                    secondaryTarget = GameObject.FindGameObjectWithTag(secondaryTargetType);
                    Pursue(secondaryTarget);
                }
            }
        }
        else 
        {
            if (GameObject.FindGameObjectWithTag(primaryTargetType) | GameObject.FindGameObjectWithTag(secondaryTargetType))
            {
                if (GameObject.FindGameObjectWithTag(primaryTargetType))
                {
                    primaryTarget = GameObject.FindGameObjectWithTag(primaryTargetType);
                    Pursue(primaryTarget);
                }
                else
                {
                    secondaryTarget = GameObject.FindGameObjectWithTag(secondaryTargetType);
                    Pursue(secondaryTarget);
                }
            }
            if (sameElement)
            {
                Pursue(sameElement);
            }
            if (primaryNemesis | secondaryNemesis)
            {
                if (primaryNemesis)
                {
                    Flee(primaryNemesis);
                }
                if (secondaryNemesis)
                {
                    Flee(secondaryNemesis);
                }
                
            }
            else
            {
                if (GameObject.FindGameObjectWithTag(primaryNemesisType) | GameObject.FindGameObjectWithTag(secondaryNemesisType))
                {
                    if (GameObject.FindGameObjectWithTag(primaryNemesisType))
                    {
                        primaryNemesis = GameObject.FindGameObjectWithTag(primaryNemesisType);
                        Flee(primaryNemesis);
                    }
                    else
                    {
                        secondaryNemesis = GameObject.FindGameObjectWithTag(secondaryNemesisType);
                        Flee(secondaryNemesis);
                    }
                }
            }
        }
    }

    void Pursue(GameObject target)
    {
        Vector3 lookDirection = (target.transform.position - transform.position).normalized;
        rigidBody.AddForce(lookDirection * speed);
    }

    void Flee(GameObject nemesis)
    {
        Vector3 lookDirection = (transform.position - nemesis.transform.position).normalized;
        rigidBody.AddForce(lookDirection * speed);
    }
}
