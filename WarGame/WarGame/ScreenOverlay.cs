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
		public bool IsDisplayingMessage { get; protected set; }
		public enum BigMessages { Dispatch=1, Movement=2, Combat=3, GameOver=4 };
		private BigMessages m_bigMessageType=BigMessages.Dispatch;
		private SpriteFont m_bigFont;

		private Texture2D m_currentMessageSprite;
		private Texture2D m_phaseMessageSprite;
		private Texture2D m_gameoverMessageSprite;
		
		private Rectangle m_upperLine; 
		private Rectangle m_lowerLine;
		private Color m_upperLineColor;
		private Color m_lowerLineColor;

		private float m_sndTimer = 0f;
		private float m_sndStartDelay=0f;
		private SoundEffect m_sndFatality;
		private SoundEffect m_sndToasty;
		private SoundEffect m_sndSwoosh;

		private Vector2 m_bigMessageOffset = Vector2.Zero;
		private Vector2 m_bigMessageLineSize;
		public float MessagePauseDuration = 1f;
		public float MessageScrollDuration = 1f;
		private float m_messageScrollTimer = 0f;
		private float m_messagePauseTimer = 0f;
		private float m_Xratio;
		private float m_Yratio;
		private float m_Xdone;

		public Rectangle TopPanelArea { get; protected set; }
		private Texture2D m_topPanelBG;
		
		private DrawDelegate DrawOverlayElement = delegate{ };
		private delegate void UpdateDlg(GameTime gameTime);
		private UpdateDlg UpdateActions = delegate {};
		public ScreenOverlay(Game game)
			: base(game)
		{
			atGame.Components.Add(this);
			this.DrawOrder = int.MaxValue-10;
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
			m_gameoverMessageSprite = atGame.Content.Load<Texture2D>("gameoverMessages");
			m_sndFatality = atGame.Content.Load<SoundEffect>("Fatality");
			m_sndToasty = atGame.Content.Load<SoundEffect>("Toasty");
			m_sndSwoosh = atGame.Content.Load<SoundEffect>("Swooshing01");

			m_currentMessageSprite = m_phaseMessageSprite;
			m_bigMessageLineSize = new Vector2(m_phaseMessageSprite.Width, m_phaseMessageSprite.Height / 4);
			base.LoadContent();

			m_topPanelBG = new Texture2D(GraphicsDevice, 1, 1);
			m_topPanelBG.SetData(new[] { Color.White });
			TopPanelArea = new Rectangle(0, 0, atGame.ScreenWidth, atGame.ScreenHeight / 8);
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			UpdateActions(gameTime);
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			
			// TODO: Add your update code here
			spriteBatch.Begin();
			DrawTopPanelItems(gameTime);
			DrawOverlayElement(gameTime);
			spriteBatch.End();

		}

		private void DrawTopPanelItems(GameTime gameTime)
		{
			spriteBatch.Draw(m_topPanelBG, TopPanelArea, Color.Black * 0.25f);
			Vector2 slotSize = new Vector2(TopPanelArea.Width/4,TopPanelArea.Height/2);
			Vector2 textOffset = new Vector2(16, slotSize.Y / 16);

			int row = 0, col = 0;
			this.spriteBatch.DrawString(ResourceManager.font, "F5:\n Place Infantry", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);

			row = 1; col = 0;
			this.spriteBatch.DrawString(ResourceManager.font, "F6:\n Place Cavalry", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);

			row = 0; col = 1;
			this.spriteBatch.DrawString(ResourceManager.font, "F7:\n Place Archer", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);

			row = 1; col = 1;
			this.spriteBatch.DrawString(ResourceManager.font, "Right-click on hex:\n Move selected unit there", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);

			row = 0; col = 2;
			this.spriteBatch.DrawString(ResourceManager.font, "END:\n End unit turn", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);

			row = 1; col = 2;
			this.spriteBatch.DrawString(ResourceManager.font, "Space:\n End all unit turns", new Vector2(slotSize.X * col, slotSize.Y * row) + textOffset,
				atGame.ActivePlayer.TeamColor);
		}

		private void DrawBigMessage(GameTime gameTime)
		{
			float timerStep=(float)gameTime.ElapsedGameTime.TotalSeconds;			
			
			float fontScale = 2f;
			m_Xdone = m_messageScrollTimer / MessageScrollDuration;

			float sign = Math.Sign(0.5f - m_Xdone);
			//Console.WriteLine(sign+" "+m_messagePauseTimer);

			m_bigMessageOffset.Y = m_Yratio * m_bigMessageLineSize.Y/3.5f;

			//unmoving text code here
			if ( (0 == (int)m_bigMessageOffset.X) && (m_messagePauseTimer += timerStep) < MessagePauseDuration)
			{				
				m_bigMessageOffset.X = 0;
			}
			else
			{
				m_messageScrollTimer += timerStep;
				m_bigMessageOffset.X = sign*m_Xratio * MathHelper.SmoothStep(0, m_bigMessageLineSize.X * 2, 0.5f * sign - m_Xdone * sign);
			}				

			this.spriteBatch.Draw(
				m_currentMessageSprite, Vector2.Zero * m_Xratio + m_bigMessageOffset,
				m_upperLine, m_upperLineColor, 0f, Vector2.Zero, m_Xratio, SpriteEffects.None, 1);

			m_bigMessageOffset.X *= -1;
			this.spriteBatch.Draw(
				m_currentMessageSprite, new Vector2(0, m_bigMessageLineSize.Y) * m_Xratio + m_bigMessageOffset,
				m_lowerLine, m_lowerLineColor, 0f, Vector2.Zero, m_Xratio, SpriteEffects.None, 1);
			m_bigMessageOffset.X *= -1;

			
			if (m_Xdone >= 1)
			{
				ResetBigMessage();
				Console.WriteLine("Message gone");
				
			}
			
		}

		public void ResetBigMessage()
		{

			IsDisplayingMessage = false;
			DrawOverlayElement -= DrawBigMessage;
			m_messageScrollTimer = 0;
			m_messagePauseTimer = 0;
			m_Xratio = atGame.ScreenWidth / m_bigMessageLineSize.X;
			m_Yratio = atGame.ScreenHeight / m_bigMessageLineSize.Y;

			m_bigMessageOffset = Vector2.Zero;
			m_bigMessageOffset.X = -m_bigMessageLineSize.X;

			m_sndTimer = 0f;
			m_sndStartDelay = 0f;
		}

		public void DisplayMessage(BigMessages msgType, Player referencePlayer=null,float scrollDuration=0.75f, float pauseDuration=1.0f)
		{
			ResetBigMessage();

			m_currentMessageSprite = (msgType == BigMessages.GameOver) ? m_gameoverMessageSprite : m_phaseMessageSprite;

			IsDisplayingMessage = true;
			if (referencePlayer == null) referencePlayer = atGame.ActivePlayer;

			int rowOffset = (int)msgType%3;

			m_sndSwoosh.Play(0.5f,0.1f,0f);

			if (msgType != BigMessages.GameOver)
			{
				m_currentMessageSprite = m_phaseMessageSprite;
				m_upperLineColor = m_lowerLineColor = atGame.ActivePlayer.TeamColor;
				m_upperLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * (rowOffset - 1), (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y);
				m_lowerLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * 3, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y);

				
				UpdateActions = delegate(GameTime gameTime)
				{
					if (m_Xdone > 0.51f && (int)m_bigMessageOffset.X != 0)
					{
						Console.WriteLine("phase");
						m_sndSwoosh.Play(0.5f, 0.1f, 0f);
						UpdateActions = delegate { };
					}

				};

			}
			else
			{

				m_currentMessageSprite = m_gameoverMessageSprite;
				m_upperLineColor = referencePlayer.TeamColor;
				m_lowerLineColor = (referencePlayer == atGame.PlayerA) ? atGame.PlayerB.TeamColor : atGame.PlayerA.TeamColor;
				m_upperLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * (rowOffset - 1), (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y);
				m_lowerLine = new Rectangle(0, (int)m_bigMessageLineSize.Y * 1, (int)m_bigMessageLineSize.X, (int)m_bigMessageLineSize.Y);

				m_sndTimer = 0f;
				m_sndStartDelay = 1.25f;

				UpdateActions = delegate(GameTime gameTime)
				{
					if ((m_sndTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) >= m_sndStartDelay)
					{
						Console.WriteLine("play");						
						m_sndFatality.Play(1f,-0.15f,0f);						
						UpdateActions = delegate { };
					}
					
				};


			}

			MessagePauseDuration = pauseDuration;
			MessageScrollDuration = scrollDuration;

			DrawOverlayElement += DrawBigMessage;

		}

		public void DisplayMessage(ATGame.GamePhase phase, Player player = null, float scrollDuration = 0.75f, float pauseDuration = 1.0f)
		{
			switch (phase)
			{
				case (ATGame.GamePhase.GP_Dispatch):
					DisplayMessage(ScreenOverlay.BigMessages.Dispatch,player, scrollDuration, pauseDuration); break;
				case (ATGame.GamePhase.GP_Movement):
					DisplayMessage(ScreenOverlay.BigMessages.Movement, player, scrollDuration, pauseDuration); break;
				case (ATGame.GamePhase.GP_Combat):
					DisplayMessage(ScreenOverlay.BigMessages.Combat, player, scrollDuration, pauseDuration); break;
				case (ATGame.GamePhase.GP_GameOver):
					DisplayMessage(ScreenOverlay.BigMessages.GameOver, player, scrollDuration, pauseDuration); break;
				default:
					DisplayMessage(ScreenOverlay.BigMessages.GameOver, player, scrollDuration, pauseDuration); break;
			}
			

		}


		internal void DisplayEasterEgg()
		{
			m_sndToasty.Play();
		}
	}
}
