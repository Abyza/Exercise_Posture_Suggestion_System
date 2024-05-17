# Exercise_Form_Correction_Unity

This folder contains the Interactive Python Notebook(.ipynb) files. 

Google Collab version: 3.10.12 [GCC 11.4.0]

# Model for Post Estimation

## Model 1
ResNet (Residual Network)

Input: image with 3 channels

Output: 16 Key points

![Project Image](https://github.com/Abyza/Project_Exercise_Posture_Correction/blob/main/Research_Paper/Image_Results/output_ResNet.jpg)

## Model 2
YOLOv8 (You Only Look Once version 8)

Input: image with 3 channels

Output: 17 Key points

![Project Image](https://github.com/Abyza/Project_Exercise_Posture_Correction/blob/main/Research_Paper/Image_Results/output_YOLOv8.jpg)

## Model 3
YOLO-NAS (You Only Look Once - Neural Architecture Search)

Input: image with 3 channels

Output: 17 Key points

![Project Image](https://github.com/Abyza/Project_Exercise_Posture_Correction/blob/main/Research_Paper/Image_Results/output_YOLO_NAS.jpg)

# Model for Human Activity Recognition

## Model 4
Post estimation

Input: image with 3 channels

Output: 16 images, heatmap where the higher value is where the key point is 

## Model 5
Exercise classification

Input: 16 key points

Output: 3 exercise class

## Model 6
Exercise classification

Input: 16 key points

Output: 3 exercise class


