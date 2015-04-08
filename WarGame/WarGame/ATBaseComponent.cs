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

	public abstract partial class ATDrawableComponent : DrawableGameComponent
	{

		protected ATGame atGame;
		protected SpriteBatch spriteBatch;
		
		protected Vector2 sprOffset = Vector2.Zero;


		public ATDrawableComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
			AlphaBlinkEnable = false;
			ColorBlinkEnable = false;
			BounceEnable = false;
			Visible = true;

			colorBlinkCycle = new List<Color>();
			colorBlinkCycle.Add(TeamColors.Red);
			colorBlinkCycle.Add(Color.White);
			colorBlinkCycle.Add(TeamColors.Blue);
			colorBlinkCycle.Add(Color.White);
		}

		public override void Initialize()
		{
			spriteBatch = new SpriteBatch(atGame.GraphicsDevice);
			DrawFX = delegate(GameTime gameTime) { };
			base.Initialize();
			
		}

		public override void Draw(GameTime gameTime)
		{
						
			DrawFX(gameTime);
			//base.Draw(gameTime);
		}
	}
}
