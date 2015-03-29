using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarGame
{
	abstract class ATComponent : GameComponent
	{
		protected ATGame atGame;

		public ATComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
		}
		
	}

	abstract class ATDrawableComponent : DrawableGameComponent
	{
		protected ATGame atGame;
		public ATDrawableComponent(Game game)
			: base(game)
		{
			atGame = (ATGame)game;
		}

	}
}
