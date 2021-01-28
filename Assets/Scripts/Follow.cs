using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {

    public Transform target = null;
    public float t = 0.5f;

    Vector3 position;
    Quaternion rotation;

    void Start() {
        position = transform.position;
    }

    void FixedUpdate() {
        if (target) {
            position = Vector3.Lerp(position, target.position, t);
            rotation = Quaternion.Lerp(rotation, target.rotation, t);            

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
