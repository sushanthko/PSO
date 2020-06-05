using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathWithObstacle : MonoBehaviour
{
    public GameObject particlePrefab;

    public int popsize = 20;// population size
    public int MAXITER = 10;  // Maximum number of iterations

    float gBestCost;// = float.MaxValue;
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
    GameObject robot;
    Vector2[] waypoints;
    Vector3 robotPos;
    float xMax;
    float yMax;
    float bound = 1;
    string direction;

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
        /*Debug.Log(robot.position);
        Debug.Log(target.position);
        Debug.DrawLine(robot.position, target.position, Color.magenta);
        Debug.Log(Physics2D.Linecast(robot.position, target.position).collider.name);
        Debug.Log(Physics2D.Linecast(robot.position, target.position).distance);
        Debug.Log(2 * (Mathf.Sqrt(2)));*/
        /*xMax = robotPos.x + bound;
        yMax = robotPos.y + bound;*/
        xMax = robotPos.x ;
        yMax = robotPos.y ;
        checkObstacle();
        initPopulation();
        StartCoroutine("RunPSO");
    }

    IEnumerator RunPSO()
    {
        int j = 0;
        //while (j < 20 && robotPos != target.position)
        while (Vector3.Distance(robotPos, target.position) >= 1)
        {
            j++;
            //Debug.Log("j: " + j);
            Debug.Log("bound: " + bound);
            Debug.Log("xMax: " + xMax);
            Debug.Log("yMax: " + yMax);
            while (iteration < MAXITER)
            {
                iteration++;
                //Debug.Log("gBestPosition " + gBestPosition);
                for (int i = 0; i < popsize; i++)
                {
                    Vector2 vel = Vector3.ClampMagnitude(getVelocity(velocities[i], positions[i], pBestPositions[i]), maxVelocity).normalized;
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
                    //Debug.Log("cost " + cost);
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
            checkObstacle();
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
        /*xMax = robotPos.x + 1;
        yMax = robotPos.y + 1;*/
        gBestCost = float.MaxValue;
        Debug.Log("bound: " + bound);
        for (int i = 0; i < popsize; i++)
        {
            Vector2 pos;
            if (direction == "v")
            {
                pos = new Vector2(Random.Range(robotPos.x - 2 * bound, robotPos.x), Random.Range(robotPos.y - bound, robotPos.y + bound));
            }else if(direction == "h")
            {
                pos = new Vector2(Random.Range(robotPos.x - bound, robotPos.x + bound), Random.Range(robotPos.y - 2 * bound, robotPos.y));
            }else
            {
                pos = new Vector2(Random.Range(robotPos.x - bound, robotPos.x + bound), Random.Range(robotPos.y - bound, robotPos.y + bound));
            }
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
            velocities[i] = Vector3.ClampMagnitude(pos, maxVelocity).normalized;
        }
    }

    void clearPopulation()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Destroy(particles[i]);
        }
    }

    void checkObstacle()
    {
        /*Debug.Log(Physics2D.Linecast(robot.position, target.position).collider.name);
        Debug.Log(Physics2D.Linecast(robot.position, target.position).distance);
        Debug.Log(2 * (Mathf.Sqrt(2)));*/

        Debug.Log("collider: "+Physics2D.Linecast(robotPos, target.position).collider.name);

        if(Physics2D.Linecast(robotPos, target.position).collider.name != "Target")
        {
            float distance = Physics2D.Linecast(robotPos, target.position).distance;
            Debug.Log("distance: "+distance);
            /*if (distance <= 2 && distance > 1)
            {
                setDirection();
                bound = 0.5f;
            }
            else if (distance <= 1 && distance > 0.5)
            {
                setDirection();
                bound = 0.25f;
            }
            else if (distance <= 0.5)
            {
                setDirection();
                bound = 0.125f;
            }*/
            bound = distance / 4;
            setDirection();
        }
        else
        {
            direction = "";
            bound = 1;
            xMax = robotPos.x + bound;
            yMax = robotPos.y + bound;
        }
       /* Debug.Log("bound: " + bound);
        Debug.Log("xMax: " + xMax);
        Debug.Log("yMax: " + yMax);*/
    }

    void setDirection()
    {
        Vector2 endCast = new Vector2();
        if (Random.Range(0f, 1f) > 0.5)
        {
            endCast.x = robotPos.x;
            endCast.y = 15;
            if (Physics2D.Linecast(robotPos, endCast).collider == null)
            {
                direction = "v";
                xMax = robotPos.x;
                //yMax++;
                yMax = robotPos.y + bound;
                return;
            }
        }
        else
        {
            //endCast = new Vector2();
            endCast.y = 35;
            endCast.y = robotPos.y;
            if (Physics2D.Linecast(robotPos, endCast).collider == null)
            {
                direction = "h";
                //xMax++;
                xMax = robotPos.x + bound;
                yMax = robotPos.y;
                return;
            }
        }
    }

}
