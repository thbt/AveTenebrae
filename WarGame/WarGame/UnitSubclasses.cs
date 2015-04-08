using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarGame
{


	public class HeavyKnight : Unit
	{
		public HeavyKnight(ATGame game, Player owner)
			: base(game, owner, 3, 4, 1, 0)
		{
			drawArea = new Rectangle(0, 0, iconSize, iconSize);

		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "icon_sheet";
			base.LoadContent();
			//spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);
			AttackSound = Game.Content.Load<SoundEffect>("SwordSweesh");
		}
		
	}

	public class Cavalry : Unit
	{
		public Cavalry(ATGame game, Player owner)
			: base(game, owner, 5, 2, 2, 1)
		{
			drawArea = new Rectangle(iconSize, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_cavalry";
			base.LoadContent();
			AttackSound = Game.Content.Load<SoundEffect>("SpearThrow");
			
		}
		public override int EvaluateContextDamage(Unit target)
		{

			if (!atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1, false).Contains(target.OccupiedHex)) return TotalAttack;
			return TotalRangedAttack;
		}
		public override void HighlightAttackRange(bool highlight = true,bool isSecondaryRange=true)
		{
			base.HighlightAttackRange(highlight,true);
			List<HexTile> meleeRange = atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1, false);

			//OccupiedHexm_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);

			foreach (HexTile h in meleeRange)
			{
				//h.ResetGraphics();
				h.ColorBlinkEnable = false;
				List<Color> reverse = h.TeamColorBlink(Owner);
				h.colorMultiplier.W = -0.5f;
				h.ColorBlinkEnable = true; //retirer le precedent blink pour eviter la superposition des effets

			}

			OccupiedHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.25f);

		}	

	}

	public class Archer : Unit
	{
		public override List<HexTile> AttackableHexes
		{
			get
			{			
				List<HexTile> meleeRange = atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1, false);
				meleeRange.AddRange(base.AttackableHexes);
				return meleeRange;
			}
		}
		public Archer(ATGame game, Player owner)
			: base(game, owner, 4, 1, 4, 4)
		{
			drawArea = new Rectangle(iconSize * 2, 0, iconSize, iconSize);
		}

		protected override void LoadContent()
		{
			//spriteSheet_path = "units_archer";
			base.LoadContent();
			AttackSound = Game.Content.Load<SoundEffect>("BowFire02");
	
		}

		public override int EvaluateContextDamage(Unit target)
		{

			if (atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1, false).Contains(target.OccupiedHex)) return TotalAttack;
			return TotalRangedAttack;
		}

		public override void HighlightAttackRange(bool highlight = true, bool isSecondaryRange=true)
		{
			base.HighlightAttackRange(highlight, false);
			List<HexTile> meleeRange = atGame.GameBoard.GetNeighboursRanged(OccupiedHex, 1,false);

			//OccupiedHexm_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);

			foreach (HexTile h in meleeRange)
			{
				h.ColorBlinkEnable = false;
				h.ResetGraphics();
				h.TeamColorBlink(atGame.OpposingPlayer, new Color(0.75f, 0.75f, 0.75f, 0.5f));
				h.colorMultiplier.W = -0.5f;
				h.ColorBlinkEnable = true; //retirer le precedent blink pour eviter la superposition des effets
				//h.AlphaBlinkEnable = true;
				//h.colorOffset = new Vector4(-0.725f, -0.725f, -0.75f, 0.5f);
				//h.colorMultiplier = new Vector4(0.25f, 0.25f,0.25f, 1f);
				//h.SetHighlighted(highlight);
			}

			OccupiedHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.25f);

		}	

	}
}
