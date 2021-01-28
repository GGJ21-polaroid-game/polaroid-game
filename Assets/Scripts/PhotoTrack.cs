using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoTrack : MonoBehaviour {
    public void MoveEnd() {
        transform.parent.parent.GetComponent<InstantCamera>().MoveEnd();
    }
}
