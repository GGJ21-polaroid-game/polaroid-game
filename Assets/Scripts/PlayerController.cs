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

    public float standHeight = 2f;
    public float crouchHeight = 1f;

    public float footSFXMinPeriod = 1f;
    public AudioSource leftFootSFX;
    public AudioSource rightFootSFX;
    public AudioClip[] footSFX;

    public SnowPlanter snowPlanter;

    float footSFXTimer = 0;
    bool lastFootLeft = false;
    int lastLeftFootSFX = 0;
    int lastRightFootSFX = 0;
    float footSFXVolume = 0;

    bool prevGrounded = false;
    bool grounded = false;
    bool laddering = false;
    bool crouching = false;
    float tilt = 0f;
    float handRoll = 0f;
    float verticalVelocity = 0f;
    Transform tiltTransform;
    Transform handTransform;
    Transform handTargetTransform;
    Rigidbody rb;
    Animator handAnim;
    IActionable handActionable;

    CapsuleCollider capsule;

    public Transform GetHand() {
        return handTransform;
    }

    void Start() {
        tiltTransform = transform.GetChild(0);
        handTransform = tiltTransform.GetChild(1);
        rb = gameObject.GetComponent<Rigidbody>();
        handTargetTransform = tiltTransform.GetChild(0);
        handAnim = handTargetTransform.GetComponent<Animator>();
        handTargetTransform = handTargetTransform.GetChild(0);
        handActionable = handTransform.GetComponent<IActionable>();
        capsule = transform.GetComponent<CapsuleCollider>();
    }

    void Update() {
        if (grounded && Input.GetButtonDown("Jump")) {
            verticalVelocity += jumpSpeed;
        }

        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;


        Vector2 lookStep = Vector2.zero;
        if (Input.GetButton("RotateHand")) {
            handRoll += Input.GetAxis("Mouse X") * mouseSensitivity * 0.5f * Time.fixedDeltaTime;
        } else {
            lookStep.x = Input.GetAxis("Mouse X");
            handRoll = Mathf.Lerp(handRoll, 0, 0.5f);
        }

        lookStep.y = Input.GetAxis("Mouse Y");
        lookStep *= mouseSensitivity * Time.fixedDeltaTime;

        tilt -= lookStep.y;
        tilt = Mathf.Clamp(tilt, -90f, 90f);
        tiltTransform.localRotation = Quaternion.Euler(tilt, 0f, 0f);

        transform.Rotate(Vector3.up * lookStep.x);

        handTargetTransform.localRotation = Quaternion.Euler(0, handRoll, 0);

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
        laddering = false;

        RaycastHit hit;
        
        Vector3 start = transform.position + Vector3.up * 2f * Time.fixedDeltaTime;
        Vector3 dir = transform.forward;
        if (Physics.Raycast(start, dir, out hit, 0.3f, LayerMask.GetMask("Ladder"))) {
            laddering = true;
            grounded = true;            
            verticalVelocity = 0;
            if (Input.GetAxis("Vertical") > 0)
                rb.MovePosition(hit.point - transform.forward * capsule.radius);
        } else {
            start = transform.position + Vector3.up * 0.1f;
            dir = Vector3.down;

            prevGrounded = grounded;
            if (verticalVelocity <= 0 && Physics.Raycast(start, dir, out hit, 0.2f, ~LayerMask.GetMask("Player"))) {
                grounded = true;
                rb.MovePosition(hit.point);
                verticalVelocity = 0;
            } else {
                grounded = false;
            }
        }

        Vector3 step;
        step = transform.right * Input.GetAxis("Horizontal");
        step += transform.forward * Input.GetAxis("Vertical");

        if (Input.GetButton("Fire3"))
            step = Vector3.ClampMagnitude(step, 0.5f);
        else
            step = Vector3.ClampMagnitude(step, 1);

        if (!laddering) {
            verticalVelocity += gravity * Time.fixedDeltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity, -terminalSpeed, terminalSpeed);
        }

        rb.velocity = (Vector3.up * verticalVelocity) + (speed * step);

        if (grounded)
            footSFXTimer += Time.deltaTime * step.magnitude;

        footSFXVolume = step.magnitude;
        if (prevGrounded != grounded) {
            footSFXTimer = footSFXMinPeriod + 0.1f;
            footSFXVolume = 1f;
        }


        if (footSFXTimer > footSFXMinPeriod) {
            int clip = Random.Range(0, footSFX.Length - 1);
            if (lastFootLeft) {
                if (lastRightFootSFX == clip)
                    clip = Random.Range(0, footSFX.Length - 1);
                rightFootSFX.clip = footSFX[clip];
                rightFootSFX.volume = footSFXVolume * 0.25f;
                rightFootSFX.Play();
            } else {
                if (lastLeftFootSFX == clip)
                    clip = Random.Range(0, footSFX.Length - 1);
                leftFootSFX.clip = footSFX[clip];
                leftFootSFX.volume = footSFXVolume * 0.25f;
                leftFootSFX.Play();
            }
            lastFootLeft = !lastFootLeft;
            footSFXTimer -= footSFXMinPeriod;

            Vector2 pos = new Vector2(transform.position.x, transform.position.z);
            //snowPlanter.Plant(pos);
        }

        float height = standHeight;

        crouching = Input.GetButton("Crouch");        
        if (Physics.Raycast(tiltTransform.position, Vector3.up, out hit, standHeight - crouchHeight + 0.3f, ~LayerMask.GetMask("Player"))) {
            crouching = true;
        }

        if (crouching)
            height = crouchHeight;
        
        capsule.height = height;
        capsule.center = Vector3.up * height * 0.5f;
        tiltTransform.localPosition = Vector3.up * Mathf.Lerp(tiltTransform.localPosition.y, height - 0.25f, 0.25f);
    }
}
