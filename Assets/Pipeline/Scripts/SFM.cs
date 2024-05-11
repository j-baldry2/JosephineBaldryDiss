using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using static Pair;
using static Graph;
using static Triangulation;
using static StructurePoint;

public class SFM : MonoBehaviour
{
    List<Mat> images = new List<Mat>();
    List<KeyPoint[]> keyPoints = new List<KeyPoint[]>();
    List<Mat> descriptors = new List<Mat>();
    List<Pair> pairs = new List<Pair>();
    List<Graph> graphs = new List<Graph>();

    [SerializeField]
    private GameObject render;


    // Start is called before the first frame update
    void Start()
    {


        //string idir = @"Assets/Pipeline/CapturesTempleRing";

        //string[] images_path = Directory.GetFiles(idir, "*.bmp");       


        string idir = @"Assets/Pipeline/Captures";

        string[] images_path = Directory.GetFiles(idir, "*.bmp");



        int nimages = images_path.Length;

        for (int i = 0; i < nimages; ++i)
        {
            string iname = images_path[i];
            Mat image = Cv2.ImRead(iname, ImreadModes.Color);

            if (image.Empty())
            {
                Debug.Log($"Error reading image: {iname}");
            }
            else
            {
                images.Add(image);
                orb(i);
            }

        }
        Debug.Log("Completed ORB");
        if (false)
        {
            //testing code
            using (StreamWriter writer = new StreamWriter("Assets/Pipeline/Testing/results.txt", append: true))
            {
                writer.WriteLine($"Completed orb");

                for (int i = 0; i < keyPoints.Count; ++i)
                {
                    writer.WriteLine($"Keypoint count for image {i} is {keyPoints[i].Length}");
                }

            }
        }


        for (int i = 0; i < nimages - 1; ++i)
        {
            for (int j = i + 1; j < nimages; ++j)
            {
                Pair nPair = new Pair(i, j);
                List<KeyPoint> kp1 = new List<KeyPoint>();
                List<KeyPoint> kp2 = new List<KeyPoint>();
                nPair.Matches = bf_matcher(descriptors[i], descriptors[j], i, j, out kp1, out kp2).ToList();
                nPair.add_keypoints(kp1, kp2);

                if (nPair.KeyPoints[0].Count > 0)
                {
                    pairs.Add(nPair);

                    if (false)
                    {
                        //Testing code
                        Mat image1 = images[nPair.Cams[0]];
                        Mat image2 = images[nPair.Cams[1]];
                        Mat outimg = new Mat();

                        List<KeyPoint> drawkeypoints1 = nPair.KeyPoints[0];
                        List<KeyPoint> drawkeypoints2 = nPair.KeyPoints[1];

                        List<DMatch> drawmatches = nPair.Matches;
                        Cv2.DrawMatches(image1, drawkeypoints1, image2, drawkeypoints2, drawmatches, outimg);
                        string drawpath = ($"Assets/Pipeline/Testing/drawmatches/pair{i}{j}.bmp");
                        Cv2.ImWrite(drawpath, outimg);

                    }

                }


            }
        }
        foreach (Pair cPair in pairs)
        {
            //FUND MATRIX
            Mat fundamentalMatrix = new Mat();
            if (!fund_est(cPair.Cams[0], cPair.Cams[1], cPair.Matches, out fundamentalMatrix))
            {
                //Dont use pair list w indeces anywhere else
                continue;
            }
            fundamentalMatrix.ConvertTo(fundamentalMatrix, MatType.CV_32FC1);
            //This just grabs first submatrix if it turns out as a 9x3 matrix
            if (fundamentalMatrix.Rows != 3)
            {
                Mat fmat = fundamentalMatrix.SubMat(new OpenCvSharp.Range(0, 3), new OpenCvSharp.Range(0, 3));
                cPair.F = fmat;
            }
            else
            {
                cPair.F = fundamentalMatrix;
            }

            //TEST FUND MAT
            cPair.fetch_intrinsics();

            //ESSENTIAL MATRIZ

            cPair.E = cPair.IntrinsicsMat[1].T() * cPair.F * cPair.IntrinsicsMat[0];
            Pair tempPair = rt_from_e(cPair);
            cPair.ExtrinsicsMat = tempPair.ExtrinsicsMat;

            //ADD GRAPH + TRIANGULATION
            Graph graph = new Graph(cPair);
            if (graph.Tracks.Count > 0)
            {
                graph = triangulation(graph);
                graphs.Add(graph);
            }


        }
        Debug.Log("Completed GM");
        //BUNDLE ADJUSTMENT STUFF WOULD GO HERE
        //TODO THIS

        //Graph.test();

        //Merging Graphs
        List<int> mergedGraphs = Enumerable.Repeat(0, graphs.Count).ToList();
        mergedGraphs[0] = 1;
        Debug.Log($"graph count is {graphs.Count}");
        Debug.Log($"sp graph count is {graphs[0].StructurePoints.Count}");
        Graph global_graph = new Graph(graphs[0]);
        int icam = 0;
        int count = 0;
        while ((icam = Graph.find_next_graph(graphs, global_graph, ref mergedGraphs)) > 0)
        {

            Graph.merge_graph(ref global_graph, graphs[icam]);
            global_graph = triangulation(global_graph);
            count++;
        }

        Debug.Log("Graphs merged");
        List<Vec3f> pa = global_graph.return_points();
        List<Vector3> vec3_pa = new List<Vector3>();
        List<Color> col_arr = new List<Color>();

        render.GetComponent<PointCloudRenderer>().SetParticles(vec3_pa.ToArray(), col_arr.ToArray());

    }

    #region ORB
    private void orb(int i)
    {
        ORB orb = ORB.Create();

        Mat desc = new Mat();
        KeyPoint[] kp;

        orb.DetectAndCompute(images[i], null, out kp, desc, false);

        descriptors.Add(desc);
        keyPoints.Add(kp);
        orb.Dispose();
    }
    #endregion
    #region Matching

    private DMatch[] bf_matcher(Mat A, Mat B, int i, int j, out List<KeyPoint> kp1, out List<KeyPoint> kp2)
    {
        var bfMatcher = new BFMatcher(NormTypes.Hamming, false);
        List<DMatch> matches = bfMatcher.Match(A, B, null).ToList();

        //Sort the matches by distance
        matches.Sort((m1, m2) => m1.Distance.CompareTo(m2.Distance));

        //Lowes ratio
        double ratioThreshold = 0.99; //Ratio threshold - lower for less matches, higher for more
        List<DMatch> goodMatches = new List<DMatch>();

        for (int k = 0; k < matches.Count - 1; k++)
        {
            if (matches[k].Distance < ratioThreshold * matches[k + 1].Distance && goodMatches.Count < 7)
            {
                goodMatches.Add(matches[k]);
            }
        }

        //Need to fetch the keypoints involved in the match
        //Adapted from https://stackoverflow.com/a/30720370
        kp1 = new List<KeyPoint>();
        kp2 = new List<KeyPoint>();

        foreach (DMatch mat in matches)
        {
            int img1_idx = mat.QueryIdx;
            int img2_idx = mat.TrainIdx;
            kp1.Add(keyPoints[i][img1_idx]);
            kp2.Add(keyPoints[j][img2_idx]);
        }
        return goodMatches.ToArray();
    }
    #endregion
    #region RANSAC
    private bool fund_est(int imgInd1, int imgInd2, List<DMatch> matches, out Mat fundamentalMatrix)
    {
        //This needs testing in full - should check error using dot product
        //https://stackoverflow.com/questions/12098363/testing-a-fundamental-matrix#:~:text=To%20test%20if%20the%20fundamental,difference%20between%20the%20cameras%20was.
        //might work?
        List<Point2f> points1 = new List<Point2f>();
        List<Point2f> points2 = new List<Point2f>();

        foreach (DMatch match in matches)
        {
            //Floating point stuff
            Point2f p1 = keyPoints[imgInd1][match.QueryIdx].Pt;
            Point2f p2 = keyPoints[imgInd2][match.ImgIdx].Pt;

            float r1x = (float)Math.Round(p1.X, 2);
            float r2x = (float)Math.Round(p2.X, 2);
            float r1y = (float)Math.Round(p1.Y, 2);
            float r2y = (float)Math.Round(p2.Y, 2);

            Point2f rp1 = new Point2f(r1x, r1y);
            Point2f rp2 = new Point2f(r2x, r2y);
            points1.Add(rp1);
            points2.Add(rp2);
        }

        if (points1.Count == 0)
        {
            fundamentalMatrix = Mat.Zeros(3, 3, MatType.CV_32FC1);
            return false;
        }

        InputArray IaPoints1 = InputArray.Create(points1);
        InputArray IaPoints2 = InputArray.Create(points2);


        fundamentalMatrix = Cv2.FindFundamentalMat(IaPoints1, IaPoints2, FundamentalMatMethod.Ransac, 3.0, 0.1);



        if (fundamentalMatrix.Empty())
        {
            fundamentalMatrix = Mat.Zeros(3, 3, MatType.CV_32FC1);
            return false;
        }
        return true;
    }
    #endregion 
    #region RTfromE
    private Pair rt_from_e(Pair pair)
    {
        //Heavily pulls from Kai Wus SFM tutorial
        //section 6 on computing essential and fundamenta matrix
        //E is essential
        //Rs is rotation matrices
        //Ts is translation matrices

        Mat E = pair.E;
        List<Mat> Rs = new List<Mat>();
        List<Mat> Ts = new List<Mat>();

        calc_rt(E, out Rs, out Ts);

        //Initialise Rts
        //Rts is a list of mats, each mat is a combination of a rm R and tv T
        // assigns the rotation matrix R to the top-left 3x3 block of the current
        // Rts matrix and assigns the translation vector t to the top-right 3x1 block.
        //this is the EXTRINSIC MATRIX
        List<Mat> Rts = new List<Mat>(4);
        for (int i = 0; i < Rs.Count; ++i)
        {
            for (int j = 0; j < Ts.Count; ++j)
            {
                Mat Rt = new Mat(3, 4, MatType.CV_32FC1);
                Mat Rblock = Rt.RowRange(0, 3).ColRange(0, 3);
                Rs[i].CopyTo(Rblock);
                Mat tblock = Rt.RowRange(0, 3).ColRange(3, 4);
                Ts[j].CopyTo(tblock);
                Rts.Add(Rt);
            }
        }

        //should do stuff here that calculates the best RTS but just picks first one 


        Mat extrmat1 = Rts[0];
        Mat extrmat2 = Rts[2];
        pair.ExtrinsicsMat.Add(extrmat1);
        pair.ExtrinsicsMat.Add(extrmat2);

        return pair;

    }

    private void calc_rt(Mat E, out List<Mat> Rs, out List<Mat> Ts)
    {
        //Heavily pulls from Kai Wus SFM tutorial
        //computation of relative rotation and translation matrices
        //from e mat using SVD
        List<Mat> R = new List<Mat>();
        List<Mat> t = new List<Mat>();
        SVD svd = new SVD(E, SVD.Flags.FullUV);

        //access the results of SVD
        Mat U = svd.U;
        Mat w = svd.W;
        Mat vt = svd.Vt;

        Mat W = new Mat(3, 3, MatType.CV_32FC1);
        W.Set<float>(0, 0, 0); W.Set<float>(0, 1, -1); W.Set<float>(0, 2, 0);
        W.Set<float>(1, 0, 1); W.Set<float>(1, 1, 0); W.Set<float>(1, 2, 0);
        W.Set<float>(2, 0, 0); W.Set<float>(2, 1, 0); W.Set<float>(2, 2, 1);

        Mat Z = new Mat(3, 3, MatType.CV_32FC1);
        Z.Set<float>(0, 0, 0); Z.Set<float>(0, 1, 1); Z.Set<float>(0, 2, 0);
        Z.Set<float>(1, 0, -1); Z.Set<float>(1, 1, 0); Z.Set<float>(1, 2, 0);
        Z.Set<float>(2, 0, 0); Z.Set<float>(2, 1, 0); Z.Set<float>(2, 2, 0);

        //s is the Skew-symmetric matrix (translation vector)
        Mat S = U * Z * U.Transpose();

        //t holds two possible translation vectors
        t.Add(U.ColRange(2, 3));
        t.Add(-U.ColRange(2, 3));

        //R holds two possible rotation matrices
        R.Add(U * W * vt.Transpose());
        R.Add(U * W.Transpose() * vt.Transpose());

        //checks determinants of rotation matrices and adjusts signs if necessary 
        //for proper orientation
        if (R[0].Determinant() < 0)
        {
            R[0] = -R[0].Clone();
        }

        if (R[1].Determinant() < 0)
        {
            R[1] = -R[1].Clone();
        }


        Rs = R;
        Ts = t;

    }

    #endregion
    #region Triangulation
    private Graph triangulation(Graph graph)
    {
        Mat intr_mat = graph.IntrinsicsMats[0];

        Mat extr_mat1 = graph.ExtrinsicsMats[0];
        Mat extr_mat2 = graph.ExtrinsicsMats[1];

        List<KeyPoint> kps1 = new List<KeyPoint>();
        List<KeyPoint> kps2 = new List<KeyPoint>();

        for (int i = 0; i < graph.Tracks.Count; ++i)
        {
            kps1.Add(graph.Tracks[i][0]);
            kps2.Add(graph.Tracks[i][1]);
        }


        Mat pts1 = new Mat(1, kps1.Count, MatType.CV_32FC2);
        Mat pts2 = new Mat(1, kps2.Count, MatType.CV_32FC2);

        Mat pts1_ud = new Mat(1, kps1.Count, MatType.CV_32FC2);
        Mat pts2_ud = new Mat(1, kps2.Count, MatType.CV_32FC2);

        if (kps1.Count == 0 || kps2.Count == 0)
        {
            return graph;
        }
        for (int i = 0; i < kps1.Count; ++i)
        {
            pts1.Set<Point2f>(0, i, kps1[i].Pt);

        }
        for (int j = 0; j < kps2.Count; ++j)
        {
            pts2.Set<Point2f>(0, j, kps2[j].Pt);
        }

        Mat dist_mat = graph.Distortion;
        intr_mat.ConvertTo(intr_mat, MatType.CV_32FC2);
        dist_mat.ConvertTo(dist_mat, MatType.CV_32FC2);


        if (true)
        {
            Cv2.UndistortPoints(pts1, pts1_ud, intr_mat, dist_mat);
            Cv2.UndistortPoints(pts2, pts2_ud, intr_mat, dist_mat);
        }


        Mat pointMat = new Mat();

        List<Mat> points4d = new List<Mat>();
        Mat inpmat1 = intr_mat * extr_mat1;
        Mat inpmat2 = intr_mat * extr_mat2;


        if (true)
        {
            Cv2.TriangulatePoints(inpmat1, inpmat2, pts1_ud, pts2_ud, pointMat);
        }

        if (false)
        {
            Cv2.TriangulatePoints(inpmat1, inpmat2, pts1, pts2, pointMat);
        }



        for (int i = 0; i < pointMat.Rows; ++i)
        {
            Point3f point;
            point.X = pointMat.At<float>(i, 0);
            point.Y = pointMat.At<float>(i, 1);
            point.Z = pointMat.At<float>(i, 2);
            graph.StructurePoints.Add(new StructurePoint(new Vec3f((float)point.X, (float)point.Y, (float)point.Z)));
            TestT(point);
        }

        return graph;
    }
    #endregion

    #region testing

    private void TestT(Point3f point)
    {
        using (StreamWriter writer = new StreamWriter("Assets/Pipeline/Testing/triangulateresults.txt", append: true))
        {
          writer.WriteLine($"({point.X},{point.Y},{point.Z})");
        }
    }  
   

    #endregion
}
