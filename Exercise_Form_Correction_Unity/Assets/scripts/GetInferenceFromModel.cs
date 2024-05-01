using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Playables;
using static pointManager;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Google.Protobuf.WellKnownTypes;
using TMPro;
using UnityEngine.SocialPlatforms;

public class GetInferenceFromModel : MonoBehaviour
{
    public TMP_Text resultText;
    public TMP_Text resultText2;

    public Texture2D[] sampleTextures;
    private int currentIndex = 0;
    public Texture2D texture;

    public pointManager pointManagerScript;

    public Texture2D targetTexture; // Texture to modify

    public Color white_color;
    public Color key_points_color;
    public Color line_color;

    public NNModel modelAsset; //postest
    public NNModel modelAsset2; //lstm

    private Model _runtimeModel;

    private IWorker _engine;

    private Model _runtimeModel2;

    private IWorker _engine2;

    public UnityEngine.UI.Image displaytarget;

    public UnityEngine.UI.Toggle showPoint;
    public UnityEngine.UI.Toggle showLine;
    public UnityEngine.UI.Toggle useCamera;

    /// <summary>
    /// A struct used for holding the results of our prediction in a way that's easy for us to view from the inspector.
    /// </summary>
    [Serializable]
    public struct Prediction
    {
        // The most likely value for this prediction
        public int predictedValue;
        // The list of likelihoods for all the possible classes
        public float[] predicted;

        public void SetPrediction(Tensor t)
        {
            // Extract the float value outputs into the predicted array.
            predicted = t.AsFloats();
            // The most likely one is the predicted value.
            predictedValue = Array.IndexOf(predicted, predicted.Max());
            Debug.Log($"Predicted {predictedValue}");
        }
    }

    public Prediction prediction;


    private WebCamTexture webcamTexture;
    public Renderer targetRenderer;
    public Texture2D convertedTexture;
    // Start is called before the first frame update


    public Texture2D textureToSave;
    public string folderPath = "resources/";
    public string fileName = "savedTexture.png";

    public Texture2D originalTexture;
    public int targetSize = 256;

    public int frameRateDetection = 2;
    void Start()
    {
        interval = 1f / frameRateDetection;
      
        // Check if the folder exists, create it if not
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Save the texture to the specified path
        

        // Get the first available webcam device
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webcamTexture = new WebCamTexture(devices[0].name);
            webcamTexture.Play();

            // Set the webcam texture to the target renderer
            if (targetRenderer != null)
            {
                targetRenderer.material.mainTexture = webcamTexture;
            }
        }
        else
        {
            Debug.LogError("No webcam devices found!");
        }

        // Set up the runtime model and worker.
        _runtimeModel = ModelLoader.Load(modelAsset);
        _engine = WorkerFactory.CreateWorker(_runtimeModel, WorkerFactory.Device.Auto);
       
        // Set up the runtime model and worker.
        _runtimeModel2 = ModelLoader.Load(modelAsset2);
        _engine2 = WorkerFactory.CreateWorker(_runtimeModel2, WorkerFactory.Device.Auto);
        // Instantiate our prediction struct.
        //prediction = new Prediction();
      
       
        
    }

    private float timer = 0f;
    public float interval = 0.5f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

        }

        if (useCamera.isOn)
        {
            useCameraPredict();
        }

    }





    public void useCameraPredict()
    {

        // Update the timer
        timer += Time.deltaTime;

        // Check if the interval has passed
        if (timer >= interval)
        {
            if (webcamTexture != null)
            {
                // Create a new Texture2D to hold the webcam image
                convertedTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
                convertedTexture.SetPixels(webcamTexture.GetPixels());
                convertedTexture.Apply();
                texture = convertedTexture;

                // Now you have the webcam image as a Texture2D (convertedTexture)
                // You can use convertedTexture for further processing or display
            }

            Texture2D resizedTexture = ResizeTexture(texture, targetSize);
            Texture2D croppedTexture = CropTexture(resizedTexture, targetSize, targetSize);
            //SaveTextureToFile(croppedTexture, folderPath + fileName);
            texture = croppedTexture;
            calculate();

            // Reset the timer
            timer = 0f;
        }
    }

    public Texture2D croppedTexture;


    Texture2D ResizeTexture(Texture2D sourceTexture, int targetSize)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        int targetWidth, targetHeight;

        if (width > height)
        {

            targetWidth = (int)((float)width / height * targetSize);
            targetHeight = targetSize;
           
        }
        else
        {
            targetWidth = targetSize;
            targetHeight = (int)((float)height / width * targetSize);
        }
        Debug.Log(targetHeight +" "+ targetWidth);
        return ScaleTexture(sourceTexture, targetWidth, targetHeight);
    }

    Texture2D CropTexture(Texture2D sourceTexture, int targetWidth, int targetHeight)
    {
        int startX = (sourceTexture.width - targetWidth) / 2;
        int startY = (sourceTexture.height - targetHeight) / 2;

        Color[] pixels = sourceTexture.GetPixels(startX, startY, targetWidth, targetHeight);

        Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    Texture2D ScaleTexture(Texture2D sourceTexture, int targetWidth, int targetHeight)
    {
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
        Graphics.Blit(sourceTexture, rt);

        RenderTexture.active = rt;
        Texture2D scaledTexture = new Texture2D(targetWidth, targetHeight);
        scaledTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        scaledTexture.Apply();

        RenderTexture.active = null;
        Destroy(rt);
        Debug.Log(targetHeight + " " + targetWidth);

        return scaledTexture;
    }

    /*
    void SaveTextureToFile(Texture2D texture, string filePath)
    {
        // Encode the texture to PNG format
        byte[] imageBytes = texture.EncodeToPNG();

        // Write the bytes to a file
        File.WriteAllBytes(filePath, imageBytes);

        Debug.Log("Texture saved to: " + filePath);
    }
    */


    public void CycleTextures()
    {
        // Increment the index to get the next texture
        currentIndex = (currentIndex + 1) % sampleTextures.Length;
        texture = sampleTextures[currentIndex];
    }

    public void calculate()
    {
        // making a tensor out of a grayscale texture
        var channelCount = 3; //grayscale, 3 = color, 4 = color+alpha
                              // Create a tensor for input from the texture.
        Tensor inputX = new Tensor(texture, channelCount);
        TensorShape shape = inputX.shape;
        //Debug.Log(shape);
        // Peek at the output tensor without copying it.
        Tensor outputY = _engine.Execute(inputX).PeekOutput();
        // Set the values of our prediction struct using our output tensor.


        PrintTensorStats(outputY);
        /*
        for (int n = 0; n < outputY.batch; n++)
        {
            for (int h = 0; h < outputY.height; h++)
            {
                for (int w = 0; w < outputY.width; w++)
                {
                    for (int c = 0; c < outputY.channels; c++)
                    {
                        float value = outputY[n, h, w, c];
                        Debug.Log("Value at [" + n + "," + h + "," + w + "," + c + "]: " + value);
                    }
                }
            }
        }
        */
  
        inputX.Dispose();
        outputY.Dispose();

    }


    void PrintTensorStats(Tensor tensor)
    {
        displaytarget.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        //Debug.Log("Tensor Shape: " + tensor.shape);
        ChangeTextureColor(white_color);

        int x1=0;
        int y1=0;
        int x2=0;
        int y2=0;
        pointManagerScript.RemoveFirstPointSet();
        pointManagerScript.PopulatePointSetListWithPoints(1,16);

        for (int c = 0; c < tensor.channels; c++)
        {
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            float sum = 0;
            int minH = 0, minW = 0, maxH = 0, maxW = 0;

            for (int h = 0; h < tensor.height; h++)
            {
                for (int w = 0; w < tensor.width; w++)
                {
                    float value = tensor[0, h, w, c];
                    sum += value;

                    if (value < minValue)
                    {
                        minValue = value;
                        minH = h;
                        minW = w;
                    }

                    if (value > maxValue)
                    {
                        maxValue = value;
                        maxH = h;
                        maxW = w;
                    }
                }
            }

            float meanValue = sum / (tensor.height * tensor.width);
            /*
            Debug.Log("Channel " + c + ": Min=(" + minH + "," + minW + ")=" + minValue +
                      ", Max=(" + maxH + "," + maxW + ")=" + maxValue +
                      ", Mean=" + meanValue);
            */
            (int scaledX, int scaledY) = ScaleCoordinates(maxW, maxH);
            x1 = x2;
            y1 = y2;
            x2 = scaledX;
            y2 = scaledY;

            pointManagerScript.EditLastPointSet(c,x2,y2);

            if (c == 0 || c == 6 || c == 10)
            {
                if (showPoint.isOn)
                {
                    ChangePixelColor(scaledX, scaledY, key_points_color);
                }
            }
            //no c==0 and c==6 and c==10
            if (c == 1 || c==2 || c == 3 || c == 4 || c == 5 || c == 7 || c == 8 
                || c == 9  || c == 11 || c == 12 || c == 13 || c == 14 || c == 15 || c == 16)
            {
                if (showPoint.isOn)
                {
                    ChangePixelColor(scaledX, scaledY, key_points_color);
                }
                if (showLine.isOn)
                {
                    DrawLineBetweenPoints(x1, y1, x2, y2);
                }

            }
        }


        int[] pointArray = ConvertPointSetToArray(pointManagerScript.getLastPointSetString());
        float[,,,] inputTensor = ReshapeInput(pointArray);
        TensorShape tensorShape = new TensorShape(1, 1, 1, 32);
        Tensor input2 = new Tensor(tensorShape, inputTensor);
        Tensor output2 = _engine2.Execute(input2).CopyOutput();
        // 0 = lunge 1=plank 2=squat 3=wallsit
        float maxValue2 = 0;
        int maxC = 0;
        for (int n = 0; n < output2.batch; n++)
        {
            for (int h = 0; h < output2.height; h++)
            {
                for (int w = 0; w < output2.width; w++)
                {

                    for (int c = 0; c < output2.channels; c++)
                    {
                        float value = output2[n, h, w, c];
                        //Debug.Log("Value at [" + n + "," + h + "," + w + "," + c + "]: " + value);

                        // Check if the current value is greater than the maxValue
                        if (value > maxValue2)
                        {
                            maxValue2 = value;
                  
                            maxC = c;
                        }
                    }
                }
            }
        }
        //Debug.Log(maxC);
        float deg1;
        float deg2; 
        if (maxC == 0)
        {
            Debug.Log("lunge");
            //leg 1 116 deg
            deg1 = pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[0], pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[2]);
            evaluateResultAngle("lunge", 1, 116f, deg1);
            //leg 2 74 deg
            deg2 = pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[3], pointManagerScript.pointSetList[9].points[4], pointManagerScript.pointSetList[9].points[5]);
            evaluateResultAngle("lunge", 2, 74f, deg2);
        }
        else if (maxC == 1)
        {
            Debug.Log("plank");
            //leg 1  158 deg
            deg1=pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[0], pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[2]);
            evaluateResultAngle("plank", 1, 158f, deg1);
            //arm 1  84 deg
            deg2 =pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[10], pointManagerScript.pointSetList[9].points[11], pointManagerScript.pointSetList[9].points[12]);
            evaluateResultAngle("plank", 2, 84f, deg2);
        }
        else if (maxC == 2)
        {
            Debug.Log("squat");
            //leg 1 85
            deg1=pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[0], pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[2]);
            evaluateResultAngle("squat", 1, 85f, deg1);
            //torso  75
            deg2 =pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[6], pointManagerScript.pointSetList[9].points[7]);
            evaluateResultAngle("squat", 2, 75f, deg2);
        }
        else if (maxC == 3)
        {
            Debug.Log("wall sit");
            //leg 1 125
            deg1=pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[0], pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[2]);
            evaluateResultAngle("wall sit", 1, 125f, deg1);
            //torso 99
            deg2 =pointManagerScript.CalculateAngle(pointManagerScript.pointSetList[9].points[1], pointManagerScript.pointSetList[9].points[6], pointManagerScript.pointSetList[9].points[7]);
            evaluateResultAngle("wall sit", 2, 99f, deg2);
        }

        input2.Dispose();
        output2.Dispose();
    }

    public void evaluateResultAngle(string strParam, int intParam, float floatParam1, float floatParam2)
    {
        // Check if the string is "lunge" and the int param is 1
        if (strParam == "lunge" && intParam == 1)
        {
            if(AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }  
        }
        else if (strParam == "lunge" && intParam == 2)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText2.text = strParam + " (leg 2)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText2.text = strParam + " (leg 2)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }
        else if (strParam == "plank" && intParam == 1)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }
        else if (strParam == "plank" && intParam == 2)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText2.text = strParam + " (arm 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText2.text = strParam + " (arm 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }
        else if (strParam == "squat" && intParam == 1)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }
        else if (strParam == "squat" && intParam == 2)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText2.text = strParam + " (torso 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText2.text = strParam + " (torso 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }

        else if (strParam == "wall sit" && intParam == 1)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText.text = strParam + " (leg 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }
        else if (strParam == "wall sit" && intParam == 2)
        {
            if (AreFloatsClose(floatParam1, floatParam2) == true)
            {
                resultText2.text = strParam + " (torso 1)" + "\n" + floatParam1 + "\n" + "good";
            }
            else
            {
                resultText2.text = strParam + " (torso 1)" + "\n" + floatParam1 + "\n" + floatParam2;
            }
        }

    }
    public bool AreFloatsClose(float float1, float float2)
    {
        // Calculate the absolute difference between the floats
        float difference = Mathf.Abs(float1 - float2);

        // Check if the difference is within the threshold (5.0f)
        if (difference <= 20.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int[] ConvertPointSetToArray(string pointSetString)
    {
        List<int> pointList = new List<int>();
        Regex regex = new Regex(@"\((\d+), (\d+)\)");

        MatchCollection matches = regex.Matches(pointSetString);
        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3)
            {
                if (int.TryParse(match.Groups[1].Value, out int x) && int.TryParse(match.Groups[2].Value, out int y))
                {
                    pointList.Add(x);
                    pointList.Add(y);
                }
            }
        }

        return pointList.ToArray();
    }
    // Reshape input array to match the model's input shape
    private float[,,,] ReshapeInput(int[] pointArray)
    {
        // Assuming pointArray length is divisible by 32 (c dimension)
        int n = pointArray.Length / 32;
        float[,,,] reshapedArray = new float[n, 1, 1, 32];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                reshapedArray[i, 0, 0, j] = pointArray[i * 32 + j];
            }
        }

        return reshapedArray;
    }


    
    public void ExitGame()
    {
        // Check if the application is running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application in built players (e.g., standalone, mobile)
        Application.Quit();
#endif
    }

    public static (int, int) ScaleCoordinates(int x, int y)
    {
        // Define the scaling factor
        float scaleFactor = 256f / 64f;

        // Scale the coordinates using linear scaling
        int scaledX = Mathf.RoundToInt(x * scaleFactor);
        int scaledY = Mathf.RoundToInt(y * scaleFactor);

        // Return the scaled coordinates
        return (scaledX, scaledY);
    }

    // Bresenham's line algorithm to get all points along the line
    private static System.Collections.Generic.IEnumerable<(int, int)> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return (x0, y0);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void DrawLineBetweenPoints(int x1, int y1, int x2, int y2)
    {


        // Bresenham's line algorithm to get all points along the line
        foreach (var point in BresenhamLine(x1, y1, x2, y2))
        {
            int x = point.Item1;
            int y = point.Item2;

            if (x >= 0 && x < targetTexture.width && y >= 0 && y < targetTexture.height)
            {
                targetTexture.SetPixel(x, y, line_color); // Set pixel color for the line
            }
        }

        targetTexture.Apply(); // Apply the pixel changes to the texture
    }


    public int brushSize = 5; // Size of the brush affecting pixels

    void ChangePixelColor(int centerX, int centerY, Color newColor)
    {
        // Iterate through the pixels within the brush size around the specified coordinates
        for (int x = centerX - brushSize; x <= centerX + brushSize; x++)
        {
            for (int y = centerY - brushSize; y <= centerY + brushSize; y++)
            {
                // Check if the coordinates are within the texture bounds
                if (x >= 0 && x < targetTexture.width && y >= 0 && y < targetTexture.height)
                {
                    // Change the color of the pixel at the specified coordinates in the modified texture
                    targetTexture.SetPixel(x, y, newColor);
                }
            }
        }

        targetTexture.Apply(); // Apply the change immediately
    }

    void ChangeTextureColor(Color newColor)
    {
        Color[] pixels = targetTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = newColor; // Set all pixels to the new color
        }

        // Apply modified pixels to the texture
        targetTexture.SetPixels(pixels);
        targetTexture.Apply();
    }

    private void OnDisable()
    {
        // Stop webcam when the script is disabled or destroyed
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    void OnApplicationQuit()
    {

        // This function is called when the application is about to quit
        Debug.Log("Application is quitting. Perform cleanup here.");

        // Example: Save game progress or clean up resources
    }

    private void OnDestroy()
    {
        // Dispose of the engine manually (not garbage-collected).
        _engine?.Dispose();
        _engine2?.Dispose();
    }
}