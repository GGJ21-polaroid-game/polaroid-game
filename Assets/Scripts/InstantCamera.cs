using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InstantCamera : MonoBehaviour, IActionable {

    public GameObject instantPhotoPrefab;
    public GameObject badPhotoPrefab;

    Animator anim;
    Animator shutterAnim;
    Animator trackAnim;

    AudioSource shutterSFX;

    Camera cam;

    Transform onTrack = null;

    bool isAiming = false;
    bool isSnapping = false;
    bool isBadPhoto = true;

    void Start() {
        anim = GetComponent<Animator>();
        shutterAnim = transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        trackAnim = transform.GetChild(3).GetChild(0).GetComponent<Animator>();
        shutterSFX = GetComponent<AudioSource>();
        cam = transform.GetChild(2).GetComponent<Camera>();
        Camera.onPostRender += OnPostRenderCallback;
    }

    void Update() {
        anim.SetBool("isAiming", isAiming);
    }

    public void PrimaryActionStart() {
        if (isAiming && onTrack == null) {
            shutterAnim.SetTrigger("snap");
            shutterSFX.Play();
            isSnapping = true;
        }
    }

    public void PrimaryActionEnd() {

    }

    public void SecondaryActionStart() {
        isAiming = true;
    }

    public void SecondaryActionEnd() {
        isAiming = false;
    }

    void OnPostRenderCallback(Camera camera) {
        if (isSnapping && camera == cam) {
            RenderTexture.active = camera.targetTexture;
            Texture2D photo = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);

            photo.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            photo.SetPixel(0, 0, Color.white);
            photo.Apply();

            GameObject instantPhotoGO;
            if (isBadPhoto) {
                instantPhotoGO = Instantiate(badPhotoPrefab);
                instantPhotoGO.GetComponent<Rigidbody>().isKinematic = true;
            } else
                instantPhotoGO = Instantiate(instantPhotoPrefab);

            InstantPhoto instantPhoto = instantPhotoGO.GetComponent<InstantPhoto>();
            instantPhoto.SetPhoto(photo);
            instantPhoto.transform.parent = trackAnim.transform;
            instantPhoto.transform.localPosition = Vector3.zero;
            instantPhoto.transform.localRotation = Quaternion.identity;
            instantPhotoGO.layer = LayerMask.NameToLayer("BadPhoto");
            instantPhoto.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("BadPhoto");
            instantPhoto.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("BadPhoto");

            trackAnim.SetTrigger("move");
            onTrack = instantPhoto.transform;

            isSnapping = false;
        }
    }

    public void MoveEnd() {
        if (isBadPhoto) {
            onTrack.parent = null;
            onTrack.GetComponent<Rigidbody>().isKinematic = false;
            onTrack.Rotate(Vector3.up, 180);
        } else {
            onTrack.parent = transform.parent;
            onTrack.gameObject.layer = LayerMask.NameToLayer("Player");
            onTrack.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Player");
            onTrack.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Player");

            onTrack.localPosition = Vector3.zero;
            onTrack.localRotation = Quaternion.identity;
            onTrack.localScale = Vector3.zero;
        }

        onTrack = null;
    }
}
