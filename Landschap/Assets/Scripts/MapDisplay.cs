using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textRender;

    public void DrawTexture(Texture2D texture)
    {
       
        textRender.sharedMaterial.mainTexture = texture;
        textRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
