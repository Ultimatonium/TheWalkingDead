using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] enemyClip;
    [SerializeField]
    private AudioClip[] playerClip;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Audio Source not set");
            return;
        }
        switch (other.tag)
        {
            case "Player":
                audioSource.PlayOneShot(playerClip[Random.Range(0, playerClip.Length)]);
                break;
            case "Enemy":
                audioSource.PlayOneShot(enemyClip[Random.Range(0, playerClip.Length)]);
                break;
            default:
                break;
        }
    }
}
