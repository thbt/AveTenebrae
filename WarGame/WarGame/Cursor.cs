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
	public class Cursor : ATDrawableComponent
	{

		public Vector2 position;

		public Texture2D sprCurrent;
		private Texture2D sprStandard;
		public Cursor(ATGame game)
			: base(game)
		{
			// TODO: Construct any child components here
			position = new Vector2(Mouse.GetState().X,Mouse.GetState().Y);

			atGame.Components.Add(this);
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here

			base.Initialize();

			sprCurrent = sprStandard;

		}

		protected override void LoadContent()
		{
			sprStandard = Game.Content.Load<Texture2D>("cursor_std");

		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			position.X = Mouse.GetState().X;
			position.Y = Mouse.GetState().Y;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			this.spriteBatch.Draw(sprCurrent, position, Color.White);

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
