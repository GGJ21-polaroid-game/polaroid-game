using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour {

    public Transform player;
    public float titleDistance = 10f;

    TextMeshPro title;

    Vector3 playerPrevPosition;
    float distance = 0f;

    void Start() {
        title = transform.GetChild(0).GetComponent<TextMeshPro>();
        playerPrevPosition = player.position;
    }
    void Update() {
        distance += (player.position - playerPrevPosition).magnitude;
        playerPrevPosition = player.position;

        title.color = Color.black * Mathf.Lerp(1, 0, distance / titleDistance);
    }
}
