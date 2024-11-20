using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LethalEquipment : MonoBehaviour
{
    enum lethalType { grenade };
    [SerializeField] lethalType type;

    public float delay = 3f, range = 5f, damage = 100f;
    float countdown;
    bool hasExploded = false;
    public ParticleSystem explosionEffect;

    public LayerMask enemyMask;

    // Start is called before the first frame update
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0 && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, range);
    }

    void Explode()
    {
                countdown = delay;
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, range, enemyMask, QueryTriggerInteraction.Ignore);
        foreach (Collider col in objectsInRange)
        {
            GameObject enemy = col.gameObject;
            if (enemy.CompareTag("Zombie"))
            {
                int i = 0;
                i++;
                Debug.Log(i);
                // linear falloff of effect
                float proximity = (transform.position - enemy.transform.position).magnitude;
                float effect = 1 - (proximity / range);
                Debug.Log(damage * effect);
                enemy.GetComponent<ZM_AI>().ReduceHP(damage * effect);
            }
        }
        Destroy(gameObject);
    }
}
