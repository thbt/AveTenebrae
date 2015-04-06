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


namespace WarGame
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public partial class InputManager : ATComponent
	{
		private int colorCycle=1; //pour tester les palettes de couleurs
		
		private KeyboardState m_kbCurState, m_kbLastState;

		private HexTile m_lastRefHex;
		private List<HexTile> m_lastRangedNeighbourhood;

		private List<HexTile> m_tileList;

		private float m_panningStep = 2.5f;
		private delegate void PhaseInputManager(GameTime gameTime);
		private PhaseInputManager PhaseKeyboardInput;
			
		public InputManager(Game game)
			: base(game)
		{
			// TODO: Construct any child components here
			atGame.Components.Add(this);
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here
			m_mCurState = Mouse.GetState();
			m_mLastState = m_mCurState;

			m_kbCurState = Keyboard.GetState();
			m_kbLastState = m_kbCurState;

			m_mPosition = Vector2.Zero;
			m_cursor = new FreeCursor(atGame);
			base.Initialize();
			m_tileList = atGame.GameBoard.GetTileList();
			m_lastRefHex = m_tileList.ElementAt(m_tileList.Count/2+atGame.GameBoard.ColumnCount/2);
			m_lastRangedNeighbourhood = new List<HexTile>();
			//m_lastRefHex.offsetColor = new Color(1f, 0f, 0f, 0.75f);
			PhaseKeyboardInput=delegate (GameTime gameTime){};
			PhaseKeyboardInput += DispatchKeyboardInput;

		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			
			m_mCurState = Mouse.GetState();
			m_kbCurState = Keyboard.GetState();

			m_mPosition.X = m_mCurState.X;
			m_mPosition.Y = m_mCurState.Y;

			MouseUpdate(gameTime);
			KeyboardUpdate(gameTime);

			base.Update(gameTime);
			m_mLastState = m_mCurState;
			m_kbLastState = m_kbCurState;
			
		}


		private void KeyboardUpdate(GameTime gameTime)
		{

			//debug mode
			if (m_kbCurState.IsKeyDown(Keys.LeftControl) || m_kbCurState.IsKeyDown(Keys.RightControl))
			{
				DebugModeKeyboardInput(gameTime);
				ATGame.DEBUG_MODE = true;
			}
			else
				ATGame.DEBUG_MODE = false;

			//general
			if (m_kbCurState.IsKeyDown(Keys.End))
			{
				//pour forcer la fin de tour d'une unité si aucun autre choix possible/avantageux
				if (atGame.ActivePlayer.SelectedUnit != null)
				{
					atGame.ActivePlayer.SelectedUnit.Freeze=true;
				}
			}

			//panning camera
			if (m_kbCurState.IsKeyDown(Keys.Left))
			{
				atGame.Panning.X -= m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Right))
			{
				atGame.Panning.X += m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Up))
			{
				atGame.Panning.Y -= m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Down))
			{
				atGame.Panning.Y += m_panningStep;
			}
			PhaseKeyboardInput(gameTime);

			
		}

		private void DebugModeKeyboardInput(GameTime gameTime)
		{

			//options debug: edition de types de tiles
			if (m_kbCurState.IsKeyDown(Keys.F1))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				hex.ChangeToPlain();
			}
			if (m_kbCurState.IsKeyDown(Keys.F2))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				hex.ChangeToHill();
			}
			if (m_kbCurState.IsKeyDown(Keys.F3))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				hex.ChangeToForest();
			}

			//options debug: placement d'unités
			if (m_kbCurState.IsKeyDown(Keys.F5))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Heavy unit = new Heavy(atGame, atGame.ActivePlayer);
				if (hex.Occupant == null)
					unit.PutOnHex(hex);
				else
					unit.Dispose();
			}
			if (m_kbCurState.IsKeyDown(Keys.F6))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Scout unit = new Scout(atGame, atGame.ActivePlayer);
				if (hex.Occupant == null)
					unit.PutOnHex(hex);
				else
					unit.Dispose();
			}
			if (m_kbCurState.IsKeyDown(Keys.F7))
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Sniper unit = new Sniper(atGame, atGame.ActivePlayer);
				if (hex.Occupant == null)
					unit.PutOnHex(hex);
				else
					unit.Dispose();
			}

			//options debug: cycling de couleurs de team
			if (m_kbCurState.IsKeyDown(Keys.C) && m_kbLastState.IsKeyUp(Keys.C))
			{
				colorCycle = (++colorCycle % 3);
				switch (colorCycle)
				{
					//red
					case 0: atGame.ActivePlayer.TeamColor = TeamColors.Red; break;
					//green
					case 1: atGame.ActivePlayer.TeamColor = TeamColors.Green; break;
					//blue
					case 2: atGame.ActivePlayer.TeamColor = TeamColors.Blue; break;
					default: atGame.ActivePlayer.TeamColor = Color.White; break;
				}

				//atGame.activePlayer.teamColor.
			}

			//options debug: force l'affichage d'un message de début de gamephase
			if (m_kbCurState.IsKeyDown(Keys.B) && m_kbLastState.IsKeyUp(Keys.B))
			{
				atGame.Overlay.DisplayMessage(ScreenOverlay.BigMessages.Dispatch);
			}

			//options debug: test de selection de group par nombre (propagation d'une source)
			if (m_kbCurState.IsKeyDown(Keys.D) && m_kbLastState.IsKeyUp(Keys.D))
			{
				HashSet<HexTile> dispatchArea = new HashSet<HexTile>();
				atGame.GameBoard.GetNeighbourGroup(
					atGame.GameBoard.GetHexAtCoordinates(m_mPosition),
					dispatchArea, 10);

			}


			//options debug: swap player turns
			if (m_kbCurState.IsKeyDown(Keys.S) && m_kbLastState.IsKeyUp(Keys.S))
			{
				atGame.SwapPlayerTurns();
			}

		}

		private void DispatchKeyboardInput(GameTime gameTime)
		{
			
			if (m_kbCurState.IsKeyDown(Keys.F5) && m_kbLastState.IsKeyUp(Keys.F5)
				&& atGame.ActivePlayer.OwnedUnits.Count(u => u is Heavy) < atGame.nbHeavies)
			{
				Console.WriteLine(atGame.ActivePlayer.OwnedUnits.Count);
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Heavy unit = new Heavy(atGame, atGame.ActivePlayer);
				if (unit.DispatchableOnHex(hex))
					unit.DispatchOnHex(hex);
				else
					unit.Dispose();
			}
			if (m_kbCurState.IsKeyDown(Keys.F6) && m_kbLastState.IsKeyUp(Keys.F6)
				&& atGame.ActivePlayer.OwnedUnits.Count(u => u is Scout) < atGame.nbScout)
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Scout unit = new Scout(atGame, atGame.ActivePlayer);
				if (unit.DispatchableOnHex(hex))
					unit.DispatchOnHex(hex);
				else
					unit.Dispose();
			}
			if (m_kbCurState.IsKeyDown(Keys.F7) && m_kbLastState.IsKeyUp(Keys.F7)
				&& atGame.ActivePlayer.OwnedUnits.Count(u => u is Sniper) < atGame.nbSnipers)
			{
				HexTile hex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				Sniper unit = new Sniper(atGame, atGame.ActivePlayer);
				if (unit.DispatchableOnHex(hex))
					unit.DispatchOnHex(hex);
				else
					unit.Dispose();
			}
		}
	}
}
