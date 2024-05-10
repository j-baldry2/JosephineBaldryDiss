using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public class StructurePoint
{
    public Vec3f point;
    public Vec3i colour;

    public StructurePoint()
    {
        point = new Vec3f(0, 0, 0);
        colour = new Vec3i(0, 0, 0);
    }    
    
    public StructurePoint(Vec3f coord)
    {
        point = coord;
        colour = new Vec3i(0, 0, 0);
    }    
    
    public StructurePoint(Vec3f coord, Vec3i colour1)
    {
        point = coord;
        colour = colour1;
    }

    public Vec3f getCoord()
    {
        return point;
    }

    public Vec3i getColour()
    {
        return colour;
    }
}
