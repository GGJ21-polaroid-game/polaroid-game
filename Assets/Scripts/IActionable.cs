using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionable {
    void PrimaryActionStart();
    void PrimaryActionEnd();
    void SecondaryActionStart();
    void SecondaryActionEnd();
}
