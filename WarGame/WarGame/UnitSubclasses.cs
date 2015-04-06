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
			: base(game, owner, 2, 4, 1, 0)
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
			: base(game, owner, 4, 2, 2, 0)
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
			: base(game, owner, 3, 1, 4, 4)
		{
			drawArea = new Rectangle(iconSize * 2, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_archer";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);

		}

		public override void HighlightAttackRange(bool highlight = true)
		{
			base.HighlightAttackRange(highlight);
			List<HexTile> meleeRange = atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1,false);

			//OccupiedHexm_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);

			foreach (HexTile h in meleeRange)
			{
				h.ColorBlinkEnable = false; //retirer le precedent blink pour eviter la superposition des effets
				h.TeamColorBlink(Owner);
				h.AlphaBlinkEnable = true;
				h.ColorBlinkEnable = true;

				//h.SetHighlighted(highlight);
			}

			OccupiedHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.25f);

		}	

	}
}
