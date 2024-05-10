using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class OpenCVContours : WebCamera
{
    [SerializeField] private FlipMode ImageFlip;
    [SerializeField] private float Threshold = 96.4f;
    [SerializeField] private bool ShowProcessingImage = true;
    [SerializeField] private float CurveAccuracy = 10f;
    [SerializeField] private float MinArea = 5000f;

    private Mat image;
    private Mat processImage = new Mat();
    private Mat contour_mask = new Mat();
    private Point[][] contours;
    private HierarchyIndex[] hierarchy;
    private Scalar contour_mean;

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        image = OpenCvSharp.Unity.TextureToMat(input);
        //contour_mask = zeros(image.size(), CV_8UC1);

        Cv2.Flip(image, image, ImageFlip);
        Cv2.CvtColor(image, processImage, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(image, contour_mask, ColorConversionCodes.BGR2GRAY, 1);
        
        Cv2.Threshold(processImage, processImage, Threshold, 255, ThresholdTypes.BinaryInv);
        Cv2.FindContours(processImage, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);

        //Cv2.DrawContours(contour_mask)
        int i = 0;
        foreach(Point[] contour in contours)
        {
            Point[] points = Cv2.ApproxPolyDP(contour, CurveAccuracy, true);
            var area = Cv2.ContourArea(contour);

            if (area > MinArea)
            {
                Cv2.Rectangle(contour_mask, new Point(0, 0), new Point(10000, 10000), new Scalar(0, 0, 0), -1);

                drawContour(processImage ,new Scalar(127, 127, 127), 2, points);
                Cv2.DrawContours(contour_mask, contours, i, new Scalar(255, 255, 255), -1, LineTypes.Link8);
   
                contour_mean = Cv2.Mean(image, contour_mask);
                drawContour(image, contour_mean, 2, points);
                //Cv2.Rectangle(contour_mask, new Point(0, 0), new Point(10000, 10000), new Scalar(255, 255, 255), -1);
            }
            i++;
        }
        //contour_mean = Cv2.Mean(image, contour_mask);


        if (output == null)
            output = OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image);
        else
            OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image, output);

        return true;
    }

    private void drawContour(Mat Image, Scalar Colour, int Thickness, Point[] Points)
    {
        for(int i = 1; i < Points.Length; i++)
        {
            Cv2.Line(Image, Points[i - 1], Points[i], Colour, Thickness);
        }
        Cv2.Line(Image, Points[Points.Length - 1], Points[0], Colour, Thickness);
    }
}

/* Adapted from
 * https://www.youtube.com/watch?v=ZV5eejYG6NI&ab_channel=MattBell
 * 
 */
