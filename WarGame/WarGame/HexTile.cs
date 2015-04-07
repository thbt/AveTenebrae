using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WarGame {
	public class HexTile : ATDrawableComponent, ISelectable {

		public HexStatus Status { get; set; }
		public enum HexStatus {
			HexDS_Normal=1,
			HexDS_DispatchableA=2,
			HexDS_DispatchableB=4,
			HexDS_VisibleA = 8,
			HexDS_VisibleB = 16,
		};

		private delegate void DrawSpriteDlg(GameTime gameTime);
		private DrawSpriteDlg DrawSprite = delegate { };

		private Texture2D hillSpr, plainSpr, forestSpr;
		protected Texture2D mainSpr;
		protected Texture2D selectSpr;		

		public Vector4 colorOffset=Vector4.Zero;
		public Vector4 colorMultiplier=Vector4.One;

		protected Vector2 sprOrigin = Vector2.Zero;
		
		public HexTile Parent { get; protected set; }
		public Point GridPosition { get; protected set;}
		public Vector2 SpritePosition { get { return sprOrigin + atGame.Panning; } }
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		public int Width { get { return mainSpr.Width; } }
		public int Height { get { return mainSpr.Height; } }
		public bool Walkable { get { return BaseCost < int.MaxValue && Occupant == null; } }

		//gameStats
		public int BaseCost { get; protected set; }
		public int FinalCost { get { return (Walkable) ? BaseCost : int.MaxValue; } }
		public int totalPathCost;
		public int defBonus { get; protected set; }
		public int atkBonus { get; protected set; }
		public int defMultiplier { get; protected set; }
		public int atkMultiplier { get; protected set; }

		public Unit Occupant { get; set; }
		public bool Highlight { get; set; }


		protected HexTile(ATGame game, int x, int y)
			: base(game) {

			Status = HexStatus.HexDS_Normal;
			GridPosition = new Point(x, y);
			Occupant = null;	
			BaseColor = Color.White;

			
		}

		public static HexTile CreatePlain(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game,x,y);
			tmp.ChangeToPlain();
			return tmp;
		}

		public HexTile ChangeToPlain()
		{
			
			BaseCost = 1;
			defBonus = 0;
			atkBonus = 0;
			defMultiplier = 1;
			atkMultiplier = 1;
			//BaseColor = Color.SandyBrown;
			DrawSprite = delegate { this.spriteBatch.Draw(plainSpr, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor); };
			

			return this;
		}
		public static HexTile CreateHill(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game, x, y);
			tmp.ChangeToHill();
			return tmp;
		}

		public HexTile ChangeToHill()
		{

			BaseCost = 3;
			defBonus = 0;
			atkBonus = 2;
			defMultiplier = 1;
			atkMultiplier = 1;
			//BaseColor = Color.MediumSeaGreen;
			DrawSprite = delegate { this.spriteBatch.Draw(hillSpr, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor); };
			

			return this;
		}
		public static HexTile CreateForest(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game, x, y);
			
			tmp.ChangeToForest();
			tmp.mainSpr = tmp.forestSpr;
			return tmp;
		}

		public HexTile ChangeToForest()
		{	
			BaseCost = 2;
			defBonus = 0;
			atkBonus = 0;
			defMultiplier = 1;
			atkMultiplier = 1;
			//BaseColor = Color.DarkGreen;
			DrawSprite = delegate { this.spriteBatch.Draw(forestSpr, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor); };		

			return this;
		}

		protected override void LoadContent() {
			mainSpr = new Texture2D(GraphicsDevice, 64, 64);
			//mainSpr = Game.Content.Load<Texture2D>("hex");
			selectSpr = Game.Content.Load<Texture2D>("hexSelected");
			forestSpr = Game.Content.Load<Texture2D>("hexForest");
			hillSpr = Game.Content.Load<Texture2D>("hexMountain");
			plainSpr = Game.Content.Load<Texture2D>("hexPlain");
		}

		public override void Initialize() {

			base.Initialize();
			sprOrigin = new Vector2(
				GridPosition.X * Width * 0.75f,
				GridPosition.Y * Height + ((GridPosition.X % 2 != 0) ? (Height / 2f) : 0));
			Console.WriteLine(SpritePosition);	
			
		}

		public override void Update(GameTime gameTime) {

			//finalColor = BaseColor;
			sprOffset = atGame.Panning;
			
			//base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {

			finalColor = BaseColor;
			//il faut continuer à calculer certains effets, sinon ils sont désynchro lorsqu'ils reviennent dans la portion affichable
			//finalColor = new Color(Color.White.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);
			base.DrawFX(gameTime);

			if (SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth+Width
				&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight+Height)
			{
				this.spriteBatch.Begin();
				//hex

				DrawSprite(gameTime);


				//hex selectionné? si oui, le marquer
				if (atGame.ActivePlayer.SelectedHex == this)
				{
					this.spriteBatch.Draw(
						selectSpr,
						new Vector2(SpritePosition.X, SpritePosition.Y),
						new Color(atGame.ActivePlayer.TeamColor.ToVector4() * 1.5f));
				}
				else if (Highlight)
				{
					this.spriteBatch.Draw(
						selectSpr,
						new Vector2(SpritePosition.X, SpritePosition.Y),
						new Color(atGame.ActivePlayer.TeamColor.ToVector4() * 0.5f));
				}
									
				//texte coords
				/*this.spriteBatch.DrawString(
					ResourceManager.font,
					GridPosition.X + "," + GridPosition.Y,
					//(SpritePosition+SpriteCenter)*0.5f,
					//new Vector2(GridPosition.X * Width * 0.75f + Width / 3f,SpritePosition.Y + Width / 3f),
					new Vector2(GridPosition.X * Width * 0.75f + sprOffset.X + Width / 4f, SpritePosition.Y + Height / 3f),
					Color.Black);*/

				/*this.spriteBatch.DrawString(
					ResourceManager.font,
					"C:"+this.BaseCost+"\nM:"+totalPathCost,
									//(SpritePosition+SpriteCenter)*0.5f,
					//new Vector2(GridPosition.X * Width * 0.75f + Width / 3f,SpritePosition.Y + Height / 3f),
					new Vector2(GridPosition.X * Width * 0.75f + sprOffset.X + Width / 4f, SpritePosition.Y + Height / 8f),
					new Color(0,0,0,0.33f));*/

				base.Draw(gameTime);
				this.spriteBatch.End();
			}

			
		}
		public void Select()
		{
			atGame.ActivePlayer.SelectedHex = this;
			Unit prev = atGame.ActivePlayer.SelectedHex.Occupant;			

			if (prev != null
				&& atGame.ActivePlayer != prev.Owner
				&& atGame.ActivePlayer.SelectedHex.Occupant.Owner == atGame.ActivePlayer)
			{
				atGame.ActivePlayer.SelectedHex.Occupant.Select();
			}
		}
		public void UnSelect()
		{
			
			atGame.ActivePlayer.SelectedHex = null;
			this.ResetGraphics();
		}

		public void SetDispatchable(bool teamA, bool teamB)
		{
			colorBlinkCycle = new List<Color>();
			ColorBlinkEnable = false;
			ResetGraphics();
			finalColor = Color.White;

			Console.WriteLine("Dispatch: {0} - {1}", teamA, teamB);
			if (teamA || teamB)
			{
				if (teamA)
				{
					Status |= HexStatus.HexDS_DispatchableA;
					colorBlinkCycle = TeamColorBlink(atGame.PlayerA);
					DrawFX += DrawColorBlink;
				}
				else
				{
					Status &= ~HexStatus.HexDS_DispatchableA;
				}

				if (teamB)
				{
					Status |= HexStatus.HexDS_DispatchableB;
					colorBlinkCycle = TeamColorBlink(atGame.PlayerB);
					DrawFX += DrawColorBlink;
				}
				else
					Status &= ~HexStatus.HexDS_DispatchableB;

				if (teamA || teamB)
				{
					colorBlinkDuration = 1.5f;
					colorBlinkTimer = 0f;
				}		
			}
		
			
			
		}

		public void SetHighlighted(bool highLight)
		{
			if (highLight)
			{
				colorMultiplier = new Vector4(1.25f, 1.5f, 1.25f, 0.75f);
				colorOffset = new Vector4(-0.5f, -0.25f, -0.25f, -0.5f);
			}
			else
			{
				colorMultiplier = Vector4.One;
				colorOffset = Vector4.Zero;
				ResetGraphics();
			}

		}

		public List<Color> TeamColorBlink(Player player, Color? otherColor=null)
		{
			this.ResetGraphics(true);
			List<Color> cbl = new List<Color>();

			if (!otherColor.HasValue)
				otherColor = this.BaseColor;			
				
			cbl.Add(player.TeamColor);
			cbl.Add(new Color((otherColor.Value.ToVector4() * 0.5f + player.TeamColor.ToVector4() * 0.7f)));
			colorBlinkCycle = cbl;
			colorBlinkDuration = 1.5f;
			colorBlinkTimer = 0f;
			colorBlinkSign = 1f;
			
			
			return cbl;
		}

		public HexNode ToHexNode()
		{
			return new HexNode(this);
		}


		public List<HexTile> GetNeighbours(){

			return atGame.GameBoard.GetNeighbours(this);
		}

		public void ResetGraphics(bool fullReset=false)
		{
			colorOffset=Vector4.Zero;
			colorMultiplier=Vector4.One;
			ColorBlinkEnable = false;
			AlphaBlinkEnable = false;
			BounceEnable = false;
			
			if (fullReset)
			{
				colorBlinkCurrent = BaseColor;
				colorBlinkIgnoreWhite = true;
				colorBlinkLoop = true;
				colorCurrentIndex = 0;
				colorBlinkSign = 1f;
				colorBlinkTimer = 0;
				colorBlinkDuration = 1f;

				blinkSign = 1;				
				colorBlinkCurrent = BaseColor;

				blinkAlpha = 0;
				finalColor = BaseColor;
				blinkTimer = 0;
			}
			
		}

		public static void ResetGraphics(List<HexTile> resetList, bool fullReset = false)
		{
			foreach (HexTile h in resetList)
				h.ResetGraphics(fullReset);			
		}

	}

}
