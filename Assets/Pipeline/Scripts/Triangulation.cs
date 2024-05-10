using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class Triangulation
{
    public Graph triangulate_nonlinear_G(Graph graph)
    {
        int ntracks = graph.Tracks.Count;
        for (int i = 0; i < ntracks; ++i)
        {
            int nkeys = graph.Tracks[i].Size();
            List<Mat> poses = new List<Mat>();
            List<Vec2f> points = new List<Vec2f>();
            for (int j = 0; j < nkeys; ++j)
            {
                KeyPoint key = graph.Tracks[i][j];
                //In kai wus he uses keypoint.index but i cant find any way to work out what the index is
                //SO im just going to try j, and this could be major problem lmao
                int ind_cam = graph.index(j);
                poses.Add(graph.IntrinsicsMats[ind_cam] * graph.ExtrinsicsMats[ind_cam]);
                points[j] = key.Pt;
            }
        }
        return graph;
    }

    public Pair triangulate_nonlinear_P(List<Mat> poses, List<Vec2f> points, Vec3f pt3d)
    {
        Pair pair = new Pair();

        return pair;
    }
}
