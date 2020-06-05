using System.Collections;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public GameObject robotPrefab;
    public GameObject particlePrefab;

    public int popsize = 20;// population size
    public int MAXITER = 10;  // Maximum number of iterations

    //float gBestCost = float.MaxValue;
    float gBestCost;
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
    //GameObject robot;
    Vector2[] waypoints;
    Vector3 robotPos;
    float xMax;
    float yMax;
    GameObject robot;

    // Start is called before the first frame update
    void Start()
    {

        particles = new GameObject[popsize];
        pBestCosts = new float[popsize];
        pBestPositions = new Vector3[popsize];
        velocities = new Vector3[popsize];
        positions = new Vector3[popsize];

        robot = GameObject.FindGameObjectWithTag("Player");
        robotPos = robot.GetComponent<Transform>().position;
        target = GameObject.FindGameObjectWithTag("Target").GetComponent<Transform>();
        initPopulation();
        StartCoroutine("RunPSO");
    }

    IEnumerator RunPSO()
    {
        int j = 0;
        while (Vector3.Distance(robotPos,target.position) >= 1)
        {
            j++;
            Debug.Log("j: " + j);
            while (iteration < MAXITER)
            {
                iteration++;
                Debug.Log("gBestCost " + gBestCost);
                for (int i = 0; i < popsize; i++)
                {
                    Vector2 vel = Vector3.ClampMagnitude(getVelocity(velocities[i], positions[i], pBestPositions[i]), maxVelocity);//.normalized;
                    Vector2 pos = getPosition(positions[i], vel);
                    if (pos.x > xMax)
                    {
                        pos.x = xMax;
                    }
                    if (pos.y > yMax)
                    {
                        pos.y = yMax;
                    }
                    float cost = Vector3.Distance(target.position, pos);
                    if (cost < gBestCost)
                    {
                        gBestCost = cost;
                        bestParticle = i;
                        gBestPosition = pos;
                    }
                    if (cost < pBestCosts[i])
                    {
                        pBestCosts[i] = cost;
                        pBestPositions[i] = pos;
                    }
                    positions[i] = pos;
                    velocities[i] = vel;
                    particles[i].transform.position = pos;
                }
                if (iteration < MAXITER)
                {
                    yield return new WaitForSeconds(waitTime);
                }
                else
                {
                    Debug.Log("gBestPosition " + gBestPosition);
                    robot.transform.position = gBestPosition;
                    yield return new WaitForSeconds(waitTime);
                }
            }
            robotPos = gBestPosition;
            iteration = 0;
            clearPopulation();
            if (Vector3.Distance(robotPos, target.position) >= 1)
            {
                initPopulation();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawLine(robot.position, target.position, Color.magenta);
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

    void initPopulation()
    {
        xMax = robotPos.x + 1;
        yMax = robotPos.y + 1;
        gBestCost = float.MaxValue;
        for (int i = 0; i < popsize; i++)
        {
            Vector2 pos = new Vector2(Random.Range(robotPos.x - 1, robotPos.x + 1), Random.Range(robotPos.y - 1, robotPos.y + 1));
            particles[i] = Instantiate(particlePrefab, pos, Quaternion.identity);
            float cost = Vector3.Distance(target.position, pos);
            if (cost < gBestCost)
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
    }

    void clearPopulation()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Destroy(particles[i]);
        }
    }
}
