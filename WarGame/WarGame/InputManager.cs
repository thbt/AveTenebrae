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
	public class InputManager : ATComponent
	{
		
		private Cursor m_cursor;
		//timestamp du dernier debut de click
		private double m_leftButtonHoldTimer = 0;
		//temps en ms avant que le click soit considéré comme maintenu
		private double m_clickInterval = 150;

		private MouseState m_mCurState, m_mLastState;
		private KeyboardState m_kbCurState, m_kbLastState;

		private Vector2 m_mPosition;

		private HexTile m_lastRefHex;
		private List<HexTile> m_lastRangedNeighbourhood;

		private List<HexTile> m_tileList;

		private float m_panningStep = 2.5f;
			
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
			m_cursor = new Cursor(atGame);
			base.Initialize();
			m_tileList = atGame.GameBoard.GetTileList();
			m_lastRefHex = m_tileList.ElementAt(m_tileList.Count/2+atGame.GameBoard.ColumnCount/2);
			m_lastRangedNeighbourhood = new List<HexTile>();
			//m_lastRefHex.offsetColor = new Color(1f, 0f, 0f, 0.75f);

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

			KeyboardUpdate(gameTime);

			MouseHover(gameTime);
			MouseLeftUpdate(gameTime);

			base.Update(gameTime);
			m_mLastState = m_mCurState;
			
		}


		private void MouseHover(GameTime gameTime)
		{
			//verifie si la souris est sortie du dernier hex
			float distFromCenter = Vector2.Distance(m_lastRefHex.SpriteCenter, m_mPosition);
			//si oui, recherche du nouveau par detection du centre le plus proche du point (hypothenuse)
			if (distFromCenter > atGame.GameBoard.HexPixelWidth / 2f)
			{
				foreach (HexTile h in m_lastRangedNeighbourhood)
				{
					h.colorMultiplier = Vector4.One;
					h.colorOffset = Vector4.Zero;
				}	
				
				//Console.WriteLine(distFromCenter);
				HexTile nextHex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				List<HexTile> rangedNeighbourhood = atGame.GameBoard.GetNeighboursRanged(nextHex, 2);
				m_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);

				foreach (HexTile h in rangedNeighbourhood)
				{
					h.colorMultiplier = new Vector4(1.25f, 1.5f, 1.25f, 1);
					h.colorOffset = new Vector4(0.5f, -0.25f, -0.25f, 0.5f);
				}

				nextHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.75f);
				m_lastRangedNeighbourhood = rangedNeighbourhood;
				m_lastRefHex = nextHex;

			}

		}

		public void MouseLeftUpdate(GameTime gameTime)
		{
			
			bool clicked = m_mCurState.LeftButton == ButtonState.Released
						&& m_mLastState.LeftButton == ButtonState.Pressed
						&& m_leftButtonHoldTimer < m_clickInterval;
			bool holding = m_mCurState.LeftButton == ButtonState.Pressed
						&& m_mLastState.LeftButton == ButtonState.Pressed
						&& m_leftButtonHoldTimer > m_clickInterval;


			if (m_mCurState.LeftButton == ButtonState.Released)
				m_leftButtonHoldTimer = 0;	
			else
				m_leftButtonHoldTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

			if (clicked)
			{
				OnLeftMouseClick(gameTime);
				clicked = false;
			}
			
		}


		public void OnLeftMouseClick(GameTime gameTime)
		{

			atGame.GameBoard.GetHexAtCoordinates(m_mPosition).Select();
			Console.WriteLine("Mouse pos: "+m_mCurState.X + " " + m_mCurState.Y);

		}

		public void OnLeftMousePressed(GameTime gameTime)
		{

		}
		public void OnLeftMouseReleased(GameTime gameTime)
		{

		}


		private void KeyboardUpdate(GameTime gameTime)
		{

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

			if (m_kbCurState.IsKeyDown(Keys.Left))
			{
				atGame.panning.X -= m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Right))
			{
				atGame.panning.X += m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Up))
			{
				atGame.panning.Y -= m_panningStep;
			}
			if (m_kbCurState.IsKeyDown(Keys.Down))
			{
				atGame.panning.Y += m_panningStep;
			}
		}
	}
}
