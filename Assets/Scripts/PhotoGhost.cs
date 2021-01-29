using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoGhost : MonoBehaviour {

    Transform centerHit = null;

    void Start() {
        RaycastHit hit;               
        if (Physics.Raycast(transform.position, transform.forward, out hit)) {
            centerHit = hit.transform;
        }
    }

    void Update() {

    }
}
