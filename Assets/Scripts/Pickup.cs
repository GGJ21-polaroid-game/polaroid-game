using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    void Start() {

    }

    void Update() {

    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            PlayerController pc = other.gameObject.GetComponent<PlayerController>();
            Transform hand = pc.GetHand();
            Inventory inventory = hand.gameObject.GetComponent<Inventory>();
            
            while (transform.childCount > 0) {
                Transform child = transform.GetChild(0);

                SetLayerToPlayer(child);

                inventory.AddItem(child);
            }

            Destroy(gameObject);
        }
    }

    void SetLayerToPlayer(Transform t) {
        t.gameObject.layer = LayerMask.NameToLayer("Player");
        for (int i = 0; i < t.childCount; ++i) {
            Transform child = t.GetChild(i);
            SetLayerToPlayer(child);
        }
    }
}
