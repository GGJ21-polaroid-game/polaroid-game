using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowPlanter : MonoBehaviour {

    public Texture2D snowPrint;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public Vector2Int size;

    Texture2D snowMap;

    void Awake() {
        snowMap = new Texture2D(size.x, size.y);

        var fillColorArray = snowMap.GetPixels();
        for (var i = 0; i < fillColorArray.Length; ++i) {
            fillColorArray[i] = Color.white;
        }
        snowMap.SetPixels(fillColorArray);
        snowMap.Apply();
    }
    void Update() {

    }

    public void Plant(Vector2 pos) {
        pos = (pos - minBounds) * (size / (maxBounds - minBounds));

        snowMap.SetPixels((int)(pos.x - snowPrint.width * 0.5f), (int)(pos.y - snowPrint.height * 0.5f), snowPrint.width, snowPrint.height, snowPrint.GetPixels());
        snowMap.Apply();
    }

    public Texture2D GetSnowMap() {
        return snowMap;
    }
}
