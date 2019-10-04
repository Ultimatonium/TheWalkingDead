using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public PlayerState state;
    private Vector3 translateVector;
    private enum WalkingState { Standing, Walking, Sprinting }
    private WalkingState walkingState;

    [Header("Audio")]
    [SerializeField]
    private AudioClip walkingClip;
    [SerializeField]
    private AudioClip sprintingClip;
    private AudioSource audioSource;

    private void Start()
    {
        Cursor.visible = false;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            Debug.LogWarning("AudioSource not found");
        }
    }

    private void Update()
    {
        if (state == PlayerState.Dead)
        {
            walkingState = WalkingState.Standing;
            audioSource.Stop();
            return;
        }
        Move();
        Rotate();
        CheckInspect();
    }

    private void CheckInspect()
    {
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, 2))
            {
                if (hit.collider.name == "ManuelDoor")
                {
                    hit.collider.GetComponent<ManuelDoor>().OpenDoor();
                }
            }
        }
    }

    private void Rotate()
    {
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X")));
    }

    private void Move()
    {
        walkingState = WalkingState.Standing;
        translateVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            translateVector += Vector3.forward;
            walkingState = WalkingState.Walking;
        }
        if (Input.GetKey(KeyCode.S))
        {
            translateVector += Vector3.back;
            walkingState = WalkingState.Walking;
        }
        if (Input.GetKey(KeyCode.A))
        {
            translateVector += Vector3.left;
            walkingState = WalkingState.Walking;
        }
        if (Input.GetKey(KeyCode.D))
        {
            translateVector += Vector3.right;
            walkingState = WalkingState.Walking;
        }
        translateVector.Normalize();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Y)) // Y to fix Unitybug
        {
            translateVector *= 3;
            walkingState = WalkingState.Sprinting;
        }
        translateVector *= Time.deltaTime;
        transform.Translate(translateVector, Space.Self);
        PlayWalking();
    }


    private void PlayWalking()
    {
        switch (walkingState)
        {
            case WalkingState.Standing:
                audioSource.Stop();
                audioSource.clip = null;
                return;
            case WalkingState.Walking:
                if (audioSource.clip == walkingClip) return;
                audioSource.clip = walkingClip;
                break;
            case WalkingState.Sprinting:
                if (audioSource.clip == sprintingClip) return;
                audioSource.clip = sprintingClip;
                break;
            default:
                break;
        }
        audioSource.Play();
    }
}
