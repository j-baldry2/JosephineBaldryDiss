using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OpenCvSharp;


public class Track
{
    public List<KeyPoint> keys;

   public Track()
    {
        keys = new List<KeyPoint>();
    }
    public Track(Track track)
    {
        keys = new List<KeyPoint>(track.keys);
    }    
    
    public Track(List<KeyPoint> kps)
    {
        keys = new List<KeyPoint>(kps);
    }

    public void AddKP(KeyPoint key)
    {
        keys.Add(key);
    }

    public void RmKP(int index)
    {
        keys.RemoveAt(index);
    }

    public KeyPoint this[int index]
    {
        get { return keys[index]; }
        set { keys[index] = value; }
    }

    public int Size()
    {
        return keys.Count;
    }

    public static bool HasOverlappingKeypoints(Track track1, Track track2)
    {
        foreach (var key1 in track1.keys)
        {
            foreach (var key2 in track2.keys)
            {
                if (key1 == key2)
                    return true;
            }
        }
        return false;
    }

    public static List<Tuple<int, int>> FindOverlappingKeypoints(Track track1, Track track2)
    {
        var indKey = new List<Tuple<int, int>>();
        for (int i = 0; i < track1.Size(); ++i)
        {
            for (int j = 0; j < track2.Size(); ++j)
            {
                //if (KeyPoint.Equality(track1[i], track2[j]))
                if (track1[i] == track2[j])
                {
                    indKey.Add(Tuple.Create(i, j));
                }
            }
        }
        return indKey;
    }

}
