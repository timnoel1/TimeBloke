#region File Description
//-----------------------------------------------------------------------------
// Modified from Microsoft XNA 3.1 Community Game Platform
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;


namespace Bloke{
    public class PlatformerGame : Microsoft.Xna.Framework.Game{

        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        public int lives = 0;
        private int era = 2;
        private bool resetFlag = false;
        private bool resetFlagK = false;
        private bool diedFlag = false;
        //private bool isPaused = false;

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 3;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.ToggleFullScreen();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
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
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState, 
                         Window.CurrentOrientation);

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Z))
            {
                if (era != 1 && level.battery > 0)
                {
                    era = 1;
                    MediaPlayer.Play(Content.Load<Song>("Sounds/Music_P"));
                    winOverlay = Content.Load<Texture2D>("Overlays/you_win");
                    loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
                    diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
                    level.pastLevel();


                    //string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
                    //using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    //    level = new Level(Services, fileStream, levelIndex);
                }
            }

            if (gamePadState.IsButtonDown(Buttons.DPadUp) ||
                gamePadState.IsButtonDown(Buttons.DPadDown) ||
                keyboardState.IsKeyDown(Keys.X))
            {
                if (era != 2 && level.battery > 0)
                {
                    era = 2;
                    MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
                    winOverlay = Content.Load<Texture2D>("Overlays/you_win");
                    loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
                    diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
                    level.presentLevel();
                }
            }

            if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                keyboardState.IsKeyDown(Keys.C))
            {
                if (era != 3 && level.battery > 0)
                {
                    era = 3;
                    MediaPlayer.Play(Content.Load<Song>("Sounds/Music_F"));
                    winOverlay = Content.Load<Texture2D>("Overlays/you_win");
                    loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
                    diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
                    level.futureLevel();
                }
            }

            if ((gamePadState.Buttons.RightShoulder == ButtonState.Pressed))
            {
                resetFlag = true;
            }
            if ((gamePadState.Buttons.RightShoulder == ButtonState.Released) && resetFlag)
            {
                lives--;
                resetFlag = false;
                ReloadCurrentLevel();
            }
            if ((keyboardState.IsKeyDown(Keys.R)))
            {
                resetFlagK = true;
            }
            if ((keyboardState.IsKeyUp(Keys.R)) && resetFlagK)
            {
                lives--;
                resetFlagK = false;
                ReloadCurrentLevel();
            }

            /*if((keyboardState.IsKeyDown(Keys.Enter) ||
               gamePadState.IsButtonDown(Buttons.Start))){
                   if (!isPaused)
                   {
                       MediaPlayer.Pause();
                       isPaused = true;
                   }
                   else
                   {
                       MediaPlayer.Resume();
                       isPaused = false;
                   }
            }*/

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Enter) ||
                gamePadState.IsButtonDown(Buttons.Start);

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if(!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            era = 2;
            MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));

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
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);




            var gameWorldSize = new Vector2(1280, 736);
            var vp = GraphicsDevice.Viewport;

            float scaleX = vp.Width / gameWorldSize.X;
            float scaleY = vp.Height / gameWorldSize.Y;
            float scale = Math.Min(scaleX, scaleY);

            float translateX = (vp.Width - (gameWorldSize.X * scale)) / 2f;
            float translateY = (vp.Height - (gameWorldSize.Y * scale)) / 2f;

            Matrix camera = Matrix.CreateScale(scale, scale, 1)
                    * Matrix.CreateTranslation(translateX, translateY, 0);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera);




            level.Draw(gameTime, spriteBatch);

            DrawHud();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);
            Vector2 gemLoc = new Vector2(15, 82);
            Vector2 battLoc = new Vector2(15, 41);

            string timeString = "TIME";
            float timeHeight = hudFont.MeasureString(timeString).Y;

            // Draw battery
            if (level.battery == 3) spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Battery3"), battLoc, Color.White);
            if (level.battery == 2) spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Battery2"), battLoc, Color.White);
            if (level.battery == 1) spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Battery1"), battLoc, Color.White);
            if (level.battery == 0) spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Battery0"), battLoc, Color.White);

            // Draw Era
            if (era == 1)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Gem_P"), gemLoc, Color.Yellow);
                DrawShadowedString(hudFont, "    - PAST", hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Green);
            }
            if (era == 2)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Gem"), gemLoc, Color.Yellow);
                DrawShadowedString(hudFont, "    - PRESENT", hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Green);
            }
            if (era == 3)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("Sprites/Gem_F"), gemLoc, Color.Yellow);
                DrawShadowedString(hudFont, "    - FUTURE", hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Green);
            }

            //Draw Health
            if (level.healthFlag)
            {
                level.healthFlag = false;
            }

            //Draw Lives
            if (level.livesFlag)
            {
                lives++;
                level.livesFlag = false;
            }
            if (diedFlag && level.Player.IsAlive)
            {
                lives--;
                diedFlag = false;
            }
            DrawShadowedString(hudFont, "    x " + lives, hudLocation + new Vector2(0.0f, timeHeight * 1.2f * 2), Color.Green);

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
            else if (!level.Player.IsAlive)
            {
                diedFlag = true;
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        public int getEra()
        {
            return era;
        }
    }
}
