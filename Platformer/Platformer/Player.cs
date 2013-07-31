using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer;

namespace Platformer
{
    class Player
    {
        // Animations
        private Animation idleAnimation;
        private Animation GunidleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation GunjumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;
        private Animation GunAnimation;
        private Animation ladderUpAnimation;
        private Animation GunladderUpAnimation;
        private Animation ladderDownAnimation;
        private Animation GunladderDownAnimation;
        private Texture2D fireTexture;
        private Rectangle fire;
        Bullet[] bullets;
        FreezeBullet[] Freezebullets;
        //sound
        private SoundEffect gunfireeffect;
        private SoundEffect DieEffect;
        //private Bullet bullet;
        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;
        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;
        // ****************************
        private const int LadderAlignment = 12;
        private bool isClimbing;
        public bool IsClimbing
        {
            get { return isClimbing; }
        }
        private bool wasClimbing;
        //This used to be private float movement;
        private Vector2 movement;

        //*****************************
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        //private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

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

        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        public void LoadContent()
        {
            bullets = new Bullet[4];
            for (int i = 0; i < 4; i++)
            {
                bullets[i] = new Bullet(Level.Content.Load<Texture2D>("Sprites/Bullet1"));
            }
            Freezebullets = new FreezeBullet[4];
            for (int i = 0; i < 4; i++)
            {
                Freezebullets[i] = new FreezeBullet(Level.Content.Load<Texture2D>("Sprites/Bullet2"));
            }

            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            GunidleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/GunIdle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            GunjumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/GunJump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            GunAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Gun"), 0.1f, false);
            ladderUpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, false);
            GunladderUpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Gun"), 0.1f, false);
            ladderDownAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, false);
            GunladderDownAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Gun"), 0.1f, false);
            fireTexture = Level.Content.Load<Texture2D>("Sprites/Fire");
            //Sounds
            gunfireeffect = level.Content.Load<SoundEffect>("Sounds/gun_fire");
            DieEffect = level.Content.Load<SoundEffect>("Sounds/died");
            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            if (level.FireFlag)
                sprite.PlayAnimation(GunidleAnimation);
            else
                sprite.PlayAnimation(idleAnimation);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            GetInput(gameTime);
            UpdateBullets(gameTime);
            ApplyPhysics(gameTime);
            UpdateFreezeBullets(gameTime);
            //LADDER
            if (IsAlive)
            {
                //This if statement deals with running/idling
                if (isOnGround)
                {
                    //If Velocity.X is > 0 in any direction, play runAnimation
                    if (Math.Abs(Velocity.X) - 0.02f > 0 && level.FireFlag == false)
                        sprite.PlayAnimation(runAnimation);
                    if (Math.Abs(Velocity.X) - 0.02f > 0 && level.FireFlag == true)
                        sprite.PlayAnimation(GunAnimation);
                    //Otherwise, sit still (idleAnimation)
                    else if (level.FireFlag == true)
                        sprite.PlayAnimation(GunidleAnimation);
                    else
                        sprite.PlayAnimation(idleAnimation);
                }
                //This if statement deals with ladder climbing
                if (isClimbing)
                {
                    //If he's moving down play ladderDownAnimation
                    if (Velocity.Y - 0.02f > 0)
                    {
                        if (level.FireFlag == false)
                            sprite.PlayAnimation(ladderDownAnimation);
                        else
                            sprite.PlayAnimation(GunladderDownAnimation);
                    }
                    //If he's moving up play ladderUpAnimation
                    else if (Velocity.Y - 0.02f < 0)
                    {
                        if (level.FireFlag == false)
                            sprite.PlayAnimation(ladderUpAnimation);
                        else
                            sprite.PlayAnimation(GunladderUpAnimation);
                    }
                    //Otherwise, just stand on the ladder (idleAnimation)
                    else
                        sprite.PlayAnimation(idleAnimation);
                }

            }
            //Reset our variables every frame
            movement = Vector2.Zero;
            wasClimbing = isClimbing;
            isClimbing = false;
            // Clear input.
            isJumping = false;
        }
        private void HandleShooting(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            fire = new Rectangle((int)this.position.X + 5, (int)this.position.Y - 30, 7, 7);
            // TODO : UPDATE FIRE
        }

        

        //Fire Bullets
        private void FireBullet(GameTime gameTime)
        {
           
            gunfireeffect.Play();
            foreach (Bullet bullet in bullets)
            {
                //Find a bullet that isn't alive
                if (!bullet.alive)
                {
                    bullet.waitTime = 0.9f; // max wait time
                    level.bulletnumber--;
                    //And set it to alive.
                    bullet.alive = true;
                    if (flip == SpriteEffects.FlipHorizontally) //Facing right
                    {

                        bullet.position = new Vector2(this.position.X + 20, this.position.Y - 35);
                        bullet.velocity = new Vector2(10.0f, 0.0f);

                    }
                    else //Facing left
                    {

                        bullet.position = new Vector2(this.position.X - 20, this.position.Y - 35);
                        bullet.velocity = new Vector2(-10.0f, 0.0f);

                    }
                    return;
                }
            }
        }


        //Fire FreezeBullets
        private void FireFreezeBullet()
        {
            gunfireeffect.Play();
            foreach (FreezeBullet freezebullet in Freezebullets)
            {
                //Find a bullet that isn't alive
                if (!freezebullet.alive)
                {
                    freezebullet.waitTime = 0.9f; // max wait time
                    level.bulletnumber--;
                    //And set it to alive.
                    freezebullet.alive = true;
                    if (flip == SpriteEffects.FlipHorizontally) //Facing right
                    {

                        freezebullet.position = new Vector2(this.position.X + 20, this.position.Y - 35);
                        freezebullet.velocity = new Vector2(10.0f, 0.0f);

                    }
                    else //Facing left
                    {

                        freezebullet.position = new Vector2(this.position.X - 20, this.position.Y - 35);
                        freezebullet.velocity = new Vector2(-10.0f, 0.0f);

                    }
                    return;
                }
            }
        }

        // Update Bullets 
        private void UpdateBullets(GameTime gameTime)
        {
            //Check all of our bullets
            foreach (Bullet bullet in bullets)
            {
                //Only update them if they're alive
                if (bullet.alive)
                {
                    //Move our bullet based on it's velocity
                    bullet.position += bullet.velocity;

                    if (bullet.waitTime > 0)
                    {
                        // Wait for some amount of time.
                        bullet.waitTime = Math.Max(0.0f, bullet.waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                        if (bullet.waitTime <= 0.0f)
                        {
                            bullet.alive = false;
                            continue;
                        }
                    }

                    //Collision rectangle for each bullet -Will also be
                    //used for collisions with enemies.

                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.position.X - bullet.sprite.Width * 2,
                        (int)bullet.position.Y - bullet.sprite.Height * 2,
                        bullet.sprite.Width * 4,
                        bullet.sprite.Height * 4);

                    //Check for collisions with the enemies
                    foreach (Enemy enemy in level.enemies)
                    {
                        if (bulletRect.Intersects(enemy.BoundingRectangle))
                        {
                            enemy.FreezebulletEnemyflag = false;
                            enemy.Alive = false;
                        }
                    }
                    ///*******

                    foreach (Giant giant in level.giants)
                    {
                        if (bulletRect.Intersects(giant.BoundingRectangle))
                        {
                            giant.Count++;
                            if (giant.Count == 100)
                            {
                                giant.Countflag = true;
                            }
                            if (giant.Countflag)
                            {
                                giant.Alive = false;
                            }

                        }
                    }
                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = Level.GetCollision(x, y);

                            //If we collide with an Impassable or Platform tile
                            //then delete our bullet.
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    bullet.alive = false;
                            }
                        }
                    }
                }
            }
        }

        // Update FreezeBullets 
        private void UpdateFreezeBullets(GameTime gameTime)
        {
            //Check all of our bullets
            foreach (FreezeBullet freezbullet in Freezebullets)
            {
                //Only update them if they're alive
                if (freezbullet.alive)
                {
                    //Move our bullet based on it's velocity
                    freezbullet.position += freezbullet.velocity;

                    //Rectangle the size of the screen so bullets that
                    //fly off screen are deleted.
                    Rectangle screenRect = new Rectangle(0, 0, 1280, 720);
                    //if (!screenRect.Contains(new Point(
                    //    (int)freezbullet.position.X,
                    //    (int)freezbullet.position.Y)))
                    //{
                    //    freezbullet.alive = false;
                    //    continue;
                    //}
                    if (freezbullet.waitTime > 0)
                    {
                        // Wait for some amount of time.
                        freezbullet.waitTime = Math.Max(0.0f, freezbullet.waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                        if (freezbullet.waitTime <= 0.0f)
                        {
                            freezbullet.alive = false;
                            continue;
                        }
                    }

                    //Collision rectangle for each bullet -Will also be
                    //used for collisions with enemies.

                    Rectangle bulletRect = new Rectangle(
                        (int)freezbullet.position.X - freezbullet.sprite.Width * 2,
                        (int)freezbullet.position.Y - freezbullet.sprite.Height * 2,
                        freezbullet.sprite.Width * 4,
                        freezbullet.sprite.Height * 4);

                    //Check for collisions with the enemies
                    foreach (Enemy enemy in level.enemies)
                    {
                        if (bulletRect.Intersects(enemy.BoundingRectangle))
                        {
                            enemy.Freezebulletflag = true;
                            enemy.Alive = false;
                        }
                    }

                    foreach (Giant giant in level.giants)
                    {
                        if (bulletRect.Intersects(giant.BoundingRectangle))
                        {
                            giant.Freezebulletflag = true;
                            giant.Alive = false;
                        }
                    }

                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = Level.GetCollision(x, y);

                            //If we collide with an Impassable or Platform tile
                            //then delete our bullet.
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    freezbullet.alive = false;
                            }
                        }
                    }
                }
            }
        }

        private void GetInput(GameTime gameTime)
        {
            // Get input state.
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            // Get analog horizontal movement
            movement.X = gamePadState.ThumbSticks.Left.X * MoveStickScale;
            movement.Y = gamePadState.ThumbSticks.Left.Y * MoveStickScale;
            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement.X) < 0.5f)
                movement.X = 0.0f;
            if (Math.Abs(movement.Y) < 0.5f)
                movement.Y = 0.0f;
            // Shooting Fire

            if (keyboardState.IsKeyDown(Keys.F) && level.FireFlag == true && level.bulletnumber > 0 )
            {
                FireBullet(gameTime);

            }
            if (keyboardState.IsKeyDown(Keys.G) && level.Fire2Flag == true && level.bulletnumber > 0)
                FireFreezeBullet();

            // If any digital horizontal movement input is found, override the analog movement.
            if (keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {

                movement.X = -1.0f;
            }
            else if (keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {

                movement.X = 1.0f;
            }
            //LADDER
            if (keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W))
            {
                isClimbing = false;
                if (IsAlignedToLadder())
                {
                    //We need to check the tile behind the player,
                    //not what he is standing on
                    if (level.GetTileCollisionBehindPlayer(position) == TileCollision.Ladder)
                    {
                        isClimbing = true;
                        isJumping = false;
                        isOnGround = false;
                        movement.Y = -1.0f;
                    }
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Down) ||
                     keyboardState.IsKeyDown(Keys.S))
            {
                isClimbing = false;
                if (IsAlignedToLadder())
                {
                    // Check the tile the player is standing on
                    if (level.GetTileCollisionBelowPlayer(level.Player.Position) == TileCollision.Ladder)
                    {
                        isClimbing = true;
                        isJumping = false;
                        isOnGround = false;
                        movement.Y = 2.0f;
                    }
                }
            }
            // Check if the player wants to jump.
            isJumping =
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }
        //LADDER
        private bool IsAlignedToLadder()
        {
            int playerOffset = ((int)position.X % Tile.Width) - Tile.Center;
            if (Math.Abs(playerOffset) <= LadderAlignment &&
                level.GetTileCollisionBelowPlayer(new Vector2(
                    level.Player.position.X,
                    level.Player.position.Y + 1)) == TileCollision.Ladder ||
                level.GetTileCollisionBelowPlayer(new Vector2(
                    level.Player.position.X,
                    level.Player.position.Y - 1)) == TileCollision.Ladder)
            {
                // Align the player with the middle of the tile
                position.X -= playerOffset;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.

            if (!isClimbing)
            {
                if (wasClimbing)
                    velocity.Y = 0;
                else
                    velocity.Y = MathHelper.Clamp(
                        velocity.Y + GravityAcceleration * elapsed,
                        -MaxFallSpeed,
                        MaxFallSpeed);
            }
            else
            {
                velocity.Y = movement.Y * MoveAcceleration * elapsed;
            }
            velocity.X += movement.X * MoveAcceleration * elapsed;
            velocity.Y = DoJump(velocity.Y, gameTime);

            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (level.FireFlag == false)
                        sprite.PlayAnimation(jumpAnimation);
                    else
                        sprite.PlayAnimation(GunjumpAnimation);

                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle.  
            Rectangle bounds = BoundingRectangle;

            // Reset flag to search for ground collision.  
            isOnGround = false;

            // For each potentially colliding movable tile.  
            foreach (var movableTile in level.movableTiles)
            {
                // Reset flag to search for movable tile collision.  
                movableTile.PlayerIsOn = false;

                //check to see if player is on tile.  
                if ((BoundingRectangle.Bottom == movableTile.BoundingRectangle.Top + 1) &&
                    (BoundingRectangle.Left >= movableTile.BoundingRectangle.Left - (BoundingRectangle.Width / 2) &&
                    BoundingRectangle.Right <= movableTile.BoundingRectangle.Right + (BoundingRectangle.Width / 2)))
                {
                    movableTile.PlayerIsOn = true;
                }

                bounds = HandleCollision(bounds, movableTile.Collision, movableTile.BoundingRectangle);
            }

            // Find all neighboring tiles.  
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // For each potentially colliding tile.  
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,  
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.  
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        bounds = HandleCollision(bounds, collision, tileBounds);
                    }
                }
            }

            // Save the new bounds bottom.  
            previousBottom = bounds.Bottom;
        }

        private Rectangle HandleCollision(Rectangle bounds, TileCollision collision, Rectangle tileBounds)
        {
            Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
            if (depth != Vector2.Zero)
            {
                float absDepthX = Math.Abs(depth.X);
                float absDepthY = Math.Abs(depth.Y);

                // Resolve the collision along the shallow axis.  
                if (absDepthY < absDepthX || collision == TileCollision.Platform)
                {
                    // If we crossed the top of a tile, we are on the ground.  
                    //if (previousBottom <= tileBounds.Top)
                    //    isOnGround = true;
                    if (previousBottom <= tileBounds.Top)
                    {
                        if (collision == TileCollision.Ladder)
                        {
                            if (!isClimbing && !isJumping)
                            {
                                // When walking over a ladder
                                isOnGround = true;
                            }
                        }
                        else
                        {
                            isOnGround = true;
                            isClimbing = false;
                            isJumping = false;
                        }
                    }
                    // Ignore platforms, unless we are on the ground.  
                    if (collision == TileCollision.Impassable || IsOnGround)
                    {
                        // Resolve the collision along the Y axis.  
                        Position = new Vector2(Position.X, Position.Y + depth.Y);

                        // Perform further collisions with the new bounds.  
                        bounds = BoundingRectangle;
                    }
                    //LADDER
                    else if (collision == TileCollision.Ladder && !isClimbing)
                    {
                        // When walking in front of a ladder, falling off a ladder
                        // but not climbing
                        // Resolve the collision along the Y axis.
                        Position = new Vector2(Position.X, Position.Y);
                        // Perform further collisions with the new bounds.
                        bounds = BoundingRectangle;
                    }
                }
                else if (collision == TileCollision.Impassable) // Ignore platforms.  
                {
                    // Resolve the collision along the X axis.  
                    Position = new Vector2(Position.X + depth.X, Position.Y);

                    // Perform further collisions with the new bounds.  
                    bounds = BoundingRectangle;
                }
            }
            return bounds;
        }

        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            sprite.PlayAnimation(dieAnimation);
            DieEffect.Play();
        }

        public void OnKilled(Giant killedBy)
        {
            isAlive = false;

            sprite.PlayAnimation(dieAnimation);
            DieEffect.Play();
        }


        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
            foreach (Bullet bullet in bullets)
            {
                if (bullet.alive)
                {
                    spriteBatch.Draw(bullet.sprite, bullet.position, Color.Gray);
                }
                foreach (FreezeBullet freezebullet in Freezebullets)
                {
                    if (freezebullet.alive)
                    {
                        spriteBatch.Draw(freezebullet.sprite, freezebullet.position, Color.White);
                    }
                }
            }
            //if (isShooting)
            //    spriteBatch.Draw(fireTexture, fire, Color.White);
        }
    }
}
