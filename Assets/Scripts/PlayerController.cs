using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public float mouseSensitivity = 100f;
    public float speed = 4f;
    public float gravity = -10f;
    public float terminalSpeed = 100f;
    public float jumpSpeed = 50f;

    public float footSFXMinPeriod = 1f;
    public AudioSource leftFootSFX;
    public AudioSource rightFootSFX;
    public AudioClip[] footSFX;

    float footSFXTimer = 0;
    bool lastFootLeft = false;
    int lastLeftFootSFX = 0;
    int lastRightFootSFX = 0;

    bool grounded = false;
    float tilt = 0f;
    float verticalVelocity = 0f;
    Transform tiltTransform;
    Transform handTransform;
    Rigidbody rb;
    Animator handAnim;
    IActionable handActionable;

    void Start() {
        tiltTransform = transform.GetChild(0);
        handTransform = tiltTransform.GetChild(1);
        rb = gameObject.GetComponent<Rigidbody>();
        handAnim = tiltTransform.GetChild(0).GetComponent<Animator>();
        handActionable = handTransform.GetComponent<IActionable>();
    }

    void Update() {
        if (grounded && Input.GetButtonDown("Jump")) {
            verticalVelocity += jumpSpeed;
        }

        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;

        Vector2 lookStep;
        lookStep.x = Input.GetAxis("Mouse X");
        lookStep.y = Input.GetAxis("Mouse Y");
        lookStep *= mouseSensitivity * Time.fixedDeltaTime;

        tilt -= lookStep.y;
        tilt = Mathf.Clamp(tilt, -90f, 90f);
        tiltTransform.localRotation = Quaternion.Euler(tilt, 0f, 0f);

        transform.Rotate(Vector3.up * lookStep.x);

        handAnim.SetBool("raised", Input.GetButton("Fire2"));

        if (Input.GetButtonDown("Fire1"))
            handActionable.PrimaryActionStart();
        if (Input.GetButtonUp("Fire1"))
            handActionable.PrimaryActionEnd();

        if (Input.GetButtonDown("Fire2"))
            handActionable.SecondaryActionStart();
        if (Input.GetButtonUp("Fire2"))
            handActionable.SecondaryActionEnd();
    }

    void FixedUpdate() {
        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 0.1f;
        Vector3 dir = Vector3.down;
        if (verticalVelocity <= 0 && Physics.Raycast(start, dir, out hit, 0.2f, ~LayerMask.GetMask("Player"))) {
            grounded = true;
            rb.MovePosition(hit.point);
            verticalVelocity = 0;
        } else {
            grounded = false;
        }

        Vector3 step;
        step = transform.right * Input.GetAxis("Horizontal");
        step += transform.forward * Input.GetAxis("Vertical");

        if (Input.GetButton("Fire3"))
            step = Vector3.ClampMagnitude(step, 0.5f);
        else
            step = Vector3.ClampMagnitude(step, 1);

        verticalVelocity += gravity * Time.fixedDeltaTime;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -terminalSpeed, terminalSpeed);

        rb.velocity = (Vector3.up * verticalVelocity) + (speed * step);

        footSFXTimer += Time.deltaTime * step.magnitude;
        if (footSFXTimer > footSFXMinPeriod) {
            int clip = 0;
            clip = Random.Range(0, footSFX.Length - 1);
            if (lastFootLeft) {
                if (lastRightFootSFX == clip)
                    clip = Random.Range(0, footSFX.Length - 1);
                rightFootSFX.clip = footSFX[clip];
                rightFootSFX.volume = step.magnitude * 0.25f;
                rightFootSFX.Play();
            } else {
                if (lastLeftFootSFX == clip)
                    clip = Random.Range(0, footSFX.Length - 1);
                leftFootSFX.clip = footSFX[clip];
                leftFootSFX.volume = step.magnitude * 0.25f;
                leftFootSFX.Play();
            }
            lastFootLeft = !lastFootLeft;
            footSFXTimer -= footSFXMinPeriod;
        }
    }
}
