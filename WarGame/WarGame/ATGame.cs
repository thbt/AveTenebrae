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

		
		public ScreenOverlay Overlay { get; private set; }
		public int nbArchers = 1, nbHeavyKnights = 1, nbCavalry = 1;
		public static bool DEBUG_MODE { get; set; }

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

		private float m_phaseChangeTimer = 0;

		public Board GameBoard { get; protected set;}

		public enum GamePhase
		{
			GP_Dispatch,
			GP_Movement,
			GP_Combat,
			GP_GameOver
		}
		public GamePhase CurrentPhase { get; set; }
 
		private delegate void GamePhaseLogic(GameTime gameTime);
		private GamePhaseLogic m_currentPhaseLogic;

		public ATGame() {
			graphics = new GraphicsDeviceManager(this);
			
			ScreenWidth = 1280;
			ScreenHeight = 720;
			
			ScreenWidth = 1024;
			ScreenHeight = 768;
		
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
			Overlay = new ScreenOverlay(this);
			Panning = Vector2.Zero;
			GameBoard = new Board(this);			

			PlayerA = new Player(this,TeamColors.Red,true);
			PlayerB = new Player(this,TeamColors.Blue,false);
			PlayerA.Name = "Human A";
			PlayerB.Name = "Human B";

			ActivePlayer = PlayerA;
			OpposingPlayer = PlayerB;

			inputManager = new InputManager(this);
			
			spriteBatch = new SpriteBatch(GraphicsDevice);
			m_currentPhaseLogic = delegate(GameTime GameTime){};			

			base.Initialize();
			Panning.Y += Overlay.TopPanelArea.Height+32;
			PrepareDispatchPhase();
			//m_currentPhaseLogic += DispatchPhase; //!!! retirer cette ligne après les tests !!!
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
			if (ActivePlayer.SelectedUnit!=null)
				ActivePlayer.SelectedUnit.UnSelect();
			if (ActivePlayer.SelectedHex != null)
				ActivePlayer.SelectedHex.UnSelect();

			Player oldActivePlayer = ActivePlayer;
			ActivePlayer = OpposingPlayer;
			OpposingPlayer = oldActivePlayer;

			if (ActivePlayer.SelectedUnit != null)
				ActivePlayer.SelectedUnit.UnSelect();
			if (ActivePlayer.SelectedHex != null)
				ActivePlayer.SelectedHex.UnSelect();

			ActivePlayer.UnfreezeUnits();
			OpposingPlayer.UnfreezeUnits();
		}


		private void PrepareDispatchPhase()
		{
			m_currentPhaseLogic -= DispatchPhase;
			int unitsPerTeam = nbArchers + nbCavalry + nbHeavyKnights;
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

			Overlay.DisplayMessage(ScreenOverlay.BigMessages.Dispatch);
			m_currentPhaseLogic+=DispatchPhase;
			CurrentPhase = GamePhase.GP_Dispatch;
		}

		private void DispatchPhase(GameTime gameTime)
		{
			int unitsPerTeam = nbArchers + nbCavalry + nbHeavyKnights;
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);
			bool readyA = (this.PlayerA.OwnedUnits.Count >= unitsPerTeam);
			bool readyB = (this.PlayerB.OwnedUnits.Count >= unitsPerTeam);


			if (ActivePlayer == PlayerA && readyA && nbFrozenUnits >= unitsPerTeam)
			{
				if (ActivePlayer.SelectedUnit != null)
					ActivePlayer.SelectedUnit.UnSelect();

				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					
					m_phaseChangeTimer = 0;
					//Overlay.DisplayMessage(ScreenOverlay.BigMessages.Dispatch);
					
					Console.WriteLine("Swapping players");
					SwapPlayerTurns();
					PrepareDispatchPhase();
				}

			}

			if (readyA && readyB && nbFrozenUnits >= unitsPerTeam)
			{
				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					foreach (HexTile h in GameBoard.GetTileList())
					{
						h.SetDispatchable(false, false);
						h.SetHighlighted(false);
						h.ResetGraphics(true);
					}

					m_phaseChangeTimer = 0;
					Console.WriteLine("Dispatch phase finished");
					m_currentPhaseLogic -= DispatchPhase;
					SwapPlayerTurns();
					HexTile.ResetGraphics(GameBoard.GetTileList(), true);
					m_currentPhaseLogic = MovementPhase;
					
					CurrentPhase = GamePhase.GP_Movement;
					Overlay.DisplayMessage(ScreenOverlay.BigMessages.Movement);
				}

			}

		}

		private void MovementPhase(GameTime gameTime)
		{
			
			//quand terminée, lancer la CombatPhase
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);
			/*foreach (Unit u in ActivePlayer.ownedUnits)
			{
				if (u.Freeze == true) nbMovedUnits++;
			}*/

			if (nbFrozenUnits >= ActivePlayer.OwnedUnits.Count)//forcé false pour les tests de movements
			{
				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					m_phaseChangeTimer = 0;
					Console.WriteLine("Movement phase finished");

					if (ActivePlayer.SelectedUnit != null)
						ActivePlayer.SelectedUnit.UnSelect();

					ActivePlayer.UnfreezeUnits();
					HexTile.ResetGraphics(GameBoard.GetTileList(), true);
					CurrentPhase = GamePhase.GP_Combat;
					m_currentPhaseLogic = CombatPhase;
					Overlay.DisplayMessage(ScreenOverlay.BigMessages.Combat);
				}				
			}
		}


		private void CombatPhase(GameTime gameTime)
		{
			
			//quand terminée, lancer la MovementPhase pour l'autre équipe
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);			

			if (nbFrozenUnits >= ActivePlayer.OwnedUnits.Count)
			{
				GameBoard.ExecuteBattle();

				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 1.5f)
				{
					m_phaseChangeTimer = 0;
					SwapPlayerTurns();
					HexTile.ResetGraphics(GameBoard.GetTileList(), true);

					if (ActivePlayer.OwnedUnits.Count > 0 && OpposingPlayer.OwnedUnits.Count > 0)
					{
						CurrentPhase = GamePhase.GP_Movement;
						m_currentPhaseLogic = MovementPhase;
						Overlay.DisplayMessage(ScreenOverlay.BigMessages.Movement);
						Console.WriteLine("Combat phase finished");
					}
					else
					{
						CurrentPhase = GamePhase.GP_GameOver;
						m_currentPhaseLogic = GameOverPhase;
						LaunchGameOver();
						
					}

				}

			}
			
		}

		private void GameOverPhase(GameTime gameTime)
		{
			m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			Console.WriteLine("Game over");
			if ((int)m_phaseChangeTimer == 10)
			{
				m_phaseChangeTimer = float.PositiveInfinity;
				Overlay.DisplayEasterEgg();
				m_currentPhaseLogic = delegate { };
			}
		}

		public void LaunchGameOver()
		{
			foreach (Unit u in Components.OfType<Unit>())
			{
				u.Kill();
			}
			CurrentPhase = GamePhase.GP_GameOver;
			m_currentPhaseLogic = GameOverPhase;
			Overlay.DisplayMessage(ATGame.GamePhase.GP_GameOver, null, 1f, float.PositiveInfinity);
		}
	}
}
