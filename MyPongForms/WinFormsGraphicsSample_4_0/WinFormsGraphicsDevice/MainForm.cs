#region File Description
//-----------------------------------------------------------------------------
// MainForm.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Windows.Forms;
#endregion

namespace WinFormsGraphicsDevice
{
    // System.Drawing and the XNA Framework both define Color types.
    // To avoid conflicts, we define shortcut names for them both.
    using GdiColor = System.Drawing.Color;
    using XnaColor = Microsoft.Xna.Framework.Color;
    using System.Diagnostics;
    using System;
    using System.Timers;
    using Microsoft.Kinect;
    using KinectTracking;
    using WongPong;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    /// <summary>
    /// Custom form provides the main user interface for the program.
    /// In this sample we used the designer to add a splitter pane to the form,
    /// which contains a SpriteFontControl and a SpinningTriangleControl.
    /// </summary>
    public partial class MainForm : Form
    {
        System.Timers.Timer updater;
        public static Player player1; //make player1
        public static Player player2; //make player2
        public static Ball ball;         //make the ball
        public static HUD hud;            //make the Huds up display


        public static int ScreenWidth = 2560;
        public static int ScreenHeight = 2048;

        //other axullairy stuff
        bool wallhit = false;
        public static bool p1hit = false;
        public static bool p2hit = false;
        public static bool ballhit = false;
        public static bool p1Tracked = false;
        public static bool p2Tracked = false;
        bool p2JustScored = false;
        bool p1JustScored = false;
        private KinectTracking.Kinect kinect;
        public const int gameTime = 16;
        //timers
        int hittimer = 0;
        int wallhittimer = 0;
        int roundtimer = 0;
        int justscoredtimer = 0;
        bool pauseOn = true;

        public MainForm()
        {
            InitializeComponent();

            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            WindowState = System.Windows.Forms.FormWindowState.Maximized;

            /*
            //This code will produce some LSD results, do not uncomment
            ScreenWidth = Screen.FromControl(this).Bounds.X;
            ScreenHeight = Screen.FromControl(this).Bounds.Y;
            */

            updater = new System.Timers.Timer(gameTime);
            updater.Elapsed += new ElapsedEventHandler(Update);
            kinect = new Kinect();  //Make kinect object

            updater.Start();

        }

        void Update(object sender, ElapsedEventArgs e)
        {
            if (hud != null)
            {
                //TODO: Add in Exit Detection
                player1.Update(gameTime, kinect.p1Hand);    //update the player 1
                //TODO: Switch back to kinect.p2Hand
                player2.Update(gameTime, kinect.p2Hand);    //update player 2
                p1Tracked = kinect.p1Tracked();
                p2Tracked = kinect.p2Tracked();
                if (p2Tracked)//TODO: Switch back to p2Tracked!
                    ball.Update(gameTime, pauseOn);       //update the ball
                manage_collisions();         //do collision logic
                Score_Check();              //check & update scores
                hud.Update(gameTime);        //update the hud
                do_animations(gameTime);    //do all game animations

                hittimer++; roundtimer++; justscoredtimer++; wallhittimer++;
            }
        }

        private void manage_collisions()
        {
            Random rand = new Random();
            if (p1hit && hittimer > 10) p1hit = false;
            if (p2hit && hittimer > 10) p2hit = false;
            if (ballhit && (hittimer > 10 || wallhittimer > 10)) ballhit = false;
            if (wallhit && wallhittimer > 15)
            {
                hud.offset.X = 0; 
                hud.offset.Y = 0;
                wallhit = false;
            }
            //ball collides with player #1
            if (!p1hit && ball.boundingBox.Intersects(player1.boundingBox))
            {
                ball.velocity.X = rand.Next(12, 20);
                ball.velocity.Y = rand.Next(-40, 40) / 10f;
                if (player1.velocity == 0)
                {
                    ball.velocity.X += 10;
                    ball.velocity.Y += rand.Next(-3, 3);
                }
                else if (player1.velocity > 8 || player1.velocity < -8)
                {
                    ball.velocity.Y = player1.velocity * .75f;
                    ball.velocity.X += 3;
                }
                /*
                ball.velocity.X = rand.Next(6, 10);
                ball.velocity.Y = rand.Next(-20, 20) / 10f;
                if (player1.velocity == 0)
                {
                    ball.velocity.X += 12;
                    ball.velocity.Y += rand.Next(-2, 2);
                }
                else if (player1.velocity > 8 || player1.velocity < -8)
                {
                    ball.velocity.Y = player1.velocity * .75f;
                    ball.velocity.X += 3;
                }*/

                p1hit = true; ballhit = true; hittimer = 0; ball.PaddleHit(player1.color);
            }

            //ball collides with player #2
            else if (!p2hit && ball.boundingBox.Intersects(player2.boundingBox))
            {
                ball.velocity.X = rand.Next(-20, -12);
                ball.velocity.Y = rand.Next(-40, 40) / 10f;
                if (player2.velocity == 0)
                {
                    ball.velocity.X -= 10;
                    ball.velocity.Y += rand.Next(-3, 3);
                }
                else if (player2.velocity > 8 || player2.velocity < -8)
                {
                    ball.velocity.Y = player2.velocity * .75f;
                    ball.velocity.X -= 3;
                }
                p2hit = true; ballhit = true; hittimer = 0; ball.PaddleHit(player2.color);
            }

            //ball colides with wall coliisions and act accordingly
            else if (ball.position.X > ScreenWidth - ball.texture.Width || ball.position.X < 0)
            { ball.velocity.X *= -1; }
            else if (!wallhit && (ball.position.Y > ScreenHeight - ball.texture.Height || ball.position.Y < 0))
            {
                ball.velocity.Y *= -1;
                ball.PaddleHit(Color.White);
                wallhit = true; ballhit = true; wallhittimer = 0;

            }

              //ball doesnt collide with player
            else
            {
            }
        }
        private void do_animations(int gameTime)
        {

            //screen hud shake when something
            if (wallhit && wallhittimer < 5)
            {
                /*List<Rectangle> r = new List<Rectangle>();
                for (int i = 0; i < hud.middle_line.Count(); i++) {
                    r.Add(new Rectangle(
                        (int)linear_tween((float)wallhittimer / 5f, hud_recs[i].X, hud_recs[i].X + 10),
                        (int)linear_tween((float)wallhittimer / 5f, hud_recs[i].Y, hud_recs[i].Y +10),
                        hud.middle_line[i].Width,hud.middle_line[i].Height));
                }
                hud.middle_line = r;
                hud.position1.X = (int)linear_tween((float)wallhittimer / 5f, hud_text1pos.X, hud_text1pos.X + 10);
                hud.position1.Y = (int)linear_tween((float)wallhittimer / 5f, hud_text1pos.Y, hud_text1pos.Y + 10);
                hud.position2.X = (int)linear_tween((float)wallhittimer / 5f, hud_text2pos.X, hud_text2pos.X + 10);
                hud.position2.Y = (int)linear_tween((float)wallhittimer / 5f, hud_text2pos.Y, hud_text2pos.Y + 10);*/
                hud.offset.X += 1;
                hud.offset.Y += 1;
                ball.scale = linear_tween((float)wallhittimer / 5f, 1, 1.3f);
            }
            else if (wallhit && wallhittimer < 10)
            {
                /*List<Rectangle> r = new List<Rectangle>();
                for (int i = 0; i < hud.middle_line.Count; i++)
                {
                    r.Add(new Rectangle(
                        (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_recs[i].X + 10, hud_recs[i].X - 10),
                        (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_recs[i].Y + 10, hud_recs[i].Y - 10),
                        hud.middle_line[i].Width, hud.middle_line[i].Height));
                }
                hud.middle_line = r;
                hud.position1.X = (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_text1pos.X + 10, hud_text1pos.X - 10);
                hud.position1.Y = (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_text1pos.Y + 10, hud_text1pos.Y - 10);
                hud.position2.X = (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_text2pos.X + 10, hud_text2pos.X - 10);
                hud.position2.Y = (int)linear_tween((float)(wallhittimer - 5) / 5f, hud_text2pos.Y + 10, hud_text2pos.Y - 10);*/
                hud.offset.X -= 2;
                hud.offset.Y -= 2;
                ball.scale = linear_tween((float)(wallhittimer - 5) / 5f, 1.3f, 1f);
            }
            else if (wallhit && wallhittimer < 15)
            {
                /*List<Rectangle> r = new List<Rectangle>();
                for (int i = 0; i < hud.middle_line.Count; i++)
                {
                    r.Add(new Rectangle(
                        (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_recs[i].X - 10, hud_recs[i].X),
                        (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_recs[i].Y - 10, hud_recs[i].Y),
                        hud.middle_line[i].Width, hud.middle_line[i].Height));
                }
                hud.middle_line = r;
                hud.position1.X = (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_text1pos.X - 10, hud_text1pos.X);
                hud.position1.Y = (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_text1pos.Y - 10, hud_text1pos.Y);
                hud.position2.X = (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_text2pos.X - 10, hud_text2pos.X);
                hud.position2.Y = (int)linear_tween((float)(wallhittimer - 10) / 5f, hud_text2pos.Y - 10, hud_text2pos.Y);*/
                hud.offset.X += 1;
                hud.offset.Y += 1;
            }


            //Paddle & ball scaling when hit
            if (p2hit & hittimer < 5) player2.scale = linear_tween((float)hittimer / 5f, 1, 1.5f);
            else if (p2hit) player2.scale = linear_tween((float)(hittimer - 5) / 5f, 1.5f, 1);
            if (p1hit & hittimer < 5) player1.scale = linear_tween((float)hittimer / 5f, 1, 1.5f);
            else if (p1hit) player1.scale = linear_tween((float)(hittimer - 5) / 5f, 1.5f, 1);
            if ((p2hit || p1hit) && hittimer < 5) ball.scale = linear_tween((float)hittimer / 5f, 1, 1.3f);
            else if ((p2hit || p1hit)) ball.scale = linear_tween((float)(hittimer - 5) / 5f, 1.3f, 1f);

            //start of round, scale ball
            if (roundtimer <= 1) { }
            else if (roundtimer < 25) ball.scale = linear_tween((float)roundtimer / 25f, 10.5f, .15f);
            else if (roundtimer < 50) ball.scale = linear_tween((float)(roundtimer - 25) / 25f, .15f, 4f);
            else if (roundtimer < 75) ball.scale = linear_tween((float)(roundtimer - 50) / 25f, 4f, 1f);
            else pauseOn = false;

            //When a person scores, make animiation
            if (!ball.isVisible && justscoredtimer < 1)
            {
                if (p1JustScored) { hud.scale1 = 2.5f; hud.textColor1 = Color.Green; }
                else if (p2JustScored) { hud.scale2 = 2.5f; hud.textColor2 = Color.LawnGreen; }
            }
            else if (!ball.isVisible && justscoredtimer < 20)
            {
                if (p1JustScored) hud.scale1 = linear_tween((float)(justscoredtimer) / 20f, 3.5f, 1f);
                else if (p2JustScored) hud.scale2 = linear_tween((float)(justscoredtimer) / 20f, 3.5f, 1f);
            }
            else if (!ball.isVisible) { hud.textColor1 = Color.White; hud.textColor2 = Color.White; }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            hud.Draw();                     //draw the hud
            player1.Draw(spriteBatch, kinect.p1Tracked(), p1hit); //draw the players
            player2.Draw(spriteBatch, kinect.p1Tracked(), p2hit); //draw the players
            ball.Draw(spriteBatch, ballhit); //draw the ball

            spriteBatch.End();
        }

        //function that checks if ball hits score line
        private void Score_Check()
        {

            //if ball passes player 1's goal
            if (ball.position.X < 5)
            {
                ball.Kill();
                hud.p2score++;
                p2JustScored = true;
                justscoredtimer = 0;
            }

            //if ball passes player 2's goal
            if (ball.position.X > ScreenWidth - ball.texture.Width - 5)
            {
                ball.Kill();
                hud.p1score++;
                p1JustScored = true;
                justscoredtimer = 0;
            }

            //if ball has been destroyed (no particles)
            if (ball.particles.Count == 0 && !pauseOn)
            {
                ball.Reset();
                if (p2JustScored) { ball.position = new Vector2(ScreenWidth * .9f, player2.position.Y + player2.texture.Height / 2); p2JustScored = false; ball.velocity.X *= -1; }
                else if (p1JustScored) { ball.position = new Vector2(ScreenWidth * .1f, player1.position.Y + player1.texture.Height / 2); p1JustScored = false; }
                roundtimer = 0;
                hud.textColor1 = Color.White;
                pauseOn = true;
            }

        }

        //helper linear tweeining animation
        private float linear_tween(float t, float start, float end)
        {
            if (t > 1.0f) return end;
            return t * end + (1.0f - t) * start;
        }
    }
}
