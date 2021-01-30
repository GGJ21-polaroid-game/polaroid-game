using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoGhost : MonoBehaviour {

    public Texture2D illustration;

    Transform centerHit = null;

    bool instantCamIntersecting = false;

    void Start() {
        RaycastHit hit;               
        if (Physics.Raycast(transform.position, transform.forward, out hit)) {
            centerHit = hit.transform;
        }
    }

    void Update() {

    }

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "InstantCamera") {
            instantCamIntersecting = true;
        }
    }

    void OnTriggerExit(Collider collider) {
        if (collider.gameObject.tag == "InstantCamera") {
            instantCamIntersecting = false;
        }
    }

    public bool IsIntersectingInstantCam() {
        return instantCamIntersecting;
    }

    public void Taken() {
        instantCamIntersecting = false;
        transform.GetComponent<Collider>().enabled = false;
    }

    public Transform GetCenterHit() {
        return centerHit;
    }
}
