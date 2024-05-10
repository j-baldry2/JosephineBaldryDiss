using UnityEngine;
using OpenCvSharp;
using System.Collections.Generic;
using System.IO;
using System;

public class Calibration : MonoBehaviour
{
    // Prepare object points
    private MatOfPoint3f objp = new MatOfPoint3f();
    private List<Point3f> objPointsList = new List<Point3f>();

    // Start is called before the first frame update
    void Start()
    {
        // Termination criteria
        TermCriteria criteria = new TermCriteria(CriteriaType.Eps | CriteriaType.MaxIter, 30, 0.001);

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                objPointsList.Add(new Point3f(j, i, 0));
            }
        }
       
        objp = MatOfPoint3f.FromArray(objPointsList.ToArray());

        // Arrays to store object points and image points from all the images
        List<MatOfPoint3f> objpoints = new List<MatOfPoint3f>(); // 3D point in real world space
        List<MatOfPoint2f> imgpoints = new List<MatOfPoint2f>(); // 2D points in image plane

        string idir = @"Assets/Pipeline/Calibrations";

        string[] images_path = Directory.GetFiles(idir, "*.bmp");

        foreach (string imagePath in images_path)
        {
            Mat img = new Mat(imagePath);
            Mat gray = img.CvtColor(ColorConversionCodes.BGR2GRAY);

            // Find the chessboard corners
            bool found = Cv2.FindChessboardCorners(gray, new Size(7, 7), out Point2f[] corners);

            // If found, add object points, image points (after refining them)
            if (found)
            {
                objpoints.Add(objp);
                Point2f[] corners2 = Cv2.CornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1), criteria);

                MatOfPoint2f mp2f = new MatOfPoint2f();
                foreach (Point2f corner in corners2) {
                    mp2f.Add(corner);
                }
                imgpoints.Add(mp2f);



               

                /*
                double mean_error = 0;
                for (int i = 0; i < objpoints.Count; i++)
                {
                    Mat imgpoints2;
                    Cv2.ProjectPoints(objpoints[i], rvecs[i], tvecs[i], mtx, dist, imgpoints2);
                    //double error = Cv2.Norm(imgpoints[i], imgpoints2, NormTypes.L2) / imgpoints2.Count;
                    mean_error += CalculateError(imgpoints[i], imgpoints2);
                }

                Console.WriteLine("Total error: " + (mean_error / objpoints.Count));
                */
            }
        }

        Mat mtx = new Mat();
        Mat dist = new Mat();
        Mat initImg = new Mat(images_path[0]);
        Mat gInitImg = initImg.CvtColor(ColorConversionCodes.BGR2GRAY);
        Mat[] rvecs, tvecs;
        Cv2.CalibrateCamera(objpoints, imgpoints, gInitImg.Size(), mtx, dist, out rvecs, out tvecs);

        Mat pImg = Cv2.ImRead(images_path[0]);
        int w = pImg.Width;
        int h = pImg.Height;

        Mat newcameramtx;
        OpenCvSharp.Rect roi;
        newcameramtx = Cv2.GetOptimalNewCameraMatrix(mtx, dist, new Size(w, h), 1, new Size(w, h), out roi);

        Mat dst = new Mat();
        Cv2.Undistort(pImg, dst, mtx, dist, newcameramtx);


        Mat croppedDst = new Mat(dst, roi);

        //Cv2.ImWrite("Assets/Pipeline/Calibrations/calibresult.bmp", croppedDst);
        WriteMatToCsv("Assets/Pipeline/Calibrations/mtx.csv", newcameramtx);
    }


    static void WriteMatToCsv(string filename, Mat mat)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            for (int row = 0; row < mat.Rows; row++)
            {
                for (int col = 0; col < mat.Cols; col++)
                {
                    writer.Write(mat.At<float>(row, col));
                    if (col < mat.Cols - 1)
                    {
                        writer.Write(",");
                    }
                }
                writer.WriteLine();
            }
        }
    }

    /*

    static double CalculateError(MatOfPoint2f expected, Mat actual)
    {
        Point2f[] actualArray;
        actual.GetArray(actualArray);
        double error = 0;
        for (int j = 0; j < expected.Length; j++)
        {
            error += Cv2.Norm(expected[j], actualArray[j]);
        }
        return error / expected.Length;
    }

    */


}
