using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame {
	class HexTile : DrawableGameComponent {
		SpriteBatch spriteBatch;
		Texture2D texture;
		Color color;

		public HexTile parent;
		public Point pos;
		public int weight;
		public bool walkable;
		public int cost;
		public HexTile(Game game, int x, int y, bool walkable = true, int weight=1)
			: base(game) {
			pos = new Point(x, y);
			this.weight = weight;
			this.cost = int.MaxValue;
			this.walkable = walkable;
			color = this.walkable ? Color.Green : Color.Brown;
		}
		
		protected override void LoadContent() {
			texture = Game.Content.Load<Texture2D>("hex");
		}

		public override void Initialize() {
			spriteBatch = new SpriteBatch(((Game1)this.Game).GraphicsDevice);
			base.Initialize();
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			this.spriteBatch.Begin();
			this.spriteBatch.Draw(texture, 
				new Vector2((pos.X * texture.Width / 2f) * 1.5f,
					pos.Y * texture.Height + ((pos.X % 2 != 0) ? (texture.Height / 2f) : 0)),
				color);
			this.spriteBatch.DrawString(ResourceManager.font, pos.X.ToString() + "," + pos.Y.ToString(),
				new Vector2((pos.X * texture.Width / 2f) * 1.5f + texture.Width / 3f,
					pos.Y * texture.Height + ((pos.X % 2 != 0) ? (texture.Height / 2f) : 0) + texture.Width / 3f),
				Color.Black);
			this.spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
