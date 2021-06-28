using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class CamReader : MonoBehaviour
{
    public RenderTexture topCamTex;
    public RenderTexture bottomCamTex;
    public Renderer target;

    private Texture2D viewTexture;

    private void Awake()
    {
        viewTexture = new Texture2D(topCamTex.width, topCamTex.height, TextureFormat.RGBA32, false);
        target.material.mainTexture = viewTexture;
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            var req = AsyncGPUReadback.Request(topCamTex, 0, TextureFormat.RGBA32);
            yield return new WaitUntil(() => req.done);
            viewTexture.LoadRawTextureData(req.GetData<byte>());
            viewTexture.Apply(false);

            req = AsyncGPUReadback.Request(bottomCamTex, 0, TextureFormat.RGBA32);
            yield return new WaitUntil(() => req.done);
            viewTexture.LoadRawTextureData(req.GetData<byte>());
            viewTexture.Apply(false);
        }
    }
}
