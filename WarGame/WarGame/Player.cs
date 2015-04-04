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
	public class Player : ATDrawableComponent
	{
		
		public Unit selUnit;
		private HexTile m_selHex;
		public HexTile SelectedHex { get { return m_selHex; } set { m_selHex = value;  } }

		public HexCursor selHexCursor;

		public Texture2D sprHexSelect;

		public List<Unit> ownedUnits;

		public HexTile.HexStatus DispatchableHex;

		private Color m_teamColor;
		public Color TeamColor
		{
			get { return m_teamColor; }
			set
			{
				m_teamColor = value;
				foreach (Unit u in ownedUnits)
				{
					u.PaletteSwap(m_teamColor);
				}
			}
		}

		public Player(ATGame game)
			: this(game, Color.White, true)
		{
			// TODO: Construct any child components here
			ownedUnits = new List<Unit>();
			/*selHexCursor = new HexCursor(game);

			List<Color> colorBlk = new List<Color>();
			colorBlk.Add(this.TeamColor);
			colorBlk.Add(Color.White);
			selHexCursor.SetColorBlink(colorBlk, 3f, true, false, true);*/
				
		}

		public Player(ATGame game,Color teamColor, bool makePlayerA=true)
			: base(game)
		{
			// TODO: Construct any child components here
			ownedUnits = new List<Unit>();
			TeamColor = teamColor;

			if (makePlayerA)
			{
				this.DispatchableHex = HexTile.HexStatus.HexDS_DispatchableA;
			}
			else{
				this.DispatchableHex = HexTile.HexStatus.HexDS_DispatchableB;
			}

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

		public override void Draw(GameTime gameTime)
		{
			/*
			if (this.selHex.SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth + Width
				&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight + Height)
			{
				Color finalColor = new Color(baseColor.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);

				this.spriteBatch.Begin();
				//hex
				this.spriteBatch.Draw(texture, new Vector2(SpritePosition.X, SpritePosition.Y), finalColor);
				//texte
				this.spriteBatch.DrawString(
					ResourceManager.font,
					GridPosition.X + "," + GridPosition.Y,
					//(SpritePosition+SpriteCenter)*0.5f,
					//new Vector2(GridPosition.X * Width * 0.75f + Width / 3f,SpritePosition.Y + Width / 3f),
					new Vector2(GridPosition.X * Width * 0.75f + sprOffset.X + Width / 4f, SpritePosition.Y + Width / 3f),
					Color.Black);

				this.spriteBatch.End();
				base.Draw(gameTime);
			}
			*/
		}
	}
}
