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
	public class UnitContextInfo : ATDrawableComponent
	{
		protected static Texture2D attackerIcons;
		protected Texture2D coloredAttackerIcons;

		protected static Texture2D defenderIcons;
		protected Texture2D coloredDefenderIcons;

		protected int iconSize = 128;
		protected static float fadeSpeed = 5f;
		        
		protected Rectangle drawArea;
		public Vector4 colorOffset = Vector4.Zero;
		public Vector4 colorMultiplier = Vector4.One;

		protected float animTimer = 0f;
		protected float animDuration = 1f;
		protected float animSprScale = 1f;
		protected float animSprAlpha = 0f;

        //variables de positionnement
		protected Vector2 sprOrigin = Vector2.Zero;
		protected Vector2 sprUnitOffset;
		protected Vector2 sprOffset = Vector2.Zero;
		public int Width { get { return drawArea.Width; } }
		public int Height { get { return drawArea.Height; } }
		public Vector2 UnitPosition { get { return (LinkedUnit.SpritePosition) + atGame.Panning; } }
		public Vector2 SpritePosition { get { return UnitPosition - sprUnitOffset; } }
		public Vector2 UnitCenter { get { return new Vector2(UnitPosition.X+Width/2,UnitPosition.Y+Height/2); } }
		
        public Unit LinkedUnit { get; protected set; }

		protected delegate void ExecuteActionsDlg(GameTime gameTime);
		protected ExecuteActionsDlg ExecuteActions;
		protected DrawDelegate DrawAnimation;

		private UnitContextInfo(ATGame game)
			: base(game)
		{
			// TODO: Construct any child components here
			this.Enabled = true;
			atGame.Components.Add(this);

			ExecuteActions += delegate { };
			DrawAnimation = delegate { };


		}

		public UnitContextInfo(ATGame game, Unit unit)
			: this(game)
		{

			LinkedUnit = unit;			

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
			attackerIcons = Game.Content.Load<Texture2D>("context_icons");
			defenderIcons = Game.Content.Load<Texture2D>("context_icons");
			coloredAttackerIcons = new Texture2D(GraphicsDevice, attackerIcons.Width, attackerIcons.Height);
			coloredDefenderIcons = new Texture2D(GraphicsDevice, defenderIcons.Width, defenderIcons.Height);
			sprUnitOffset = new Vector2(attackerIcons.Width / 4,attackerIcons.Height / 4);

			base.LoadContent();			

		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here

			//PaletteSwap(atGame.activePlayer.TeamColor);
			base.Update(gameTime);

			ExecuteActions(gameTime);
			
		}
				
		public void Show()
		{
			animTimer = 0;
			animSprAlpha = 0f;
			animSprScale = 1f;
			animDuration = 0.25f;
			Enabled = true;
			Visible = true;
			PaletteSwap(LinkedUnit.Owner.TeamColor);
			Console.WriteLine("Show info");
			DrawAnimation = DrawGrow;
			
		}

		public void Hide()
		{
			animDuration = 0.75f;
			animTimer = 0;
			DrawAnimation = DrawShrink;

		}
	
		public override void Draw(GameTime gameTime)
		{
			
			//if (Displayed)
			{			
				//check si dans la partie visible
				if (UnitCenter.X > -Width*2 && UnitCenter.X < atGame.ScreenWidth + Width*2
				&& UnitCenter.Y > -Height*2 && UnitCenter.Y < atGame.ScreenHeight + Height*2)
				{
					finalColor = new Color(BaseColor.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);

					this.spriteBatch.Begin();

					DrawAnimation(gameTime);
					base.DrawFX(gameTime);

					this.spriteBatch.End();
					base.Draw(gameTime);

					
				}
			}
		}

		public void DrawGrow(GameTime gameTime)
		{
			
			if ((animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) <= animDuration)
			{
				animSprAlpha = MathHelper.SmoothStep(0, 1, animTimer / animDuration);
				DrawStable(gameTime);
			}
			else
			{
				DrawStable(gameTime);
				DrawAnimation=DrawStable;
			}
		}

		public void DrawShrink(GameTime gameTime)
		{
			if ((animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) <= animDuration)
			{
				animSprAlpha = MathHelper.SmoothStep(1, 0, animTimer / animDuration);
				DrawStable(gameTime);
			}
			else
			{
				AlphaBlinkEnable = false;
				BounceEnable = false;
				ColorBlinkEnable = false;
				Visible = false;
				Enabled = false;
				DrawAnimation = delegate { };
			}

		}

		public void DrawStable(GameTime gameTime)
		{
			//Console.WriteLine("Draw stable");
			drawArea = new Rectangle(
				(int)(LinkedUnit.SpritePosition.X - sprUnitOffset.X),
				(int)(LinkedUnit.SpritePosition.Y - sprUnitOffset.Y),
				(int)(attackerIcons.Width * animSprScale), (int)(attackerIcons.Height *animSprScale));

			this.spriteBatch.Draw(coloredAttackerIcons, drawArea, null, Color.White*animSprAlpha, 0f, new Vector2(0.00005f,0.00005f), SpriteEffects.None, 1);

			float fontScale = 1.25f;
			float XoffsetMult=4.75f;
			//atk power
			this.spriteBatch.DrawString(ResourceManager.font, LinkedUnit.Strength.ToString(),
				SpritePosition - atGame.Panning + new Vector2(sprUnitOffset.X / XoffsetMult, sprUnitOffset.Y), Color.LightGreen * animSprAlpha, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 1); 
			//movement points
			this.spriteBatch.DrawString(ResourceManager.font, LinkedUnit.MovementPoints.ToString(),
				SpritePosition - atGame.Panning + new Vector2(attackerIcons.Width - sprUnitOffset.X * .85f + sprUnitOffset.X / XoffsetMult, sprUnitOffset.Y), Color.LightGreen * animSprAlpha, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 1);
			this.spriteBatch.DrawString(ResourceManager.font, LinkedUnit.RangedStrength.ToString(),
				SpritePosition - atGame.Panning + new Vector2(sprUnitOffset.X / XoffsetMult, attackerIcons.Height - 1.75f * sprUnitOffset.Y), Color.LightGreen * animSprAlpha, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 1);
			this.spriteBatch.DrawString(ResourceManager.font, LinkedUnit.Range.ToString(),
				SpritePosition - atGame.Panning + new Vector2(attackerIcons.Width - sprUnitOffset.X * .85f + sprUnitOffset.X / XoffsetMult, attackerIcons.Height - 1.75f * sprUnitOffset.Y), Color.LightGreen * animSprAlpha, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 1);

		}

		public void PaletteSwap(Color targetColor)
		{

			coloredAttackerIcons = new Texture2D(GraphicsDevice, attackerIcons.Width, attackerIcons.Height);
			Console.WriteLine(targetColor);

			int nbPixels = attackerIcons.Width * attackerIcons.Height;
			Color[] pixels = new Color[nbPixels];
			attackerIcons.GetData(pixels);

			for (int p = 0; p < nbPixels; p++)
			{
				Color pix = pixels[p];
				byte a = pix.A;

				if (pix.R == pix.G && pix.G == pix.B)
				{

					pix.R = (byte)((pix.R*targetColor.R) / 256f);
					pix.G = (byte)((pix.G*targetColor.G) / 256f);
					pix.B = (byte)((pix.B*targetColor.B) / 256f);

				}
				/*else
					pix.A = 0;*/

				pix.A = a;
				pixels[p] = pix;
			}

			coloredAttackerIcons.SetData(pixels);
			
		}

		public void Desaturate()
		{

			coloredAttackerIcons = new Texture2D(GraphicsDevice, attackerIcons.Width, attackerIcons.Height);
			//Console.WriteLine(targetColor);

			int nbPixels = attackerIcons.Width * attackerIcons.Height;
			Color[] pixels = new Color[nbPixels];
			attackerIcons.GetData(pixels);

			for (int p = 0; p < nbPixels; p++)
			{
				Color pix = pixels[p];
				byte a = pix.A;

				float avg = ((pix.R + pix.G + pix.B) / 3f) / 255f;

				pix.R = (byte)(avg * 255 );
				pix.G = (byte)(avg * 255 );
				pix.B = (byte)(avg * 255 );

				pix.A = a;
				pixels[p] = pix;
			}

			coloredAttackerIcons.SetData(pixels);

		}

		public bool IsUnderCursor(MouseState mouseState)
		{
			Rectangle boundingBox=new Rectangle((int)UnitPosition.X,(int)UnitPosition.Y,drawArea.Width,drawArea.Height);
			return (boundingBox.Contains(new Point(mouseState.X, mouseState.Y)));

		}

		public void ResetGraphics()
		{
			colorOffset = Vector4.Zero;
			colorMultiplier = Vector4.One;
			ColorBlinkEnable = false;
		}


	}




}
