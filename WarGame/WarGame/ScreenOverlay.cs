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
	public class ScreenOverlay : ATDrawableComponent
	{
		public enum BigMessages { Dispatch=1, Movement=2, Combat=3, GameOver=0 };
		private BigMessages m_bigMessageType=BigMessages.Dispatch;
		private SpriteFont m_bigFont;


		private Texture2D m_phaseMessageSprite;
		
		private Rectangle m_upperLine; 
		private Rectangle m_lowerLine;

		private Vector2 m_bigMessageOffset = Vector2.Zero;
		private Vector2 m_bigMessageLineSize;
		public float MessagePauseDuration = 1f;
		public float MessageScrollDuration = 1f;
		private float m_messageScrollTimer = 0f;
		private float m_messagePauseTimer = 0f;
		private float m_Xratio;
		private float m_Yratio;

		
		private DrawDelegate DrawOverlayElement = delegate{ };
		public ScreenOverlay(Game game)
			: base(game)
		{
			atGame.Components.Add(this);
			// TODO: Construct any child components here
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here

			base.Initialize();
		}

		protected override void LoadContent()
		{
			m_bigFont = atGame.Content.Load<SpriteFont>("Fonts/BigMessage");
			m_phaseMessageSprite = atGame.Content.Load<Texture2D>("phaseMessages");
			m_bigMessageLineSize = new Vector2(m_phaseMessageSprite.Width, m_phaseMessageSprite.Height / 4);
			base.LoadContent();
			
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			//m_bigMessageOffset.X = (m_bigMessageOffset.X+500f * (float)gameTime.ElapsedGameTime.TotalSeconds) % atGame.ScreenWidth;
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			// TODO: Add your update code here
			spriteBatch.Begin();
			DrawOverlayElement(gameTime);
			spriteBatch.End();

		}

		private void DrawBigMessage(GameTime gameTime)
		{
			float timerStep=(float)gameTime.ElapsedGameTime.TotalSeconds;
			
			
			float fontScale = 2f;
			float Xdone = m_messageScrollTimer / MessageScrollDuration;

			float sign = Math.Sign(0.5f - Xdone);
			//Console.WriteLine(sign+" "+m_messagePauseTimer);

			m_bigMessageOffset.Y = m_Yratio * m_bigMessageLineSize.Y/3.5f;

			if ( (0 == (int)m_bigMessageOffset.X) && (m_messagePauseTimer += timerStep) < MessagePauseDuration)
			{
				m_bigMessageOffset.X = 0;
			}
			else
			{
				m_messageScrollTimer += timerStep;
				m_bigMessageOffset.X = sign*m_Xratio * MathHelper.SmoothStep(0, m_bigMessageLineSize.X * 2, 0.5f * sign - Xdone * sign);
			}	
			

			this.spriteBatch.Draw(
				m_phaseMessageSprite, Vector2.Zero*m_Xratio + m_bigMessageOffset,
				m_upperLine, atGame.ActivePlayer.TeamColor, 0f, Vector2.Zero, m_Xratio, SpriteEffects.None, 1);

			m_bigMessageOffset.X *= -1;
			this.spriteBatch.Draw(
				m_phaseMessageSprite, new Vector2(0, m_bigMessageLineSize.Y) * m_Xratio + m_bigMessageOffset,
				m_lowerLine, atGame.ActivePlayer.TeamColor, 0f, Vector2.Zero, m_Xratio, SpriteEffects.None, 1);
			m_bigMessageOffset.X *= -1;

			/*if (m_bigMessageOffset.X > m_bigMessageLineSize.X*2){
				DrawOverlayElement -= DrawBigMessage;
			}				*/
			
			{
				//m_bigMessageOffset.X += 5f;
			}
		}

		public void ResetBigMessage()
		{
			DrawOverlayElement -= DrawBigMessage;
			m_messageScrollTimer = 0;
			m_messagePauseTimer = 0;
			m_Xratio = atGame.ScreenWidth / m_bigMessageLineSize.X;
			m_Yratio = atGame.ScreenHeight / m_bigMessageLineSize.Y;

			m_bigMessageOffset = Vector2.Zero;
			m_bigMessageOffset.X = -m_bigMessageLineSize.X;
		}

		public void DisplayMessage(BigMessages msgType, Player player=null,float scrollDuration=1.5f, float pauseDuration=1.5f)
		{
			ResetBigMessage();
			
			if (player == null) player = atGame.ActivePlayer;

			int rowOffset = (int)msgType;
			Console.WriteLine(rowOffset);

			m_upperLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * rowOffset, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y * (rowOffset));
						
			m_lowerLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * 3, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y * 4);


			MessagePauseDuration = pauseDuration;
			MessageScrollDuration = scrollDuration;

			DrawOverlayElement += DrawBigMessage;
		}

	}
}
