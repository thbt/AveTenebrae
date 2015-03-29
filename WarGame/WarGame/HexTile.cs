using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame {
	public class HexTile : ATDrawableComponent {

		SpriteBatch spriteBatch;
		protected Texture2D texture;
		Color baseColor;
		Color offsetColor;
		
		public HexTile Parent { get; protected set; }
		public Point GridPosition { get; protected set;}
		public Vector2 SpritePosition { get; protected set; }
		public int Width { get { return texture.Width; } }
		public int Height { get { return texture.Height; } }
		public int Cost { get; protected set;}
		public bool Walkable { get { return Cost < int.MaxValue; } }



		public HexTile(ATGame game, int x, int y, int cost=1)
			: base(game) {

			GridPosition = new Point(x, y);
			
			Cost = cost;
			
			baseColor = this.Walkable ? Color.Green : Color.Brown;
		}
		
		protected override void LoadContent() {
			texture = Game.Content.Load<Texture2D>("hex");

		}

		public override void Initialize() {
			spriteBatch = new SpriteBatch(atGame.Game).GraphicsDevice);

			base.Initialize();

			SpritePosition = new Vector2(GridPosition.X * Width * 0.75f, GridPosition.Y * Height + ((GridPosition.X % 2 != 0) ? (Height / 2f) : 0));
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {

			Color finalColor = baseColor;			
			
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
