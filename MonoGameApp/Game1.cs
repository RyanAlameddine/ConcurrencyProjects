using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameApp.Sync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MonoGameApp.Sync.Coroutine;

namespace MonoGameApp
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont baseFont;
        SpriteFont objectFont;
        Dino dino;

        List<Cactus> cacti = new List<Cactus>();
        Random random = new Random();

        public static float Speed = 2;
        public static Color FloorColor = Color.LightGray;
        public static Color ScoreColor = Color.LightGray;
        public static Color Background = Color.Black;
        public static float Score = 0;
        public static int HighScore = 99999;
        public static string IpAddr = "";
        public static int HighScoreMine = 0;

        public static SoundEffect[] soundEffects = new SoundEffect[3];

        Song soviets;
        Song tetris;

        string floor = "floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor floor ";
        //MonogameSynchronizationContext msgc = new MonogameSynchronizationContext();

        public static TcpClient TcpClient = new TcpClient();
        private int loadedHighScore = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            SynchronizationContext.SetSynchronizationContext(Coroutine.SyncContext);
        }

        protected override void Initialize()
        {
            StartCoroutine(async () =>
            {
                while (true)
                {

                    if(Score >= loadedHighScore)
                    {
                        HighScore = (int) Score;
                        ScoreColor = new Color((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
                    }
                    else
                    {
                        HighScore = loadedHighScore;
                        ScoreColor = Color.LightGray;
                    }
                    await Task.Delay(100);
                    if (!dino.gameover)
                    {
                        Speed += 0.0001f;
                    }
                    Debug.WriteLine(Speed);
                }
            });

            StartCoroutine(async () =>
            {
                while (true)
                {
                    floor = floor.Substring(1) + floor[0];
                    if(Speed == 0)
                    {
                        break;
                    }
                    int awaits = (int) (1f / Speed);
                    for (int i = -1; i < awaits; i++)
                    {
                        await Task.Yield();
                    }
                }
            });
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            baseFont = Content.Load<SpriteFont>("basefont");
            objectFont = Content.Load<SpriteFont>("objectfont");
            soviets = Content.Load<Song>("soviets");
            tetris = Content.Load<Song>("tetris");

            soundEffects[0] = Content.Load<SoundEffect>("jump");
            soundEffects[1] = Content.Load<SoundEffect>("score");
            soundEffects[2] = Content.Load<SoundEffect>("die");

            if (random.NextDouble() < .7)
            {
                MediaPlayer.Play(tetris);
                MediaPlayer.Volume = .2f;
            }
            else
            {
                MediaPlayer.Play(soviets);
            }
            MediaPlayer.IsRepeating = true;

            dino = new Dino(new Vector2(30, Dino.startHeight), Content.Load<SpriteFont>("dinofont"), Color.White);


            StartCoroutine(async () =>
            {
                while (!dino.gameover)
                {
                    cacti.Add(new Cactus(objectFont, (float)(random.NextDouble() + 1), random));
                    await Task.Delay((int) (random.NextDouble() * 2000 + 50));
                    for (int i = 0; i < cacti.Count; i++)
                    {
                        if(cacti[i].position.X < -100)
                        {
                            cacti.RemoveAt(i);
                            i--;
                        }
                    }
                }
            });

            StartCoroutine(async () =>
            {
                while (true)
                {
                    if (dino.gameover)
                    {
                        MediaPlayer.Volume = 0.99f * MediaPlayer.Volume;
                    }
                    await Task.Yield();
                }
            });

            StartCoroutine(async () =>
            {
                await TcpClient.ConnectAsync("192.168.1.172", 9999);
                NetworkStream stream = TcpClient.GetStream();
                byte[] buffer = new byte[4] { 0, 0, 0, 0 };
                await stream.WriteAsync(buffer, 0, 4);
                await stream.ReadAsync(buffer, 0, 4);
                loadedHighScore = BitConverter.ToInt32(buffer, 0);
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
                IpAddr = Encoding.UTF8.GetString(str, 0, count);
                TcpClient.GetStream().Close();
                TcpClient.Close();
                TcpClient = new TcpClient();

                await Task.Delay(5000);
            });
        }

        protected override void UnloadContent()
        {
            
        }

        //bool doTask = false;

        KeyboardState prevKs;
        //CancellationTokenSource source;
        //Task runningTask;

        protected override void Update(GameTime gameTime)
        {
            //execute all continuations waiting
            Coroutine.ExecuteContinuations();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (prevKs.IsKeyUp(Keys.Up))
                {
                    soundEffects[0].Play();
                }
                dino.Jump();
            }
            else
            {
                dino.accellerationcount = 100;
                dino.Jumping = false;
            }


            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                dino.down = true;
            }
            else
            {
                dino.down = false;
            }

            //if(Keyboard.GetState().IsKeyDown(Keys.Up) && prevKs.IsKeyUp(Keys.Up))
            //{
            //    dino.ScoreSell();
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Up))
            //{
            //    dino.Jump();
            //}
            dino.Update(gameTime, cacti);


            prevKs = Keyboard.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Background);

            spriteBatch.Begin();

            spriteBatch.DrawString(baseFont, floor, new Vector2(0, 450), FloorColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            spriteBatch.DrawString(baseFont, "Score: " + (int)Score, new Vector2(600, 20), ScoreColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(baseFont, "HighScore: " + HighScore, new Vector2(620, 50), Color.LightGray, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            dino.Draw(spriteBatch);
            foreach(Cactus cactus in cacti)
            {
                cactus.Draw(spriteBatch, GraphicsDevice);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
