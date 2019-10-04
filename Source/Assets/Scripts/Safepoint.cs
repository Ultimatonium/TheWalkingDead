using UnityEngine;

public class Safepoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameController.Instance.state = GameState.Won;
    }
}
