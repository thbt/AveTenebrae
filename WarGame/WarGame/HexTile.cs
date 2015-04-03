using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WarGame {
	public class HexTile : ATDrawableComponent, ISelectable {

		public HexDisplayStatus DisplayStatus { get; set; }
		public enum HexDisplayStatus {
			HexDS_Normal=1,
			HexDS_Dispatchable=2 };		

		protected Texture2D selectSpr;
		protected Texture2D mainSpr;
		private Color baseColor;

		public Vector4 colorOffset=Vector4.Zero;
		public Vector4 colorMultiplier=Vector4.One;

		protected Vector2 sprOrigin = Vector2.Zero;
		protected Vector2 sprOffset=Vector2.Zero;
		
		public HexTile Parent { get; protected set; }
		public Point GridPosition { get; protected set;}
		public Vector2 SpritePosition { get { return sprOrigin+sprOffset;} }
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		public int Width { get { return mainSpr.Width; } }
		public int Height { get { return mainSpr.Height; } }
		public bool Walkable { get { return Cost < int.MaxValue; } }

		//gameStats
		public int Cost { get; protected set; }
		public int defBonus { get; protected set; }
		public int atkBonus { get; protected set; }
		public int defMultiplier { get; protected set; }
		public int atkMultiplier { get; protected set; }

		public Unit Occupant { get; set; }


		protected HexTile(ATGame game, int x, int y)
			: base(game) {

			DisplayStatus = HexDisplayStatus.HexDS_Normal;
			GridPosition = new Point(x, y);
			Occupant = null;	
			baseColor = this.Walkable ? Color.Green : Color.Brown;
		}

		public static HexTile CreatePlain(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game,x,y);
			tmp.ChangeToPlain();
			return tmp;
		}

		public HexTile ChangeToPlain()
		{

			Cost = 1;
			defBonus = 0;
			atkBonus = 0;
			defMultiplier = 1;
			atkMultiplier = 1;
			baseColor = Color.SandyBrown;
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

			Cost = 2;
			defBonus = 0;
			atkBonus = 2;
			defMultiplier = 1;
			atkMultiplier = 1;
			baseColor = Color.MediumSeaGreen;
			return this;
		}
		public static HexTile CreateForest(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game, x, y);
			tmp.ChangeToForest();
			return tmp;
		}

		public HexTile ChangeToForest()
		{	
			Cost = 2;
			defBonus = 0;
			atkBonus = 0;
			defMultiplier = 1;
			atkMultiplier = 1;
			baseColor = Color.DarkGreen;
			return this;
		}

		protected override void LoadContent() {
			mainSpr = Game.Content.Load<Texture2D>("hex");
			selectSpr = Game.Content.Load<Texture2D>("hexSelected");

		}

		public override void Initialize() {

			base.Initialize();
			sprOrigin = new Vector2(
				GridPosition.X * Width * 0.75f,
				GridPosition.Y * Height + ((GridPosition.X % 2 != 0) ? (Height / 2f) : 0));
			Console.WriteLine(SpritePosition);
		}

		public override void Update(GameTime gameTime) {

			sprOffset = atGame.Panning;
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {

			if (SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth+Width
				&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight+Height)
			{
				
				finalColor = new Color(baseColor.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);

				if ((DisplayStatus & HexDisplayStatus.HexDS_Dispatchable) == HexDisplayStatus.HexDS_Dispatchable)
				{
					//colorMultiplier = new Vector4(0.25f, 0.25f, 0.25f, 1);
					DrawColorBlink(gameTime);
				}

				if (DrawFX != null)
					base.DrawFX(gameTime);

				this.spriteBatch.Begin();
				//hex
				this.spriteBatch.Draw(mainSpr, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor);

				//hex selectionné? si oui, le marquer
				if (atGame.ActivePlayer.selHex == this)
					this.spriteBatch.Draw(selectSpr, new Vector2(SpritePosition.X, SpritePosition.Y), new Color(baseColor.ToVector4() *1.975f));


				//texte coords
				/*this.spriteBatch.DrawString(
					ResourceManager.font,
					GridPosition.X + "," + GridPosition.Y,
					//(SpritePosition+SpriteCenter)*0.5f,
					//new Vector2(GridPosition.X * Width * 0.75f + Width / 3f,SpritePosition.Y + Width / 3f),
					new Vector2(GridPosition.X * Width * 0.75f + sprOffset.X + Width / 4f, SpritePosition.Y + Width / 3f),
					Color.Black);*/



				this.spriteBatch.End();
				//base.Draw(gameTime);
			}

		}
		public void Select()
		{
			atGame.ActivePlayer.selHex = this;
			if (atGame.ActivePlayer.selHex.Occupant != null && atGame.ActivePlayer.selHex.Occupant.Owner == atGame.ActivePlayer)
			{
				atGame.ActivePlayer.selHex.Occupant.Select();
			}
		}
		public void UnSelect()
		{
			atGame.ActivePlayer.selHex = null;
		}


		public List<HexTile> GetNeighbours(){

			throw new NotImplementedException();
		}


	}

}
