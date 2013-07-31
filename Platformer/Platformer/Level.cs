using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    class Level
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;
        public bool KeyFlag = false;
        public bool FireFlag = false;
        public bool BulletFlag = false;
        public bool Fire2Flag = false;
        // Entities in the level.
        // private Bullet bullet1;
        //public int bulletIntCount;
        public TileCollision collision;
        public int bulletnumber = 40;
        public int HighScore = 0;
        public int Score1 = 0;
        List<int> listOfHighscores = new List<int>();
        public bool Highscorebool = false;
        public bool gameover = false;
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Key> keys = new List<Key>();
        public List<Enemy> enemies = new List<Enemy>();
        public List<Giant> giants = new List<Giant>();
        private List<Fire> fires = new List<Fire>();
        private List<Fire2> fires2 = new List<Fire2>();
        public List<BulletView> bullets = new List<BulletView>();
        public List<MovableTile> movableTiles = new List<MovableTile>();
        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        /////////////////////////

        private SoundEffect bulletEffect;

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        private float cameraPosition;
        public StreamWriter highscore;
        public int Score
        {
            get { return score; }
        }
        int score;

        public int Lives
        {
            get { return lives; }
        }
        int lives = 3;

        public int Bullets
        {
            get { return BUllets; }
        }
        int BUllets;


        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            //layers = new Texture2D[3];
            //for (int i = 0; i < layers.Length; ++i)
            //{
            //    // Choose a random segment if each background layer for level variety.
            //    layers[i] = Content.Load<Texture2D>("Backgrounds/Layer1");
            //}

            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);


        }

        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                // Key
                case 'K':
                    return LoadKeyTile(x, y);

                // Bullet
                case 'B':
                    return LoadBulletTile(x, y);

                // Fire
                case 'F':
                    return LoadFireTile(x, y);

                // Fire2
                case 'Z':
                    return LoadFire2Tile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);


                // Giant enemies
                case 'Q':
                    return LoadGiantTile(x, y, "GiantMonster");


                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "Monster");

                // Movable Tiles
                case 'R':
                    return LoadMovableTile(x, y);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("Block", 7, TileCollision.Impassable);

                // Leader
                case 'L':
                    return LoadTile("Leader", TileCollision.Ladder);
                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            return LoadTile(baseName, collision);
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadGiantTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            giants.Add(new Giant(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadKeyTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            keys.Add(new Key(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadBulletTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            bullets.Add(new BulletView(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadFireTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            fires.Add(new Fire(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadFire2Tile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            fires2.Add(new Fire2(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        //**********************************
        public TileCollision GetTileCollisionBehindPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y - 1) / Tile.Height;
            // Prevent escaping past the level ends.
            if (x == Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y == Height)
                return TileCollision.Passable;
            return tiles[x, y].Collision;
        }
        public TileCollision GetTileCollisionBelowPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y) / Tile.Height;
            // Prevent escaping past the level ends.
            if (x == Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y == Height)
                return TileCollision.Passable;

            // BUG FIXED
            if (y > Height)
                return TileCollision.Passable;
            // BUG FIXED
            if (y < 0)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }
        //**************************************************
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        private Tile LoadMovableTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            movableTiles.Add(new MovableTile(this, new Vector2(position.X, position.Y), collision));

            return new Tile(null, TileCollision.Passable);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                //score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState);
                UpdateGems(gameTime);
                UpdateKeys(gameTime);
                UpdateBullets(gameTime);
                UpdateFires(gameTime);
                UpdateFires2(gameTime);

                // Update movable tiles
                UpdateMovableTiles(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);
                UpdateGiants(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        private void UpdateKeys(GameTime gameTime)
        {
            for (int i = 0; i < keys.Count; ++i)
            {
                Key key = keys[i];

                if (key.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    keys.RemoveAt(i--);
                    OnKeyCollected(key, Player);
                }
            }
        }
        private void UpdateBullets(GameTime gameTime)
        {
            //bulletIntCount = bullet1.bulletFire;
            for (int i = 0; i < bullets.Count; ++i)
            {
                BulletView bullet = bullets[i];

                if (bullet.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    //BUllets += 10;
                    bulletnumber += 10;
                    bullets.RemoveAt(i--);
                    OnKeyCollected(bullet, Player);
                }
            }
        }

        private void UpdateFires(GameTime gameTime)
        {
            for (int i = 0; i < fires.Count; ++i)
            {
                // bulletnumber--;
                Fire fire = fires[i];

                if (fire.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    fires.RemoveAt(i--);
                    OnKeyCollected(fire, Player);
                }
            }
        }

        private void UpdateFires2(GameTime gameTime)
        {
            for (int i = 0; i < fires2.Count; ++i)
            {
                Fire2 fire2 = fires2[i];

                if (fire2.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    fires2.RemoveAt(i--);
                    OnKeyCollected(fire2, Player);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.Alive)
                {
                    enemy.Update(gameTime);

                    // Touching an enemy instantly kills the player
                    if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                    {
                        lives--;
                        OnPlayerKilled(enemy);
                    }
                }
            }
        }
        private void UpdateGiants(GameTime gameTime)
        {
            foreach (Giant giant in giants)
            {
                if (giant.Alive)
                {
                    giant.Update(gameTime);

                    // Touching an enemy instantly kills the player
                    if (giant.BoundingRectangle.Intersects(Player.BoundingRectangle))
                    {
                        lives--;
                        OnGiantKilled(giant);
                    }
                }
            }
        }
        private void UpdateMovableTiles(GameTime gameTime)
        {
            for (int i = 0; i < movableTiles.Count; ++i)
            {
                MovableTile movableTile = movableTiles[i];
                movableTile.Update(gameTime);

                if (movableTile.PlayerIsOn)
                {
                    //Make player move with tile if the player is on top of tile  
                    player.Position += movableTile.Velocity;
                }
            }
        }

        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;
            gem.OnCollected(collectedBy);
        }

        private void OnKeyCollected(Key key, Player collectedBy)
        {
            score += Key.PointValue;
            KeyFlag = true;
        }

        private void OnKeyCollected(Fire2 fire2, Player collectedBy)
        {
            score += Fire2.PointValue;
            Fire2Flag = true;
        }

        private void OnKeyCollected(BulletView bullet, Player collectedBy)
        {
            bulletEffect = content.Load<SoundEffect>("Sounds/Pump Shotgun");
            bulletEffect.Play();
            score += BulletView.PointValue;
            BulletFlag = true;
        }

        private void OnKeyCollected(Fire fire, Player collectedBy)
        {
            score += Fire.PointValue;
            FireFlag = true;
        }

        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        private void OnGiantKilled(Giant killedBy)
        {
            Player.OnKilled(killedBy);
        }

        private void OnExitReached()
        {
            if (KeyFlag)
            {
                Player.OnReachedExit();
                reachedExit = true;

            }
        }
        public void WriteHighScore()
        {
            using (StreamReader reader = new StreamReader("score.txt"))
            {
                /////


                string line;

                while ((line = reader.ReadLine()) != null)
                {

                    Score1 = int.Parse(line);
                    listOfHighscores.Add(Score1);

                }

            }

            listOfHighscores.Sort();
            listOfHighscores.Reverse();
            if (score > listOfHighscores[9])
            {
                Highscorebool = true;
                listOfHighscores.Add(score);
                listOfHighscores.Sort();
                listOfHighscores.Reverse();
            }

            //HighScore = listOfHighscore;

            using (StreamWriter writer = new StreamWriter("score.txt"))
            {
                for (int i = 0; i < 10; i++)
                    writer.WriteLine(listOfHighscores[i]);
                writer.Close();
            }
        }



        public void StartNewLife()
        {
            Player.Reset(start);
            foreach (Enemy enemy in enemies)
            {
                enemy.Reset();
            }

            foreach (Giant giant in giants)
            {
                giant.Reset();
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            foreach (Key key in keys)
                key.Draw(gameTime, spriteBatch);

            foreach (BulletView bullet in bullets)
                bullet.Draw(gameTime, spriteBatch);


            foreach (Fire fire in fires)
                fire.Draw(gameTime, spriteBatch);

            foreach (Fire2 fire2 in fires2)
                fire2.Draw(gameTime, spriteBatch);

            foreach (var movableTile in movableTiles)
                movableTile.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (Giant giant in giants)
                giant.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
    }
}
