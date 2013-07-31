using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{

    class MovableTile
    {
        private Texture2D texture;
        private Vector2 origin;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        public Vector2 Velocity
        {
            get { return velocity; }
        }
        Vector2 velocity;

        /// <summary>  
        /// Gets whether or not the player's feet are on the MovableTile.  
        /// </summary>  
        public bool PlayerIsOn { get; set; }

        public bool EnemyIsOn { get; set; }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public FaceDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        FaceDirection direction = FaceDirection.Left;

        public TileCollision Collision
        {
            get { return collision; }
            set { collision = value; }
        }
        private TileCollision collision;

        private Rectangle localBounds;
        private float waitTime;
        private const float MaxWaitTime = 1.0f;
        private const float MoveSpeed = 170.0f;

        public MovableTile(Level level, Vector2 position, TileCollision collision)
        {
            this.level = level;
            this.position = position;
            this.collision = collision;

            LoadContent();
        }

        public void LoadContent()
        {
            texture = collision == TileCollision.Platform ? Level.Content.Load<Texture2D>("Tiles/Remove") : Level.Content.Load<Texture2D>("Tiles/Remove");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            // Calculate bounds within texture size.  
            localBounds = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are moving towards.  
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.  
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.  
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall that is not a MovableTile move in other direction.  
                if (Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Impassable || Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Platform)
                {
                    velocity = new Vector2(0.0f, 0.0f);
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.  
                    velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }

            //if (level.movableTiles.Count > 0)  
            //{  
            //    // If we are about to run into a MovableTile move in other direction.  
            //    foreach (var movableTile in level.movableTiles)  
            //    {  
            //        if (BoundingRectangle != movableTile.BoundingRectangle)  
            //        {  
            //            if (BoundingRectangle.Intersects(movableTile.BoundingRectangle))  
            //            {  
            //                direction = (FaceDirection)(-(int)direction);  
            //                velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);  
            //            }  
            //        }  
            //    }  
            //}  
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}


