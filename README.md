# 3D-Calibration-VCam

## Procedure (motorized rig)

Preconditions:
- Headset tilt up
- Camera down

Steps:
1. Do v2 monitor, python [graycodeCalibration.py](https://github.com/JurajVincur/pnsutils/blob/main/graycodeCalibration.py) 0
2. Rename produced files, HeightCalibration_blur.png -> MD_HeightCalibration.png, WidthCalibration_blur.png -> MD_WidthCalibration.png
3. Move camera up using [controller.py](https://github.com/JurajVincur/pnsutils/blob/main/calibrationRig/controller.py)
4. Do v2 monitor, python [graycodeCalibration.py](https://github.com/JurajVincur/pnsutils/blob/main/graycodeCalibration.py) 0
5. Rename produced files, HeightCalibration_blur.png -> MU_HeightCalibration.png, WidthCalibration_blur.png -> MU_WidthCalibration.png
6. Capture pattern with t265, python [triangulateChecker2.py](https://github.com/JurajVincur/pnsutils/blob/main/triangulateChecker2.py) t265
7. Move camera down using [controller.py](https://github.com/JurajVincur/pnsutils/blob/main/calibrationRig/controller.py)
8. Tilt headset down using [controller.py](https://github.com/JurajVincur/pnsutils/blob/main/calibrationRig/controller.py)
9. Do v2 headset, python [graycodeCalibration.py](https://github.com/JurajVincur/pnsutils/blob/main/graycodeCalibration.py) 1
10. Rename produced files, HeightCalibration_blur.png -> CD_HeightCalibration.png, WidthCalibration_blur.png -> CD_WidthCalibration.png
11. Move camera up using [controller.py](https://github.com/JurajVincur/pnsutils/blob/main/calibrationRig/controller.py)
12. Do v2 headset, python [graycodeCalibration.py](https://github.com/JurajVincur/pnsutils/blob/main/graycodeCalibration.py) 1
13. Rename produced files, HeightCalibration_blur.png -> CU_HeightCalibration.png, WidthCalibration_blur.png -> CU_WidthCalibration.png
14. Capture pattern with lmc, python [triangulateChecker2.py](https://github.com/JurajVincur/pnsutils/blob/main/triangulateChecker2.py) lmc [index]
15. Align ET cams using [eyeCameraAligner.py](https://github.com/JurajVincur/pnsutils/blob/main/eyeCameraAligner.py)
16. Get relative poses between sensors based on observations from steps 6, 14, 15 using [kabschPoints.py](https://github.com/JurajVincur/pnsutils/blob/main/kabschPoints.py)
17. Copy/move produced PNG files to root directory of this project
18. Update [config.json](config.json) and [cameraCalibration.json](cameraCalibration.json) to match your setup. In my case distCoeffs are set to 0 because gradients are computed based on already undistorted images. If you are using only one stereo camera make sure that camera attributes are the same for both devices under deviceCalibrations.
19. Open project in Unity, run scene Callibration-Vcam, do v1 without 2) Create Reflector Mask

https://www.youtube.com/watch?v=SqglkrRwO90

Motorized rig files:
- [model](https://www.tinkercad.com/things/e5AmGILqEts-motorized-pns-calibration-rig)
- [code](https://github.com/JurajVincur/pnsutils/tree/main/calibrationRig)
- [alternative mount](https://www.tinkercad.com/things/dLEQhVlKpmY-pns-calibration-rig-mount-single-camera)
