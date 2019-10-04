using UnityEngine;

public class ManuelDoor : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioClip doorClip;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource not found");
        }
    }

    public void OpenDoor()
    {
        audioSource.PlayOneShot(doorClip);
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
    }
}
