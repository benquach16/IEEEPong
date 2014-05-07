#region File Description
//-----------------------------------------------------------------------------
// SpinningTriangleControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion
using WongPong;
namespace WinFormsGraphicsDevice
{
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to draw animating
    /// 3D graphics inside a WinForms application. It hooks the Application.Idle
    /// event, using this to invalidate the control, which will cause the animation
    /// to constantly redraw.
    /// </summary>
    class GraphicsHandler : GraphicsDeviceControl
    {
        //initialize some varaibles here
        
        BasicEffect effect;
        Stopwatch timer;
        SpriteBatch spriteBatch;
        public ContentManager Content;
  
        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
            // Create our effect.
            effect = new BasicEffect(GraphicsDevice);
            effect.VertexColorEnabled = true;


            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };

            spriteBatch = new SpriteBatch(GraphicsDevice);


            ServiceContainer services = new ServiceContainer();
            Content = new ContentManager(Services, "Content");

            MainForm.player1 = new Player(Content, Microsoft.Xna.Framework.Color.Red, Microsoft.Xna.Framework.Color.IndianRed, 1); //make player1
            MainForm.player2 = new Player(Content, Microsoft.Xna.Framework.Color.Blue, Microsoft.Xna.Framework.Color.CadetBlue, 2); //make player2


            Texture2D ballTex = Content.Load<Texture2D>("Artwork/ball");
            MainForm.ball = new Ball(Content);
            MainForm.ball.particleTexture = Content.Load<Texture2D>("Artwork/ball_particle");

            //MainForm.hud.LoadContent(Content, GraphicsDevice);

            //TODO: REPLACE THIS WITH NON SHITTY FONT
            SpriteFont font = Content.Load<SpriteFont>("defaultFont");

            // Start the animation timer.
            MainForm.hud = new HUD(Content, GraphicsDevice);
            

            timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            float aspect = GraphicsDevice.Viewport.AspectRatio;
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);
            // Set renderstates.
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            spriteBatch.Begin();
            MainForm.hud.Draw();                     //draw the hud
            MainForm.player1.Draw(spriteBatch, MainForm.p1Tracked, MainForm.p1hit); //draw the players
            MainForm.player2.Draw(spriteBatch, MainForm.p2Tracked, MainForm.p2hit); //draw the players
            MainForm.ball.Draw(spriteBatch, MainForm.ballhit); //draw the ball
            
            spriteBatch.End();
        }
    }
}
