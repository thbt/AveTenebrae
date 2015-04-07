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
		protected static Texture2D spriteSheet;
		protected Texture2D coloredSpriteSheet;
		protected static string spriteSheet_path;

		protected string unitClass;
		protected int iconSize = 64;
		protected static float moveSpeed = 5f;
		        
		protected Rectangle drawArea;
		public Vector4 colorOffset = Vector4.Zero;
		public Vector4 colorMultiplier = Vector4.One;

        //variables de positionnement
		protected Vector2 sprOrigin = Vector2.Zero;
		protected Vector2 sprOffset = Vector2.Zero;
		public int Width { get { return drawArea.Width; } }
		public int Height { get { return drawArea.Height; } }
		public Vector2 SpritePosition { get { return sprOrigin+atGame.Panning;} }

		protected Vector2 nextDest;
		protected HexTile nextDestTile;
		public Vector2 SpriteCenter { get { return new Vector2(SpritePosition.X+Width/2,SpritePosition.Y+Height/2); } }
		
        public Player Owner { get; protected set; }
		public UnitContextInfo InfoPopup { get; protected set; }
		
		public HexTile OccupiedHex { get; protected set; }
		public virtual List<HexTile> ReachableHexes	{
			get { return atGame.GameBoard.GetNeighboursRanged(OccupiedHex, this.Movement, true); }
		}
		public virtual List<HexTile> AttackableHexes {
			get{ return atGame.GameBoard.GetNeighboursRanged(OccupiedHex, this.Range, false);}
		}
		public bool VisibleToOpponent { get { return CheckVisibility(); } }

		public HashSet<Unit> Attackers { get; protected set; }

		public int MovementPoints;
		public readonly int Movement;		
		public readonly int Strength;
		public readonly int Range;
		public readonly int RangedStrength;

		protected bool m_freeze = false;
		public bool Freeze { get { return m_freeze; } 
			set {
				if (!m_freeze && value)
			{
				AlphaBlinkEnable = false; Console.WriteLine("Freezing unit");	
				DrawFX += DrawFrozen;
				if (atGame.CurrentPhase != ATGame.GamePhase.GP_Dispatch)
					ResetRangeGraphics();
			}
				else if (!value)
			{
				DrawFX -= DrawFrozen; Console.WriteLine("UnFreezing unit");
				PaletteSwap(Owner.TeamColor);
				MovementPoints = Movement;
			}
				m_freeze = value;
			}
		}

		protected delegate void ExecuteActionsDlg(GameTime gameTime);
		protected ExecuteActionsDlg ExecuteActions;

		public Unit(ATGame game)
			: base((Game)game)
		{
			// TODO: Construct any child components here
			atGame.Components.Add(this);
			Attackers = new HashSet<Unit>();
			ExecuteActions += delegate(GameTime gameTime) { };
			InfoPopup = new UnitContextInfo(game, this);
			//ReachableHexes = new List<HexTile>();
			//AttackableHexes = new List<HexTile>();
		}

		public Unit(ATGame game, Player owner, int move, int str, int range, int rangedStr)
			: this(game)
		{
			this.Visible = false;
			MovementPoints=move;
			Movement = move;
			Strength=str;
			Range=range;
			RangedStrength=rangedStr;
			Owner = owner;
			
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
			unitClass = this.GetType().Name;
		}

		protected override void LoadContent()
		{
			spriteSheet_path = "icon_sheet";
			spriteSheet = Game.Content.Load<Texture2D>(spriteSheet_path);
			coloredSpriteSheet = spriteSheet;
			
			base.LoadContent();			
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here

			//PaletteSwap(atGame.activePlayer.TeamColor);
			//base.Update(gameTime);

			ExecuteActions(gameTime);
			
		}
				
		public void Select()
		{
			Unit prev = atGame.ActivePlayer.SelectedUnit;
			atGame.ActivePlayer.SelectedUnit = this;

			//deselectionner la precedente unit�
			if (prev != null)
			{
				if ( atGame.CurrentPhase != ATGame.GamePhase.GP_Dispatch )
					prev.ResetRangeGraphics();

				prev.UnSelect();
			}


			if (this != prev &&  atGame.CurrentPhase != ATGame.GamePhase.GP_Dispatch)
				ResetRangeGraphics();

			
			if (this == atGame.ActivePlayer.SelectedUnit)
			{
				this.InfoPopup.Show();

				if (atGame.CurrentPhase == ATGame.GamePhase.GP_Movement)
					this.HighlightMovementRange();
				else
					if (atGame.CurrentPhase == ATGame.GamePhase.GP_Combat)
						this.HighlightAttackRange();
			}
						
			this.SetAlphaBlink(0, 0.5f, 1f, 1.5f, true);
		}

		public void UnSelect()
		{

			
			//List<HexTile> reachablePlaces = atGame.GameBoard.GetNeighboursRanged(OccupiedHex, this.MovementPoints);
			//OccupiedHexm_lastRefHex.colorOffset = new Vector4(0f, 0f, 0f, 0f);

			if (atGame.CurrentPhase != ATGame.GamePhase.GP_Dispatch)
			{
				ResetRangeGraphics();
				OccupiedHex.ResetGraphics();
			}

			colorOffset = Vector4.Zero;

			AlphaBlinkEnable = false;
			BounceEnable = false;
			ColorBlinkEnable = false;
			this.InfoPopup.Hide();
			
		}

		public bool TargetUnit(Unit target, bool mark)
		{
			if (target == null) return false;
			//recliquer sur une cible unfreeze l'unit� en cours
			if (target.Attackers.Contains(this) && !mark)
			{	
				Freeze = target.MarkTargetedBy(this, false);
			}
			else if (!Freeze && mark)
			{	
				Freeze = target.MarkTargetedBy(this, true);
			}
			return target.Attackers.Contains(this);
		}
		/// <summary>
		/// Marque l'unit� comme (d�)cibl�e et ajoute/retire cet attaquant dans sa liste de menaces
		/// </summary>
		/// <param name="attacker">Attaquant de la cible - Si null, retourne le nombre d'attaquants de cette cible</param>
		/// <param name="mark">true: ajouter et marquer - false: retirer et d�cibler</param>
		/// <returns>true si l'attaquant est dans la liste des menaces</returns>
		public bool MarkTargetedBy(Unit attacker, bool mark)
		{
			
			if (attacker == null ) return false;

			if (attacker != null){
				if ( mark){
					if (attacker.AttackableHexes.Contains(this.OccupiedHex) && this.OccupiedHex.Occupant.Owner == atGame.OpposingPlayer)
					{
						Attackers.Add(attacker);
						this.SetColorBlink(atGame.ActivePlayer.GetColorBlinkList(), 0.5f, true, false, true);
					}
											
				}
				else
				{
					ColorBlinkEnable = false;
					Attackers.Remove(attacker);
				}
					
			}

			return Attackers.Contains(attacker);
		}

		public void PutOnHex(HexTile hex)
		{
			sprOrigin = hex.SpritePosition-atGame.Panning;
			//si deja plac� ailleurs, liberer la case de d�part
			if (OccupiedHex != null)
				OccupiedHex.Occupant = null;

			if (hex.Occupant != null)
			{
				//
				Owner.OwnedUnits.Remove(this);
				atGame.Components.Remove(this);
			}
			else
				hex.Occupant = this;

			
			this.PaletteSwap(Owner.TeamColor);
			
			this.OccupiedHex = hex;
			this.Visible = true;
		}

		public bool DispatchableOnHex(HexTile hex)
		{
			return (( hex.Status & atGame.ActivePlayer.DispatchableHex ) == atGame.ActivePlayer.DispatchableHex
				&& hex.Occupant == null );
		}

		public void DispatchOnHex(HexTile hex)
		{
			PutOnHex(hex);
			Owner.OwnedUnits.Add(this);
			Console.WriteLine(unitClass+" Spawned");
		}
		public void MoveTo(GameTime gameTime)
		{
			//(float)gameTime.ElapsedGameTime.TotalSeconds * moveSpeed
			//float remDist = Vector2.Distance(nextDest, sprOrigin - atGame.Panning) / Vector2.Distance(OccupiedHex.SpritePosition, sprOrigin - atGame.Panning);

			nextDest = nextDestTile.SpritePosition;
			float totalDist = Vector2.Distance(OccupiedHex.SpritePosition, nextDest);
			
			sprOrigin += (nextDest - OccupiedHex.SpritePosition) * moveSpeed / totalDist;

			Console.WriteLine(unitClass + " at " + sprOrigin + " moving to " + nextDest);
			if (Vector2.Distance(SpritePosition, nextDest) <= 0.5f * moveSpeed)
			{
				if (atGame.CurrentPhase != ATGame.GamePhase.GP_Dispatch)
				{
					HexTile.ResetGraphics(ReachableHexes, true);
					OccupiedHex.ResetGraphics(true);
				}


				PutOnHex(nextDestTile);
				ExecuteActions -= MoveTo;
				Freeze = true;
				BounceEnable = false;
				Console.WriteLine(unitClass + " arrived at " + nextDest);

			}
				
		}

		public void StartMoveTo(HexTile tileDest, bool checkForRange=false, bool checkIfFrozen=false)
		{
			bool frozenValidity = ( (!Freeze && checkIfFrozen) || !checkIfFrozen);
			bool rangeValidity = ((checkForRange && ReachableHexes.Contains(tileDest)) || !checkForRange);

			if (frozenValidity && rangeValidity)
				if (Vector2.Distance(SpritePosition, OccupiedHex.SpritePosition) <= 1)
				{
					float totalDist = Vector2.Distance(OccupiedHex.SpritePosition, tileDest.SpritePosition);
					float expectedDuration = totalDist/moveSpeed;

					nextDestTile = tileDest;
					ExecuteActions += MoveTo;
				
					SetBounce(12, -1, expectedDuration/100, false, true);
				
				}
		
		}

		
		
		private bool CheckVisibility()
		{
 			return ( true || Owner==atGame.ActivePlayer || ATGame.DEBUG_MODE );
		}
		public override void Draw(GameTime gameTime)
		{
			if (VisibleToOpponent)
			{			
				//check si dans la partie visible
				if (SpriteCenter.X > -Width && SpriteCenter.X < atGame.ScreenWidth+Width
				&& SpriteCenter.Y > -Height && SpriteCenter.Y < atGame.ScreenHeight+Height)
				{
					finalColor = new Color(Color.White.ToVector4() * colorMultiplier + colorOffset * colorOffset.W);

					this.spriteBatch.Begin();                
					base.DrawFX(gameTime);

					//hex
					this.spriteBatch.Draw(coloredSpriteSheet, SpritePosition+bounceOffset, drawArea, finalColor);

					this.spriteBatch.End();
					base.Draw(gameTime);
				}
			}
		}

		public void DrawFrozen(GameTime gameTime)
		{
			Vector4 grey = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);
			Desaturate();
			finalColor = new Color(grey * colorMultiplier + colorOffset * colorOffset.W);
		}

		public void HighlightMovementRange(bool highlight=true)
		{

			List<HexTile> mvt = ReachableHexes;

			mvt.Add(OccupiedHex);
			foreach (HexTile h in mvt)
			{
					
				h.ColorBlinkEnable = false; //retirer le precedent blink pour eviter la superposition des effets
				h.ResetGraphics(true);
				h.TeamColorBlink(Owner);					
					
				h.colorMultiplier.W = 1f;
				h.ColorBlinkEnable = true;

				//h.SetHighlighted(highlight);
			}

			OccupiedHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.75f);

		}
		public virtual void HighlightAttackRange(bool highlight = true)
		{

			List<HexTile> atk = AttackableHexes;

			atk.Add(OccupiedHex);
			foreach (HexTile h in atk)
			{
				
				h.ColorBlinkEnable = false; //retirer le precedent blink pour eviter la superposition des effets
				h.ResetGraphics(true);
				h.TeamColorBlink(Owner);
				h.colorMultiplier.W = -0.5f;
				h.ColorBlinkEnable = true;

				//h.SetHighlighted(highlight);
			}

			OccupiedHex.colorOffset = new Vector4(0.25f, 0.25f, 0.5f, 0.75f);

		}	

		public virtual void ResetRangeGraphics(){
			HexTile.ResetGraphics(ReachableHexes, true);
			HexTile.ResetGraphics(AttackableHexes, true);
		}


		public void PaletteSwap(Color targetColor)
		{

			coloredSpriteSheet = new Texture2D(GraphicsDevice, spriteSheet.Width, spriteSheet.Height);
			Console.WriteLine(targetColor);

			int nbPixels = spriteSheet.Width * spriteSheet.Height;
			Color[] pixels = new Color[nbPixels];
			spriteSheet.GetData(pixels);

			for (int p = 0; p < nbPixels; p++)
			{
				Color pix = pixels[p];
				byte a = pix.A;

				if (pix.R == pix.G && pix.G == pix.B)
				{

					pix.R = (byte)((pix.R*targetColor.R) / 256f);
					pix.G = (byte)((pix.G*targetColor.G) / 256f);
					pix.B = (byte)((pix.B*targetColor.B) / 256f);

				}
				/*else
					pix.A = 0;*/

				pix.A = a;
				pixels[p] = pix;
			}

			coloredSpriteSheet.SetData(pixels);
			
		}

		public void Desaturate()
		{

			coloredSpriteSheet = new Texture2D(GraphicsDevice, spriteSheet.Width, spriteSheet.Height);
			//Console.WriteLine(targetColor);

			int nbPixels = spriteSheet.Width * spriteSheet.Height;
			Color[] pixels = new Color[nbPixels];
			spriteSheet.GetData(pixels);

			for (int p = 0; p < nbPixels; p++)
			{
				Color pix = pixels[p];
				byte a = pix.A;

				float avg = ((pix.R + pix.G + pix.B) / 3f) / 255f;

				pix.R = (byte)(avg * 255 );
				pix.G = (byte)(avg * 255 );
				pix.B = (byte)(avg * 255 );

				pix.A = a;
				pixels[p] = pix;
			}

			coloredSpriteSheet.SetData(pixels);

		}


		public List<Unit> GetPotentialAttackers(Unit filtered=null) {

			Player threat = (Owner == atGame.ActivePlayer) ? atGame.OpposingPlayer : atGame.ActivePlayer;

			List<Unit> threatList = new List<Unit>();
			foreach (Unit u in threat.OwnedUnits) {
				if (u.AttackableHexes.Contains(OccupiedHex) && u != filtered)
					threatList.Add(u);
			}

			return threatList;
		}

		public bool IsUnderCursor(MouseState mouseState)
		{
			Rectangle boundingBox=new Rectangle((int)SpritePosition.X,(int)SpritePosition.Y,drawArea.Width,drawArea.Height);
			return (boundingBox.Contains(new Point(mouseState.X, mouseState.Y)));

		}

		public void ResetGraphics()
		{
			colorOffset = Vector4.Zero;
			colorMultiplier = Vector4.One;
			ColorBlinkEnable = false;

		}

		public static void ResetGraphics(List<Unit> resetList)
		{
			foreach (Unit u in resetList)
				u.ResetGraphics();
		}


	}




}
