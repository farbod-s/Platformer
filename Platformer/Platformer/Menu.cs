using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
namespace Platformer
{
    class Menu
    {
        ContentManager content;

        Rectangle title;

        Rectangle startButton;
        Rectangle howToPlayButton;
        Rectangle aboutButton;
        Rectangle exitButton;
        Rectangle backButton;
        Rectangle highscoreButton;
        Texture2D titleTexture;

        Texture2D startBtnTexture;
        Texture2D howToPlayBtnTexture;
        Texture2D aboutBtnTexture;
        Texture2D exitBtnTexture;
        Texture2D backBtnTexture;

        Texture2D startBtnTextureOver;
        Texture2D howToPlayBtnTextureOver;
        Texture2D aboutBtnTextureOver;
        Texture2D exitBtnTextureOver;
        Texture2D highscoreBtnTexture;
        Texture2D highscoreBtnTextureover;
        //Texture2D backBtnTextureOver;

        //SoundEffect buttonSound;

        public enum MenuState { start, htp, about,highscore, exit, suspend, ready }
        MenuState state;

        enum ButtonFocus { resume, htp, about,highscore, exit, none }
        ButtonFocus focus;

        String gameName;
        String htpName;
        String aboutName;
        String highscoreName;

        String[] instructions;
        String[] authors;
        //String[] highscore;
        List<string> highscore = new List<string>();
        int windowWidth, windowHeight;

        public Menu(GraphicsDevice device, IServiceProvider serviceProvider)
        {
            content = new ContentManager(serviceProvider, "Content");

            state = MenuState.suspend;
            focus = ButtonFocus.none;

            gameName = "Where is my jacket?";
            htpName = "How to play";
            aboutName = "About us";
            highscoreName = "High Score";
            instructions = new String[2];
            instructions[0] = "use arrow keys for moving";
            instructions[1] = "use 'ctrl' for actions";

            authors = new String[3];
            authors[0] = "Nima hemmati";
            authors[1] = "Farbod Samsamipour";
            authors[2] = "Mohammad Abdous";
            using (StreamReader reader = new StreamReader("score.txt"))
            {
                /////


                string line;

                while ((line = reader.ReadLine()) != null)
                {

                    //Score1 = int.Parse(line);
                    highscore.Add(line);

                }

            }
            this.windowWidth = device.Viewport.Width;
            this.windowHeight = device.Viewport.Height;

            title.Width = 400; title.Height = 250;
            title = new Rectangle((windowWidth / 2 - title.Width / 2), (1 * (windowHeight / 7) - (title.Height / 2)) + 25, title.Width, title.Height);

            startButton.Width = 222; startButton.Height = 65;
            startButton = new Rectangle((windowWidth / 2 - startButton.Width / 2), (2 * (windowHeight / 7) - (startButton.Height / 2)) + 75, startButton.Width, startButton.Height);

            howToPlayButton.Width = 222; howToPlayButton.Height = 65;
            howToPlayButton = new Rectangle((windowWidth / 2 - howToPlayButton.Width / 2), (3 * (windowHeight / 7)) + 50, howToPlayButton.Width, howToPlayButton.Height);

            aboutButton.Width = 222; aboutButton.Height = 65;
            aboutButton = new Rectangle((windowWidth / 2 - aboutButton.Width / 2), (4 * (windowHeight / 7) + (aboutButton.Height / 2)) + 25, aboutButton.Width, aboutButton.Height);

            highscoreButton.Width = 222; highscoreButton.Height = 65;
            highscoreButton = new Rectangle((windowWidth / 2 - highscoreButton.Width / 2)-100, (4 * (windowHeight / 7) + (highscoreButton.Height / 2)) + 25, highscoreButton.Width, highscoreButton.Height);

            exitButton.Width = 222; exitButton.Height = 65;
            exitButton = new Rectangle((windowWidth / 2 - exitButton.Width / 2), 5 * (windowHeight / 7) + 2 * (exitButton.Height / 2), exitButton.Width, exitButton.Height);

            backButton.Width = 100; backButton.Height = 65;
            backButton = new Rectangle((windowWidth / 2 - backButton.Width / 2) , 5 * (windowHeight / 7) + 80, backButton.Width, backButton.Height);

            LoadContent();
        }

        public void LoadContent()
        {
            //buttonSound = this.content.Load<SoundEffect>("Sounds/Button");

            titleTexture = this.content.Load<Texture2D>("Menu/Title");

            startBtnTexture = this.content.Load<Texture2D>("Menu/Resume");
            howToPlayBtnTexture = this.content.Load<Texture2D>("Menu/HTP");
            aboutBtnTexture = this.content.Load<Texture2D>("Menu/About");
            exitBtnTexture = this.content.Load<Texture2D>("Menu/Exit");
            highscoreBtnTexture = this.content.Load<Texture2D>("Menu/Highscore");
            startBtnTextureOver = this.content.Load<Texture2D>("Menu/Resume_Over");
            howToPlayBtnTextureOver = this.content.Load<Texture2D>("Menu/HTP_Over");
            aboutBtnTextureOver = this.content.Load<Texture2D>("Menu/About_Over");
            exitBtnTextureOver = this.content.Load<Texture2D>("Menu/Exit_Over");
            highscoreBtnTextureover = this.content.Load<Texture2D>("Menu/Highscore_Over");
            backBtnTexture = this.content.Load<Texture2D>("Menu/Back");
        }

        public MenuState GetMenuState()
        {
            return this.state;
        }

        public void SetMenuState(MenuState _state)
        {
            this.state = _state;
        }

        public void Update(GameTime gameTime)
        {
            MouseState cursor = Mouse.GetState();

            if (cursor.LeftButton == ButtonState.Pressed && state == MenuState.ready)
            {
                if (cursor.X >= startButton.X && cursor.X <= startButton.X + startButton.Width && cursor.Y >= startButton.Y && cursor.Y <= startButton.Y + startButton.Height)
                    state = MenuState.start;
                if (cursor.X >= howToPlayButton.X && cursor.X <= howToPlayButton.X + howToPlayButton.Width && cursor.Y >= howToPlayButton.Y && cursor.Y <= howToPlayButton.Y + howToPlayButton.Height)
                    state = MenuState.htp;
                if (cursor.X >= aboutButton.X && cursor.X <= aboutButton.X + aboutButton.Width && cursor.Y >= aboutButton.Y && cursor.Y <= aboutButton.Y + aboutButton.Height)
                    state = MenuState.about;
                if (cursor.X >= highscoreButton.X && cursor.X <= highscoreButton.X + highscoreButton.Width && cursor.Y >= highscoreButton.Y && cursor.Y <= highscoreButton.Y + highscoreButton.Height)
                    state = MenuState.highscore;
                if (cursor.X >= exitButton.X && cursor.X <= exitButton.X + exitButton.Width && cursor.Y >= exitButton.Y && cursor.Y <= exitButton.Y + exitButton.Height)
                    state = MenuState.exit;

                // important!
                System.Threading.Thread.Sleep(100);
            }

            else if (cursor.LeftButton == ButtonState.Pressed && (state == MenuState.htp || state == MenuState.about || state == MenuState.highscore))
            {
                if (cursor.X >= backButton.X && cursor.X <= backButton.X + backButton.Width && cursor.Y >= backButton.Y && cursor.Y <= backButton.Y + backButton.Height)
                    state = MenuState.ready;

                // important!
                System.Threading.Thread.Sleep(200);
            }

            else if (state == MenuState.ready)
            {
                focus = ButtonFocus.none;

                if (cursor.X >= startButton.X && cursor.X <= startButton.X + startButton.Width && cursor.Y >= startButton.Y && cursor.Y <= startButton.Y + startButton.Height)
                {
                    focus = ButtonFocus.resume;
                    //buttonSound.Play();
                }
                if (cursor.X >= howToPlayButton.X && cursor.X <= howToPlayButton.X + howToPlayButton.Width && cursor.Y >= howToPlayButton.Y && cursor.Y <= howToPlayButton.Y + howToPlayButton.Height)
                {
                    focus = ButtonFocus.htp;
                    //buttonSound.Play();
                }
                if (cursor.X >= aboutButton.X && cursor.X <= aboutButton.X + aboutButton.Width && cursor.Y >= aboutButton.Y && cursor.Y <= aboutButton.Y + aboutButton.Height)
                {
                    focus = ButtonFocus.about;
                    //buttonSound.Play();
                }
                if (cursor.X >= highscoreButton.X && cursor.X <= highscoreButton.X + highscoreButton.Width && cursor.Y >= highscoreButton.Y && cursor.Y <= highscoreButton.Y + highscoreButton.Height)
                {
                    focus = ButtonFocus.highscore;
                    //buttonSound.Play();
                }
                if (cursor.X >= exitButton.X && cursor.X <= exitButton.X + exitButton.Width && cursor.Y >= exitButton.Y && cursor.Y <= exitButton.Y + exitButton.Height)
                {
                    focus = ButtonFocus.exit;
                    //buttonSound.Play();
                }
            }
        }

        public void Draw(SpriteBatch batch, SpriteFont arial)
        {
            batch.Begin();

            if (state == MenuState.exit)
                return;

            // how to play
            if (state == MenuState.htp)
            {
                batch.DrawString(arial, htpName.ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(htpName.ToString()).X / 2), (float)(1 * (this.windowHeight / 5))), new Color(167, 50, 157));
                for (int i = 0; i < instructions.Length; i++)
                    batch.DrawString(arial, instructions[i].ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(instructions[i].ToString()).X / 2), (float)((i + 2) * (this.windowHeight / 5))), Color.Tomato);
                batch.Draw(backBtnTexture, backButton, Color.White);
                batch.DrawString(arial, "Back", new Vector2(backButton.X + 20, backButton.Y + 5), Color.White);
            }

            // about
            else if (state == MenuState.about)
            {
                batch.DrawString(arial, aboutName.ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(aboutName.ToString()).X / 2), (float)(1 * (this.windowHeight / 6))), new Color(167, 50, 157));
                for (int i = 0; i < authors.Length; i++)
                    batch.DrawString(arial,  authors[i].ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(authors[i].ToString()).X / 2), (float)((i + 2) * (this.windowHeight / 6))), Color.Tomato);
                batch.Draw(backBtnTexture, backButton, Color.White);
                batch.DrawString(arial, "Back", new Vector2(backButton.X + 20, backButton.Y + 5), Color.White);

            }

            else if (state == MenuState.highscore)
            {
                batch.DrawString(arial, highscoreName.ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(highscoreName.ToString()).X / 2), (float)(1 * (this.windowHeight / 6))), new Color(167, 50, 157));
                for (int i = 0; i < highscore.Count; i++)
                    batch.DrawString(arial, (i+1).ToString() + ".     " + highscore[i].ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(highscore[i].ToString()).X / 2), (float)((i + 2) * (this.windowHeight / 18)) + 80), Color.Tomato);
                batch.Draw(backBtnTexture, backButton, Color.White);
                batch.DrawString(arial, "Back", new Vector2(backButton.X + 20, backButton.Y + 5), Color.White);

            }
            // main menu
            else if (state == MenuState.ready)
            {
                //batch.DrawString(arial, gameName.ToString(), new Vector2((float)(this.windowWidth / 2 - arial.MeasureString(gameName.ToString()).X / 2), (float)50), new Color(129, 221, 127));
                batch.Draw(titleTexture, title, Color.White);

                if (focus == ButtonFocus.resume)
                    batch.Draw(startBtnTextureOver, startButton, Color.White);
                else
                    batch.Draw(startBtnTexture, startButton, Color.White);
                if (focus == ButtonFocus.htp)
                    batch.Draw(howToPlayBtnTextureOver, howToPlayButton, Color.White);
                else
                    batch.Draw(howToPlayBtnTexture, howToPlayButton, Color.White);
                if (focus == ButtonFocus.about)
                    batch.Draw(aboutBtnTextureOver, aboutButton, Color.White);
                else
                    batch.Draw(aboutBtnTexture, aboutButton, Color.White);
                /*if (focus == ButtonFocus.highscore)
                    batch.Draw(highscoreBtnTextureover, highscoreButton, Color.White);
                else
                    batch.Draw(highscoreBtnTexture, highscoreButton, Color.White);
                if (focus == ButtonFocus.exit)
                    batch.Draw(exitBtnTextureOver, exitButton, Color.White);
                else*/
                    batch.Draw(exitBtnTexture, exitButton, Color.White);
            }

            batch.End();
        }
    }

}
