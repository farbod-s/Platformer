using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    enum FaceDirection1
    {

        Left = -1,
        Right = 1,
        //Top = 1 ,
    }

    class Giant
    {
        public int Count = 0;
        public float Sumelapsedfreezetime = 0.0f;
        public float elapsedfreezetime;
        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }
        bool alive;


        public bool Countflag
        {
            get { return countflag; }
            set { countflag = value; }
        }
        bool countflag;

        //public bool bulletflag = false;
        //public bool freezebulletflag = false;

        public bool Freezebulletflag
        {
            get { return freezebulletflag; }
            set { freezebulletflag = value; }
        }
        bool freezebulletflag;

        public bool DrawFlag
        {
            get { return drawflag; }
            set { drawflag = value; }
        }
        bool drawflag;

        public bool Bulletflag
        {
            get { return bulletflag; }
            set { bulletflag = value; }
        }
        bool bulletflag;

        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        public Vector2 HomePos
        {
            get { return homepos; }
        }
        Vector2 homepos;

        private Rectangle localBounds;

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;
        private Animation dieAnimation;

        private FaceDirection1 direction = FaceDirection1.Left;

        private float waitTime;
        private float FreezTime;

        private const float MaxWaitTime = 0.5f;

        private const float MaxFreezTime = 3.0f;

        private const float MoveSpeed = 100.0f;

        public Giant(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.alive = true;
            this.homepos = position;

            LoadContent(spriteSet);
        }

        public void Reset()
        {
            this.position = homepos;
            this.alive = true;
        }

        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.15f, false);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Update(GameTime gameTime)
        {
            if (alive)
            {

                elapsedfreezetime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Sumelapsedfreezetime += elapsedfreezetime;
            }
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
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
                    direction = (FaceDirection1)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }

            ////////////////////////////


            //if (FreezTime > 0)
            //{
            //    // Wait for some amount of time.
            //    FreezTime = Math.Max(0.0f, FreezTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
            //    if (FreezTime <= 0.0f && !alive)
            //    {
            //        // Then turn around.
            //        alive = true;
            //        //direction = (FaceDirection)(-(int)direction);
            //    }
            //}
            //else
            //{

            //    if (!alive && freezebulletflag)
            //        FreezTime = MaxWaitTime;
            //    else
            //    {
            //        Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
            //        position = position + velocity;
            //    }
            //}

            //}
            //else
            //{
            //    // If we are about to run into a wall or off a cliff, start waiting.
            //    //if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
            //    //    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
            //    //{
            //    FreezTime = MaxFreezTime;
            //    //  }
            //    //    //else
            //    //    //{
            //    //        // Move in the current direction.
            //    //        Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
            //    //        position = position + velocity;
            //    //   // }
            //}


        }
        ////////////////////////////
        //if ((float)gameTime.ElapsedGameTime.TotalSeconds - elapsed >= 3.0f && freezebulletflag)
        //    alive = true;


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0)
                if (MaxFreezTime - Sumelapsedfreezetime >= 0.0f)
                {
                    sprite.PlayAnimation(idleAnimation);
                }
                else if (!alive)
                    sprite.PlayAnimation(dieAnimation);
                else
                {
                    sprite.PlayAnimation(runAnimation);
                }


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (!alive && freezebulletflag == true)
                sprite.Draw(gameTime, spriteBatch, Position, flip);
            if (alive)
                sprite.Draw(gameTime, spriteBatch, Position, flip);

        }
    }
}
