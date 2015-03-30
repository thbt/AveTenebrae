using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WarGame {
	public class HexTile : ATDrawableComponent {
				
		protected Texture2D texture;
		Color baseColor;
		public Color offsetColor;
		
		public HexTile Parent { get; protected set; }
		public Point GridPosition { get; protected set;}
		public Vector2 SpritePosition { get; protected set; }
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		public int Width { get { return texture.Width; } }
		public int Height { get { return texture.Height; } }

		public bool Walkable { get { return Cost < int.MaxValue; } }

		//gameStats
		public int Cost { get; protected set; }
		public int defBonus { get; protected set; }
		public int atkBonus { get; protected set; }
		public int defMultiplier { get; protected set; }
		public int atkMultiplier { get; protected set; }



		protected HexTile(ATGame game, int x, int y)
			: base(game) {

			GridPosition = new Point(x, y);						
			baseColor = this.Walkable ? Color.Green : Color.Brown;
		}

		public static HexTile CreatePlain(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game,x,y);
			tmp.Cost = 1;
			tmp.defBonus = 0;
			tmp.atkBonus = 0;
			tmp.defMultiplier = 1;
			tmp.atkMultiplier = 1;
			tmp.baseColor = Color.LightGreen;
			return tmp;
		}

		public static HexTile CreateHill(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game, x, y);
			tmp.Cost = 2;
			tmp.defBonus = 0;
			tmp.atkBonus = 2;
			tmp.defMultiplier = 1;
			tmp.atkMultiplier = 1;
			tmp.baseColor = Color.MediumSeaGreen;
			return tmp;
		}
		public static HexTile CreateForest(ATGame game, int x, int y)
		{
			HexTile tmp = new HexTile(game, x, y);
			tmp.Cost = 2;
			tmp.defBonus = 0;
			tmp.atkBonus = 0;
			tmp.defMultiplier = 1;
			tmp.atkMultiplier = 1;
			tmp.baseColor = Color.DarkGreen;
			return tmp;
		}
		
		protected override void LoadContent() {
			texture = Game.Content.Load<Texture2D>("hex");

		}

		public override void Initialize() {

			base.Initialize();
			SpritePosition = new Vector2(
				GridPosition.X * Width * 0.75f,
				GridPosition.Y * Height + ((GridPosition.X % 2 != 0) ? (Height / 2f) : 0));
			Console.WriteLine(SpritePosition);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {

			Color finalColor = new Color(baseColor.ToVector4()+offsetColor.ToVector4());			
			
			this.spriteBatch.Begin();
			//hex
			this.spriteBatch.Draw(texture, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor);
			//texte
			this.spriteBatch.DrawString(
				ResourceManager.font,
				GridPosition.X + "," + GridPosition.Y,
				new Vector2(GridPosition.X * Width * 0.75f + Width / 3f,
				SpritePosition.Y + Width / 3f),
				Color.Black);

			this.spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
