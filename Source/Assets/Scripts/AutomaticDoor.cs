using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioClip doorClip;
    private AudioSource audioSource;

    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource not found");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SwitchDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            SwitchDoor();
        }
    }

    private void SwitchDoor()
    {
        audioSource.PlayOneShot(doorClip);
        meshRenderer.enabled = !meshRenderer.enabled;
        boxCollider.enabled = !boxCollider.enabled;
    }
}
