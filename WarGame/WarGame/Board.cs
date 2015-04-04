using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WarGame {
	public class Board : ATComponent {

		public HexTile[,] tileMap {get; protected set;}
		public readonly int ColumnCount, RowCount;
		public int BoardPixelWidth { get { return (int)tileMap[RowCount - 1, ColumnCount - 1].SpritePosition.X + tileMap[RowCount - 1, ColumnCount - 1].Width; } }
		public int BoardPixelHeight { get { return (int)tileMap[RowCount - 1, ColumnCount - 1].SpritePosition.Y + tileMap[RowCount - 1, ColumnCount - 1].Height; } }
		public int HexPixelWidth { get { return (int)tileMap[0, 0].Width; } }
		public int HexPixelHeight { get { return (int)tileMap[0, 0].Height; } }
		
		public Dictionary<HexTile, List<HexTile>> tileGraph;
		public Board(ATGame game) : base(game) {

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
			int[,] tmpMap = new int[20, 20];
			for (int y=0; y<tmpMap.GetLength(0); y++)
				for (int x=0; x < tmpMap.GetLength(1); x++)
				{
					tmpMap[y, x] = ResourceManager.Random.Next(1, 4);
				
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
		
		public List<HexTile> GetNeighbours(HexTile tile) {


			List<HexTile> neighbours = new List<HexTile>();

			Point p = tile.GridPosition;

			bool xParity = tile.GridPosition.X % 2 != 0; // true : pair, false : impair
			// Pour une tile (X,Y) avec x pair, les voisins sont :
			if (!xParity) { // pair
				if (p.X > 0 && p.Y > 0) neighbours.Add(tileMap[ p.Y - 1,p.X - 1]);//NO
				if (p.Y > 0) neighbours.Add(tileMap[p.Y - 1,p.X]); //N
				if (p.X < ColumnCount-1 && p.Y > 0) neighbours.Add(tileMap[ p.Y - 1,p.X + 1]); //NE
				if (p.X < ColumnCount-1) neighbours.Add(tileMap[ p.Y,p.X + 1]); //SE
				if (p.X > 0) neighbours.Add(tileMap[p.Y,p.X - 1]);
				if (p.Y < RowCount-1) neighbours.Add(tileMap[p.Y + 1,p.X]);
			}// Pour une tile (X,Y) avec x impair, the neighbors are:			
			else { // impair
				if (p.X > 0) neighbours.Add(tileMap[ p.Y,p.X - 1]);
				if (p.Y > 0) neighbours.Add(tileMap[ p.Y - 1,p.X]);
				if (p.X < ColumnCount-1) neighbours.Add(tileMap[ p.Y,p.X + 1]);
				if (p.X < ColumnCount-1 && p.Y < RowCount-1) neighbours.Add(tileMap[ p.Y + 1,p.X + 1]);
				if (p.Y < RowCount-1) neighbours.Add(tileMap[ p.Y + 1,p.X]);
				if (p.X > 0 && p.Y < RowCount-1) neighbours.Add(tileMap[p.Y + 1,p.X - 1]);
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
				if (t.Walkable)
					tileGraph.Add(t, this.GetNeighbours(t));
			}
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

		public HexTile GetHexAtCoordinates(Vector2 coords)
		{
			return GetHexAtCoordinates(coords.X, coords.Y);
		}

		public List<HexTile> GetNeighboursRanged(HexTile source, int range)
		{			
			//prend la liste des voisins directs
			List<HexTile> neighbours = GetNeighbours(source);

			//si portée supérieure à 0, va chercher les voisins lointains
			if (--range > 0)
			{
				HashSet<HexTile> childrenNeighbours = new HashSet<HexTile>();

				foreach (HexTile sub in neighbours)
				{
					childrenNeighbours.UnionWith(GetNeighboursRanged(sub, range));
				}
				neighbours.AddRange(childrenNeighbours);
			}

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
