using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using static MonoGameApp.Sync.Coroutine;

namespace MonoGameApp
{
    public class Dino
    {
        public const int startHeight = 400;

        public Vector2 position;
        public Color color;
        public SpriteFont dinoFont;
        public string text = "DINO";

        private float velocity = 0;
        public int accellerationcount = 0;
        public bool gameover = false;
        Rectangle hitBox;

        private Color gameOverColor = Color.Black;
        private string gameOverText = "-----Game Over-----";

        public float gravity = 1f;
        public bool down = false;

        public Dino(Vector2 position, SpriteFont dinoFont, Color color)
        {
            this.position = position;
            this.color = color;
            this.dinoFont = dinoFont;

            StartCoroutine(async () =>
            {
                while (true)
                {
                    if (!gameover)
                    {
                        Game1.Score += 10 + 10*(Game1.Speed-2);
                    }

                    text = text.Substring(1) + text[0];
                    if (Game1.Speed == 0)
                    {
                        break;
                    }
                    int awaits = (int)(1f / Game1.Speed);
                    for (int i = -1; i < awaits; i++)
                    {
                        await Task.Yield();
                    }
                    await Task.Yield();
                    await Task.Yield();
                    await Task.Yield();
                    await Task.Yield();
                    await Task.Yield();
                }
            });


        }

        public void Update(GameTime gameTime, List<Cactus> cacti)
        {
            Vector2 stringMeasure = dinoFont.MeasureString(text[0].ToString());
            hitBox = new Rectangle((int)position.X, (int)position.Y, (int) (stringMeasure.X / 2 * .5), (int) (stringMeasure.Y / 2 * .5));
            position.Y += velocity;

            if (position.Y >= startHeight)
            {
                velocity = 0;
                position.Y = startHeight;
                accellerationcount = 0;
                scoreCost = 100;
            }
            else if(position.Y < startHeight)
            {
                velocity += gravity;
                if (down)
                {
                    velocity += gravity;
                }
            }

            if(position.Y < 0)
            {
                position.Y = 0;
                velocity = -velocity/3;
            }

            foreach(Cactus cactus in cacti)
            {
                cactus.UpdateHitBox();
                if ((cactus.HitBox.Intersects(hitBox) || (cactus.big && cactus.BigBox.Intersects(hitBox)) || (cactus.huge && cactus.HugeBox.Intersects(hitBox))) && !gameover)
                {
                    gameover = true;
                    Game1.soundEffects[2].Play(1, 0, 0);
                    StartCoroutine(async () =>
                    {
                        await Game1.TcpClient.ConnectAsync("192.168.1.172", 9999);
                        NetworkStream stream = Game1.TcpClient.GetStream();

                        byte[] buffer = BitConverter.GetBytes((int) Game1.Score);
                        await stream.WriteAsync(buffer, 0, 4);
                        await stream.ReadAsync(buffer, 0, 4);
                        Game1.HighScore = BitConverter.ToInt32(buffer, 0);
                        byte[] str = new byte[15];
                        await stream.ReadAsync(str, 0, 15);
                        int count = 0;
                        for (int i = 0; i < 15; i++)
                        {
                            if (str[i] != 0)
                            {
                                count++;
                            }
                        }
                        Game1.IpAddr = Encoding.UTF8.GetString(str, 0, count);

                        Game1.TcpClient.Close();
                    });
                    StartCoroutine(async () =>
                    {
                        while (true)
                        {
                            gameOverText = gameOverText[gameOverText.Length - 1] + gameOverText.Substring(0, gameOverText.Length - 1);
                            await Task.Delay(100);
                        }
                    });
                    StartCoroutine(async () =>
                    {
                        while (Game1.Speed > 0.001)
                        {
                            Game1.Speed = Game1.Speed + .01f * (0 - Game1.Speed);
                            Game1.Background = Color.Lerp(Game1.Background, Color.DarkRed, 0.02f);
                            Game1.FloorColor = Color.Lerp(Game1.FloorColor, Color.DarkRed, 0.02f);
                            color = Color.Lerp(color, Color.DarkRed, 0.02f);
                            foreach(Cactus cact in cacti)
                            {
                                cact.color = Color.Lerp(cact.color, Color.DarkRed, 0.02f);
                            }
                            
                            await Task.Yield();
                        }
                        Game1.Speed = 0;
                        color = Color.DarkRed;
                    });
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(dinoFont, text[0].ToString(), position, color, 0f, Vector2.Zero, .5f, SpriteEffects.None, 0);

            if (gameover)
            {
                spriteBatch.DrawString(dinoFont, gameOverText, new Vector2(230, 100), gameOverColor, 0f, Vector2.Zero, .5f, SpriteEffects.None, 0);
            }

            /*
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { Color.Red });
            spriteBatch.Draw(pixel, hitBox, Color.White);
            */
        }

        int scoreCost = 100;
        private void scoreSell()
        {
            if (!gameover)
            {
                Game1.Score -= scoreCost;
                Debug.WriteLine(scoreCost);
                scoreCost += 100;
            }
        }

        public bool Jumping = false;
        public void Jump()
        {
            if (accellerationcount < 7)
            {
                accellerationcount++;
                velocity = -10;
            }
            else
            {
                if (!Jumping)
                {
                    if (Game1.Score >= scoreCost)
                    {
                        scoreSell();
                        accellerationcount = 100;
                        velocity = -10;
                    }
                }
            }
            Jumping = true;
        }
    }
}
