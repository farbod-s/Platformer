using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace Platformer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D HighscoreOverlay;
        Rectangle imagerectangle;
       
        //HUD
        private Texture2D HudTexture;
        private Texture2D image;
        //Menu
        private KeyboardState keyboard;

        private int windowWidth, windowHeight;

        private Menu menu;
        private Menu.MenuState menuState;

        private SpriteFont arial;
        private Level level;
        private bool wasContinuePressed;
        private int levelIndex = -1;
        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private KeyboardState keyboardState;

        private const int numberOfLevels = 5; // number of levels

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            windowWidth = graphics.PreferredBackBufferWidth;
            windowHeight = graphics.PreferredBackBufferHeight;
            imagerectangle.Width = 222; imagerectangle.Height = 65;
            imagerectangle = new Rectangle((windowWidth / 2 - imagerectangle.Width / 2) - 100, (4 * (windowHeight / 7) + (imagerectangle.Height / 2)) + 25, imagerectangle.Width, imagerectangle.Height);

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //#if WINDOWS
            //    TextInput textinput = new TextInput();
            //    textinput.ShowInTaskbar = false;
            //    textinput.Show();

        //#endif
            menu = new Menu(GraphicsDevice, Services);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = this.Content.Load<SpriteFont>("Fonts/Hud");
            arial = this.Content.Load<SpriteFont>("Fonts/Arial");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            HighscoreOverlay = Content.Load<Texture2D>("Overlays/you_highscore");
           image = Content.Load<Texture2D>("Overlays/you_highscore");
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));

                MediaPlayer.Volume = 0.2f;

            }
            catch { }
            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            keyboard = Keyboard.GetState();

            /* MENU */
            if (keyboard.IsKeyDown(Keys.Escape))
            {
                menuState = Menu.MenuState.ready;
                menu.SetMenuState(menuState);
            }

            if (menuState != Menu.MenuState.suspend)
            {
                menu.Update(gameTime);
                menuState = menu.GetMenuState();
                if (menuState == Menu.MenuState.exit)
                    Exit();
                if (menuState == Menu.MenuState.start)
                {
                    menuState = Menu.MenuState.suspend;
                    menu.SetMenuState(menuState);
                }
            }
            /* MENU */

            else
            {
                // Handle polling for our input and handling high-level input
                HandleInput();

                // update our level, passing down the GameTime along with all of our input states
                level.Update(gameTime, keyboardState);
            }

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();

            // Exit the game when back is pressed.
            if (keyboardState.IsKeyDown(Keys.Back))
                Exit();

            bool continuePressed = keyboardState.IsKeyDown(Keys.Space);

            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                if (level.Lives <= 0)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                    {
                        //level.gameover = true;
                        level. WriteHighScore();
                        

                        //System.Threading.Thread.Sleep(5000);

                        //menuState = Menu.MenuState.ready;
                        //menu.SetMenuState(menuState);
                        
                        ReloadCurrentLevel();
                        //Exit();
                    }
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else 
                    {
                        level.WriteHighScore();

                        //System.Threading.Thread.Sleep(5000);

                        //menuState = Menu.MenuState.ready;
                        //menu.SetMenuState(menuState);
                        ReloadCurrentLevel();
                    }
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        
        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }
  
        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //protected override void Draw(GameTime gameTime)
        //{
        //    spriteBatch.Begin();
        //    spriteBatch.Draw(image, imagerectangle, Color.White);

        //        KeyboardState keyboardState = Keyboard.GetState();
        //        string playerName = "";
        //        Keys[] pressedKeys = keyboardState.GetPressedKeys();
        //        int i = 0;

        //        while (!keyboardState.IsKeyDown(Keys.Enter))
        //        {
        //            playerName += pressedKeys[i].ToString();
        //            i++;

        //            spriteBatch.DrawString(hudFont, playerName, new Vector2(100 , 100), Color.White);
        //        }

        //        base.Draw(gameTime);
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(new Color(11, 48, 79));

           // spriteBatch.Begin();

            graphics.GraphicsDevice.Clear(new Color(11, 48, 79));

            //spriteBatch.Begin();
           // spriteBatch.Draw(image, imagerectangle, Color.White);
            if (menuState != Menu.MenuState.suspend)
            {
                menu.Draw(spriteBatch, arial);
                IsMouseVisible = true;
            }
            /* MENU */

            else
            {
                IsMouseVisible = false;

                level.Draw(gameTime, spriteBatch);

                DrawHud();
            }
            
            //spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor,"Sprites/Elapsed_hud");

            // Draw score
            DrawShadowedString(hudFont, "" + level.Score.ToString(), hudLocation +
                               new Vector2(100 * 1f+10, 0.0f), Color.Yellow, "Sprites/Score-hud");

            // Draw Highscore
            //DrawShadowedString(hudFont, "" + level.HighScore.ToString(), hudLocation +
            //                   new Vector2(100 * 2f + 10, 0.0f), Color.Yellow, "Sprites/Score-hud");

          
            // Draw Lives
            DrawShadowedString(hudFont,"" + level.Lives.ToString(),
                               hudLocation + new Vector2(100 * 2.0f, 0.0f), Color.Yellow,"Sprites/live");

            // Draw Key
            if(level.KeyFlag==false)
            DrawShadowedString(hudFont, ""  ,
                               hudLocation + new Vector2(100 * 3f, 0.0f), Color.Yellow, "Sprites/key-false");
            else if(level.KeyFlag==true)
                DrawShadowedString(hudFont, "" ,
                             hudLocation + new Vector2(100 * 3f, 0.0f), Color.Yellow, "Sprites/key-true");
            // Draw bullets
            DrawShadowedString(hudFont, "  " + level.bulletnumber.ToString(),
                               hudLocation + new Vector2(100 * 4f, 0f), Color.Yellow, "Sprites/Bullet-hud");

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive && !level.gameover)
            {
                status = diedOverlay;
            }
            else if (level.Highscorebool)
            {
                status = HighscoreOverlay;
            }
            
            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color,string imagepath)
        {
            if (imagepath != "false")
            {
                HudTexture = Content.Load<Texture2D>(imagepath);
               // image = Content.Load<Texture2D>(imagepath);
              spriteBatch.Draw(HudTexture, position, Color.White);
            }
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f) + new Vector2(26, 0), Color.Black);
            spriteBatch.DrawString(font, value, position + new Vector2(26, 0), color);
        }
    }
}
