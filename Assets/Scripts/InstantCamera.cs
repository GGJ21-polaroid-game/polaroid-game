using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InstantCamera : MonoBehaviour, IActionable {

    public GameObject instantPhotoPrefab;
    public GameObject badPhotoPrefab;
    public GameObject origPhotoPrefab;

    public Inventory inventory;

    public PhotoGhost[] photoGhosts;
    public PhotoGhost finalPG;

    public float finalPGTime = 5f;
    float finalPGTimestamp = 0;
    int takenC = 0;

    GameObject[] originals;
    PhotoGhost validPG;
    int validPGIdx;

    Vector3 camOrigPosition;
    Quaternion camOrigRotation;
    float startTimestamp = 0;

    Animator anim;
    Animator shutterAnim;
    Animator trackAnim;

    AudioSource shutterSFX;

    Camera cam;

    Transform onTrack = null;

    bool isAiming = false;
    bool isSnapping = false;
    bool isBadPhoto = true;
    bool isSnappingGhost = false;

    int photoGhostIdx = 0;

    void Start() {
        anim = GetComponent<Animator>();
        shutterAnim = transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        trackAnim = transform.GetChild(3).GetChild(0).GetComponent<Animator>();
        shutterSFX = GetComponent<AudioSource>();
        cam = transform.GetChild(2).GetComponent<Camera>();
        Camera.onPostRender += OnPostRenderCallback;

        camOrigPosition = cam.transform.position;
        camOrigRotation = cam.transform.rotation;

        originals = new GameObject[photoGhosts.Length];

        startTimestamp = Time.time;
    }

    void Update() {
        anim.SetBool("isAiming", isAiming);

        if (isSnappingGhost)
            isSnapping = true;

        if (startTimestamp + 1 < Time.time && !isSnapping && photoGhostIdx < photoGhosts.Length) {
            PhotoGhost ghost = photoGhosts[photoGhostIdx];

            cam.transform.position = ghost.transform.position;
            cam.transform.rotation = ghost.transform.rotation;

            isSnappingGhost = true;

            ++photoGhostIdx;
        }

        if (finalPGTimestamp > 0.1 && Time.time - finalPGTimestamp > finalPGTime) {
            isBadPhoto = false;

            validPG = finalPG;

            shutterAnim.SetTrigger("snap");
            shutterSFX.Play();
            isSnapping = true;

            finalPGTimestamp = 0;

            inventory.SwitchToCamera();
        }
    }

    public void PrimaryActionStart() {
        if (isAiming && onTrack == null && finalPGTimestamp < 0.1) {
            isBadPhoto = true;

            for (int i = 0; i < photoGhosts.Length; ++i) {
                PhotoGhost ghost = photoGhosts[i];
                if (ghost.IsIntersectingInstantCam()) {
                    Vector3 rotDiff = ghost.transform.rotation.eulerAngles - transform.rotation.eulerAngles + Vector3.right * 270;
                    float rotDiffMag = Mathf.Abs(rotDiff.x) + Mathf.Abs(rotDiff.y) + Mathf.Abs(rotDiff.z);
                    if (rotDiffMag > 180f)
                        rotDiffMag = Mathf.Abs(rotDiffMag - 360);

                    if (rotDiffMag < 10) {

                        Transform centerHit = null;
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, -transform.up, out hit, 1000f, ~LayerMask.GetMask("Player"))) {
                            centerHit = hit.transform;
                        }

                        if (true || centerHit == ghost.GetCenterHit()) {
                            isBadPhoto = false;
                            validPG = ghost;
                            validPGIdx = i;

                            takenC++;
                            if (takenC == photoGhosts.Length) {
                                finalPGTimestamp = Time.time;
                            }

                            break;
                        }
                    }
                }
            }

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

            if (isSnappingGhost) {
                isSnappingGhost = false;

                GameObject origPhotoGO = Instantiate(origPhotoPrefab);
                OriginalPhoto origPhoto = origPhotoGO.GetComponent<OriginalPhoto>();

                origPhoto.SetPhoto(photo);

                origPhotoGO.layer = LayerMask.NameToLayer("Player");
                origPhotoGO.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Player");
                origPhotoGO.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Player");

                originals[photoGhostIdx-1] = origPhotoGO;

                cam.transform.position = camOrigPosition;
                cam.transform.rotation = camOrigRotation;

                origPhotoGO.transform.localScale = Vector3.zero;
                origPhotoGO.transform.parent = transform.parent;

                //var bytes = photo.EncodeToPNG();
                //File.WriteAllBytes(Application.dataPath + "/Photos/" + photoGhostIdx + ".png", bytes);

            } else {
                GameObject instantPhotoGO;
                if (isBadPhoto) {
                    instantPhotoGO = Instantiate(badPhotoPrefab);
                    instantPhotoGO.GetComponent<Rigidbody>().isKinematic = true;
                } else {
                    instantPhotoGO = Instantiate(instantPhotoPrefab);
                    photo = validPG.illustration;
                }

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
            }

            isSnapping = false;
        }
    }

    public void MoveEnd() {
        if (isBadPhoto) {
            onTrack.parent = null;
            onTrack.GetComponent<Rigidbody>().isKinematic = false;
            onTrack.Rotate(Vector3.up, 180);
        } else {
            onTrack.gameObject.layer = LayerMask.NameToLayer("Player");
            onTrack.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Player");
            onTrack.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Player");

            onTrack.localPosition = Vector3.zero;
            onTrack.localRotation = Quaternion.identity;
            onTrack.localScale = Vector3.zero;

            if (validPG != finalPG)
                inventory.RemoveItem(originals[validPGIdx].transform);
            inventory.AddItem(onTrack);
        }

        onTrack = null;
    }
}
