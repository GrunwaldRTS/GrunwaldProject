using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentController1 : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }
    [SerializeField] GameObject Tomb;
    public bool FinishedPath { get; private set; } = false;
    bool terrainGenerated;
    public float Health { get; set; } = 100 ;
    bool gotUpdated = false;
    public bool isAlive = true;
    public bool AttackTaskStop { get; set; } = false;
    public bool RotateTaskStop { get; set; } = false;
    public bool isAttacking { get; set; } = false;
    private Coroutine rotationCoroutine = null;
    private Coroutine attackCoroutine = null;
    private float StoppingDistance = 3f;
    Renderer warriorRenderer;
    CapsuleCollider warriorColider;
    private float rotationSpeed = 0.05f;
    private Quaternion rotation = Quaternion.identity;
    GameObject band;
    private float backstabAngleThreshold = 135f;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        warriorRenderer = transform.gameObject.GetComponent<Renderer>();
        warriorColider = transform.gameObject.GetComponent<CapsuleCollider>();
        band = transform.parent.gameObject;
    }

    private float GetDotProduct(GameObject obj1, GameObject obj2)
    {
        // Kierunek obiektu 1
        Vector3 direction1 = obj1.transform.forward;

        // Kierunek do obiektu 2
        Vector3 toObject2 = (obj2.transform.position - obj1.transform.position).normalized;
        toObject2.y = 0f; // Ignoruj ró¿nice w wysokoœci

        // Iloczyn skalarny jednostkowych wektorów kierunkowych
        float dotProduct = Vector3.Dot(direction1, toObject2);

        return dotProduct;
    }

    public void DealDamage(float dmg)
    {
        Health -= dmg;   
    }
    public void DealDamage(float dmg, GameObject enemyWarrior)
    {
        Health -= dmg;
        if (!isAttacking)
        {
            Attack(enemyWarrior);
        }
        GameObject enemyWarband = enemyWarrior.transform.parent.gameObject;
        band.GetComponent<WarBandController1>().GettingAttacked(enemyWarband);
    }
    private void warriorFallen()
    {
        isAlive = false;
        Vector3 adjustedPos = transform.position;
        adjustedPos.y = adjustedPos.y - .3f;
        GameObject newCross = Instantiate(Tomb, adjustedPos, transform.rotation);
        warriorRenderer.enabled = false;
        warriorColider.enabled = false;
        SetChildrenRenderers(false);
        StopAllCoroutines();
    }

    private void SetChildrenRenderers(bool value)
    {
        foreach(Transform childTransform in transform)
        {
            childTransform.gameObject.GetComponent<MeshRenderer>().enabled = value;
        }
    }

    private void Start()
	{
        EventManager.OnChunkMeshesInstanced.AddListener(() => { terrainGenerated = true; });
	}
	private void Update()
    {
        if (!terrainGenerated) return;
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending && gotUpdated)
        {
            OnDestinationReached();
        }
        if (Health <= 0 && isAlive)
        {
            warriorFallen();
        }
    }
    IEnumerator Rotator(Quaternion targetRotation)
    {
        float t = 0;
        while (t <= 1 && !RotateTaskStop)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            t += rotationSpeed * Time.deltaTime;

            yield return null;
        }
        RotateTaskStop = false;
        Debug.Log("Stopping rotation");
    }

    IEnumerator AttackTask( GameObject warriorToAttack, float interval)
    {
        isAttacking = true;
        while (!AttackTaskStop && isAlive)
        {
            yield return new WaitForSeconds(interval);
            if (Vector3.Distance(transform.position, warriorToAttack.transform.position) > StoppingDistance)
            {
                agent.SetDestination(warriorToAttack.transform.position);
            }
            else
            {
                agent.ResetPath();
                if (GetDotProduct(transform.gameObject,warriorToAttack) < .5f)
                {
                    Rotate(warriorToAttack.transform.position);
                }
                yield return new WaitForSeconds(1.6f);
                if(GetDotProduct(warriorToAttack, transform.gameObject) < -.3f)
                {
                    warriorToAttack.GetComponent<AgentController1>().DealDamage(10, transform.gameObject);    
                }
                else
                {
                    warriorToAttack.GetComponent<AgentController1>().DealDamage(5, transform.gameObject);
                }
                if (!warriorToAttack.GetComponent<AgentController1>().isAlive)
                {
                    GameObject enemyWarband = warriorToAttack.transform.parent.gameObject;
                    List<GameObject> warriorsByDistance = band.GetComponent<WarBandController1>().GetWarriorsInWarBandByDistance(enemyWarband, transform.position);
                    bool found = false;
                    foreach(GameObject enem in warriorsByDistance)
                    {
                        if (enem.GetComponent<AgentController1>().isAlive)
                        {
                            warriorToAttack = enem;
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        AttackTaskStop = true;
                    }
                }
            }
            yield return new WaitForSeconds(interval);
        }
        isAttacking = false;
        AttackTaskStop = false;
        Debug.Log("Stopping attack");
    }
    public void Attack(GameObject warrior)
    {
        StopAllCoroutines();
        if (isAlive)
        {
            StartCoroutine(AttackTask(warrior, .1f));
        }
    }

    private void Rotate(Quaternion targetRotation)
    {
        if(rotationCoroutine != null){
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(Rotator(targetRotation));
    }
    private void Rotate(Vector3 pos)
    {
        Quaternion targetRotation = Quaternion.LookRotation((pos - transform.position).normalized);
        targetRotation.x = 0;
        targetRotation.z = 0;
        rotationCoroutine = StartCoroutine(Rotator(targetRotation));
    }
    public void MoveToDestination(Vector3 pos, Quaternion targetRotation)
    {
		gotUpdated = true;
        FinishedPath = false;
        rotation = targetRotation;
        agent.SetDestination(pos);
        StopAllCoroutines();
        isAttacking = false;
    }
    private void OnDestinationReached()
    {
        Rotate(rotation);
        gotUpdated = false;
        FinishedPath = true;
    }
}