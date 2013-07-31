using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    enum FaceDirection
    {

        Left = -1,
        Right = 1,
        //Top = 1 ,
    }

    class Enemy
    {
        public int Count1 = 0;
        public float Sumelapsedfreezetime = 0.0f;
        public float elapsedfreezetime;
        public float waitfreezTime;
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
        //public bool bulletflag = false;
        //public bool freezebulletflag = false;

        //**************************


        public bool FreezebulletEnemyflag
        {
            get { return freezebulletEnemyflag; }
            set { freezebulletEnemyflag = value; }
        }
        bool freezebulletEnemyflag;


        //**************************
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

        public Rectangle SpotlightRectangle
        {
            get
            {
                int left = (int)Math.Round
                     (Position.X - sprite.Origin.X) +
                     localBounds.X;
                int top = (int)Math.Round
                     (Position.Y - sprite.Origin.Y);
                if ((int)direction == (int)FaceDirection.Right)
                {
                    playerDirection = (int)FaceDirection.Right;

                    return new Rectangle(
                         left + localBounds.Width,
                         top,
                         spotlightTexture.Width / 2,
                         (spotlightTexture.Height / 2));
                }
                else
                {
                    playerDirection = (int)FaceDirection.Left;

                    return new Rectangle(
                         left - spotlightTexture.Width / 2,
                         top,
                         spotlightTexture.Width / 2,
                         (spotlightTexture.Height / 2));
                }
            }
        }
        public int playerDirection;
        public bool iSeeYou;
        Texture2D spotlightTexture;

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;
        private Animation dieAnimation;

        private FaceDirection direction = FaceDirection.Left;

        private float waitTime;
        private float FreezTime;

        private const float MaxWaitTime = 0.5f;

        private const float MaxFreezTime = 3.0f;

        private const float MoveSpeed = 100.0f;

        public Enemy(Level level, Vector2 position, string spriteSet)
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

            spotlightTexture = Level.Content.Load<Texture2D>("Overlays/spotlight2");
        }

        public void Update(GameTime gameTime)
        {
            if (SpotlightRectangle.Intersects(Level.Player.BoundingRectangle))
                iSeeYou = true;
            else
                iSeeYou = false;

            if (alive)
            {

                elapsedfreezetime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Sumelapsedfreezetime += elapsedfreezetime;
            }

            if (iSeeYou)
            {
                // Calculate tile position based on the side we are walking towards.
                float _posX = Position.X + localBounds.Width / 2 * (int)direction;
                int _tileX = (int)Math.Floor(_posX / Tile.Width) - (int)direction;
                int _tileY = (int)Math.Floor(Position.Y / Tile.Height);

                if (!(Level.GetCollision(_tileX + (int)direction, _tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(_tileX + (int)direction, _tileY) == TileCollision.Passable))
                    position.X += (float)playerDirection;
            }

            if (!iSeeYou)
            {
                if (Math.Abs(level.Player.Position.X - position.X) < spotlightTexture.Width / 2
                    && Math.Abs(level.Player.Position.Y - position.Y) < spotlightTexture.Height / 2)
                {
                    direction = (FaceDirection)(-(int)direction);
                    playerDirection = (int)direction;
                }
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
                    direction = (FaceDirection)(-(int)direction);
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
            foreach (Enemy enemy in level.enemies)
            {
                if (!freezebulletEnemyflag && !alive)
                {
                    waitfreezTime = 0.9f; // max wait time
                    if (waitfreezTime > 0)
                    {
                        // Wait for some amount of time.
                        waitfreezTime = Math.Max(0.0f, waitfreezTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                        if (waitfreezTime <= 0.0f)
                        {
                            alive = true;
                            continue;
                        }
                    }
                }
            }
        }

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

            //if (iSeeYou)
            //    spriteBatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Red);
            //else
            //    spriteBatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.White);

        }
    }
}
