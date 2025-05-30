using UnityEngine;

public class PlayerEffects3D : MonoBehaviour
{
    [Header("Trail Effect")]
    public TrailRenderer dashTrail;

    [Header("Particles")]
    public ParticleSystem jumpFX;
    public ParticleSystem landFX;

    [Header("Footsteps")]
    public AudioSource audioSource;
    public AudioClip footstepClip;
    public float stepRate = 0.5f;
    private float lastStepTime;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;
    private bool wasGrounded;
    private bool isGrounded;

    [Header("Camera Shake (Optional)")]
    public Camera mainCam;
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    private CharacterController controller;
    private Vector3 lastPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        CheckGrounded();
        HandleFootsteps();
    }

    public void PlayDashEffect()
    {
        if (dashTrail != null)
        {
            dashTrail.Clear();
            dashTrail.emitting = true;
            Invoke(nameof(StopDashTrail), 0.2f);
        }

        ShakeCamera();
    }

    void StopDashTrail()
    {
        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    public void PlayJumpEffect()
    {
        if (jumpFX != null)
            Instantiate(jumpFX, groundCheck.position, Quaternion.identity);
    }

    public void PlayLandEffect()
    {
        if (landFX != null)
            Instantiate(landFX, groundCheck.position, Quaternion.identity);

        ShakeCamera();
    }

    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (!wasGrounded && isGrounded)
            PlayLandEffect();
    }

    void HandleFootsteps()
    {
        Vector3 horizontalMove = transform.position - lastPosition;
        horizontalMove.y = 0;

        if (isGrounded && horizontalMove.magnitude > 0.01f)
        {
            if (Time.time > lastStepTime + stepRate)
            {
                audioSource.PlayOneShot(footstepClip);
                lastStepTime = Time.time;
            }
        }

        lastPosition = transform.position;
    }

    void ShakeCamera()
    {
        if (mainCam != null)
            StartCoroutine(DoCameraShake());
    }

    System.Collections.IEnumerator DoCameraShake()
    {
        Vector3 originalPos = mainCam.transform.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = originalPos;
    }
}
