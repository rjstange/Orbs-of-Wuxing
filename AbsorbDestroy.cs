using UnityEngine;

public class AbsorbDestroy : MonoBehaviour
{
    public string absorb;
    public string destroy;
    private float force = 0.25f;
    private Rigidbody rigidBody;
    private FindTarget movementScript;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        movementScript = GetComponent<FindTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // On Collision
    private void OnCollisionEnter(Collision collision)
    {
        // Sets a force multiplier that is a fraction of the magnitude of the relative velocity of the collision
        float forceMultiplier = 1 + collision.relativeVelocity.magnitude / 10;
        
        // Then store a variable that is the force multiplied by the force multiplier
        float energy = force * forceMultiplier;
        
        // If colliding with an object whose tag contains the string stored within "absorb"
        if (collision.gameObject.tag.Contains(absorb))
        {
            float growth = energy * 0.80f;
            float damage = energy * 0.25f;
            
            transform.localScale += new Vector3(growth, growth, growth);
            rigidBody.mass += growth;
            movementScript.speed += growth;

            collision.gameObject.transform.localScale -= new Vector3(damage, damage, damage);
            collision.gameObject.GetComponent<Rigidbody>().mass -= damage;
            collision.gameObject.GetComponent<FindTarget>().speed -= damage;
        }
        // If colliding with an object whose tag contains the string stored within "destroy"
        else if (collision.gameObject.tag.Contains(destroy))
        {
            float shrinkage = energy * 0.25f;
            float damage = energy * 0.80f;

            transform.localScale -= new Vector3(shrinkage, shrinkage, shrinkage);
            rigidBody.mass -= shrinkage;
            movementScript.speed -= shrinkage;

            collision.gameObject.transform.localScale -= new Vector3(damage, damage, damage);
            collision.gameObject.GetComponent<Rigidbody>().mass -= damage;
            collision.gameObject.GetComponent<FindTarget>().speed -= damage;
        }

        if (transform.localScale.x <= 1.0f)
        {
            Destroy(gameObject);
        }
    }
}
