using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenCvSharp;

public class Pair
{
    public List<int> Cams { get; set; }
    public List<DMatch> Matches { get; set; }
    public List<int> gmInd;
    public Mat F { get; set; }
    public Mat E { get; set; }
    public List<Mat> IntrinsicsMat { get; set; }
    public List<Mat> ExtrinsicsMat { get; set; }
    public Mat Distortion;
    public List<List<KeyPoint>> KeyPoints;


    public Pair()
    {
        Cams = new List<int>();
        Matches = new List<DMatch>();
        KeyPoints = new List<List<KeyPoint>>();
        F = new Mat();
        E = new Mat();
        IntrinsicsMat = new List<Mat>();
        ExtrinsicsMat = new List<Mat>();
        Distortion = new Mat();
    }
    


    public Pair(int cam1, int cam2)
    {
        Cams = new List<int>() { cam1, cam2 };
        Matches = new List<DMatch>();
        F = new Mat();
        E = new Mat();
        IntrinsicsMat = new List<Mat>();
        ExtrinsicsMat = new List<Mat>();

        KeyPoints = new List<List<KeyPoint>>();
        Distortion = new Mat();
    }

    public void fetch_intrinsics()
    {
     
            float w = 3.218422891183427623e+02f;
            float h = 2.698713050040576604e+02f;
            float f = 5.112829578864060522e+02f;
  


            //temple ring params
            //float w = 1520f;
            //float h = 302f;
            //float f = 246f;



        Mat intrinsics_mat_0 = new Mat(3, 3, MatType.CV_32FC1);
        Mat intrinsics_mat_1 = new Mat(3, 3, MatType.CV_32FC1);

        intrinsics_mat_0.Set<float>(0, 0, f);
        intrinsics_mat_0.Set<float>(1, 1, f);
        intrinsics_mat_0.Set<float>(0, 2, w);
        intrinsics_mat_0.Set<float>(1, 2, h);
        intrinsics_mat_0.Set<float>(2, 2, 1.0f);

        intrinsics_mat_1 = intrinsics_mat_0.Clone();

        IntrinsicsMat.Add(intrinsics_mat_0);
        IntrinsicsMat.Add(intrinsics_mat_1);

        Distortion = new Mat(8, 1, MatType.CV_32FC1);

        float k1 = 4.064841722630019305e-02f;
        float k2 = 5.223025204791901244e-01f;
        float p1 = 3.312888358651672107e-03f;
        float p2 = 6.604118745730668157e-04f;
        float k3 = -2.323822976729395418e+00f;
        float k4 = 0f;
        float k5 = 0f;
        float k6 = 0f;

        Distortion.Set<float>(0, 0, k1);
        Distortion.Set<float>(1, 0, k2);
        Distortion.Set<float>(2, 0, p1);
        Distortion.Set<float>(3, 0, p2);
        Distortion.Set<float>(4, 0, k3);
        Distortion.Set<float>(5, 0, k4);
        Distortion.Set<float>(6, 0, k5);
        Distortion.Set<float>(7, 0, k6);


    }

    public void add_keypoints(List<KeyPoint> kps1, List<KeyPoint> kps2) {
        KeyPoints.Add(kps1);
        KeyPoints.Add(kps2);
    }

}


