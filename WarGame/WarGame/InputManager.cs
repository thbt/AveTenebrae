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
		private double m_clickInterval = 125;

		private MouseState m_mCurState, m_mLastState;
		private Vector2 m_mPosition;

		private HexTile m_lastRefHex;
		private List<HexTile> m_tileList;

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
			m_mPosition = Vector2.Zero;
			m_cursor = new Cursor(atGame);
			base.Initialize();
			m_tileList = atGame.GameBoard.GetTileList();
			m_lastRefHex = m_tileList.ElementAt(m_tileList.Count/2+atGame.GameBoard.ColumnCount/2);
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
			m_mPosition.X = m_mCurState.X;
			m_mPosition.Y = m_mCurState.Y;

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
				//Console.WriteLine(distFromCenter);
				HexTile nextHex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition.X, m_mPosition.Y);
				/*List<HexTile> rangedNeighbourhood = atGame.GameBoard.GetNeighboursRanged(nextHex, 3);

				foreach (HexTile h in rangedNeighbourhood)
				{
					h.colorOffset = new Vector4(1f, -0.25f, -0.25f, 0.75f);
				}
				*/
				m_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);
				nextHex.colorOffset = new Vector4(1f, -0.25f, -0.25f, 0.75f);
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


			Console.WriteLine("Mouse pos: "+m_mCurState.X + " " + m_mCurState.Y);
			float xPos = (float)m_mPosition.X;
			float yPos = (float)m_mPosition.Y;
			float gWidth = (float)(atGame.GameBoard.BoardPixelWidth) * (float)atGame.GameBoard.tileMap.GetLength(0);
			float gHeight = (float)(atGame.GameBoard.BoardPixelHeight) * (float)atGame.GameBoard.tileMap.GetLength(1);
		
			//! ZONE TEMPORAIREMENT BROUILLON 	
			//foreach (atGame.GameBoard.GetTileList()
			/*

			Vector2 gPosition = new Vector2(
				xPos / gWidth * 0.75f,
				yPos / gHeight + (((int)xPos % 2 != 0) ? (gHeight / 2f) : 0));
			int column = (int)(xPos / atGame.GameBoard.HexPixelWidth);
			int row;

			bool colIsOdd = (int)column % 2 != 0;

			// Is the row an even number?
			if (colIsOdd) // No: Calculate normally
				row = (int)(yPos / (float)atGame.GameBoard.HexPixelHeight);
			else // Yes: Offset mouse.x to match the offset of the row
				row = (int)((yPos + atGame.GameBoard.HexPixelHeight / 2f) / (float)atGame.GameBoard.HexPixelHeight);



			// column is more complex because it has to
			// take into account that every other row
			// is offset by half the width of a hexagon

			//return hexagons[row][column];

			Console.WriteLine("Mouse gridpos: " +column+ " "+row);
			//Board selBoard=atGame.GameBoard.tileMap[];
			*/
		}

		public void OnLeftMousePressed(GameTime gameTime)
		{

		}
		public void OnLeftMouseReleased(GameTime gameTime)
		{

		}
	}
}
