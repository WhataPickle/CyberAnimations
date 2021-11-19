using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public Animator animator;

    public bool colOnOff = false;

    public SphereCollider interactionRange;
    public List<Collider> RagColliders = new List<Collider>();
    public List<Rigidbody> RagRBs = new List<Rigidbody>();

    private bool respawning = false;

    private void Awake()
    {
        GetColliders();
    }


    void Update()
    {
        TurnRagdollOnOff(colOnOff);
        if (colOnOff)
        {
            if (!respawning)
            {
                StartCoroutine(Respawn());
            }
        }
    }

    void GetColliders() //Gets all rigidbodies and colliders of children components
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        Rigidbody[] rb = gameObject.GetComponentsInChildren<Rigidbody>();

        foreach(Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.enabled = false;
                RagColliders.Add(c);
            }
        }
        foreach(Rigidbody r in rb)
        {
            if(r.gameObject != this.gameObject)
            {
                r.isKinematic = true;
                RagRBs.Add(r);
            }
        }
    }

    public void TurnRagdollOnOff(bool y) //If y is true enables ragdoll
    {
        if (y)
        {
            animator.enabled = false;
            interactionRange.enabled = false;
            foreach (Collider c in RagColliders)
            {
                c.enabled = true;
            }
            foreach(Rigidbody r in RagRBs)
            {
                r.isKinematic = false;
            }
        }
        else
        {
            animator.enabled = true;
            interactionRange.enabled = true;
            foreach (Collider c in RagColliders)
            {
                c.enabled = false;
            }
            foreach (Rigidbody r in RagRBs)
            {
                r.isKinematic = true;
            }
        }
    }

    private IEnumerator Respawn() //Respawns enemy at random location and resets rigidbodies
    {
        respawning = true;
        yield return new WaitForSeconds(3);
        transform.position = new Vector3(Random.Range(-5, 6), 0, Random.Range(-5, 6));
        colOnOff = false;
        respawning = false;
        yield break;
    }
}
