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

		protected string unitClass;
		protected int iconSize = 64;
		protected static float moveSpeed = 5f;
		        
		protected Rectangle drawArea;
		public Vector4 colorOffset = Vector4.Zero;
		public Vector4 colorMultiplier = Vector4.One;

        //variables de positionnement
		protected Vector2 sprOrigin = Vector2.Zero;
		protected Vector2 sprOffset = Vector2.Zero;
		public int Width { get { return drawArea.Width; } }
		public int Height { get { return drawArea.Height; } }
		public Vector2 SpritePosition { get { return sprOrigin+atGame.Panning;} }

		protected Vector2 nextDest;
		protected HexTile nextDestTile;
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		
        public Player Owner { get; protected set; }
		
		public HexTile OccupiedHex { get; protected set; }

		public readonly int Movement;
		public readonly int Strength;
		public readonly int Range;
		public readonly int RangedStrength;

		protected bool m_freeze = false;
		public bool Freeze { get { return m_freeze; } 
			set { m_freeze = value;
			if (m_freeze)
			{
				AlphaBlinkEnable = false;
				DrawFX += DrawFrozen;
			}
			else
			{
				DrawFX -= DrawFrozen;
				PaletteSwap(Owner.TeamColor);
			}
			}
		}

		protected delegate void ExecuteActionsDlg(GameTime gameTime);
		protected ExecuteActionsDlg ExecuteActions;

		public Unit(ATGame game)
			: base((Game)game)
		{
			// TODO: Construct any child components here
			atGame.Components.Add(this);
			ExecuteActions += delegate(GameTime gameTime) { };

		}

		public Unit(ATGame game, Player owner, int move, int str, int range, int rangedStr)
			: this(game)
		{
			this.Visible = false;
			Movement=move;
			Strength=str;
			Range=range;
			RangedStrength=rangedStr;
			Owner = owner;
			
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
			unitClass = this.GetType().Name;
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

			//PaletteSwap(atGame.activePlayer.TeamColor);
			//base.Update(gameTime);
			ExecuteActions(gameTime);
			
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
			ColorBlinkEnable = false;
		}

		public void PutOnHex(HexTile hex)
		{
			sprOrigin = hex.SpritePosition-atGame.Panning;
			//si deja plac� ailleurs, liberer la case de d�part
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

			
			this.PaletteSwap(Owner.TeamColor);
			
			this.OccupiedHex = hex;
			this.Visible = true;
		}

		public bool DispatchableOnHex(HexTile hex)
		{
			return (( hex.Status & atGame.ActivePlayer.DispatchableHex ) == atGame.ActivePlayer.DispatchableHex
				&& hex.Occupant == null );
		}

		public void DispatchOnHex(HexTile hex)
		{
			PutOnHex(hex);
			Owner.ownedUnits.Add(this);
			Console.WriteLine(unitClass+" Spawned");
		}


		public void MoveTo(GameTime gameTime)
		{
			//(float)gameTime.ElapsedGameTime.TotalSeconds * moveSpeed
			//float remDist = Vector2.Distance(nextDest, sprOrigin - atGame.Panning) / Vector2.Distance(OccupiedHex.SpritePosition, sprOrigin - atGame.Panning);
			//
			nextDest = nextDestTile.SpritePosition;
			float totalDist = Vector2.Distance(OccupiedHex.SpritePosition, nextDest);
			
			sprOrigin += (nextDest - OccupiedHex.SpritePosition) * moveSpeed / totalDist;

			//sprOrigin = Vector2.SmoothStep(SpritePosition, nextDest, Vector2.Distance(SpritePosition, nextDest)/totalDist);

			Console.WriteLine(unitClass + " at " + sprOrigin + " moving to " + nextDest);
			if (Vector2.Distance(SpritePosition, nextDest) <= 0.5f * moveSpeed)
			{
				PutOnHex(nextDestTile);
				ExecuteActions -= MoveTo;
				Freeze = true;
				BounceEnable = false;
				Console.WriteLine(unitClass + " arrived at " + nextDest);
			}
				
		}
		/*public void StartMoveTo(Vector2 dest)
		{
			if (Vector2.Distance(SpritePosition,OccupiedHex.SpritePosition) <= 1)
			{
				nextDest = dest;
				ExecuteActions += MoveTo;
			}

			Console.WriteLine(unitClass + " starts moving to "+dest);
		}*/
		public void StartMoveTo(HexTile tileDest)
		{
			if (Vector2.Distance(SpritePosition, OccupiedHex.SpritePosition) <= 1)
			{
				float totalDist = Vector2.Distance(OccupiedHex.SpritePosition, tileDest.SpritePosition);
				float expectedDuration = totalDist/moveSpeed;

				nextDestTile = tileDest;
				ExecuteActions += MoveTo;
				
				SetBounce(12, -1, expectedDuration/100, false, true);
			}
		
		}


		public override void Draw(GameTime gameTime)
		{
			
			//check si dans la partie visible
			if (SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth+Width
			&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight+Height)
			{
				finalColor = new Color(Color.White.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);

				this.spriteBatch.Begin();                
                base.DrawFX(gameTime);

				//hex
				this.spriteBatch.Draw(coloredSpriteSheet, SpritePosition+bounceOffset, drawArea, finalColor);

				this.spriteBatch.End();
				base.Draw(gameTime);
			}
			
		}

		public void DrawFrozen(GameTime gameTime)
		{
			Vector4 grey = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);
			Desaturate();
			finalColor = new Color(grey * colorMultiplier + colorOffset * colorOffset.W);
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

		public void Desaturate()
		{

			coloredSpriteSheet = new Texture2D(GraphicsDevice, spriteSheet.Width, spriteSheet.Height);
			//Console.WriteLine(targetColor);

			int nbPixels = spriteSheet.Width * spriteSheet.Height;
			Color[] pixels = new Color[nbPixels];
			spriteSheet.GetData(pixels);

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
