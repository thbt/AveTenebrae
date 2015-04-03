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
	public abstract class Unit : ATDrawableComponent, ISelectable
	{
		protected static Texture2D spriteSheet;
		protected Texture2D coloredSpriteSheet;
		protected static string spriteSheet_path;

		protected int iconSize = 64;
		        
		protected Rectangle drawArea;
		public Vector4 colorOffset = Vector4.Zero;
		public Vector4 colorMultiplier = Vector4.One;

        //variables de positionnement
		protected Vector2 sprOrigin = Vector2.Zero;
		protected Vector2 sprOffset = Vector2.Zero;
		public int Width { get { return drawArea.Width; } }
		public int Height { get { return drawArea.Height; } }
		public Vector2 SpritePosition { get { return sprOrigin+sprOffset;} }
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		
        public Player Owner { get; protected set; }
		
		public HexTile OccupiedHex { get; protected set; }

		public readonly int Movement;
		public readonly int Strength;
		public readonly int Range;
		public readonly int RangedStrength;

		public Unit(ATGame game)
			: base((Game)game)
		{
			// TODO: Construct any child components here
			atGame.Components.Add(this);
			

		}

		public Unit(ATGame game, Player owner, int move, int str, int range, int rangedStr)
			: this(game)
		{
			Movement=move;
			Strength=str;
			Range=range;
			RangedStrength=rangedStr;
			Owner = owner;
			Owner.ownedUnits.Add(this);

            
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
			spriteSheet_path = "icon_sheet";
			spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);
			coloredSpriteSheet = spriteSheet;
			
			base.LoadContent();			
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here

            sprOffset = atGame.Panning;

			//PaletteSwap(atGame.activePlayer.TeamColor);
			base.Update(gameTime);
			
		}
				
		public void Select()
		{
			if (atGame.ActivePlayer.selUnit != null )
				atGame.ActivePlayer.selUnit.UnSelect();

			atGame.ActivePlayer.selUnit = this;
			SetAlphaBlink(0, 0.5f, 1f, 1.5f, true);

		}

		public void UnSelect()
		{
			AlphaBlinkEnable = false;
			BounceEnable = false;
		}

		public void PutOnHex(HexTile hex)
		{
			//si deja placé ailleurs, liberer la case de départ
			if (OccupiedHex != null)
				OccupiedHex.Occupant = null;

			if (hex.Occupant != null)
			{
				//
				Owner.ownedUnits.Remove(this);
				atGame.Components.Remove(this);
			}
			else
				hex.Occupant = this;
			
			sprOrigin = hex.SpritePosition;
			this.PaletteSwap(Owner.TeamColor);
			
			this.OccupiedHex = hex;
		}

		public override void Draw(GameTime gameTime)
		{
			
			//check si dans la partie visible
			if (SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth+Width
			&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight+Height)
			{

                finalColor = Color.White;

				this.spriteBatch.Begin();

                if (DrawFX != null )
                    base.DrawFX(gameTime);

				//hex
				this.spriteBatch.Draw(coloredSpriteSheet, new Vector2(SpritePosition.X, SpritePosition.Y)+bounceOffset, drawArea, finalColor);

				this.spriteBatch.End();
				base.Draw(gameTime);
			}
			
		}


 

		public void PaletteSwap(Color targetColor)
		{

			coloredSpriteSheet = new Texture2D(GraphicsDevice, spriteSheet.Width, spriteSheet.Height);
			Console.WriteLine(targetColor);

			int nbPixels = spriteSheet.Width * spriteSheet.Height;
			Color[] pixels = new Color[nbPixels];
			spriteSheet.GetData(pixels);

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

			coloredSpriteSheet.SetData(pixels);
			
		}

		public bool IsUnderCursor(MouseState mouseState)
		{
			Rectangle boundingBox=new Rectangle((int)SpritePosition.X,(int)SpritePosition.Y,drawArea.Width,drawArea.Height);
			return (boundingBox.Contains(new Point(mouseState.X, mouseState.Y)));

		}
	}



	public class Heavy : Unit
	{
		public Heavy(ATGame game, Player owner) : base(game, owner, 2, 2, 1, 0) {
			drawArea = new Rectangle(0, 0, iconSize, iconSize);
			Console.WriteLine("Infantry Spawned");
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "icon_sheet";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}


	}

	public class Scout : Unit
	{
		public Scout(ATGame game, Player owner) : base(game, owner, 4, 4, 1, 0) {
			drawArea = new Rectangle(iconSize, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_cavalry";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}

	}

	public class Sniper : Unit
	{
		public Sniper(ATGame game, Player owner) : base(game, owner, 2, 1, 2, 4) {
			drawArea = new Rectangle(iconSize*2, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_archer";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}

	}

}
