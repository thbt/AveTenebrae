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

		public ATDrawableComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
		}

		public override void Initialize()
		{
			spriteBatch = new SpriteBatch(atGame.GraphicsDevice);
			base.Initialize();

		}

	}
}
