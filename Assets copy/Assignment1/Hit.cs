using UnityEngine;
using TMPro;

public class Hit : MonoBehaviour
{
    public int score = 0;
    public TMP_Text ScoreBoard;

    private void Start()
    {
        UpdateScoreboard();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Resolve compound arrow colliders through their root Rigidbody.
        Arrow arrow = collision.rigidbody != null
            ? collision.rigidbody.GetComponent<Arrow>()
            : collision.gameObject.GetComponentInParent<Arrow>();
        if (arrow == null || arrow.has_scored) return;
        score += 10;
        arrow.has_scored = true;
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        if (ScoreBoard != null) ScoreBoard.text = "Score: " + score;
    }
}
