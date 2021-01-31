using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, IActionable {

    public float scrollSensitivity = 10f;

    float scroll;

    bool isPrimaryActioning = false;
    bool isSecondaryActioning = false;
    bool isSwitching = false;

    int activeIndex;
    Transform prevActive;
    Transform active;
    IActionable activeActionable;

    Animator handAnim;

    void Start() {
        scroll = 0;

        activeIndex = 0;

        prevActive = null;
        active = null;
        if (transform.childCount > 0) {
            active = transform.GetChild(0);
            activeActionable = active.GetComponent<IActionable>();
        }

        for (int i = 1; i < transform.childCount; ++i) {
            transform.GetChild(i).localScale = Vector3.zero;
        }

        handAnim = GetComponent<Animator>();
    }

    void Update() {
        if (transform.childCount > 0) {
            if (!isPrimaryActioning && !isSecondaryActioning && !isSwitching) {
                int pai = activeIndex;

                scroll += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;

                int step = (int)Mathf.Floor(scroll);
                scroll = scroll - step;

                activeIndex = (int)Mathf.Repeat(activeIndex + step, transform.childCount);

                if (pai != activeIndex) {
                    prevActive = active;
                    active = transform.GetChild(activeIndex);
                    activeActionable = active.GetComponent<IActionable>();
                    handAnim.SetTrigger("switch");

                    isSwitching = true;
                }
            }
        }
    }

    public void AddItem(Transform item) {

        item.parent = transform;
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
        item.localScale = Vector3.zero;

        prevActive = active;
        active = item;
        activeIndex = transform.childCount-1;
        activeActionable = active.GetComponent<IActionable>();
        handAnim.SetTrigger("switch");

        isSwitching = true;
    }

    public void RemoveItem(Transform item) {
        for (int i = 0; i < transform.childCount; ++i) {
            Transform child = transform.GetChild(i);
            if (child == item) {
                child.parent = null;
                Destroy(child.gameObject);
            }
        }
    }

    public Transform GetPlayer() {
        return transform.parent.parent;
    }

    public void PrimaryActionStart() {
        isPrimaryActioning = true;
        if (active)
            activeActionable.PrimaryActionStart();
    }

    public void PrimaryActionEnd() {
        isPrimaryActioning = false;
        if (active)
            activeActionable.PrimaryActionEnd();
    }

    public void SecondaryActionStart() {
        isSecondaryActioning = true;
        if (active)
            activeActionable.SecondaryActionStart();
    }

    public void SecondaryActionEnd() {
        isSecondaryActioning = false;
        if (active)
            activeActionable.SecondaryActionEnd();
    }

    public void Switch() {
        if (prevActive)
            prevActive.localScale = Vector3.zero;
        active.localScale = Vector3.one;
    }

    public void FinishSwitch() {
        isSwitching = false;
    }

    public void SwitchToCamera() {
        activeIndex = 0;
        prevActive = active;
        active = transform.GetChild(activeIndex);
        activeActionable = active.GetComponent<IActionable>();
        handAnim.SetTrigger("switch");

        isSwitching = true;
    }
}
