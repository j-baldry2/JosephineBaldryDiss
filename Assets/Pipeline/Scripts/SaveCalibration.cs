using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class SaveCalibration : WebCamera
{
    private Mat image;
    private bool bRecord = false;

    private float nextActionTime = 0.0f;
    public float period = 0.1f;

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        image = OpenCvSharp.Unity.TextureToMat(input);

        if (bRecord)
        {
            Debug.Log("pretend im saving rn");
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("capture_{0}.bmp", timeStamp);
            string filepath = System.IO.Path.Combine("Assets/Pipeline/Calibrations", filename);
            filepath = filepath.Replace("/", @"\");
            Cv2.ImWrite(filepath, image);
        }

        if (output == null)
            output = OpenCvSharp.Unity.MatToTexture(image);
        else
            OpenCvSharp.Unity.MatToTexture(image, output);

        return true;
    }

    public void Capture()
    {
        bRecord = true;
    }

    public void Stop_capture()
    {
        bRecord = false;
    }

}

/*
 * TODO: Hook this up to the button  - done
 * When button pressed - done
 * Save the image as a frame. - done
 * slow it down lmao - idk how to do this
 * */

/*
 * Adaprted from
 * https://discussions.unity.com/t/execute-code-every-x-seconds-with-update/3626/2
 * feature extraction - boilerplate 
 * image capture - path code 
 * */