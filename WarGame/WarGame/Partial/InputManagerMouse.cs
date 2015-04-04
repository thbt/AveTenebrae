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

		private FreeCursor m_cursor;
		//timestamp du dernier debut de click
		private double m_leftButtonHoldTimer = 0;
		private double m_middleButtonHoldTimer = 0;
		private double m_rightButtonHoldTimer = 0;
		//temps en ms avant que le click soit considéré comme maintenu
		private double m_clickInterval = 150;

		private Vector2 m_mPosition;
		private MouseState m_mCurState, m_mLastState;
		

			
		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>

		private void MouseUpdate(GameTime gameTime)
		{

			MouseHover(gameTime);
			MouseLeftUpdate(gameTime);
			MouseMiddleUpdate(gameTime);
			MouseRightUpdate(gameTime);

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
						
			{
				foreach (Unit u in atGame.ActivePlayer.ownedUnits)
				{
					if (u.IsUnderCursor(m_mCurState))
					{
						u.Select();
					}
				}
				atGame.GameBoard.GetHexAtCoordinates(m_mPosition).Select();
			}

			Console.WriteLine("Mouse pos: "+m_mCurState.X + " " + m_mCurState.Y);

		}

		public void OnLeftMousePressed(GameTime gameTime)
		{

		}
		public void OnLeftMouseReleased(GameTime gameTime)
		{

		}

		public void MouseMiddleUpdate(GameTime gameTime)
		{

			bool clicked = m_mCurState.MiddleButton == ButtonState.Released
						&& m_mLastState.MiddleButton == ButtonState.Pressed
						&& m_middleButtonHoldTimer < m_clickInterval;
			bool holding = m_mCurState.MiddleButton == ButtonState.Pressed
						&& m_mLastState.MiddleButton == ButtonState.Pressed
						&& m_middleButtonHoldTimer > m_clickInterval;


			if (m_mCurState.MiddleButton == ButtonState.Released)
				m_middleButtonHoldTimer = 0;
			else
				m_middleButtonHoldTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

			if (clicked)
			{
				OnMiddleMouseClick(gameTime);
				clicked = false;
			}
			if (holding)
			{
				OnMiddleMousePressed(gameTime);
			}

		}

		private void OnMiddleMousePressed(GameTime gameTime)
		{
			atGame.Panning += m_mPosition - new Vector2(m_mLastState.X, m_mLastState.Y);
		}

		private void OnMiddleMouseClick(GameTime gameTime)
		{
			//throw new NotImplementedException();
		}

		public void MouseRightUpdate(GameTime gameTime)
		{

			bool clicked = m_mCurState.RightButton == ButtonState.Released
						&& m_mLastState.RightButton == ButtonState.Pressed
						&& m_rightButtonHoldTimer < m_clickInterval;
			bool holding = m_mCurState.RightButton == ButtonState.Pressed
						&& m_mLastState.RightButton == ButtonState.Pressed
						&& m_rightButtonHoldTimer > m_clickInterval;


			if (m_mCurState.RightButton == ButtonState.Released)
				m_rightButtonHoldTimer = 0;
			else
				m_rightButtonHoldTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

			if (clicked)
			{
				OnRightMouseClick(gameTime);
				clicked = false;
			}

		}

		private void OnRightMouseClick(GameTime gameTime)
		{
			if (atGame.ActivePlayer.selUnit != null)
			{
				HexTile destTile = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				if (destTile.Occupant == null)
				{
					atGame.ActivePlayer.selUnit.StartMoveTo(destTile);
				}
			}
		}

	}
}
