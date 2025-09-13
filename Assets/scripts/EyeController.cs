using UnityEngine;

public class EyeController : MonoBehaviour
{
    [Header("Target Puzzle Piece")]
    public Transform targetPiece; // The piece this eye should look at

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // Smoothness of rotation

    void Update()
    {
        if (targetPiece != null)
        {
            // Direction from eye to target
            Vector3 direction = targetPiece.position - transform.position;

            if (direction != Vector3.zero)
            {
                // Desired rotation
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly rotate the eye
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    // Called when the chosen puzzle piece changes
    public void SetTarget(Transform newTarget)
    {
        targetPiece = newTarget;
    }
}
