using Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Enemy : MonoBehaviour
{
    public bool walkInRoom = false;
    private NavMeshAgent agent;
    private EnemyState state;
    private List<Vector3> waypoint = new List<Vector3>();
    private int actialWaypointIndex;
    private GameObject target;
    private float observeRadius;

    private void Awake()
    {
        observeRadius = 5;
        state = EnemyState.Walking;
    }

    private void Start()
    {
        FindWaypoints();

        waypoint.Shuffle();
        transform.position = waypoint[0];
        actialWaypointIndex = 1;
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = float.MaxValue;
    }

    private void FixedUpdate()
    {
        target = CheckEnemyInArea();
        switch (state)
        {
            case EnemyState.Walking:
                if (target != null) state = EnemyState.Attacking;
                SetWaypoint();
                agent.speed = 0.5f;
                break;
            case EnemyState.Attacking:
                if (target != null) agent.destination = target.transform.position;
                else state = EnemyState.Walking;
                agent.speed = 1f;
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().state = PlayerState.Dead;
        }
    }

    private void FindWaypoints()
    {
        if (walkInRoom) FindWaypointsInRoom();
        else FindWaypointsInWorld();
    }

    private void FindWaypointsInRoom()
    {
        foreach (RaycastHit hit in Physics.RaycastAll(transform.position, Vector3.down))
        {
            if (hit.collider.name == "Floor")
            {
                for (int i = 0; i < hit.collider.gameObject.transform.childCount; i++)
                {
                    if (hit.collider.gameObject.transform.GetChild(i).name == "Spawnpoint")
                    {
                        waypoint.Add(hit.collider.gameObject.transform.GetChild(i).transform.position);
                    }
                }
                break;
            }
        }
    }

    private void FindWaypointsInWorld()
    {
        foreach (GameObject spawnpoint in FindObjectsOfType<GameObject>().Where(obj => obj.name == "Spawnpoint"))
        {
            waypoint.Add(spawnpoint.transform.position);
        }
    }

    private void SetWaypoint()
    {
        if (new Vector3(transform.position.x, 0, transform.position.z) == waypoint[actialWaypointIndex])
        {
            if (actialWaypointIndex < waypoint.Count - 1)
            {
                actialWaypointIndex++;
            }
            else
            {
                actialWaypointIndex = 0;
            }
        }
        agent.destination = waypoint[actialWaypointIndex];
    }

    private GameObject CheckEnemyInArea()
    {
        foreach (Collider item in Physics.OverlapSphere(transform.position, observeRadius))
        {
            if (item.tag != "Player") continue;
            float shrinkFactor = 0.25f;
            float colliderRadius = item.GetComponent<CapsuleCollider>().radius;
            Vector3 dir = item.transform.position - transform.position;
            Vector3 cross = Vector3.Cross(item.transform.position - transform.position, Vector3.down).normalized;
            Vector3 edgePoint = cross * (colliderRadius - shrinkFactor);
            Vector3 edge1 = dir += edgePoint;
            Vector3 edge2 = dir -= 2 * edgePoint;
            Debug.DrawRay(transform.position, edge1, Color.red);
            Debug.DrawRay(transform.position, edge2, Color.green);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, edge1, out hit))
            {
                if (hit.collider.tag == "Player")
                {
                    return item.gameObject;
                }
            }
            if (Physics.Raycast(transform.position, edge2, out hit))
            {
                if (hit.collider.tag == "Player")
                {
                    return item.gameObject;
                }
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Handles.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, observeRadius);
        for (int i = 0; i < waypoint.Count; i++)
        {
            if (i + 1 >= waypoint.Count)
            {
                Gizmos.DrawLine(new Vector3(waypoint[i].x, transform.position.y, waypoint[i].z), new Vector3(waypoint[0].x, transform.position.y, waypoint[0].z));
                Handles.ArrowHandleCap(0, new Vector3(waypoint[i].x, transform.position.y, waypoint[i].z), Quaternion.LookRotation(waypoint[0] - waypoint[i]), 2f, EventType.Repaint);
            }
            else
            {
                Gizmos.DrawLine(new Vector3(waypoint[i].x, transform.position.y, waypoint[i].z), new Vector3(waypoint[i + 1].x, transform.position.y, waypoint[i + 1].z));
                Handles.ArrowHandleCap(0, new Vector3(waypoint[i].x, transform.position.y, waypoint[i].z), Quaternion.LookRotation(waypoint[i + 1] - waypoint[i]), 2f, EventType.Repaint);
            }
        }
#endif
    }
}
