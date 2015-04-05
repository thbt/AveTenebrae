using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarGame
{


	public class Heavy : Unit
	{
		public Heavy(ATGame game, Player owner)
			: base(game, owner, 2, 2, 1, 0)
		{
			drawArea = new Rectangle(0, 0, iconSize, iconSize);

		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "icon_sheet";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}


	}

	public class Scout : Unit
	{
		public Scout(ATGame game, Player owner)
			: base(game, owner, 4, 4, 1, 0)
		{
			drawArea = new Rectangle(iconSize, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_cavalry";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}

	}

	public class Sniper : Unit
	{
		public Sniper(ATGame game, Player owner)
			: base(game, owner, 2, 1, 2, 4)
		{
			drawArea = new Rectangle(iconSize * 2, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_archer";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}

	}
}
