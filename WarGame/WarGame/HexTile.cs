using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame {
	class HexTile : ATDrawableComponent {

		SpriteBatch spriteBatch;
		Texture2D texture;
		Color color;

		public HexTile parent;
		public Point Position { get; protected set;}
		public int Cost { get; protected set;}
		public bool Walkable { get { return Cost < int.MaxValue; } }

		public HexTile(ATGame game, int x, int y, int cost=1)
			: base(game) {

			Position = new Point(x, y);
			Cost = cost;

			color = this.Walkable ? Color.Green : Color.Brown;
		}
		
		protected override void LoadContent() {
			texture = Game.Content.Load<Texture2D>("hex");
		}

		public override void Initialize() {
			spriteBatch = new SpriteBatch(((ATGame)this.Game).GraphicsDevice);
			base.Initialize();
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			
			this.spriteBatch.Begin();
			this.spriteBatch.Draw(texture, 
				new Vector2((Position.X * texture.Width / 2f) * 1.5f,
					Position.Y * texture.Height + ((Position.X % 2 != 0) ? (texture.Height / 2f) : 0)),
				color);
			this.spriteBatch.DrawString(ResourceManager.font, Position.X.ToString() + "," + Position.Y.ToString(),
				new Vector2((Position.X * texture.Width / 2f) * 1.5f + texture.Width / 3f,
					Position.Y * texture.Height + ((Position.X % 2 != 0) ? (texture.Height / 2f) : 0) + texture.Width / 3f),
				Color.Black);
			this.spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
