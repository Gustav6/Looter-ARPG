using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotate : MonoBehaviour
{
    static public float angle;
    static public Vector2 dir;
    void Update()
    {
        dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = rotation;

        // Get the current Z rotation of the GameObject
        float zRotation = transform.eulerAngles.z;

        // Normalize the rotation angle between 0 and 360
        zRotation = NormalizeAngle(zRotation);

        // Check if the rotation is greater than 90 degrees and less than 270 degrees
        if (zRotation > 90f && zRotation < 270f)
        {
            // Flip the GameObject's Y scale
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);

            if (Player.Instance.facingRight && !TransitionSystem.transitionPairs.ContainsKey(Player.Instance.SpriteTransform.gameObject))
            {
                TransitionSystem.AddTransition(new ScaleTransition(Player.Instance.SpriteTransform, 0.1f, new Vector3(1, 1), TransitionType.SmoothStop2), Player.Instance.SpriteTransform.gameObject);
                Player.Instance.facingRight = false;
            }
        }
        else
        {
            // Ensure the Y scale is positive when the rotation is not in the flip range
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);

            if (!Player.Instance.facingRight && !TransitionSystem.transitionPairs.ContainsKey(Player.Instance.SpriteTransform.gameObject))
            {
                TransitionSystem.AddTransition(new ScaleTransition(Player.Instance.SpriteTransform, 0.1f, new Vector3(-1, 1), TransitionType.SmoothStop2), Player.Instance.SpriteTransform.gameObject);
                Player.Instance.facingRight = true;
            }
        }
    }

    // Normalizes an angle to be within 0 to 360 degrees
    float NormalizeAngle(float angle)
    {
        while (angle < 0f)
        {
            angle += 360f;
        }

        while (angle >= 360f)
        {
            angle -= 360f;
        }

        return angle;
    }
}
