using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    [SerializeField] private Type type;
    [SerializeField] private Component script;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter Collision");

        switch (type)
        {
            case Type.trap:
                if (collision.CompareTag("Player"))
                {
                    // Play animation

                    ((Trap)script).animator.SetBool("Active", true);
                }
                break;
            case Type.breakable:
                if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
                {
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exit Collision");

        switch (type)
        {
            case Type.trap:
                if (collision.CompareTag("Player"))
                {
                    // Play animation

                    ((Trap)script).animator.SetBool("Active", false);
                }
                break;
            case Type.breakable:
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
