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
		private SpriteFont m_bigFont;

		private string m_dispatchMessage="Dispatch: {0}";
		private string m_currentMessage="";

		private Texture2D m_phaseMessageSprite;
		private Vector2 m_bigMessageOffset = Vector2.Zero;
		private Vector2 m_bigMessageLineSize;
		public float MessagePauseDuration = 1f;
		public float MessageScrollDuration = 1f;
		private float m_messageScrollTimer = 0f;
		private float m_messagePauseTimer = 0f;
		
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
			
			m_currentMessage = "Test Message";
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
			//DrawBigMessage(gameTime);
			spriteBatch.End();

		}

		private void DrawBigMessage(GameTime gameTime)
		{
			float timerStep=(float)gameTime.ElapsedGameTime.TotalSeconds;
			
			
			float fontScale = 5f;
			float Xdone = m_messageScrollTimer / MessageScrollDuration;
			float Xratio = atGame.ScreenWidth/m_bigMessageLineSize.X;
			float sign = Math.Sign(0.5f - Xdone);
			Console.WriteLine(sign+" "+m_messagePauseTimer);
			

			Rectangle upperLine = new Rectangle((int)Vector2.Zero.X, (int)Vector2.Zero.Y, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y);
			Rectangle lowerLine = new Rectangle((int)Vector2.Zero.X, (int)m_bigMessageLineSize.Y*3, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y*4);

			m_bigMessageOffset.Y = Xratio * m_bigMessageLineSize.Y;

			if ( (0 == (int)m_bigMessageOffset.X) && (m_messagePauseTimer += timerStep) < MessagePauseDuration)
			{
				m_bigMessageOffset.X = 0;
			}
			else
			{
				m_messageScrollTimer += timerStep;
				m_bigMessageOffset.X = sign*Xratio * MathHelper.SmoothStep(0, m_bigMessageLineSize.X * 2, 0.5f * sign - Xdone * sign);
			}	
			

			this.spriteBatch.Draw(
				m_phaseMessageSprite, Vector2.Zero*Xratio + m_bigMessageOffset,
				upperLine, atGame.ActivePlayer.TeamColor, 0f, Vector2.Zero, Xratio, SpriteEffects.None, 1);

			m_bigMessageOffset.X *= -1;
			this.spriteBatch.Draw(
				m_phaseMessageSprite, new Vector2(0, m_bigMessageLineSize.Y) * Xratio + m_bigMessageOffset,
				lowerLine, atGame.ActivePlayer.TeamColor, 0f, Vector2.Zero, Xratio, SpriteEffects.None, 1);
			m_bigMessageOffset.X *= -1;

			/*if (m_bigMessageOffset.X > m_bigMessageLineSize.X*2){
				DrawOverlayElement -= DrawBigMessage;
			}				*/
			
			{
				//m_bigMessageOffset.X += 5f;
			}
		}

		public void DisplayDispatchMessage(float scrollDuration=1f, float pauseDuration=1f)
		{
			DrawOverlayElement -= DrawBigMessage;
			MessagePauseDuration = pauseDuration;
			MessageScrollDuration = scrollDuration;
			m_messageScrollTimer=0;
			m_messagePauseTimer = 0;
			m_bigMessageOffset = Vector2.Zero;
			m_bigMessageOffset.X -= m_bigMessageLineSize.X;
			DrawOverlayElement += DrawBigMessage;
		}

		private string DispatchMessage(Player player)
		{
			return string.Format(m_dispatchMessage, player.Name);
		} 
	}
}
