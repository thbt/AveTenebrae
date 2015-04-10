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
using AStar;
using System.Threading;



namespace WarGame {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ATGame : Microsoft.Xna.Framework.Game {

		
		public ScreenOverlay Overlay { get; private set; }
		public int nbArchers = 3, nbHeavyKnights = 3, nbCavalry = 3;
		public static bool DEBUG_MODE { get; set; }

		GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		InputManager inputManager;

		public Vector2 Panning;
		public int ScreenWidth { get; protected set; }
		public int ScreenHeight { get; protected set; }

		public Player PlayerA { get; private set;}
		public Player PlayerB { get; private set;}
        public Player ActivePlayer {  get; private set;}
		public Player OpposingPlayer {get; private set;}

		private float m_genericTimer1 = 0f;
		private float m_genericTimer2 = 0f;
		private float m_genericTimer3 = 0f;

		private HashSet<Unit> m_targetedUnits = new HashSet<Unit>();
		private bool m_attackPlayed=false;

		private float m_phaseChangeTimer = 0;

        private Song m_zicFF7;
        private Song m_zicDispatch;

		public Board GameBoard { get; protected set;}

		public enum GamePhase
		{
			GP_Dispatch,
			GP_Movement,
			GP_Combat,
			GP_GameOver
		}
		public GamePhase CurrentPhase { get; set; }
 
		private delegate void GamePhaseLogic(GameTime gameTime);
		private GamePhaseLogic m_currentPhaseLogic;

        
        private BattleSubPhase m_battleSubPhase = BattleSubPhase.None;
        private HashSet<Unit> m_helpers;
        private HashSet<Unit> m_atkers;
        private HashSet<Unit> m_killed;

		public ATGame() {
			graphics = new GraphicsDeviceManager(this);

            bool demo = false;

            if (!demo)
            {
                ScreenWidth = 1280; ScreenHeight = 720;
            }
            else
            {
                ScreenWidth = 1024; ScreenHeight = 768;
                graphics.IsFullScreen = true;
            }
		
			graphics.PreferredBackBufferWidth = ScreenWidth;
			graphics.PreferredBackBufferHeight = ScreenHeight;
			graphics.SynchronizeWithVerticalRetrace = true;
			//graphics.PreferMultiSampling = true;			
			graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            m_helpers = new HashSet<Unit>();
            m_atkers = new HashSet<Unit>();
            m_killed = new HashSet<Unit>();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			// TODO: Add your initialization logic here
			Overlay = new ScreenOverlay(this);
			Panning = Vector2.Zero;
			GameBoard = new Board(this);			

			PlayerA = new Player(this,TeamColors.Red,true);
			PlayerB = new Player(this,TeamColors.Blue,false);
			PlayerA.Name = "Human A";
			PlayerB.Name = "Human B";

			ActivePlayer = PlayerA;
			OpposingPlayer = PlayerB;

			inputManager = new InputManager(this);
			
			spriteBatch = new SpriteBatch(GraphicsDevice);
			m_currentPhaseLogic = delegate(GameTime GameTime){};			

			base.Initialize();
			Panning.Y += Overlay.TopPanelArea.Height+32;
			PrepareDispatchPhase();
			//m_currentPhaseLogic += DispatchPhase; //!!! retirer cette ligne après les tests !!!
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			ResourceManager.font = Content.Load<SpriteFont>("Fonts/Arial");
            m_zicFF7 = Content.Load<Song>("combat");
            m_zicDispatch = Content.Load<Song>("placement");
			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
				|| Keyboard.GetState().IsKeyDown(Keys.Escape) )
				this.Exit();

			m_genericTimer1 = (m_genericTimer1+(float)gameTime.ElapsedGameTime.TotalSeconds)%float.MaxValue;
			m_genericTimer2 = (m_genericTimer2 + (float)gameTime.ElapsedGameTime.TotalSeconds) % float.MaxValue;
			m_genericTimer3 = (m_genericTimer3+ (float)gameTime.ElapsedGameTime.TotalSeconds) % float.MaxValue;
			// TODO: Add your update logic here
			
			m_currentPhaseLogic(gameTime);
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {

			spriteBatch.Begin();
			GraphicsDevice.Clear(Color.MidnightBlue);
	// TODO: Add your drawing code here

			base.Draw(gameTime);
			spriteBatch.End();
		}

		public void ResetTimers(float phase = 0, float generic1 = 0, float generic2 = 0, float generic3 = 0)
		{
			//m_phaseChangeTimer = phase;
			m_genericTimer1 = generic1;
			m_genericTimer2 = generic2;
			m_genericTimer3 = generic3;

		}

		public void SwapPlayerTurns()
		{

			if (ActivePlayer.SelectedUnit!=null)
				ActivePlayer.SelectedUnit.UnSelect();
			if (ActivePlayer.SelectedHex != null)
				ActivePlayer.SelectedHex.UnSelect();

			Player oldActivePlayer = ActivePlayer;
			ActivePlayer = OpposingPlayer;
			OpposingPlayer = oldActivePlayer;

			if (ActivePlayer.SelectedUnit != null)
				ActivePlayer.SelectedUnit.UnSelect();
			if (ActivePlayer.SelectedHex != null)
				ActivePlayer.SelectedHex.UnSelect();

            ActivePlayer.ResetUnitStatus();
			ActivePlayer.UnfreezeUnits();
            OpposingPlayer.ResetUnitStatus();
			OpposingPlayer.UnfreezeUnits();
            
		}


		private void PrepareDispatchPhase()
		{
            if (MediaPlayer.State != MediaState.Playing )
            {
                MediaPlayer.Volume = 0.25f;
                MediaPlayer.Play(m_zicDispatch);
                MediaPlayer.IsRepeating = true;
            }


			m_currentPhaseLogic -= DispatchPhase;

			int unitsPerTeam = nbArchers + nbCavalry + nbHeavyKnights;
			int dispatchSpotsPerTeam = unitsPerTeam * 2;
			int dispatchColumns = 3;

			//dispatch team A
			for (int x = 0; x < dispatchColumns; x++)
			{
				for (int y = 0; y < GameBoard.RowCount; y++)
				{
					GameBoard.tileMap[y, x].SetDispatchable(true, false);
				}
			}


			//dispatch team B
			for (int x = dispatchColumns; x > 0; x--)
			{
				for (int y = 0; y < GameBoard.RowCount; y++)
				{
					GameBoard.tileMap[y, GameBoard.ColumnCount-x].SetDispatchable(false, true);
				}
			}

			Overlay.DisplayMessage(ScreenOverlay.BigMessages.Dispatch);
			m_currentPhaseLogic+=DispatchPhase;
			CurrentPhase = GamePhase.GP_Dispatch;
		}

		private void DispatchPhase(GameTime gameTime)
		{
			int unitsPerTeam = nbArchers + nbCavalry + nbHeavyKnights;
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);
			bool readyA = (this.PlayerA.OwnedUnits.Count >= unitsPerTeam);
			bool readyB = (this.PlayerB.OwnedUnits.Count >= unitsPerTeam);


			if (ActivePlayer == PlayerA && readyA && nbFrozenUnits >= unitsPerTeam)
			{
				if (ActivePlayer.SelectedUnit != null)
					ActivePlayer.SelectedUnit.UnSelect();

				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					
					m_phaseChangeTimer = 0;
					//Overlay.DisplayMessage(ScreenOverlay.BigMessages.Dispatch);
					
					Console.WriteLine("Swapping players");
					SwapPlayerTurns();
					PrepareDispatchPhase();
				}

			}

			if (readyA && readyB && nbFrozenUnits >= unitsPerTeam)
			{
                if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
                {
                    foreach (HexTile h in GameBoard.GetTileList())
                    {
                        h.SetDispatchable(false, false);
                        h.SetHighlighted(false);
                        h.ResetGraphics(true);
                    }
                    ResetTimers();

                    Console.WriteLine("Dispatch phase finished");
                    m_currentPhaseLogic -= DispatchPhase;
                    SwapPlayerTurns();
                    HexTile.ResetGraphics(GameBoard.GetTileList(), true);
                    m_currentPhaseLogic = MovementPhase;

                    CurrentPhase = GamePhase.GP_Movement;
                    Overlay.DisplayMessage(ScreenOverlay.BigMessages.Movement);

                    MediaPlayer.Volume = 0.35f;
                    MediaPlayer.Play(m_zicFF7);

                    m_phaseChangeTimer = 0f;
                }
                else
                    MediaPlayer.Volume -= m_phaseChangeTimer/2;

			}

		}

		private void MovementPhase(GameTime gameTime)
		{
			
			//quand terminée, lancer la CombatPhase
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);
			/*foreach (Unit u in ActivePlayer.ownedUnits)
			{
				if (u.Freeze == true) nbMovedUnits++;
			}*/

			if (nbFrozenUnits >= ActivePlayer.OwnedUnits.Count)//forcé false pour les tests de movements
			{
				if ((m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					m_phaseChangeTimer = 0;
					Console.WriteLine("Movement phase finished");

					if (ActivePlayer.SelectedUnit != null)
						ActivePlayer.SelectedUnit.UnSelect();

					ActivePlayer.UnfreezeUnits();
					HexTile.ResetGraphics(GameBoard.GetTileList(), true);
					CurrentPhase = GamePhase.GP_Combat;
					m_currentPhaseLogic = CombatPhase;
					Overlay.DisplayMessage(ScreenOverlay.BigMessages.Combat);
				}				
			}
            else m_phaseChangeTimer = 0;
		}


		private void CombatPhase(GameTime gameTime)
		{			
			
			int nbFrozenUnits = ActivePlayer.OwnedUnits.Count(u => u.Freeze == true);			
			//quand terminée, lancer la MovementPhase pour l'autre équipe
			if (nbFrozenUnits >= ActivePlayer.OwnedUnits.Count)
			{
				if (ActivePlayer.SelectedUnit != null)
					ActivePlayer.SelectedUnit.UnSelect();

				if ( (m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.5f)
				{
					m_targetedUnits.Clear();
					foreach (Unit u in OpposingPlayer.OwnedUnits)
						if (u.Attackers.Count > 0)
						{
							m_targetedUnits.Add(u);							
						}

					ResetTimers();
					Console.WriteLine("Goto Execute Battle");
					
                    HexTile.ResetGraphics(GameBoard.GetTileList(), true);
                    m_battleSubPhase = BattleSubPhase.StartAttack;
                    m_phaseChangeTimer = -0.25f;
                    m_currentPhaseLogic = ExecuteBattle;
				}
			}			
		}
        private enum BattleSubPhase { Init, StartAttack, StartDeathAnim, CalculateOutcome, EndPhase, Other, None, PlayDeathAnim, StartBattleAnim };
		/// <summary>
		/// Une fois toutes les unités fixées sur une action, déroulement animé du combat
		/// </summary>
		/// <returns>true quand le combat est fini</returns>
		private void ExecuteBattle(GameTime gameTime)
		{
			if (ActivePlayer.SelectedUnit != null)			
				ActivePlayer.SelectedUnit.UnSelect();

			
			float killDelay = 0.85f;
			float attackdelay = 0.45f;
			float ms = 1000;


            //tant qu'il y a des unités engagées en combat
            if (m_targetedUnits.RemoveWhere(unit => (unit.Attackers.Count == 0)) < m_targetedUnits.Count && m_targetedUnits.Count > 0)
            {
                
				Unit u = m_targetedUnits.Reverse().ElementAt(0);

				//si les sons d'attaques ont été joués et les scores ont calculés, tuer/blesser/reculer les unités perdantes
				if (m_genericTimer1 >= killDelay)
				{
                    //un seul passage ici par groupe de combat
                    if (m_battleSubPhase == BattleSubPhase.CalculateOutcome)
                    {

                        float scoreAtk = 0, scoreDef = 0;
                        foreach (Unit a in m_atkers) { scoreAtk += a.EvaluateContextDamage(u); }
                        foreach (Unit d in m_helpers) { scoreDef += d.EvaluateContextDamage(u.Attackers.ElementAt(0)); }

                        float ratioAtk = (scoreAtk+1) / (scoreDef+1);
                        float ratioDef = (scoreDef + 1) / (scoreAtk + 1);
                        int dice = ResourceManager.Random.Next(1, 7);
                        int dmgAtk = (int)Math.Ceiling(ratioAtk * dice);
                        dice = ResourceManager.Random.Next(1, 7);
                        int dmgDef= (int)Math.Ceiling(ratioDef * dice);

                        Console.WriteLine("Score Atk  = " + scoreAtk + " - Score Def = " + scoreDef);
                        Console.WriteLine("Ratio Atk  = " + ratioAtk + " - Ratio Def = " + ratioDef);
                        Console.WriteLine("Dmg Atk  = " + dmgAtk + " - Dmg Def = " + dmgDef);

                        foreach (Unit a in m_atkers){
                           a.SetBounce(1, -1, (a.HealthPoints/a.BaseStrength)*0.125f, true, true);
                           
                           if ( a.TakeDamageAndReportDeath(dmgDef)){
                               m_killed.Add(a);
                           }
                        }
                        m_atkers.Except(m_killed);
                        m_helpers.Add(u);
                        foreach (Unit d in m_helpers)
                        {
                            d.SetBounce(1, -1, (d.HealthPoints / d.BaseStrength) * 0.125f, true, true);
                            if (d.TakeDamageAndReportDeath(dmgAtk))
                            {
                                m_killed.Add(d);
                            }
                        }
                        m_helpers.Except(m_killed);

                        Console.WriteLine("Targeted ----\n" + m_targetedUnits.Count);
                        Console.WriteLine("Helpers ----\n" + m_helpers.Count);
                        Console.WriteLine("Attackers ----\n" + m_atkers.Count);

                        m_genericTimer1 = 0;
                        m_targetedUnits.Remove(u);
                        m_battleSubPhase = BattleSubPhase.StartBattleAnim;
                    }
                    if (m_battleSubPhase == BattleSubPhase.StartBattleAnim)
                    {
                        m_targetedUnits.Remove(u);
                        u.HitSound.Play();
                        if (m_killed.Count > 0) m_battleSubPhase = BattleSubPhase.StartDeathAnim;
                        //else m_battleSubPhase = BattleSubPhase.StartAttack;                        
                    }

                    //joue une fois par groupe de combat tant qu'il en reste
                    if (m_battleSubPhase == BattleSubPhase.StartDeathAnim  && m_killed.Count > 0 )
					{

                        m_killed.ElementAt(0).ScreamSound.Play(0.75f, 0, 0);
						foreach (Unit k in m_killed) { k.Kill(true); }
                        m_battleSubPhase = BattleSubPhase.PlayDeathAnim;
					}
                    //verification periodique que l'animation de mort des unités est finie ou non 
                    if (m_battleSubPhase == BattleSubPhase.PlayDeathAnim && !m_killed.ElementAt(0).Visible)
					{
                        
                        m_killed.Remove(u);
                        m_killed.Clear();
                        
                        if (m_killed.Count == 0)
                        {
                            
                            m_battleSubPhase = BattleSubPhase.StartAttack;
                        }
                        m_genericTimer2 = -1f;
					}					
				}
				//sinon jouer les sons d'attaque apres un delai 'cosmetique'
                if (m_genericTimer2 > attackdelay && m_battleSubPhase == BattleSubPhase.StartAttack )
				{
                    foreach (Unit a in m_atkers) { a.ResetGraphics(); }
                    foreach (Unit d in m_helpers) { d.ResetGraphics(); }
                    HashSet<Unit> prevHelpers = new HashSet<Unit>(m_helpers);
                    HashSet<Unit> prevAtkers = new HashSet<Unit>(m_atkers);
                    m_atkers.Clear();
                    m_helpers.Clear();
                    
                    //creation des groupes de combats
                    foreach (Unit hu in OpposingPlayer.OwnedUnits)
                    {
                        foreach (Unit atk in u.Attackers)
                        {
                            if (atk.HealthPoints > 0)
                            {
                                m_atkers.Add(atk);
                                //ajout d'une unité alliée si elle est à portée, n'est pas deja attaquée ni engagée dans un autre sauvetage
                                if (hu.AttackableHexes.Contains(atk.OccupiedHex) && hu.Attackers.Count == 0 && !prevHelpers.Contains(hu))
                                    m_helpers.Add(hu);
                            }

                        }
                    }
                    					
                    m_battleSubPhase = BattleSubPhase.CalculateOutcome;
                    //u.Attackers.Reverse();
                    foreach (Unit atk in m_atkers)
                    {
                        atk.AttackSound.Play();
                        atk.Slash();
                    }

                    foreach (Unit def in m_helpers)
                    {
                        def.AttackSound.Play();
                        def.Slash();
                    }
                       
                    u.AttackSound.Play();
                    u.Slash();

					m_genericTimer2 = -1f;
				}
				else// if (m_phaseChangeTimer > 0)
				{
					//ResetTimers();
				}

			}

			//conditions d'arret de la phase
            if ( m_targetedUnits.Count <= 0 && (m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds) > 1.5f)
			{
				if (ActivePlayer.OwnedUnits.Count > 0
					&& OpposingPlayer.OwnedUnits.Count > 0)
				{
					SwapPlayerTurns();
					CurrentPhase = GamePhase.GP_Movement;
					m_currentPhaseLogic = MovementPhase;
					Overlay.DisplayMessage(ScreenOverlay.BigMessages.Movement);
					Console.WriteLine("Combat phase finished");
				}
				else if (m_phaseChangeTimer > 2f)
				{
					LaunchGameOver();
				}
			}
		}

		private void GameOverPhase(GameTime gameTime)
		{
			m_phaseChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            MediaPlayer.Volume -= m_phaseChangeTimer / 10000;
			if ((int)m_phaseChangeTimer == 10)
			{
				m_phaseChangeTimer++;
				Overlay.DisplayEasterEgg();
			}
			else if ((int)m_phaseChangeTimer == 13){
				m_currentPhaseLogic = delegate { };
				m_phaseChangeTimer = float.PositiveInfinity;
                MediaPlayer.Stop();
				this.Exit();
			}
		}

		public void LaunchGameOver()
		{
			m_phaseChangeTimer = 0;
			m_currentPhaseLogic = GameOverPhase;
			Console.WriteLine("Game over");
			foreach (Unit u in Components.OfType<Unit>())
			{
				u.Freeze = false;
				u.SetBounce(24, -1, 0.8f+0.4f*(float)ResourceManager.Random.NextDouble(), false, true);
			}
			CurrentPhase = GamePhase.GP_GameOver;			
			Overlay.DisplayMessage(ATGame.GamePhase.GP_GameOver, null, 1f, float.PositiveInfinity);
		}
	}
}
