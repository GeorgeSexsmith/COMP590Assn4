using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Assignment 1 keeps head-directed aiming. Either Quest index trigger replaces
// the phone's touch press; controller orientation deliberately does not aim.
public class ButtonClick : MonoBehaviour
{
    public BallPreFab ballPreFab;
    private readonly Queue<BallPreFab> shots = new Queue<BallPreFab>();
    private const int MaxShots = 64;

    private void Update()
    {
        bool fire = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)
                 || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
#if UNITY_EDITOR
        fire |= Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        fire |= Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
        if (fire) Fire();
    }

    public void Fire()
    {
        if (ballPreFab == null) return;
        // This component is on the tracked center-eye anchor, matching the
        // original camera's spawn point, orientation, force and gravity.
        BallPreFab ball = Instantiate(ballPreFab, transform.position, transform.rotation);
        Rigidbody body = ball.GetComponent<Rigidbody>();
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.AddForce(transform.forward * Random.Range(3200, 3700));
        shots.Enqueue(ball);
        // Bound the cost of a long session on the headset.
        while (shots.Count > MaxShots)
        {
            BallPreFab oldest = shots.Dequeue();
            if (oldest != null) Destroy(oldest.gameObject);
        }
    }
}
