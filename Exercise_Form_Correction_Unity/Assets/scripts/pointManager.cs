using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class pointManager : MonoBehaviour
{
    public List<PointSet> pointSetList = new List<PointSet>();

    void Start()
    {
        PopulatePointSetListWithPoints(10, 16);
    }

    public float[,,] ConvertPointSetListToTensor(List<PointSet> pointSets)
    {
        // Assuming each PointSet has the same number of points
        int numPointSets = pointSets.Count;
        int numPointsPerSet = pointSets[0].points.Count;

        // Create a multi-dimensional array to hold the data
        float[,,] tensorData = new float[numPointSets, numPointsPerSet, 2];

        // Populate the array with Point data
        for (int i = 0; i < numPointSets; i++)
        {
            PointSet pointSet = pointSets[i];
            for (int j = 0; j < numPointsPerSet; j++)
            {
                Point point = pointSet.points[j];
                tensorData[i, j, 0] = point.x;
                tensorData[i, j, 1] = point.y;
            }
        }

        // Create the tensor from the array data
        //Tensor tensor = new Tensor(tensorData);

        return tensorData;
    }

    public void CalculateAndPrintAngle(Point p1, Point p2, Point p3)
    {
        // Calculate vectors from p2 to p1 and p3
        Vector2 v1 = new Vector2(p1.x - p2.x, p1.y - p2.y);
        Vector2 v2 = new Vector2(p3.x - p2.x, p3.y - p2.y);

        // Calculate dot product and magnitudes
        float dotProduct = Vector2.Dot(v1, v2);
        float magnitudeV1 = v1.magnitude;
        float magnitudeV2 = v2.magnitude;

        // Calculate angle in radians using the dot product formula
        float cosTheta = dotProduct / (magnitudeV1 * magnitudeV2);
        float angleRadians = Mathf.Acos(cosTheta);

        // Convert angle to degrees
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        Debug.Log("Angle formed at vertex (" + p2.x + ", " + p2.y + "): " + angleDegrees + " degrees");
    }

    public float CalculateAngle(Point p1, Point p2, Point p3)
    {
        // Calculate vectors from p2 to p1 and p3
        Vector2 v1 = new Vector2(p1.x - p2.x, p1.y - p2.y);
        Vector2 v2 = new Vector2(p3.x - p2.x, p3.y - p2.y);

        // Calculate dot product and magnitudes
        float dotProduct = Vector2.Dot(v1, v2);
        float magnitudeV1 = v1.magnitude;
        float magnitudeV2 = v2.magnitude;

        // Calculate angle in radians using the dot product formula
        float cosTheta = dotProduct / (magnitudeV1 * magnitudeV2);
        float angleRadians = Mathf.Acos(cosTheta);

        // Convert angle to degrees
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        return angleDegrees;
    }


    public void RemoveFirstPointSet()
    {
        if (pointSetList.Count > 0)
        {
            pointSetList.RemoveAt(0); // Remove the first PointSet
        }
        else
        {
            Debug.LogWarning("pointSetList is empty. Cannot remove the first PointSet.");
        }
    }

    public void EditLastPointSet(int pointIndex, float newX, float newY)
    {
        int lastSetIndex = pointSetList.Count - 1;
        if (lastSetIndex >= 0)
        {
            PointSet lastPointSet = pointSetList[lastSetIndex];
            if (pointIndex >= 0 && pointIndex < lastPointSet.points.Count)
            {
                Point pointToEdit = lastPointSet.points[pointIndex];
                pointToEdit.x = newX;
                pointToEdit.y = newY;
                //Debug.Log("Edited point at index " + pointIndex + " in last PointSet to (" + newX + ", " + newY + ")");
            }
            else
            {
                Debug.LogWarning("Invalid point index: " + pointIndex);
            }
        }
        else
        {
            Debug.LogWarning("No PointSet available for editing.");
        }
    }


    public void PopulatePointSetListWithPoints(int numPointSets, int numPointsPerSet)
    {
        for (int i = 0; i < numPointSets; i++)
        {
            PointSet pointSet = new PointSet();
            PopulatePointSet(pointSet, numPointsPerSet);
            pointSetList.Add(pointSet);
        }
    }

    void PopulatePointSet(PointSet pointSet, int numPoints)
    {
        for (int i = 0; i < numPoints; i++)
        {
            pointSet.AddPoint(new Point(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
        }
    }

    public void PrintLastPointSet()
    {
        if (pointSetList.Count > 0)
        {
            PointSet lastPointSet = pointSetList[pointSetList.Count - 1];
            string pointSetString = lastPointSet.GetPointSetAsString();
            Debug.Log("Last PointSet: " + pointSetString);
        }
        else
        {
            Debug.LogWarning("pointSetList is empty.");
        }
    }

    public string getLastPointSetString()
    {
        if (pointSetList.Count > 0)
        {
            PointSet lastPointSet = pointSetList[pointSetList.Count - 1];
            string pointSetString = lastPointSet.GetPointSetAsString();
            return pointSetString;
        }
        else
        {
            return "empty";
        }
    }

    void PrintAllPointSets()
    {
        foreach (PointSet pointSet in pointSetList)
        {
            string pointSetString = pointSet.GetPointSetAsString();
            Debug.Log(pointSetString);
        }
    }

    // Point class to store x and y coordinates
    public class Point
    {
        public float x;
        public float y;

        public Point(float xCoord, float yCoord)
        {
            x = xCoord;
            y = yCoord;
        }
    }

    // PointSet class to store a set of Point objects
    public class PointSet
    {
        public List<Point> points = new List<Point>();

        // Method to add a Point to the PointSet
        public void AddPoint(Point point)
        {
            points.Add(point);
        }

        // Method to get all points in the PointSet as a formatted string
        public string GetPointSetAsString()
        {
            string pointSetString = "PointSet: ";
            foreach (Point point in points)
            {
                pointSetString += "(" + point.x + ", " + point.y + ") ";
            }
            return pointSetString;
        }
    }
}
