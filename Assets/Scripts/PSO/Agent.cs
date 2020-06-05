using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject globalBestPrefab;
    float sizeX = 30;
    float sizeY = 9;

    public int popsize=20;// population size
    public int MAXITER=3000;  // Maximum number of iterations
 
    float gBestCost = float.MaxValue;
    int bestParticle;
    Vector3[] velocities;
    Vector3[] positions;
    Vector3[] pBestPositions;
    float[] pBestCosts;
    Vector3 gBestPosition;
    GameObject[] particles;
    public Transform target;
    public float waitTime = 1;
    public float maxVelocity = 10;
    public float startInertia = 0.9f;
    public float endInertia = 0.4f;
    private int iteration = 0;
    public float c1 = 2;
    public float c2 = 2;
    GameObject globalBest;

    // Start is called before the first frame update
    void Start()
    {
        particles = new GameObject[popsize];
        pBestCosts = new float[popsize];
        pBestPositions = new Vector3[popsize];
        velocities = new Vector3[popsize];
        positions = new Vector3[popsize];
        for (int i = 0; i < popsize; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-sizeX, sizeX), Random.Range(-sizeY, sizeY));
            particles[i] = Instantiate(agentPrefab, pos, Quaternion.identity);
            float cost = Vector3.Distance(target.position, pos);
            if(cost < gBestCost)
            {
                gBestCost = cost;
                bestParticle = i;
                gBestPosition = pos;
            }
            pBestPositions[i] = pos;
            pBestCosts[i] = cost;
            positions[i] = pos;
            velocities[i] = Vector3.ClampMagnitude(pos, maxVelocity);//.normalized;
        }
        showGlobalBest();
        StartCoroutine("RunPSO");
    }

    IEnumerator RunPSO()
    {
        while ( iteration < MAXITER)
        {
            iteration++;
            Debug.Log("gBestCost " + gBestCost);
            for (int i = 0; i < popsize; i++)
            {
                Vector2 vel = Vector3.ClampMagnitude(getVelocity(velocities[i], positions[i], pBestPositions[i]), maxVelocity);//.normalized;
                Vector2 pos = getPosition(positions[i], vel);
                float cost = Vector3.Distance(target.position, pos);
                Debug.Log("cost " + cost);
                if (cost < gBestCost)
                {
                    gBestCost = cost;
                    bestParticle = i;
                    gBestPosition = pos;
                }
                if(cost < pBestCosts[i])
                {
                    pBestCosts[i] = cost;
                    pBestPositions[i] = pos;
                }
                positions[i] = pos;
                velocities[i] = vel;
                particles[i].transform.position = pos;
            }
            showGlobalBest();
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3 getVelocity(Vector3 previousVelocity, Vector3 previousPosition, Vector3 pBest)
    {
        return getInertia() * previousVelocity + c1 * Random.Range(0f, 1f) * (pBest - previousPosition) + c2 * Random.Range(0f, 1f) * (gBestPosition - previousPosition);
    }

    Vector3 getPosition(Vector3 previousPosition, Vector3 currentVelocity)
    {
        return previousPosition + currentVelocity;
    }

    float getInertia()
    {
        return startInertia - ((startInertia - endInertia) * iteration / MAXITER);
    }

    void showGlobalBest()
    {
        if(globalBest != null)
        {
            Destroy(globalBest);
        }
        globalBest =  Instantiate(globalBestPrefab, gBestPosition, Quaternion.identity);
    }
}
