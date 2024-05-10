/*using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class FrameExtraction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string videoPath = "path/to/video.mp4";
        string framesDir = "path/to/frames";
        int savedFrames = ExtractFrames(videoPath, framesDir);

        Console.WriteLine($"Saved {savedFrames} frames.");
    }

    static int ExtractFrames(string videoPath, string framesDir, bool overwrite = false, int start = -1, int end = -1, int every = 1)
    {
        videoPath = Path.GetFullPath(videoPath);
        framesDir = Path.GetFullPath(framesDir);

        string videoDir = Path.GetDirectoryName(videoPath);
        string videoFilename = Path.GetFileName(videoPath);

        if (!File.Exists(videoPath))
            throw new FileNotFoundException("Video file not found.", videoPath);

        using var capture = new VideoCapture(videoPath);

        start = start < 0 ? 0 : start;
        end = end < 0 ? (int)capture.Get(VideoCaptureProperties.FrameCount) : end;

        capture.Set(VideoCaptureProperties.PosFrames, start);
        int frame = start;
        int whileSafety = 0;
        int savedCount = 0;

        Mat image = new Mat();
        while (frame < end)
        {
            capture.Read(image);

            if (whileSafety > 500)
                break;

            if (image.Empty())
            {
                whileSafety++;
                continue;
            }

            if (frame % every == 0)
            {
                whileSafety = 0;
                string savePath = Path.Combine(framesDir, videoFilename, $"{frame:D10}.jpg");

                if (!File.Exists(savePath) || overwrite)
                {
                    Cv2.ImWrite(savePath, image);
                    savedCount++;
                }
            }

            frame++;
        }

        return savedCount;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
*/