using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour {

    public Transform player;

    public float[] startDistances;
    public float[] fadeDistances;
    public float[] fullADistances;
    public float[] fullBDistances;
    public string[] texts;

    TextMeshPro tmp;

    Vector3 playerPrevPosition;
    float distance = 0f;

    void Start() {
        playerPrevPosition = player.position;

        tmp = transform.GetChild(0).GetComponent<TextMeshPro>();
    }
    void Update() {
        distance += (player.position - playerPrevPosition).magnitude;
        playerPrevPosition = player.position;

        for (int i = 0; i < startDistances.Length; ++i) {
            if (distance > startDistances[i] && distance < fadeDistances[i]) {
                tmp.text = texts[i];

                if (distance < fullADistances[i])
                    tmp.color = Color.black * Mathf.Lerp(0, 1, (distance - startDistances[i]) / (fullADistances[i] - startDistances[i]));
                else if (distance > fullBDistances[i])
                    tmp.color = Color.black * Mathf.Lerp(1, 0, (distance - fullBDistances[i]) / (fadeDistances[i] - fullBDistances[i]));

                break;
            }
        }
    }
}
