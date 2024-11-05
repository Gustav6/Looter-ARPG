using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class FlockingBehavior : MonoBehaviour
{
    public List<GameObject> agents;
    public int amountOfAgents;
    public GameObject prefab;
    public float radius;
    public float minDistanceFromNextAgent;

    void Start()
    {
        for (int i = 0; i < amountOfAgents; i++)
        {
            GameObject agent = Instantiate(prefab, transform);

            agent.transform.position = RandomPosition(radius);

            agents.Add(agent);
        }
    }

    void Update()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            CalculateMove(agents[i]);
        }
    }

    public void CalculateMove(GameObject agent)
    {
        agent.GetComponent<Rigidbody2D>().linearVelocity = Test(agent);
    }

    public Vector2 Test(GameObject agent)
    {
        Vector2 separationVelocity = Vector2.zero;
        float numberOfAgentsToAvoid = 0;

        foreach (GameObject other in agents)
        {
            if (ReferenceEquals(other, agent))
            {
                continue;
            }

            Vector3 otherPosition = other.transform.position;

            float distance = Vector2.Distance(otherPosition, agent.transform.position);

            if (distance < minDistanceFromNextAgent)
            {
                Vector2 otherAgentToCurrent = agent.transform.position - otherPosition;
                Vector2 directionToTravel = otherAgentToCurrent.normalized;
                Vector2 weightedVelocity;

                if (distance != 0)
                {
                    weightedVelocity = directionToTravel / distance;
                }
                else
                {
                    float x = Random.Range(-1, 1);
                    float y = Random.Range(-1, 1);

                    weightedVelocity = new Vector2(x, y);
                }

                separationVelocity += weightedVelocity;
                numberOfAgentsToAvoid++;
            }
        }

        if (numberOfAgentsToAvoid > 0)
        {
            separationVelocity /= numberOfAgentsToAvoid;
            separationVelocity *= 2;
        }

        Debug.Log(separationVelocity.ToString());

        return separationVelocity;
    }

    public Vector2 RandomPosition(float radius)
    {
        float r = radius * Mathf.Sqrt(Random.Range(0.0001f, 1));
        float theta = Random.Range(0.0001f, 1) * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
    }
}
