using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame
{
	public abstract class ATComponent : GameComponent
	{
		protected ATGame atGame;

		public ATComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
		}
		
	}

	public abstract class ATDrawableComponent : DrawableGameComponent
	{

		protected ATGame atGame;
		protected SpriteBatch spriteBatch;

		//variables de clignotement alpha des sprites
		private bool m_blinkEnable;
		protected bool AlphaBlinkEnable
		{
			get { return m_blinkEnable; }
			set
			{
				m_blinkEnable = value;
				if (m_blinkEnable)
					DrawFX += DrawAlphaBlink;
				else
					DrawFX -= DrawAlphaBlink;
			}
		}

		protected float blinkAlphaMin = 0.5f;
		protected float blinkAlphaMax = 1.0f;
		protected float blinkAlpha = 0.75f;
		protected float blinkSign = 1f;
		protected float blinkTimer = 0;
		protected float blinkDuration = 0.75f;

		//variables de clignotement coloré des sprites
		private bool m_colorBlinkEnable;
		protected bool ColorBlinkEnable
		{
			get { return m_colorBlinkEnable; }
			set
			{
				m_blinkEnable = value;
				if (m_blinkEnable){
					if (colorBlinkCycle == null)
					{
						colorBlinkCycle = new List<Color>();
						colorBlinkCycle.Add(TeamColors.Red);
						colorBlinkCycle.Add(TeamColors.Blue);
					}
						
					DrawFX += DrawColorBlink;
				}
					
				else
					DrawFX -= DrawColorBlink;
			}
		}


		protected List<Color> colorBlinkCycle;
		//protected float blinkAlphaMax = 1.0f;
		//protected float blinkAlpha = 0.75f;
		protected Color colorBlinkCurrent = Color.White;
		protected bool colorBlinkIgnoreAlpha = true;
		protected bool colorBlinkLoop = true;
		protected int colorCurrentIndex = 0;
		protected float colorBlinkSign = 1f;
		protected float colorBlinkTimer = 0;
		protected float colorBlinkDuration = 0.75f;


		//variables de mouvement/flottement des sprites
		private bool m_bounceEnable;
		protected bool BounceEnable
		{
			get { return m_bounceEnable; }
			set
			{
				m_bounceEnable = value;
								
				if (m_bounceEnable){
					 DrawFX += DrawBounce;
				}					
				else
					DrawFX -= DrawBounce;
			}
		}

		protected Vector2 bounceOffset = Vector2.Zero;
		protected float bounceYmaxPixels = 8;
		protected float bounceYSign = -1; //default: upward
		protected float bounceTimer = 0;
		protected float bounceDuration = 0.75f;
		protected bool bounceSignAlternance = true;

		//delegates d'effets speciaux de sprites
		protected delegate void DrawDelegate(GameTime gameTime);
		protected DrawDelegate DrawFX;

		protected Color finalColor = Color.White;


		public ATDrawableComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
			AlphaBlinkEnable = false;
			BounceEnable = false;

			colorBlinkCycle = new List<Color>();
			colorBlinkCycle.Add(TeamColors.Red);
			colorBlinkCycle.Add(TeamColors.Blue);
		}

		public override void Initialize()
		{
			spriteBatch = new SpriteBatch(atGame.GraphicsDevice);
			base.Initialize();
			
		}
		/// <summary>
		/// Prepare l'effet de bondissement du sprite
		/// </summary>
		/// <param name="pixelAmplitude">Hauteur du bond en pixels</param>
		/// <param name="startSign">Direction de départ (1=bas, -1=haut)</param>
		/// <param name="cycleDuration">Durée d'un cycle de bondissement</param>
		/// <param name="alternSign">Si true, flotte de haut en bas, sinon rebondi contre une ligne horizontale imaginaire</param>
		/// <param name="enable">Activer immédiatement l'effet</param>
		public void SetBounce(float pixelAmplitude, float startSign, float cycleDuration, bool alternSign, bool enable = false)
		{
			BounceEnable = enable;
			bounceYmaxPixels = pixelAmplitude;
			bounceYSign = startSign;
			bounceDuration = cycleDuration;
			bounceSignAlternance = alternSign;
		}
		/// <summary>
		/// Prepare l'effet de clignotement du sprite
		/// </summary>
		/// <param name="alphaStart">Opacité de départ (obsolète?)</param>
		/// <param name="alphaMin">Plancher d'opacité</param>
		/// <param name="alphaMax">Plafond d'opacité</param>
		/// <param name="cycleDuration">Durée d'un cycle de clignotement</param>
		/// <param name="enable">Activer immédiatement l'effet</param>
		public void SetAlphaBlink(float alphaStart, float alphaMin, float alphaMax, float cycleDuration, bool enable = false)
		{
			AlphaBlinkEnable = enable;
			blinkAlpha = alphaStart;
			blinkAlphaMin = alphaMin;
			blinkAlphaMax = alphaMax;
			blinkDuration = cycleDuration;
		}

		/// <summary>
		/// Calcule l'effet de clignotement de sprite
		/// </summary>
		/// <param name="gameTime"></param>
		protected void DrawAlphaBlink(GameTime gameTime)
		{
			blinkTimer = (float)(blinkTimer + gameTime.ElapsedGameTime.TotalSeconds);
			if (blinkTimer >= blinkDuration)
			{
				blinkTimer = 0;
				//if (BlinkEnable)
				blinkSign = -blinkSign;
			}
			blinkAlpha = (float)Math.Sin(Math.PI * blinkTimer / blinkDuration) * (blinkAlphaMax - blinkAlphaMin) + blinkAlphaMin;

			//Console.WriteLine(blinkAlpha);
			finalColor *= blinkAlpha;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="colorCycle"></param>
		/// <param name="cycleDuration"></param>
		/// <param name="cycleLoopMode"></param>
		public void SetColorBlink(List<Color> colorCycle, float cycleDuration, bool cycleLoopMode, bool ignoreAlpha, bool enable = false)
		{
			ColorBlinkEnable = enable;
			colorBlinkIgnoreAlpha = ignoreAlpha;
			colorBlinkLoop = cycleLoopMode;
			colorBlinkCycle = colorCycle;
			blinkDuration = cycleDuration;
		}

		/// <summary>
		/// Calcule l'effet de clignotement de sprite
		/// </summary>
		/// <param name="gameTime"></param>
		protected void DrawColorBlink(GameTime gameTime)
		{
			colorBlinkTimer = (float)(colorBlinkTimer + gameTime.ElapsedGameTime.TotalSeconds);
			int nextIndex = (colorCurrentIndex + 1) % colorBlinkCycle.Count;

			if (colorBlinkTimer >= colorBlinkDuration)
			{
				colorBlinkTimer = 0;
				colorCurrentIndex = nextIndex;
				//if (BlinkEnable)
				blinkSign = -blinkSign;
			}
			
			Vector4 previous = colorBlinkCycle.ElementAt(colorCurrentIndex).ToVector4();
			Vector4 next = colorBlinkCycle.ElementAt(nextIndex).ToVector4();

			Vector4 mix = (next * (float)Math.Sin(Math.PI * colorBlinkTimer / colorBlinkDuration)
							-(previous * (float)Math.Sin(Math.PI * colorBlinkTimer / colorBlinkDuration)) );

			//Console.WriteLine(blinkAlpha);
			finalColor = new Color(finalColor.ToVector4() + mix);
		}

		/// <summary>
		/// Calcule l'effet de bondissement de sprite
		/// </summary>
		/// <param name="gameTime"></param>
		protected void DrawBounce(GameTime gameTime)
		{
			bounceTimer = (float)(bounceTimer + gameTime.ElapsedGameTime.TotalSeconds);

			if (bounceTimer >= bounceDuration)
			{
				bounceTimer %= bounceDuration;
				if (bounceSignAlternance)
					bounceYSign = -bounceYSign;
			}
			bounceOffset.Y = (((float)Math.Sin(Math.PI * bounceTimer / bounceDuration) * bounceYmaxPixels) * bounceYSign);

		}

		public override void Draw(GameTime gameTime)
		{

			if (DrawFX != null)
				DrawFX(gameTime);

			base.Draw(gameTime);
		}
	}
}
