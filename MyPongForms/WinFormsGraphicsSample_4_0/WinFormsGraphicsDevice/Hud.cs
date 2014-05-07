using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinFormsGraphicsDevice;

namespace WongPong {
    public class HUD {

        public int p1score,p2score;
        private string p1string, p2string;
        public Color textColor1,textColor2;
        public float scale1, scale2;

        //These are the positions of the score being displayed.
        public Vector2 text1pos, text2pos;
        //This is the offset vector, which causes the screen to shake.
        public Vector2 offset;

        public Texture2D dummyTexture;
        public List<Rectangle> middle_line;
        private SpriteBatch spriteBatch;
        public SpriteFont spritefont;

        //particle constructor
        public HUD(ContentManager content, GraphicsDevice graphicsdevice) {

            //set primary attributes
            int rec_width = 8;
            int rec_height = 33;
            text1pos = new Vector2(MainForm.ScreenWidth * .25f, MainForm.ScreenHeight * .05f);
            text2pos = new Vector2(MainForm.ScreenWidth * .75f, MainForm.ScreenHeight * .05f);
            offset = new Vector2(0, 0);
            //Set other attributes
            middle_line = new List<Rectangle>();
            scale1 = 1.0f; scale2 = 1.0f;
            textColor1 = Color.White; textColor2 = Color.White;
            p1score = 0; p1string = "0";
            p2score = 0; p2string = "0";
            for (int i = 0; i < MainForm.ScreenHeight; i += rec_height + rec_height / 3) {
                Rectangle r = new Rectangle(MainForm.ScreenWidth / 2 - rec_width / 2, i, rec_width, rec_height);
                middle_line.Add(r);
            }

            LoadContent(content, graphicsdevice);
        }
        
        //Load Content Method
        public void LoadContent(ContentManager content, GraphicsDevice graphicsdevice) {

            spritefont = content.Load<SpriteFont>("SpriteFonts/MyFont1");
            spriteBatch = new SpriteBatch(graphicsdevice);
            dummyTexture = new Texture2D(graphicsdevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });

           
        }

        //Update method
        public void Update(int gametime) {
            p1string = "" + p1score;
            p2string = "" + p2score;
          
        }

        //Draw method
        public void Draw() {
            spriteBatch.Begin();
            for (int i = 0; i < middle_line.Count(); i++)
            {
                spriteBatch.Draw(dummyTexture, middle_line[i], null, Color.White, 0f, offset/10, SpriteEffects.None, 0);
            }
            spriteBatch.DrawString(spritefont, p1string, text1pos, textColor1, 0f, offset, scale1, SpriteEffects.None, 0f);
            spriteBatch.DrawString(spritefont, p2string, text2pos, textColor2, 0f, offset, scale2, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

    }
}
