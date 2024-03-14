using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AgentController1 : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] GameObject Tomb;
    [SerializeField] GameObject weapon;

    public NavMeshAgent Agent { get; private set; }
    public bool FinishedPath { get; private set; } = false;
    public float Health { get; set; } = 100;
    public bool AttackTaskStop { get; set; } = false;
    public bool RotateTaskStop { get; set; } = false;
    public bool isAttacking { get; set; } = false;

    public bool isAlive = true;
    bool terrainGenerated;
    bool gotUpdated = false;
    float StoppingDistance = 4.5f;
    float rotationSpeed = 0.05f;
    float backstabAngleThreshold = 135f;
    Coroutine rotationCoroutine = null;
    Coroutine attackCoroutine = null;
    Renderer warriorRenderer;
    CapsuleCollider warriorColider;
    GameObject band;
    Animator weaponAnimator;
    Quaternion rotation = Quaternion.identity;

    //netcode
    //general

    //server
    //client
    
    #region internal
    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.enabled = false;

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        int vertexIndex = Random.Range(0, triangulation.vertices.Length);

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Debug.Log(hit.position);
            Agent.Warp(hit.position);
            Agent.enabled = true;
        }

        warriorRenderer = transform.gameObject.GetComponent<Renderer>();
        warriorColider = transform.gameObject.GetComponent<CapsuleCollider>();
        band = transform.parent.gameObject;
        weaponAnimator = weapon.GetComponent<Animator>();
    }
    private void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance && !Agent.pathPending && gotUpdated)
        {
            OnDestinationReached();
        }
        if (Health <= 0 && isAlive)
        {
            warriorFallen();
        }
    }
    private void FixedUpdate()
    {
        
    }
    private void OnDestinationReached()
    {
        Rotate(rotation);
        gotUpdated = false;
        FinishedPath = true;
        weaponAnimator.SetTrigger("IdleTrig");
    }
    private void Rotate(Quaternion targetRotation)
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(Rotator(targetRotation));
    }
    private void Rotate(Vector3 pos)
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        Quaternion targetRotation = Quaternion.LookRotation((pos - transform.position).normalized);
        targetRotation.x = 0;
        targetRotation.z = 0;
        rotationCoroutine = StartCoroutine(Rotator(targetRotation));
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
        foreach (Transform childTransform in transform)
        {
            if (childTransform.gameObject.GetComponent<MeshRenderer>() != null)
            {
                childTransform.gameObject.GetComponent<MeshRenderer>().enabled = value;
            }
            else
            {
                childTransform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = value;
            }
        }
    }
    #endregion internal
    #region external
    [ServerRpc]
    public void MoveToDestinationServerRpc(InputState inputState)
    {
        MoveToDestination(inputState);
    }
    public void MoveToDestination(InputState inputState)
    {
        if (!IsServer)
        {
            MoveToDestinationServerRpc(inputState);
        }
        else
        {
            Debug.Log(inputState.NewDestination);
        }
        

        gotUpdated = true;
        FinishedPath = false;
        rotation = inputState.TargetRotation;
        Agent.SetDestination(inputState.NewDestination);
        StopAllCoroutines();
        isAttacking = false;
    }
    [ServerRpc]
    public void AttackServerRpc(NetworkObjectReference reference)
    {
        GameObject warrior = null;
        if (reference.TryGet(out NetworkObject netObject))
        {
            warrior = netObject.gameObject;
        }

        Attack(warrior);
    }
    public void Attack(GameObject warrior)
    {
        AttackServerRpc(new NetworkObjectReference(warrior));

        StopAllCoroutines();
        if (isAlive)
        {
            StartCoroutine(AttackTask(warrior, .1f));
        }
    }
    IEnumerator AttackTask(GameObject warriorToAttack, float interval)
    {
        isAttacking = true;
        while (!AttackTaskStop && isAlive)
        {
            yield return new WaitForSeconds(interval);
            if (Vector3.Distance(transform.position, warriorToAttack.transform.position) > StoppingDistance)
            {
                Agent.SetDestination(warriorToAttack.transform.position);
            }
            else
            {
                Agent.ResetPath();
                if (GetDotProduct(transform.gameObject, warriorToAttack) < 1f)
                {
                    Rotate(warriorToAttack.transform.position);
                }
                yield return new WaitForSeconds(1.6f);
                if (GetDotProduct(warriorToAttack, transform.gameObject) < -.3f)
                {
                    warriorToAttack.GetComponent<AgentController1>().DealDamage(10, transform.gameObject);
                    weaponAnimator.SetTrigger("CriticalTrig");
                }
                else
                {
                    warriorToAttack.GetComponent<AgentController1>().DealDamage(5, transform.gameObject);
                    weaponAnimator.SetTrigger("SlashTrig");
                }
                if (!warriorToAttack.GetComponent<AgentController1>().isAlive)
                {
                    GameObject enemyWarband = warriorToAttack.transform.parent.gameObject;
                    List<GameObject> warriorsByDistance = band.GetComponent<WarBandController1>().GetWarriorsInWarBandByDistance(enemyWarband, transform.position);
                    bool found = false;
                    foreach (GameObject enem in warriorsByDistance)
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
    public void DealDamage(float dmg, GameObject enemyWarrior)
    {
        DealDamage(dmg);
        if (!isAttacking)
        {
            Attack(enemyWarrior);
        }
        GameObject enemyWarband = enemyWarrior.transform.parent.gameObject;
        band.GetComponent<WarBandController1>().GettingAttacked(enemyWarband);
    }
    public void DealDamage(float dmg)
    {
        Health -= dmg;   
    }
    #endregion external
}