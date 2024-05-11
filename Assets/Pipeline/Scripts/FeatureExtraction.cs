using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class FeatureExtraction : WebCamera
{
    //Pre processing
    private Mat image;
    private Mat compImage;
    private Mat processImageL = new Mat();
    private Mat processImageC = new Mat();

    //ORB
    private KeyPoint[] keyPoints1;
    private KeyPoint[] keyPoints2;
    private Mat descriptor1 = new Mat();
    private Mat descriptor2 = new Mat();

    //Matching
    private DMatch[] bfMatches;
    private Mat bfView = new Mat();

    //Lowes Ratio
    private InputArray ratioKP1;
    private InputArray ratioKP2;

    //Ransac and FM estimation
    private Mat fundamentalMatrix = new Mat();


    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        //dead code. would be used for real time pc gen
        image = OpenCvSharp.Unity.TextureToMat(input);
        compImage = Cv2.ImRead(@"Assets/Pipeline/Scripts/previmg.bmp", ImreadModes.Color);
        Cv2.ImWrite(@"Assets/Pipeline/Scripts/previmg.bmp", image);


        if (compImage != null)
        {
        to_greyscale();
        orb();
        brute_force_match();
        }


        if (output == null)
            output = OpenCvSharp.Unity.MatToTexture(bfView);
        else
            OpenCvSharp.Unity.MatToTexture(bfView, output);
        
        return true;
    }

    private void to_greyscale()
    {
        Cv2.CvtColor(image, processImageL, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(compImage, processImageC, ColorConversionCodes.BGR2GRAY);
    }

    private void orb()
    {
        ORB fast = ORB.Create();

        fast.DetectAndCompute(processImageL, null, out keyPoints1, descriptor1, false);
        fast.DetectAndCompute(processImageC, null, out keyPoints2, descriptor2, false);
    }



    private void brute_force_match()
    {
        var bfMatcher = new BFMatcher(NormTypes.L2, false);
        bfMatches = bfMatcher.Match(descriptor1, descriptor2, null);
        //Cv2.DrawMatches(image, keyPoints1, compImage, keyPoints2, bfMatches, bfView);
    }

    private void ratio_test()
    {
        /*
       for (keyPoint m, keyPoint n in bfMatches)
        {
            Debug.Log("hi")
        }
        */
    }

    private void ransac()
    {
        /*
        //https://shimat.github.io/opencvsharp_docs/html/26e45900-fb4e-cd0a-8979-9f47d00b6187.htm
        fundamentalMatrix = Cv2.FindFundamentalMat(keyPoints1, keyPoints2);
        //Cv2.DrawMatches(image, keyPoints1, compImage, keyPoints2, E, bfView);
        */
    }

}

/* Adapted from
 * https://www.youtube.com/watch?v=ZV5eejYG6NI&ab_channel=MattBell
 * and
 * https://docs.opencv.org/4.x/da/df5/tutorial_py_sift_intro.html
 */

//Technically runs atm but it doesnt really work - if you point it at a random object it will still show matches
//might try flann
//and then get it to run on the previous frame