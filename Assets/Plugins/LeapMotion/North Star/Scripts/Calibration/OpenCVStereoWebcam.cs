using System.Runtime.InteropServices;
using UnityEngine;
using OpenCvSharp;

namespace Leap.Unity.AR.Testing
{
    public class OpenCVStereoWebcam : MonoBehaviour
    {
        public VirtualCamera virtualCamera;

        public int deviceNumber = 0;
        public Renderer leftDisplay, rightDisplay;
        public bool updateScreenAutomatically = true;

        [System.NonSerialized]
        public byte[] leftData, rightData;
        [System.NonSerialized]
        public Mat leftImage, rightImage;

        public VideoCapture cap;
        Mat webcamImage, grayImage, leftImageSlice, rightImageSlice;
        Texture2D leftTexture, rightTexture;

        void Start()
        {
            cap = new OpenCvSharp.VideoCapture(deviceNumber);
            //cap.Fps = 30;
            cap.FrameWidth = 1280;
            cap.FrameHeight = 480;
            cap.AutoExposure = 0;
            cap.Exposure = -4;

            webcamImage = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth, MatType.CV_8UC3);
            grayImage = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth, MatType.CV_8UC1);
            leftImageSlice = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth / 2, MatType.CV_8UC1);
            rightImageSlice = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth / 2, MatType.CV_8UC1);
            leftImage = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth / 2, MatType.CV_8UC1);
            rightImage = new Mat(virtualCamera.FrameHeight, virtualCamera.FrameWidth / 2, MatType.CV_8UC1);
            leftData = new byte[virtualCamera.FrameHeight * virtualCamera.FrameWidth / 2];
            rightData = new byte[virtualCamera.FrameHeight * virtualCamera.FrameWidth / 2];
        }

        void Update()
        {
            if (virtualCamera != null && virtualCamera.CameraImage != null)
            {
                virtualCamera.CameraImage.CopyTo(grayImage);

                // Convert back to color
                Cv2.CvtColor(grayImage, webcamImage, ColorConversionCodes.GRAY2BGR);

                // Get the Left Image Data
                leftImageSlice = new Mat(grayImage, new OpenCvSharp.Rect(0, 0, virtualCamera.FrameWidth / 2, virtualCamera.FrameHeight));
                leftImageSlice.CopyTo(leftImage);
                Marshal.Copy(leftImage.Data, leftData, 0, virtualCamera.FrameHeight * virtualCamera.FrameWidth / 2);

                // Display the Left Image Texture
                if (updateScreenAutomatically) updateScreen(leftImage, true);

                // Get the Right Image Data
                rightImageSlice = new Mat(grayImage, new OpenCvSharp.Rect(virtualCamera.FrameWidth / 2, 0, virtualCamera.FrameWidth / 2, virtualCamera.FrameHeight));
                rightImageSlice.CopyTo(rightImage);
                Marshal.Copy(rightImage.Data, rightData, 0, virtualCamera.FrameHeight * virtualCamera.FrameWidth / 2);

                // Display the Right Image Texture
                if (updateScreenAutomatically) updateScreen(rightImage, false);
            }
        }
        private void OnDestroy()
        {
            cap.Release();
            webcamImage.Release();
            grayImage.Release();
            leftImageSlice.Release();
            rightImageSlice.Release();
            leftImage.Release();
            rightImage.Release();
        }

        public void updateScreen(Mat image, bool isLeft)
        {
            Renderer display = isLeft ? leftDisplay : rightDisplay;

            // Display the Right Image Texture
            if (display != null)
            {
                if (isLeft)
                {
                    fillTexture(image, ref leftTexture);
                }
                else
                {
                    fillTexture(image, ref rightTexture);
                }
                display.material.mainTexture = isLeft ? leftTexture : rightTexture;
            }
        }

        public void changeDeviceNumber(int newDeviceNumber)
        {
            if (cap.IsOpened()) cap.Release();
            cap = null;
            deviceNumber = newDeviceNumber;
            cap = new OpenCvSharp.VideoCapture(deviceNumber);
        }

        public static void fillTexture(Mat input, ref Texture2D output, byte[] bytes = null)
        {
            bool textureGood = (output != null &&
                                output.width == input.Width && output.height == input.Height &&
                              ((input.Type() == MatType.CV_8UC3 && output.format == TextureFormat.RGB24) ||
                               (input.Type() == MatType.CV_8UC1 && output.format == TextureFormat.R8)));
            if (!textureGood)
            {
                TextureFormat format = (input.Type() == MatType.CV_8UC3) ? TextureFormat.RGB24 : TextureFormat.R8;
                output = new Texture2D(input.Width, input.Height, format, false);
            }
            if (bytes == null)
            {
                output.LoadRawTextureData(input.Data, input.Width * input.Height * ((input.Type() == MatType.CV_8UC3) ? 3 : 1));
            }
            else
            {
                output.LoadRawTextureData(bytes);
            }
            output.Apply();
        }
    }
}