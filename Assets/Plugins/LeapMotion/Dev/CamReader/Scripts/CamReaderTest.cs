using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamReaderTest : MonoBehaviour
{
    public Renderer target;
    private VirtualCamera vcam;

    IEnumerator Start()
    {
        vcam = GetComponent<VirtualCamera>();
        target.material.mainTexture = vcam.CameraTexture;

        while (true)
        {
            yield return StartCoroutine(vcam.Read());
        }
    }
}
