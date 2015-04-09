using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame {
	public class Board : ATDrawableComponent {


		private Texture2D m_bgGradient;

		public HexTile[,] tileMap {get; protected set;}
		public readonly int ColumnCount, RowCount;
		public int BoardPixelWidth { get { return (int)tileMap[RowCount - 1, ColumnCount - 1].SpritePosition.X + tileMap[RowCount - 1, ColumnCount - 1].Width; } }
		public int BoardPixelHeight { get { return (int)tileMap[RowCount - 1, ColumnCount - 1].SpritePosition.Y + tileMap[RowCount - 1, ColumnCount - 1].Height; } }
		public int HexPixelWidth { get { return (int)tileMap[0, 0].Width; } }
		public int HexPixelHeight { get { return (int)tileMap[0, 0].Height; } }


		
		public Dictionary<HexTile, List<HexTile>> tileGraph;
		public Board(ATGame game) : base(game) {

			game.Components.Add(this);
			//variables pour tests, à cleaner plus tard
			const int n = int.MaxValue;
			const int p = 1;
			const int f = 2;
			const int h = 3;

			/*
			int[,] tmpMap = new int[,] 
            {
                {p,p,p,p,p,p,p,p,p,p,p,p,p},
                {f,f,f,f,p,f,f,f,f,f,p,f,f},
                {f,f,f,f,p,h,h,h,h,h,p,h,f},
                {h,h,h,h,p,h,p,h,p,p,f,f,f},
                {p,p,p,p,p,p,p,p,h,p,h,f,f},
                {p,p,h,p,p,p,p,h,h,h,h,h,h},
				{f,f,f,f,p,h,h,h,h,h,p,h,f},
                {h,h,h,h,p,h,p,h,p,p,f,f,f},
            };*/
			int[,] tmpMap = new int[12, 13];
			for (int y=0; y<tmpMap.GetLength(0); y++)
				for (int x=0; x < tmpMap.GetLength(1); x++)
				{
					int random = (int)ResourceManager.Random.Next(0, 101);
					if (random < 70) tmpMap[y, x] = p;
					else if (random < 90) tmpMap[y, x] = f;
					else tmpMap[y, x] = h;
				
				}


			/* +---> x = upperbound1
			 * |
			 * v
			 * y = upperbound0
			 */

			ColumnCount = tmpMap.GetUpperBound(1) + 1;
			RowCount = tmpMap.GetUpperBound(0) + 1;
			Console.WriteLine("Taille du plateau: {0},{1}", ColumnCount, RowCount);
			tileMap = new HexTile[RowCount, ColumnCount];

			for(int y = 0; y < RowCount; y++) {
				for(int x = 0; x < ColumnCount; x++) {
					//à reviser?
					switch(tmpMap[y, x]){
						case p: tileMap[y, x]=HexTile.CreatePlain(atGame,x,y);
							break;
						case h: tileMap[y, x] = HexTile.CreateHill(atGame, x, y);
							break;
						case f: tileMap[y, x] = HexTile.CreateForest(atGame, x, y);
							break;
	
					}
					
					
					atGame.Components.Add(tileMap[y, x]);
				}
			}
		}
		
		public List<HexTile> GetNeighbours(HexTile tile, int movement=int.MaxValue) {


			List<HexTile> neighbours = new List<HexTile>();

			Point p = tile.GridPosition;

			bool xParity = tile.GridPosition.X % 2 != 0; // true : pair, false : impair
			// Pour une tile (X,Y) avec x pair, les voisins sont :
			if (!xParity) { // pair
				if (p.X > 0 && p.Y > 0 && tileMap[p.Y - 1, p.X - 1].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y - 1, p.X - 1]);//NO
				if (p.Y > 0 && tileMap[p.Y - 1, p.X].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y - 1, p.X]); //N
				if (p.X < ColumnCount - 1 && p.Y > 0 && tileMap[p.Y - 1, p.X + 1].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y - 1, p.X + 1]); //NE
				if (p.X < ColumnCount - 1 && tileMap[p.Y, p.X + 1].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y, p.X + 1]); //SE
				if (p.X > 0 && tileMap[p.Y, p.X - 1].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y, p.X - 1]);
				if (p.Y < RowCount - 1 && tileMap[p.Y + 1, p.X].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y + 1, p.X]);
			}// Pour une tile (X,Y) avec x impair, the neighbors are:			
			else { // impair
				if (p.X > 0 && tileMap[p.Y, p.X - 1].FinalCost <= movement)
					neighbours.Add(tileMap[ p.Y,p.X - 1]);
				if (p.Y > 0 && tileMap[p.Y - 1, p.X].FinalCost <= movement)
					neighbours.Add(tileMap[ p.Y - 1,p.X]);
				if (p.X < ColumnCount - 1 && tileMap[p.Y, p.X + 1].FinalCost <= movement)
					neighbours.Add(tileMap[ p.Y,p.X + 1]);
				if (p.X < ColumnCount - 1 && p.Y < RowCount - 1 && tileMap[p.Y + 1, p.X + 1].FinalCost <= movement)
					neighbours.Add(tileMap[ p.Y + 1,p.X + 1]);
				if (p.Y < RowCount - 1 && tileMap[p.Y + 1, p.X].FinalCost <= movement)
					neighbours.Add(tileMap[ p.Y + 1,p.X]);
				if (p.X > 0 && p.Y < RowCount - 1 && tileMap[p.Y + 1, p.X - 1].FinalCost <= movement)
					neighbours.Add(tileMap[p.Y + 1,p.X - 1]);
			}
			
			return neighbours;
		}

		public List<HexTile> GetTileList() {
			//List<HexTile> tileList = new List<HexTile>();
			/*
			foreach (HexTile t in tileMap) {
				tileList.Add(t);
			}*/
			List<HexTile> tileList = tileMap.Cast<HexTile>().ToList();
			return tileList;
		}

		public void CreateGraph() {
			tileGraph = new Dictionary<HexTile,List<HexTile>>(ColumnCount * RowCount);
			foreach (HexTile t in this.tileMap) { // tile
				//if (t.Walkable)
					tileGraph.Add(t, this.GetNeighbours(t));
			}
		}

		public List<HexTile> FindPath(HexTile start, HexTile goal) {

			Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();
			Dictionary<HexTile, int> costSoFar = new Dictionary<HexTile, int>();

			var frontier = new PriorityQueue<HexTile>();
			frontier.Enqueue(start, 0);

			cameFrom.Add(start, start);
			costSoFar.Add(start, 0);

			while(frontier.Count > 0) {
				var current = frontier.Dequeue();

				if(current.Equals(goal)) {
					break;
				}

				foreach(HexTile next in current.GetNeighbours()) {
					int newCost = costSoFar[current] + next.FinalCost;
					if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) {
						costSoFar.Add(next, newCost);
						int heuristic = Math.Abs(next.GridPosition.X - goal.GridPosition.X) + 
							Math.Abs(next.GridPosition.Y - goal.GridPosition.Y);
						int priority = newCost + heuristic;
						frontier.Enqueue(next, priority);
						cameFrom.Add(next, current);
					}
				}
			}

			// On retrace le chemin à l'envers pour créer la liste
			HexTile currentTile = goal;
			List<HexTile> path = new List<HexTile>();
			path.Add(currentTile);
			while(currentTile != start) {
				currentTile = cameFrom[currentTile];
				path.Add(currentTile);
			}

			return path;
		}

		private static HexTile GetLowestCost(ref List<HexTile> liste) {
			HexTile best = liste[0];

			foreach(HexTile t in liste) {
				if(best != null && best.FinalCost > t.FinalCost)
					best = t;
			}

			return best;
		}

		/*public List<HexTile> Dijkstra(HexTile start, HexTile goal) {
			List<HexTile> path = new List<HexTile>();

			// On crée la liste ouverte et la liste fermée
			List<HexTile> open = this.GetTileList();

			// On remonte le path
			//return GetPathFromExit(from, to);
		} 
		*/

		
		public override void Initialize() {
			// TODO: Add your initialization code here
			

			base.Initialize();
		}

		protected override void LoadContent()
		{
			base.LoadContent();
			m_bgGradient = atGame.Content.Load<Texture2D>("bg_gradient");
		}

		public override void Update(GameTime gameTime) {
			// TODO: Add your update code here

			base.Update(gameTime);
		}

		public HexTile GetHexAtCoordinates(float x, float y){
			
			HexTile nextHex = null;
			float bestDist = float.PositiveInfinity;
			Vector2 position = new Vector2(x, y);

			foreach (HexTile hex in GetTileList())
			{
				float testDist = Vector2.Distance(position, hex.SpriteCenter);
				if (testDist < bestDist)
				{
					bestDist = testDist;
					nextHex = hex;
				}
			}
			return nextHex;

		}

		public override void Draw(GameTime gameTime)
		{

			Color gradColor = Color.DarkSlateBlue*1f;
			gradColor.R = (byte)(gradColor.R*0.85f);
			spriteBatch.Begin();
			spriteBatch.Draw(m_bgGradient, new Rectangle(0, 0, atGame.ScreenWidth, (int)(atGame.ScreenHeight)), gradColor * 0.75f);

			base.Draw(gameTime);
			spriteBatch.End();
		}

		public HexTile GetHexAtCoordinates(Vector2 coords)
		{
			return GetHexAtCoordinates(coords.X, coords.Y);
		}

		public List<HexTile> GetNeighboursRanged(HexTile source, int range, bool useCosts = true, HexTile previousSource = null)
		{
			//prend la liste des voisins directs

			int stepCost = 1;
			List<HexTile> neighbours = new List<HexTile>();
			HashSet<HexTile> childrenNeighbours = new HashSet<HexTile>();

			//selection avec prise en compte des couts de deplacement
			if (useCosts)
			{
				neighbours = GetNeighbours(source, range);

				foreach (HexTile sub in neighbours)
				{
					if (sub != previousSource)
					{
						
						childrenNeighbours.UnionWith(GetNeighboursRanged(sub, range - sub.FinalCost, useCosts, source));
					}
				}
			}
			//selection par portée absolue: si portée supérieure à 0, va chercher les voisins lointains
			
			else
			{
				neighbours = GetNeighbours(source);
				foreach (HexTile sub in neighbours)
				{
					
					//if (sub.totalPathCost >= source.totalPathCost + stepCost || sub.totalPathCost == 0)
					sub.totalPathCost = source.totalPathCost + stepCost;
					if (range > stepCost)
					{						
						childrenNeighbours.UnionWith(GetNeighboursRanged(sub, range - stepCost, useCosts));
						sub.totalPathCost += stepCost;
					}

				}

			}

			neighbours.AddRange(childrenNeighbours);


			return neighbours;

		}

		/// <summary>
		/// NE MARCHE PAS!
		/// Selectionne N cases par propagation à partir de celle choisie comme centre
		/// </summary>
		/// <param name="source"></param>
		/// <param name="hexGroup"></param>
		/// <param name="count"></param>
		public void GetNeighbourGroup(HexTile source, HashSet<HexTile> hexGroup, int count)
		{
			//prend la liste des voisins directs
			List<HexTile> directNeighbours = GetNeighbours(source);

			if (hexGroup.Count < count)
			{

				foreach (HexTile sub in directNeighbours)
				{

					List<HexTile> farNeighbours = GetNeighbours(sub);


					if (hexGroup.Count < count)
					{


						sub.Status = HexTile.HexStatus.HexDS_DispatchableA;
						hexGroup.Add(sub);
						if ((sub.Status & HexTile.HexStatus.HexDS_DispatchableA)
							!= HexTile.HexStatus.HexDS_DispatchableA)
						{
							hexGroup.Union<HexTile>(farNeighbours);
							GetNeighbourGroup(sub, hexGroup, count);
						}

					}


				}
				hexGroup.Add(source);
				source.Status = HexTile.HexStatus.HexDS_DispatchableA;


			}


		}

	}
}
