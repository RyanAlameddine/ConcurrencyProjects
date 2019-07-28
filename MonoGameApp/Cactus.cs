using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MonoGameApp.Sync.Coroutine;

namespace MonoGameApp
{
    public class Cactus
    {
        public const int startHeight = 450;

        public Vector2 position = new Vector2(800, startHeight);
        public float scale;
        public Color color = Color.Green;
        public SpriteFont font;
        public string text = "cactus";
        public bool big = false;
        public bool huge = false;

        public Rectangle HitBox;
        public Rectangle BigBox;
        public Rectangle HugeBox;

        Vector2 bigCactusSettings;
        Vector2 hugeCactusSettings;

        public Cactus(SpriteFont font, float scale, Random random)
        {
            this.font = font;
            this.scale = scale;

            color = new Color((float) random.NextDouble() * 0.25f, (float) random.NextDouble() * .3f + .25f, (float) random.NextDouble() * .1f);

            float x = random.NextDouble() > 0.5 ? 35 : -35;
            bigCactusSettings = new Vector2(x, (float) (random.NextDouble() * -100 - 30));
            hugeCactusSettings = new Vector2(-x, (float)(random.NextDouble() * -100 - 30));
            if (scale > 1.9)
            {
                big = true;
                if(random.NextDouble() > 0.8)
                {
                    huge = true;
                    text = "HUGECACTUS";
                }
                else
                {
                    text = "BIGCACTUS";
                }
            }

            StartCoroutine(async () =>
            {
                while (position.X > -100)
                {
                    position.X -= 5.5f * Game1.Speed;
                    await Task.Yield();
                }
            });
        }

        public void UpdateHitBox()
        {
            Vector2 stringMeasure = font.MeasureString(text);
            float tmpX = stringMeasure.X * scale / 2;
            stringMeasure.X = stringMeasure.Y * scale / 2;
            stringMeasure.Y = tmpX;
            HitBox = new Rectangle((int)position.X, (int)(position.Y - stringMeasure.Y), (int)stringMeasure.X, (int) stringMeasure.Y);


            stringMeasure = font.MeasureString("cactusarm");
            tmpX = stringMeasure.X * scale / 4;
            stringMeasure.X = stringMeasure.Y * scale / 4;
            stringMeasure.Y = tmpX;
            float newX;
            if (bigCactusSettings.X > 0)
            {
                newX = bigCactusSettings.X + 30;
            }
            else
            {
                newX = bigCactusSettings.X - 20;
            }
            BigBox = new Rectangle((int) (position.X + newX), (int)(position.Y + bigCactusSettings.Y + 10 - stringMeasure.Y), (int)stringMeasure.X, (int)stringMeasure.Y);

            stringMeasure = font.MeasureString("cactusarm");
            tmpX = stringMeasure.X * scale / 4;
            stringMeasure.X = stringMeasure.Y * scale / 4;
            stringMeasure.Y = tmpX;
            if (hugeCactusSettings.X > 0)
            {
                newX = hugeCactusSettings.X + 30;
            }
            else
            {
                newX = hugeCactusSettings.X - 20;
            }
            HugeBox = new Rectangle((int)(position.X + newX), (int)(position.Y + hugeCactusSettings.Y + 10 - stringMeasure.Y), (int)stringMeasure.X, (int)stringMeasure.Y);

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            /*
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { Color.Red });
            spriteBatch.Draw(pixel, BigBox, Color.White);
            spriteBatch.Draw(pixel, HugeBox, Color.White);
            */

            spriteBatch.DrawString(font, text, position, color, -MathHelper.PiOver2, Vector2.Zero, scale / 2, SpriteEffects.None, 0);
            if (big)
            {
                spriteBatch.DrawString(font, "-big", position + new Vector2(bigCactusSettings.X, bigCactusSettings.Y), color, 0, Vector2.Zero, scale / 4, SpriteEffects.None, 0);
                float newX;
                if (bigCactusSettings.X > 0)
                {
                    newX = bigCactusSettings.X + 30;
                }
                else
                {
                    newX = bigCactusSettings.X - 20;
                }
                spriteBatch.DrawString(font, "cactusarm", position + new Vector2(newX, bigCactusSettings.Y + 10), color, -MathHelper.PiOver2, Vector2.Zero, scale / 4, SpriteEffects.None, 0);

                
            }

            if (huge)
            {
                float newX;
                spriteBatch.DrawString(font, "huge", position + new Vector2(hugeCactusSettings.X, hugeCactusSettings.Y), color, 0, Vector2.Zero, scale / 4, SpriteEffects.None, 0);
                if (hugeCactusSettings.X > 0)
                {
                    newX = hugeCactusSettings.X + 30;
                }
                else
                {
                    newX = hugeCactusSettings.X - 20;
                }
                spriteBatch.DrawString(font, "cactusarm", position + new Vector2(newX, hugeCactusSettings.Y + 10), color, -MathHelper.PiOver2, Vector2.Zero, scale / 4, SpriteEffects.None, 0);
            }



        }
    }
}
