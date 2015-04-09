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

        private List<HexTile> m_lastPath=new List<HexTile>();
			
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
            Unit selUnit = atGame.ActivePlayer.SelectedUnit;
			//verifie si la souris est sortie du dernier hex
			float distFromCenter = Vector2.Distance(m_lastRefHex.SpriteCenter, m_mPosition);
			//si oui, recherche du nouveau par detection du centre le plus proche du point (hypothenuse)
			if (distFromCenter > atGame.GameBoard.HexPixelWidth / 2f)
			{
				HexTile nextHex = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				nextHex.Highlight = true;
				m_lastRefHex.Highlight = false;

				if (atGame.CurrentPhase == ATGame.GamePhase.GP_Combat)
				{

					if (m_lastRefHex.Occupant != null)
					{
						foreach (Unit u in m_lastRefHex.Occupant.GetPotentialAttackers())
						{
							//if (!u.Freeze)
							{
								if ( u != selUnit)
									u.AlphaBlinkEnable = false;
							}

						}
						
					}


					if (nextHex.Occupant != null)
					{

						if (nextHex.Occupant != m_lastRefHex.Occupant && nextHex.Occupant.Owner == atGame.OpposingPlayer)
						{

							foreach (Unit u in nextHex.Occupant.GetPotentialAttackers())
							{
								if (!u.Freeze)
								{
									if (!u.AlphaBlinkEnable && u != atGame.ActivePlayer.SelectedUnit)
										u.SetAlphaBlink(1f, 0.5f, 1f, 0.33f, true);
								}

							}
						}

					}
				}
                if ( selUnit != null)
                {

                }
	
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
			Unit clickedUnit = null;	
			//for now, only checks for click on own units
			{
				foreach (Unit u in atGame.ActivePlayer.OwnedUnits)
				{
					if (u.IsUnderCursor(m_mCurState))
					{
						Console.WriteLine("___click");
						clickedUnit = u;
						clickedUnit.Select();
						break;
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
			Unit selUnit=atGame.ActivePlayer.SelectedUnit;
			if (atGame.CurrentPhase == ATGame.GamePhase.GP_Dispatch)
			{
				if (selUnit != null)
				{
					HexTile destTile = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
					if (destTile.Occupant == null && selUnit.DispatchableOnHex(destTile))
					{
						//selUnit.StartMoveTo(destTile,false,true);
                        selUnit.StartPathTo(destTile, false, true);
					}
				}
			}

			else if (atGame.CurrentPhase == ATGame.GamePhase.GP_Movement)
			{
				if (selUnit != null)
				{
					HexTile destTile = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
					if (destTile.Occupant == null && selUnit.ReachableHexes.Contains(destTile))
					{
						selUnit.StartPathTo(destTile,true,true);
					}
				}
			}

			else if (atGame.CurrentPhase == ATGame.GamePhase.GP_Combat)
			{
				HexTile destTile = atGame.GameBoard.GetHexAtCoordinates(m_mPosition);
				if (selUnit != null)
				{

					if (destTile.Occupant != null )
					{
						bool alreadyAttacking=destTile.Occupant.Attackers.Contains(selUnit);
						bool targetLockable = (!alreadyAttacking && !selUnit.Freeze);

						if (targetLockable=selUnit.TargetUnit(destTile.Occupant, targetLockable))
						{
							Console.WriteLine("Target Locked");
							foreach (Unit u in destTile.Occupant.GetPotentialAttackers(selUnit))
							{
								alreadyAttacking = destTile.Occupant.Attackers.Contains(u);
								targetLockable = (!alreadyAttacking && !u.Freeze && targetLockable);
								//u.TargetUnit(destTile.Occupant, targetLockable);
								if (targetLockable)
									u.TargetUnit(destTile.Occupant, true);
							}														
						}
						else if (alreadyAttacking){
							Console.WriteLine("Target Unlocked");
							foreach (Unit u in destTile.Occupant.GetPotentialAttackers())
							{
								u.TargetUnit(destTile.Occupant, false);
							}		
						}							
						else
							Console.WriteLine("Undefined state");
					}
					else
						Console.WriteLine("Invalid or no target");
				}
			}
		}

	}
}
