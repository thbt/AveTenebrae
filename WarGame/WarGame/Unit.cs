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
	public abstract class Unit : ATDrawableComponent, ISelectable
	{

		public Vector2 SpritePos { get; protected set; }
		public HexTile OccupiedHex { get; protected set; }

		public readonly int Movement;
		public readonly int Strength;
		public readonly int Range;
		public readonly int RangedStrength;

		public Unit(ATGame game)
			: base((Game)game)
		{
			// TODO: Construct any child components here
		}

		public Unit(ATGame game, int move, int str, int range, int rangedStr)
			: this(game)
		{
			Movement=move;
			Strength=str;
			Range=range;
			RangedStrength=rangedStr;
			// TODO: Construct any child components here
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here

			base.Initialize();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here

			base.Update(gameTime);
		}

		public void Select()
		{
			atGame.activePlayer.selUnit = this;
			OccupiedHex.Select();
		}
	}

	public class Infantry : Unit
	{
		public Infantry(ATGame game) : base(game, 2, 2, 1, 0) { }
	}

	public class Cavalry : Unit
	{
		public Cavalry(ATGame game) : base(game, 4,4, 1, 0) { }

	}

	public class Archer : Unit
	{
		public Archer(ATGame game) : base(game, 2, 1, 2, 4) { }

	}

}
