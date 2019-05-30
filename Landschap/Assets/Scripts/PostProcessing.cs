using UnityEngine;

//behaviour which should lie on the same gameobject as the main camera
public class PostProcessing : MonoBehaviour {
	//material that's applied when doing postprocessing
	[SerializeField]
	private Material postprocessMaterial, terrainMaterial;


    private void Start(){
    Camera cam = GetComponent<Camera>();
    cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
    }

    //method which is automatically called by unity after the camera is done rendering
    void OnRenderImage(RenderTexture source, RenderTexture destination){
		//draws the pixels from the source texture to the destination texture
		Graphics.Blit(source, destination, postprocessMaterial);
    //Graphics.Blit(source, destination, terrainMaterial);
	}
}
