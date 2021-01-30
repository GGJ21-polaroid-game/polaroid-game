using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplySnowMap : MonoBehaviour {
    public SnowPlanter snowPlanter;

    void Start() {
        transform.GetComponent<MeshRenderer>().material.mainTexture = snowPlanter.GetSnowMap();
    }
    void Update() {
        transform.GetComponent<MeshRenderer>().material.mainTexture = snowPlanter.GetSnowMap();
    }
}