using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace KinectTracking
{

    /// <summary>
    /// 
    /// Simple Kinect Interface
    ///     
    /// 
    /// </summary>
    class Kinect
    {
        public KinectSensor kinectSensor = null;
        Skeleton[] skeletons;
        public int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        public static int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        public static double sensitivity = 1.5;
        //Cursor is the point at which the dominant hand is located. 
        //The mouse's position gets set to this point.

        /* Here we save the "player" skeleton, the current one being dominantly tracked by the Kinect.
         * Player is selected by being the first skeleton arbritrarily tracked by the kinect. It then
         * stays as this person until (s)he leaves the sensor's range, or something eclipses them from view.
         * Player will then switch to the next person being tracked. Tracking people only occurs at "near"
         * range from the kinect, so accidently picking up background persons should not be an issue.
         * */
        public static Skeleton player1 = null;
        //Assume the player is right handed. More on this later.
        public static JointType dominantHand = JointType.HandRight, dominantHand2 = JointType.HandRight;

        //This is the index of the current player.
        int p1Index = -1;
        int p2Index = -1;
        public Skeleton player2 = null;
        public bool verbose = false;
        public Vector2 p1Hand;
        public Vector2 p2Hand;

        // USE to see if the kinect is enabled
        public bool enabled
        {
            get
            {
                return kinectSensor != null;
            }
        }
        public Kinect()
        {
            //This is the code to connect to the kinect. It waits for the Kinect to be plugged in to start the mouse.
            while (kinectSensor == null)
            {
                kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
                if (kinectSensor == null)
                {
                    Console.WriteLine("Kinect not found. Please connect your Kinect to the computer and press enter.");
                    if (Console.ReadLine() == "q")
                    {
                        Environment.Exit(0);
                    }
                }
            }
            //Enables skeletons to be read, and creates a global skeletons variable to prevent constant memory allocations.
            kinectSensor.SkeletonStream.Enable();
            skeletons = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
            kinectSensor.SkeletonFrameReady += kinect_SkeletonFrameReady;
            kinectSensor.DepthStream.Range = DepthRange.Near;
            kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
            kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;//Standing Mode

            //Sets how smooth the Kinect senses the joints of the player,
            //and effectively, the mouse.
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.1f,
                Prediction = 0.5f,
                JitterRadius = 0.1f,
                MaxDeviationRadius = 0.1f
            };
            kinectSensor.SkeletonStream.Enable(parameters);


            kinectSensor.Start();
        }
        ~Kinect()
        {
            if (enabled)
                kinectSensor.Stop();
        }
        public bool p1Tracked()
        {
            return p1Index >= 0;
        }
        public bool p2Tracked()
        {
            return p2Index >= 0;
        }
        public void pause()
        {
            kinectSensor.Stop();
        }

        public void start()
        {
            kinectSensor.Start();
        }

        public void initialize(int elevationAngle = 0)
        {
            Console.WriteLine("Here.");
            while (kinectSensor == null)
            {
                kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
                if (kinectSensor == null)
                {
                    Console.WriteLine("Kinect not found. Please connect your Kinect to the computer and press enter.");
                    if (Console.ReadLine() == "q")
                    {
                        Environment.Exit(0);
                    }
                }
            }

            // limits elevation angle to keep the motors from trying too extreme an angle
            if (elevationAngle >= 26)
            {
                elevationAngle = 26;
            }
            else if (elevationAngle <= -26)
            {
                elevationAngle = -26;
            }
            // set a call back function to process skeleton data
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.1f,
                Prediction = 0.5f,
                JitterRadius = 0.1f,
                MaxDeviationRadius = 0.1f
            };

            kinectSensor.SkeletonStream.Enable(parameters);
            kinectSensor.Start();
            kinectSensor.ElevationAngle = elevationAngle;

        }

        public bool isTracked(int index)
        {
            if (index < 0) { return false; }
            if (skeletons == null || skeletons.ElementAt(index) == null)
            {
                return false;
            }
            else
            {
                return skeletons.ElementAt(index).TrackingState == SkeletonTrackingState.Tracked;
            }
        }
        public void vOut(string s)
        {
            if (verbose)
                Console.WriteLine(s);
        }

        // Process skeleton data
        public void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Honestly, I dont know. I didn't know the using keyword existed outside of #include
            using (SkeletonFrame SFrame = e.OpenSkeletonFrame())
            {
                //If skeletonOpenFrame returned something
                if (SFrame != null)
                {
                    //Set the skeletons to contain the array of skeletons from SFrame
                    skeletons = new Skeleton[SFrame.SkeletonArrayLength];
                    SFrame.CopySkeletonDataTo(skeletons);


                    //If player is no being tracked on screen
                    if (!isTracked(p1Index))
                    {
                        p1Index = -1; player1 = null;
                        vOut("Player 1 not Tracked.");
                    }

                    //If player2 is no longer on screen
                    if (!isTracked(p2Index))
                    {
                        p2Index = -1; player2 = null;
                        vOut("Player 2 not Tracked.");
                    }
                    //If one of them aren't on screen, try to find them.
                    if (p1Index == -1 || p2Index == -1)
                    {
                        for (int i = 0; i < skeletons.Count(); i++)
                        {
                            if (i != p1Index && i != p2Index && isTracked(i))
                            {
                                if (player1 == null)
                                {
                                    p1Index = i;
                                    player1 = skeletons.ElementAt(i);
                                    if (p2Index != -1) { break; }
                                }
                                else if (player2 == null)
                                {
                                    p2Index = i;
                                    player2 = skeletons.ElementAt(i);
                                    break;
                                }
                            }
                        }
                    }
                    if (p1Index >= 0)
                        player1 = skeletons.ElementAt(p1Index);
                    if (p2Index >= 0)
                        player2 = skeletons.ElementAt(p2Index);


                    vOut("Player1(" + p1Index + ") == Null:" + (player1 == null) + ", Player2(" + p2Index + ") == Null:" + (player2 == null));
                    //We want the player on the left to be player #1.
                    //This way they can control the pong paddle on the left.
                    // Or
                    //We would rather always have a  player1, because it is easiest to give only player one the
                    //controls for things like exiting, pausing, or changing options.
                    if ((player2 != null && player1 != null
                                &&
                                player2.Joints[JointType.HipCenter].Position.X < player1.Joints[JointType.HipCenter].Position.X)
                            ||
                                (player1 == null && player2 != null)
                        )
                    {
                        Skeleton tmp = player2;
                        player2 = player1;
                        player1 = tmp;

                        int temp = p1Index;
                        p1Index = p2Index;
                        p2Index = temp;
                    }
                    vOut("Player1(" + p1Index + ") == Null:" + (player1 == null) + ", Player2(" + p2Index + ") == Null:" + (player2 == null));

                    if (p1Index >= 0)
                    {
                        vOut("X");
                        //If the player's dominant hand is below the screen
                        if ((.5 - player1.Joints[dominantHand].Position.Y) * screenHeight * sensitivity > screenHeight)
                        {
                            //Switch to the player's other hand.
                            JointType offHand = dominantHand == JointType.HandRight ? JointType.HandLeft : JointType.HandRight;
                            //If the offHand is NOT below the screen, switch hands.
                            if ((.5 - player1.Joints[offHand].Position.Y) * screenHeight * sensitivity <= screenHeight)
                            {
                                dominantHand = offHand;
                            }
                        }
                        p1Hand.X = (int)((player1.Joints[dominantHand].Position.X + .5) * screenWidth * sensitivity);
                        p1Hand.Y = (int)((.5 - player1.Joints[dominantHand].Position.Y) * screenHeight * sensitivity);

                        //System.Windows.Forms.Cursor.Position = cursor;

                        vOut("Player 1: " + p1Hand.X + ", " + p1Hand.Y);
                    }
                    else
                    {
                        //Console.WriteLine("Nobody Tracked.");
                        p1Hand.X = -9001;
                    }

                    //check if there is a player 2 being tracked
                    if (p2Index >= 0)
                    {

                        //If the player's dominant hand is below the screen
                        if ((.5 - player2.Joints[dominantHand2].Position.Y) * screenHeight * sensitivity > screenHeight)
                        {
                            //Switch to the player2's other hand.
                            JointType offHand = dominantHand2 == JointType.HandRight ? JointType.HandLeft : JointType.HandRight;
                            //If the offHand is NOT below the screen, switch hands.
                            if ((.5 - player2.Joints[offHand].Position.Y) * screenHeight * sensitivity <= screenHeight)
                            {
                                dominantHand2 = offHand;
                            }
                        }
                        //cant have the mouse spazzing out
                        p2Hand.X = (int)((player2.Joints[dominantHand2].Position.X + .5) * screenWidth * sensitivity);
                        p2Hand.Y = (int)((.5 - player2.Joints[dominantHand2].Position.Y) * screenHeight * sensitivity);
                        //cant have mouse spazzing but lets keep track of positions
                        //System.Windows.Forms.cursor2.Position = cursor2;

                        vOut("Player2: " + p2Hand.X + ", " + p2Hand.Y);
                    }
                    else
                    {
                        p2Hand.X = -9001;
                        //Console.WriteLine("Nobody 2 Tracked.");
                    }
                }
            }
        }
    }
}
