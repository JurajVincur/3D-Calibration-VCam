using OpenCvSharp;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class VirtualCamera : MonoBehaviour
{
    public string screenHeightCalibrationName;
    public string screenrWidthCalibrationName;
    public string monitorHeightCalibrationName;
    public string monitorWidthCalibrationName;

    public RenderTexture monitorTexture;
    public RenderTexture screenTexture;

    public bool combine;
    public bool updateMonitorCamera = true;
    public bool autoRead;

    public int FrameWidth
    {
        get;
        private set;
    }

    public int FrameHeight
    {
        get;
        private set;
    }

    public Mat CameraImage
    {
        get;
        private set;
    }

    public Mat ScreenMask
    {
        get;
        private set;
    }

    public Texture2D CameraTexture
    {
        get;
        private set;
    }

    private Mat screenWorkingMat;
    private Mat monitorWorkingMat;
    private Mat cameraWorkingMat;
    private Mat cameraMonitorMat = null;

    private Mat screenMapX;
    private Mat screenMapY;
    private Mat monitorMapX;
    private Mat monitorMapY;

    private Mat monitorMask;

    private void Awake()
    {
        screenWorkingMat = new Mat(screenTexture.height, screenTexture.width, MatType.CV_8UC1);
        monitorWorkingMat = new Mat(monitorTexture.height, monitorTexture.width, MatType.CV_8UC1);

        LoadMaps();

        cameraWorkingMat = new Mat(screenMapX.Height, screenMapX.Width, MatType.CV_8UC1);
        CameraImage = new Mat(cameraWorkingMat.Height, cameraWorkingMat.Width, MatType.CV_8UC1);

        CameraTexture = new Texture2D(cameraWorkingMat.Width, cameraWorkingMat.Height, TextureFormat.R8, false);
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (autoRead)
            {
                yield return StartCoroutine(Read());
            }
            else
            {
                yield return null;
            }
        }
    }

    private void LoadMaps()
    {
        Mat heightCalibration = Cv2.ImRead(Application.dataPath + "/../" + monitorHeightCalibrationName, ImreadModes.Unchanged);
        Mat widthCalibration = Cv2.ImRead(Application.dataPath + "/../" + monitorWidthCalibrationName, ImreadModes.Unchanged);

        monitorMask = new Mat();
        Cv2.Threshold(heightCalibration, monitorMask, 1, 255, ThresholdTypes.BinaryInv);
        monitorMask.ConvertTo(monitorMask, MatType.CV_8UC1);

        monitorMapY = new Mat();
        monitorMapX = new Mat();
        heightCalibration.ConvertTo(monitorMapY, MatType.CV_32FC1, (monitorTexture.height - 1) / 65535f);
        widthCalibration.ConvertTo(monitorMapX, MatType.CV_32FC1, (monitorTexture.width - 1) / 65535f);

        FrameHeight = heightCalibration.Height;
        FrameWidth = heightCalibration.Width;

        heightCalibration.Release();
        widthCalibration.Release();

        heightCalibration = Cv2.ImRead(Application.dataPath + "/../" + screenHeightCalibrationName, ImreadModes.Unchanged);
        widthCalibration = Cv2.ImRead(Application.dataPath + "/../" + screenrWidthCalibrationName, ImreadModes.Unchanged);

        ScreenMask = new Mat();
        Cv2.Threshold(heightCalibration, ScreenMask, 0, 255, ThresholdTypes.Binary);
        ScreenMask.SetTo(0, monitorMask);
        Cv2.Erode(ScreenMask, ScreenMask, Mat.Ones(7, 7, MatType.CV_8UC1));
        ScreenMask.ConvertTo(ScreenMask, MatType.CV_8UC1);

        screenMapY = new Mat();
        screenMapX = new Mat();
        heightCalibration.ConvertTo(screenMapY, MatType.CV_32FC1, (screenTexture.height - 1) / 65535f);
        widthCalibration.ConvertTo(screenMapX, MatType.CV_32FC1, (screenTexture.width - 1) / 65535f * 0.5f);

        Mat screenMapXShift = Mat.Zeros(screenMapX.Height, screenMapX.Width, MatType.CV_32FC1);
        screenMapXShift.ColRange(screenMapX.Width >> 1, screenMapX.Width).SetTo((screenTexture.width - 1) * 0.5f);
        Cv2.Add(screenMapX, screenMapXShift, screenMapX);
        screenMapXShift.Release();

        heightCalibration.Release();
        widthCalibration.Release();
    }

    public Coroutine ReadRoutine()
    {
        return StartCoroutine(Read());
    }

    public IEnumerator Read()
    {
        if (updateMonitorCamera == true || cameraMonitorMat == null)
        {
            yield return new WaitForEndOfFrame();
            AsyncGPUReadbackRequest req = AsyncGPUReadback.Request(monitorTexture, 0, TextureFormat.R8);
            yield return new WaitUntil(() => req.done);

            //transform to opencv mat
            byte[] bytes = req.GetData<byte>().ToArray();
            Marshal.Copy(bytes, 0, monitorWorkingMat.Data, bytes.Length);

            if (cameraMonitorMat == null)
            {
                cameraMonitorMat = new Mat(cameraWorkingMat.Height, cameraWorkingMat.Width, cameraWorkingMat.Type());
            }

            //remap
            Cv2.Remap(monitorWorkingMat, cameraMonitorMat, monitorMapX, monitorMapY);
        }

        if (combine)
        {
            yield return new WaitForEndOfFrame();
            AsyncGPUReadbackRequest req = AsyncGPUReadback.Request(screenTexture, 0, TextureFormat.R8);
            yield return new WaitUntil(() => req.done);

            //transform to opencv mat
            byte[] bytes = req.GetData<byte>().ToArray();
            Marshal.Copy(bytes, 0, screenWorkingMat.Data, bytes.Length);

            //remap
            Cv2.Remap(screenWorkingMat, cameraWorkingMat, screenMapX, screenMapY);

            cameraMonitorMat.CopyTo(CameraImage); //just in time to make sure image is correct / combined
            Cv2.Add(CameraImage, cameraWorkingMat, CameraImage, ScreenMask);
        }
        else
        {
            cameraMonitorMat.CopyTo(CameraImage);
        }

        CameraTexture.LoadRawTextureData(CameraImage.Data, FrameWidth * FrameHeight);
        CameraTexture.Apply(false);
    }

    private void OnDestroy()
    {
        screenWorkingMat.Release();
        monitorWorkingMat.Release();
        cameraWorkingMat.Release();
        if (cameraMonitorMat != null)
        {
            cameraMonitorMat.Release();
        }

        monitorMapX.Release();
        monitorMapY.Release();
        monitorMask.Release();
        screenMapX.Release();
        screenMapY.Release();
        ScreenMask.Release();
        CameraImage.Release();
    }
}
