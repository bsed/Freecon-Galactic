using System;
using System.Collections;
using Freecon.Client.Core.Coroutines;
using Freecon.Client.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LerpVisualTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LerpVisualTest : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        private Texture2D _dotTexture;

        protected Vector2 _position;

        protected ICoroutineManager _coroutineManager;

        public LerpVisualTest()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = true;
            _coroutineManager = new CoroutineManager();
            _coroutineManager.Run(DoSomethingAsync());
        }

        public IEnumerable DoSomethingAsync()
        {
            _position = new Vector2(10, 10);
            Console.WriteLine("Starting");

            var enumerator = MoveToPosition(new Vector2(200, 200)).GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return 0;
            }

            Console.WriteLine("Done");
        }

        public IEnumerable MoveToPosition(Vector2 destination)
        {
            float amount = 0;
            var startPosition = _position;

            while (Vector2.Distance(_position, destination) > 5)
            {
                amount += 0.005f;
                _position = Vector2.Lerp(startPosition, destination, amount);

                yield return 0;
            }
        } 

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            CreateDotTexture();
        }

        private void CreateDotTexture()
        {
            _dotTexture = new Texture2D(GraphicsDevice, 10, 10);

            Color[] data = new Color[10 * 10];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Color.Chocolate;
            }

            _dotTexture.SetData(data);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            _coroutineManager.Update(new XNAGameTimeService(gameTime));

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            _spriteBatch.Draw(_dotTexture, _position, Color.Red);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
