using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AStar;



namespace WarGame {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ATGame : Microsoft.Xna.Framework.Game {

		GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		InputManager inputManager;

		public Vector2 Panning;
		public int ScreenWidth { get; protected set; }
		public int ScreenHeight { get; protected set; }

		public Player PlayerA { get; private set;}
		public Player PlayerB { get; private set;}
		public Player ActivePlayer;
		public Player OpposingPlayer;

		public Board GameBoard { get; protected set;}

		private delegate void GamePhaseLogic(GameTime gameTime);
		private GamePhaseLogic m_currentPhaseLogic;

		public ATGame() {
			graphics = new GraphicsDeviceManager(this);
			ScreenWidth = 1280;
			ScreenHeight = 720;
			graphics.PreferredBackBufferWidth = ScreenWidth;
			graphics.PreferredBackBufferHeight = ScreenHeight;
			graphics.SynchronizeWithVerticalRetrace = true;
			graphics.PreferMultiSampling = true;
			
			graphics.ApplyChanges();
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			// TODO: Add your initialization logic here
			Panning = Vector2.Zero;
			GameBoard = new Board(this);			

			PlayerA = new Player(this,TeamColors.Red,true);
			PlayerB = new Player(this,TeamColors.Blue,false);
			ActivePlayer = PlayerA;
			OpposingPlayer = PlayerB;

			inputManager = new InputManager(this);
			
			spriteBatch = new SpriteBatch(GraphicsDevice);
			m_currentPhaseLogic = delegate(GameTime GameTime){};

			base.Initialize();

			PrepareDispatchPhase();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			ResourceManager.font = Content.Load<SpriteFont>("Fonts/Arial");
			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here
			
			m_currentPhaseLogic(gameTime);
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {

			spriteBatch.Begin();
			GraphicsDevice.Clear(Color.MidnightBlue);
			
			// TODO: Add your drawing code here

			base.Draw(gameTime);
			spriteBatch.End();
		}

		public void SwapPlayerTurns()
		{
			if (ActivePlayer.selUnit!=null)
				ActivePlayer.selUnit.UnSelect();
			if (ActivePlayer.SelectedHex != null)
				ActivePlayer.SelectedHex.UnSelect();

			Player oldActivePlayer = ActivePlayer;
			ActivePlayer = OpposingPlayer;
			OpposingPlayer = oldActivePlayer;
		}


		public void PrepareDispatchPhase()
		{
			int unitsPerTeam = 9;
			int dispatchSpotsPerTeam = unitsPerTeam * 2;
			int dispatchColumns = 3;

			//dispatch team A
			for (int x = 0; x < dispatchColumns; x++)
			{
				for (int y = 0; y < GameBoard.RowCount; y++)
				{
					GameBoard.tileMap[y, x].SetDispatchable(true, false);
				}
			}


			//dispatch team B
			for (int x = dispatchColumns; x > 0; x--)
			{
				for (int y = 0; y < GameBoard.RowCount; y++)
				{
					GameBoard.tileMap[y, GameBoard.ColumnCount-x].SetDispatchable(false, true);
				}
			}

			m_currentPhaseLogic+=DispatchPhase;
		}

		public void DispatchPhase(GameTime gameTime)
		{
			int unitsPerTeam=9;
			bool readyA = (this.PlayerA.ownedUnits.Count >= unitsPerTeam);
			bool readyB = (this.PlayerB.ownedUnits.Count >= unitsPerTeam);

			if (ActivePlayer==PlayerA && readyA)
			{
				Console.WriteLine("Swapping players");
				SwapPlayerTurns();
			}

			if (readyA && readyB)
			{
				Console.WriteLine("Dispatch phase finished");
				m_currentPhaseLogic -= DispatchPhase;
			}

		}
	}
}
