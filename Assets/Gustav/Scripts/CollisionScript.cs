using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    [SerializeField] private Type type;
    [SerializeField] private Component script;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case Type.trap:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log("Enter Collision");

                    // Play animation

                    ((Trap)script).animator.SetBool("Active", true);
                }
                break;
            case Type.breakable:
                if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
                {
                    Debug.Log("Enter Collision");

                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (type)
        {
            case Type.trap:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log("Exit Collision");

                    // Play animation

                    ((Trap)script).animator.SetBool("Active", false);
                }
                break;
            case Type.breakable:

                Debug.Log("Exit Collision");

                break;
            default:
                break;
        }
    }

    private enum Type
    {
        trap,
        breakable
    }
}
