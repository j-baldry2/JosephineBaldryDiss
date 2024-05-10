using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using OpenCvSharp;
using static Pair;
using static Track;
using static StructurePoint;

public class Graph
{
    public int ncams;
    public List<int> Cams;
    public List<Mat> IntrinsicsMats;
    public List<Mat> ExtrinsicsMats;
    public Mat Distortion;
    public List<Track> Tracks;
    public List<StructurePoint> StructurePoints;

    public static void test()
    {
        double[,] invdata = {
            { 1.0, 2.0, 3.0, 4.0 },
            { 5.0, 6.0, 7.0, 8.0 },
            { 9.0, 10.0, 11.0, 12.0 }
        };

        double[,] c1data = {
            { 1.0, 2.0, 3.0, 4.0 },
            { 5.0, 6.0, 7.0, 8.0 },
            { 9.0, 10.0, 11.0, 12.0 }
        };

        double[,] c2data = {
            { 13.0, 14.0, 15.0, 16.0 },
            { 17.0, 18.0, 19.0, 20.0 }
        };

        Mat invTestMatrix = new Mat(3, 4, MatType.CV_64FC1, invdata);
        Mat c1TestMatrix = new Mat(3, 4, MatType.CV_64FC1, c1data);
        Mat c2TestMatrix = new Mat(3, 3, MatType.CV_64FC1, c2data);

        Mat resultstestInv = Inv_Rt(invTestMatrix);
        Mat resultstestConc = Concat_Rt(c1TestMatrix, c2TestMatrix);

        List<string> invstringlist = new List<string>();

        for (int i = 0; i < resultstestInv.Rows; ++i)
        {
            string thisrow = "";
            for (int j = 0; j < resultstestInv.Cols; ++j)
            {
                thisrow += resultstestInv.At<float>(i, j);
            }
            invstringlist.Add(thisrow);
        }

        List<string> concstringlist = new List<string>();

        for (int i = 0; i < resultstestConc.Rows; ++i)
        {
            string thiconcsrow = "";
            for (int j = 0; j < resultstestConc.Cols; ++j)
            {
                thiconcsrow += resultstestConc.At<float>(i, j);
            }
            concstringlist.Add(thiconcsrow);
        }

        using (StreamWriter writer = new StreamWriter("Assets/Pipeline/Testing/inconv.txt", append: true))
        {
            writer.WriteLine($"Inverse Results");
            for (int i = 0; i < invstringlist.Count; ++i)
            {
                writer.WriteLine($"{invstringlist[i]}");
            }            
            
            writer.WriteLine($"Concatenation Results");
            for (int i = 0; i < concstringlist.Count; ++i)
            {
                writer.WriteLine($"{concstringlist[i]}");
            }
            
        }

    }

    public Graph()
    {
        ncams = 0;
        Cams = new List<int>();
        IntrinsicsMats = new List<Mat>();
        ExtrinsicsMats = new List<Mat>();
        Tracks = new List<Track>();
        StructurePoints = new List<StructurePoint>();
        Distortion = new Mat();
    }
    public Graph(Pair pair)
    {
        int nmatches = pair.Matches.Count;
        ncams = 2;
        Cams = pair.Cams;
        IntrinsicsMats = pair.IntrinsicsMat;
        ExtrinsicsMats = pair.ExtrinsicsMat;
        Distortion = pair.Distortion;

        Tracks = new List<Track>();
        StructurePoints = new List<StructurePoint>(nmatches);

        for (int i = 0; i < nmatches; ++i)
        {
            Track cTrack = new Track();
            Tracks.Add(cTrack);
            
        }


        for (int i = 0; i < nmatches; ++i)
        {
            Tracks[i].AddKP(pair.KeyPoints[0][i]);
            Tracks[i].AddKP(pair.KeyPoints[1][i]);
        }

    }

    public Graph(Graph graph)
    {
        ncams = graph.ncams;
        Cams = graph.Cams;
        IntrinsicsMats = graph.IntrinsicsMats;
        ExtrinsicsMats = graph.ExtrinsicsMats;
        Distortion = graph.Distortion;

        Tracks = new List<Track>();
        StructurePoints = new List<StructurePoint>();

        for (int i = 0; i < graph.Tracks.Count; ++i)
        {
            Tracks.Add(graph.Tracks[i]);
        }
        for (int i = 0; i < graph.StructurePoints.Count; ++i)
        {
            StructurePoints.Add(graph.StructurePoints[i]);
        }

    }

    public static int find_next_graph(List<Graph> graphs, Graph graph, ref List<int> mergedGraph)
    {
        //Translated directly from Kai Wu's SFM tutorial
        int count_max = 0;
        int ind_selected = -1;
        for (int i = 0; i < graphs.Count; ++i)
        {
            if (mergedGraph[i] == 1)
            {
                continue;
            }

            Graph graph1 = graphs[i];
            List<int> common_cams = graph1.Cams.Intersect(graph.Cams).ToList();

            if (!common_cams.Any())
            {
                continue;
            }

            int count = 0;
            List<int> common_tracks = new List<int>();
            for (int m = 0; m < graph.Tracks.Count; ++m)
            {
                for (int n = 0; n < graph1.Tracks.Count; ++n)
                {
                    Track track = graph.Tracks[m];
                    Track track1 = graph1.Tracks[n];
                    if (Track.HasOverlappingKeypoints(track, track1)) {
                        count++;
                    }
                }
            }

            if (count > count_max)
            {
                count_max = count;
                ind_selected = i;
            }
        }
        if (ind_selected > 0)
        {
            mergedGraph[ind_selected] = 1;
        }

        using (StreamWriter writer = new StreamWriter("Assets/Pipeline/Scripts/mergegraph.txt", append: true))
        {
            string ls = "";
            // Write the reprojection error to the file
            foreach (int i in mergedGraph)
            {
                ls += i;
            }
            writer.WriteLine($"state of the graph is: {ls}");
        }

        return ind_selected;
    }

    public static void merge_graph(ref Graph graph1, Graph graph2)
    {
        //Translated directly from Kai Wu's SFM tutorial
        List<int> common_cams = graph1.Cams.Intersect(graph2.Cams).ToList();
        List<int> diff_cams = graph2.Cams.Except(graph1.Cams).ToList();

        if (!common_cams.Any()) {
            return;
        }

        int ind_cam1 = graph1.index(common_cams[0]);
        int ind_cam2 = graph2.index(common_cams[0]);

        Mat Rt1 = graph1.ExtrinsicsMats[ind_cam1];
        Mat Rt2 = graph2.ExtrinsicsMats[ind_cam2];

        Mat Rt21 = new Mat();
        Cv2.HConcat(Rt1, -Rt2, Rt21);
        Mat Rt21Inv = Inv_Rt(Rt21);

        for (int i = 0; i < graph2.StructurePoints.Count; ++i)
        {
            Vec3f pt3d = graph2.StructurePoints[i].getCoord();
            Mat pt3dHomogeneous = new Mat(4, 1, MatType.CV_32F);
            pt3dHomogeneous.Set<float>(0, 0, pt3d.Item0);
            pt3dHomogeneous.Set<float>(1, 0, pt3d.Item1);
            pt3dHomogeneous.Set<float>(2, 0, pt3d.Item2);
            pt3dHomogeneous.Set<float>(3, 0, 1.0f);
            Mat transformed_point = Rt21Inv * pt3dHomogeneous;

            Vec3f transformedPt3d = new Vec3f(transformed_point.At<float>(0, 0), transformed_point.At<float>(1, 0), transformed_point.At<float>(2, 0));

            graph2.StructurePoints[i] = new StructurePoint(transformedPt3d);

        }

        for (int i = 0; i < diff_cams.Count; ++i)
        {
            graph1.Cams.Add(diff_cams[i]);
            int ind_cam = graph2.index(diff_cams[i]);
            graph1.IntrinsicsMats.Add(graph2.IntrinsicsMats[ind_cam]);
            graph1.ExtrinsicsMats.Add(Graph.Concat_Rt(Rt21, graph2.ExtrinsicsMats[ind_cam]));
        }

        /*
        if (true)
        {
            Mat ext_mat1 = Graph.Concat_Rt(graph2.ExtrinsicsMats[0], Rt21Inv);
            Mat ext_mat2 = Graph.Concat_Rt(graph2.ExtrinsicsMats[1], Rt21Inv);
            Mat pose1 = graph2.IntrinsicsMats[0] * ext_mat1;
            Mat pose2 = graph2.IntrinsicsMats[1] * ext_mat2;

            for (int i = 0; i < graph2.StructurePoints.Count; ++i)
            {
                Vec3f pt3d = graph2.StructurePoints[i].getCoord();
                Mat pt3dHomogeneous = new Mat(4, 1, MatType.CV_32F);
                pt3dHomogeneous.Set<float>(0, 0, pt3d.Item0);
                pt3dHomogeneous.Set<float>(1, 0, pt3d.Item1);
                pt3dHomogeneous.Set<float>(2, 0, pt3d.Item2);
                pt3dHomogeneous.Set<float>(3, 0, 1.0f);

                Mat pt1_m = pose1 * pt3dHomogeneous;
                Mat pt2_m = pose2 * pt3dHomogeneous;
                Vec3f pt1 = new Vec3f(pt1_m.At<float>(0), pt1_m.At<float>(1), pt1_m.At<float>(2));
                Vec3f pt2 = new Vec3f(pt2_m.At<float>(0), pt2_m.At<float>(1), pt2_m.At<float>(2));

                Vec2f pt1_head = new Vec2f(pt1.Item0 / pt1.Item2, pt1.Item1 / pt1.Item2);
                Vec2f pt2_head = new Vec2f(pt2.Item0 / pt2.Item2, pt2.Item1 / pt2.Item2);
                Vec2f key1 = graph2.Tracks[i][0].Pt;
                Vec2f key2 = graph2.Tracks[i][1].Pt;

                Vec2f projectedPt1 = new Vec2f(pt1[0] / pt1[2], pt1[1] / pt1[2]); // Normalized image coordinates for pt1
                Vec2f projectedPt2 = new Vec2f(pt2[0] / pt2[2], pt2[1] / pt2[2]); // Normalized image coordinates for pt2

                Vec2f dx = new Vec2f(projectedPt1.Item0 - key1.Item0 + projectedPt2.Item0 - key2.Item0,
                                    projectedPt1.Item1 - key1.Item1 + projectedPt2.Item1 - key2.Item1);

                float dotProduct = dx.Item0 * dx.Item0 +
                   dx.Item1 * dx.Item1;
                using (StreamWriter writer = new StreamWriter("Assets/Pipeline/Scripts/reprojection_error.txt", append: true))
                {
                    // Write the reprojection error to the file
                    writer.WriteLine($"reprojection error: {dotProduct}");
                }
            }
        
        }
           */

        graph1.ncams = graph1.Cams.Count;
        int ntracks = graph1.Tracks.Count;
        for (int j = 0; j < graph2.Tracks.Count; ++j)
        {
            Track track2 = graph2.Tracks[j];
            bool track_connected = false;
            for(int i = 0; i < ntracks; ++i)
            {
                Track track1 = graph1.Tracks[i];
                List<Tuple<int, int>> common_features = Track.FindOverlappingKeypoints(track1, track2);
                if (common_features.Any())
                {
                    //DO merge tracks
                    //Merge tracks doesnt actually do anything? because track 1 just goes after the loop is over

                    track2 = Graph.merge_tracks(track1, track2, common_features);
                    track_connected = true;
                    break;
                }
            }
        if (!track_connected)
            {
                graph1.Tracks.Add(track2);
                //used to be graph1.StructurePoints.Add(graph2.StructurePoints[j]) but tracks and sp had different lengths idk why though
                graph1.StructurePoints = graph2.StructurePoints;
            }
        }

        //graph1 = Triangulate(graph1);

        

    }

    public static Track merge_tracks(Track track1, Track track2, List<Tuple<int, int>> ind_key)
    {
        List<int> ind = new List<int>();
        for (int i = 0; i < ind_key.Count; ++i)
        {
            ind.Add(ind_key[i].Item2);
        }

        for (int i = 0; i < track2.Size(); ++i)
        {
            if (!ind.Contains(i))
            {
                track1.AddKP(track2[i]);
            }
        }

        return track1;
    }

    public int index(int icam)
    {
        for (int i = 0; i < ncams; ++i)
        {
            if (icam == Cams[i])
            {
                return (i);
            }
        }
        return -1;
    }


    public static Mat Concat_Rt(Mat outer_Rt, Mat inner_Rt)
    {
        Mat Rt = new Mat(3, 4, MatType.CV_32F);

        Mat outerRtBlock = outer_Rt.SubMat(0, 3, 0, 3);
        Mat innerRtBlock = inner_Rt.SubMat(0, 3, 0, 3);
        Mat resultBlock = outerRtBlock.Mul(innerRtBlock);
        resultBlock.CopyTo(Rt.SubMat(0, 3, 0, 3));

        Mat outerRtBlock1 = outer_Rt.SubMat(0, 3, 0, 3);
        Mat innerRtBlock1 = inner_Rt.SubMat(0, 3, 0, 1);
        Mat resultBlock1 = outerRtBlock1 * innerRtBlock1;
        Mat outerRtBlock2 = outer_Rt.SubMat(0, 3, 3, 4);
        Mat resultBlock2 = resultBlock1 + outerRtBlock2;
        resultBlock2.CopyTo(Rt.SubMat(0, 3, 3, 4));

        return Rt;
    }

    public static Mat Inv_Rt(Mat r_Rt)
    {
        Mat Rt_inv = new Mat(3, 4, MatType.CV_32F);

        //inverse of rotation matric
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Rt_inv.Set<float>(i, j, r_Rt.At<float>(j, i));
            }
        }

        //inverse of translation matrix
        for (int i = 0; i < 3; i++)
        {
            float sum = 0;
            for (int j = 0; j < 3; j++)
            {
                sum -= r_Rt.At<float>(j, i) * r_Rt.At<float>(j, 3);
            }
            Rt_inv.Set<float>(i, 3, sum);
        }

        return Rt_inv;
    }

    public List<Vec3f> return_points()
    {
        List<Vec3f> point_array = new List<Vec3f>();
        foreach(StructurePoint sp in StructurePoints)
        {
            point_array.Add(sp.getCoord());
        }
        return point_array;
    }

}
