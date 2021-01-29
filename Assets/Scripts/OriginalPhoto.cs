using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginalPhoto : MonoBehaviour, IActionable {

    public void PrimaryActionStart() { }

    public void PrimaryActionEnd() { }

    public void SecondaryActionStart() { }

    public void SecondaryActionEnd() { }

    public void SetPhoto(Texture tex) {
        transform.GetChild(1).GetComponent<MeshRenderer>().material.mainTexture = tex;
    }
}
