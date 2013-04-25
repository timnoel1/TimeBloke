#region File Description
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Bloke
{
    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    class Battery
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly Color Color = Color.White;

        // Animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        // Gets the current position in world space.
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        // Gets a circle which bounds this bar in world space.
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        // Constructs a new bar
        public Battery(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }

        // Loads the gem texture and collected sound.
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Battery1");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        // Bounces up and down in the air
        public void Update(GameTime gameTime)
        {
            if (level.Player.era == 1)
            {
                texture = Level.Content.Load<Texture2D>("Sprites/Battery1");
                collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            }
            if (level.Player.era == 2)
            {
                texture = Level.Content.Load<Texture2D>("Sprites/Battery1");
                collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            }
            if (level.Player.era == 3)
            {
                texture = Level.Content.Load<Texture2D>("Sprites/Battery1");
                collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            }

            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve
            // Include the X coordinate so that neighboring bars bounce in a nice wave pattern.           
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        // Called when this bar has been collected by a player and removed from the level
        public void OnCollected(Player collectedBy)
        {
            level.healthFlag = true;
            collectedSound.Play();
        }

        // Draws a bar in the appropriate color.
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
