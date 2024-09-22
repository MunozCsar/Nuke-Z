using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/*
    Este script controla el comportamiento de la inteligencia artificial de los zombies en el juego.
    Gestiona la navegaci�n de los zombies, su interacci�n con las barreras y jugadores, as� como su vida y muerte.
*/

public class ZM_AI : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundEffects;

    public int zm_aggro_level;

    public Animator zm_Animator;
    public GameObject nearestObject, bodyCollider, headCollider, soulPrefab;
    private NavMeshAgent agent = null;
    public float patrolRange;
    private Transform target;
    public GameObject[] targetBarrier;
    public bool focusBarrier, isAlive, isInZone;
    public float t_barrier, barrier_coolDown, d_Nearest, hp;

    void Start()
    {
        hp = GameManager.Instance.zm_HP;
        target = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 2f;
        targetBarrier = GameObject.FindGameObjectsWithTag("DestroyBarrier");
        nearestObject = targetBarrier[0];
        d_Nearest = Vector3.Distance(transform.position, nearestObject.transform.position);
        

        // Encuentra la barrera m�s cercana al zombie
        for (int i = 1; i < targetBarrier.Length; i++)
        {
            float distanceToCurrent = Vector3.Distance(transform.position, targetBarrier[i].transform.position);

            if (distanceToCurrent < d_Nearest)
            {
                nearestObject = targetBarrier[i];
                d_Nearest = distanceToCurrent;
            }
        }

    }

    void Update()
    {
        if (target.GetComponent<PlayerController>().isDowned)
        {
            if (agent.remainingDistance <= agent.stoppingDistance) //done with path
            {
                if (RandomPoint(transform.position, patrolRange, out Vector3 point)) //pass in our centre point and radius of area
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                    agent.SetDestination(point);
                }
            }
        }
        else
        {
            if (!focusBarrier && isAlive)
            {
                MoveToTarget();
            }
            else if (isAlive)
            {
                DestroyBarrier();
            }
            if (!isAlive)
            {
                isInZone = false;
            }
        }

        // Controla la velocidad y el da�o del zombie seg�n la oleada actual del juego

        if (GameManager.Instance.isPaused)
        {
            GetComponent<AudioSource>().Pause();
        }
        else if(!GameManager.Instance.isPaused)
        {
            GetComponent<AudioSource>().UnPause();
        }

        // if(GameManager.Instance.wave < 3)
        // {

        //     agent.speed = 2f;
        //     zm_Animator.SetBool("zombie_Run", false);
        // }
        // else if(GameManager.Instance.wave >= 3 && GameManager.Instance.wave < 5)
        // {

        //     agent.speed = 3f;
        //     zm_Animator.SetBool("zombie_Run", false);
        // }
        // else if(GameManager.Instance.wave >= 5)
        // {
        //     agent.speed = 4f;
        //     zm_Animator.SetBool("zombie_Run", true);
        // }

        // Controla el movimiento del zombie hacia su objetivo


    }

        public void ZM_Aggro(int aggro_level)
        {
            switch (aggro_level) 
            {
                case 0: 
                    GameManager.Instance.zm_Damage = 50;
                    agent.speed = 0f;
                    agent.ResetPath();
                    break;
                case 1:
                    GameManager.Instance.zm_Damage = 50;
                    agent.speed = 2f;
                    zm_Animator.SetBool("zombie_Run", false);
                    break;
                case 2:
                break;
            }
        }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    // Hace que el zombie se mueva hacia su objetivo
    private void MoveToTarget()
    {
        agent.SetDestination(target.position);
    }

    // Hace que el zombie se mueva hacia y destruya una barrera
    private void DestroyBarrier()
    {
        agent.SetDestination(nearestObject.transform.position);
    }

    // Reduce los puntos de salud del zombie y gestiona su muerte en caso de llegar a cero
    // Detecta disparos a la cabeza
    public void ReduceHP(float damage, bool headShot)
    {
        hp -= damage;
        if (hp <= 0)
        {
            ZM_Death(headShot);
        }
        else
        {
            GameManager.Instance.AddPoints(GameManager.Instance.pointsOnHit);
        }
    }

    // Sobrecarga de m�todo para reducir los puntos de salud del zombie sin especificar disparo a la cabeza
    public void ReduceHP(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            ZM_Death();
        }
        else
        {
            GameManager.Instance.AddPoints(GameManager.Instance.pointsOnHit);
        }
    }

    // Gestiona la muerte del zombie y otorga puntos al jugador
    public void ZM_Death(bool headShot)
    {
        GetComponent<RandomGrunts>().enabled = false;
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(soundEffects[Random.Range(3, 6)]);
        GameManager.Instance.killScore++;
        GameManager.Instance.zm_alive--;
        if (headShot)
        {
            GameManager.Instance.AddPoints(GameManager.Instance.pointsOnHead);
            GetComponent<Animator>().Play("Zombie_Headshot");
            agent.speed = 0f;
            agent.isStopped = true;
            isAlive = false;
            headCollider.SetActive(false);
            bodyCollider.SetActive(false);
            GetComponent<SphereCollider>().enabled = false;
        }
        else
        {
            GameManager.Instance.AddPoints(GameManager.Instance.pointsOnKill);
            GetComponent<Animator>().Play("Zombie_Death");
            agent.speed = 0f;
            agent.isStopped = true;
            isAlive = false;
            headCollider.SetActive(false);
            bodyCollider.SetActive(false);
            GetComponent<SphereCollider>().enabled = false;
        }
        // Si no est� atacando a una barrera, permite que se instancie un powerup
        if (!focusBarrier)
        {
            DropPowerUp();
        }
        HarvestSoul();
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        Destroy(this.gameObject, 15f);
        GameManager.Instance.zombieList.Remove(this.gameObject);
    }

    // Sobrecarga de m�todo para gestionar la muerte del zombie sin especificar disparo a la cabeza
    public void ZM_Death()
    {
        GetComponent<RandomGrunts>().enabled = false;
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(soundEffects[Random.Range(3, 6)]);
        GameManager.Instance.AddPoints(GameManager.Instance.pointsOnHead);
        GameManager.Instance.killScore++;
        GameManager.Instance.zm_alive--;
        GetComponent<Animator>().Play("Zombie_Death");
        agent.speed = 0f;
        agent.isStopped = true;
        isAlive = false;
        headCollider.SetActive(false);
        bodyCollider.SetActive(false);
        // Si no est� atacando a una barrera, permite que se instancie un powerup
        if (!focusBarrier)
        {
            DropPowerUp();
        }
        HarvestSoul();
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        GameManager.Instance.zombieList.Remove(gameObject);
        Destroy(this.gameObject, 15f);
    }

    // Mata a todos los zombies en la escena y limpia la lista
    public void ZM_Nuke()
    {
        GetComponent<RandomGrunts>().enabled = false;
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().PlayOneShot(soundEffects[Random.Range(3,6)]);
        Instantiate(GameManager.Instance.bloodFX[4], transform.position, GameManager.Instance.bloodFX[4].transform.rotation);
        GetComponent<Animator>().Play("Zombie_Death");
        agent.speed = 0f;
        agent.isStopped = true;
        isAlive = false;
        headCollider.SetActive(false);
        bodyCollider.SetActive(false);
        // Si no est� atacando a una barrera, permite que se instancie un powerup
        if (!focusBarrier)
        {
            DropPowerUp();
        }
        HarvestSoul();
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        Destroy(this.gameObject, 15f);
    }

    // Crea un alma en la zona correspondiente si el zombie muere en ella
    public void HarvestSoul()
    {
        if (isInZone && GameManager.Instance.powerOn)
        {
            Instantiate(soulPrefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
        }
    }

    // Genera un objeto de potenciador al azar cuando el zombie muere
    public void DropPowerUp()
    {
        if (GameManager.Instance.powerUp_current < GameManager.Instance.powerUp_max)
        {
            bool dropPowerUp = false;
            float rnd = Random.Range(0f, 1f);
            if (rnd < GameManager.Instance.powerUpChance)
            {
                dropPowerUp = true;
            }
            if (dropPowerUp.Equals(true))
            {
                int rndINT = Random.Range(0, GameManager.Instance.powerUpArray.Length);
                Vector3 pos = new(this.transform.position.x, this.transform.position.y + 2, this.transform.position.z);
                GameManager.Instance.InstancePowerUp(rndINT, pos);
                GameManager.Instance.powerUp_current++;
            }
        }
    }

    // Detecta la salida del jugador o el zombie de una zona espec�fica
    private void OnTriggerExit(Collider other)
    {
        // Detecta si el zombie ha dejado de hacer colision con el jugador y desactiva la animaci�n de ataque
        if (other.CompareTag("Player"))
        {
            agent.speed = 2f;
            zm_Animator.SetBool("isAttacking", false);
        }
        // Detecta si el zombie ha entrado en una zona de destrucci�n de barrera, asigna el valor a la variable y activa la animaci�n de ataque
        if (other.CompareTag("DestroyBarrier"))
        {
            focusBarrier = false;
            zm_Animator.SetBool("isAttacking", false);
        }
        if (other.CompareTag("SoulZone"))
        {
            isInZone = false;
        }
    }

    // Detecta la permanencia del jugador o el zombie en una zona espec�fica
    private void OnTriggerStay(Collider other)
    {
        // Detecta si el zombie ha colisionado con el jugador y activa la animaci�n de ataque
        if (other.CompareTag("Player"))
        {
            zm_Animator.SetBool("isAttacking", true);
        }
        if (other.CompareTag("SoulZone"))
        {
            isInZone = true;
        }

        if (other.CompareTag("DestroyBarrier") && isAlive)
        {
            if (t_barrier < barrier_coolDown)
            {
                t_barrier += Time.deltaTime;
            }
            else if (t_barrier >= barrier_coolDown && other.transform.parent.GetComponent<BarrierLogic>().hitPoints > 0)
            {
                zm_Animator.SetBool("isAttacking", true);
                t_barrier = 0f;
                other.transform.parent.GetComponent<BarrierLogic>().ReduceHitPoints();
            }

            if (other.transform.parent.GetComponent<BarrierLogic>().hitPoints <= 0)
            {
                focusBarrier = false;
                
            }
        }
    }

    public void PlaySound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        int rnd = Random.Range(0, 3);
        audioSource.pitch = Random.Range(0.85f, 0.9f);
        audioSource.PlayOneShot(soundEffects[rnd]);
    }
}

