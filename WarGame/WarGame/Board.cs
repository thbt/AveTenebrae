using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WarGame {
	class Board : GameComponent {
		public HexTile[,] tiles;
		public int width, height;
		public int[,] map;
		public Dictionary<HexTile, List<HexTile>> graph;
		public Board(Game game) : base(game) {
			map = new int[,] 
            {
                {1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1},
                {1,1,1,1,1,0,1},
                {1,1,0,1,1,0,1},
                {1,1,0,1,1,0,1},
                {1,1,0,1,1,1,1},
            };

			/* +---> x = upperbound1
			 * |
			 * v
			 * y = upperbound0
			 */

			width = map.GetUpperBound(1) + 1;
			height = map.GetUpperBound(0) + 1;
			Console.WriteLine("{0},{1}", width, height);
			tiles = new HexTile[height, width];

			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					tiles[y, x] = new HexTile(((Game1)this.Game), x, y, map[y, x] == 1 ? true : false, map[y, x] == 1 ? 1 : 0);
					((Game1)this.Game).Components.Add(tiles[y, x]);
				}
			}
		}
		
		public List<HexTile> GetNeighbours(HexTile tile) {
			// Pour une tile (X,Y) avec x pair, les voisins sont :

			// Pour une tile (X,Y) avec x impair, the neighbors are:

			List<HexTile> neighbours = new List<HexTile>();

			Point p = tile.pos;

			bool xParity = tile.pos.X % 2 != 0; // true : pair, false : impair

			if (xParity) { // pair
				if (p.X > 0 && p.Y > 0) neighbours.Add(tiles[p.X - 1, p.Y - 1]);
				if (p.Y > 0) neighbours.Add(tiles[p.X, p.Y - 1]);
				if (p.X < width-1 && p.Y > 0) neighbours.Add(tiles[p.X + 1, p.Y - 1]);
				if (p.X < width-1) neighbours.Add(tiles[p.X + 1, p.Y]);
				if (p.X > 0) neighbours.Add(tiles[p.X - 1, p.Y]);
				if (p.Y < height-1) neighbours.Add(tiles[p.X, p.Y + 1]);
			} else { // impair
				if (p.X > 0) neighbours.Add(tiles[p.X - 1, p.Y]);
				if (p.Y > 0) neighbours.Add(tiles[p.X, p.Y - 1]);
				if (p.X < width-1) neighbours.Add(tiles[p.X + 1, p.Y]);
				if (p.X < width-1 && p.Y < height-1) neighbours.Add(tiles[p.X + 1, p.Y + 1]);
				if (p.Y < height-1) neighbours.Add(tiles[p.X, p.Y + 1]);
				if (p.X > 0 && p.Y < height-1) neighbours.Add(tiles[p.X - 1, p.Y + 1]);
			}

			return neighbours;
		}

		public List<HexTile> GetTileList() {
			List<HexTile> tileList = new List<HexTile>();

			foreach (HexTile t in tiles) {
				tileList.Add(t);
			}

			return tileList;
		}

		public void CreateGraph() {
			graph = new Dictionary<HexTile,List<HexTile>>(width * height);
			foreach (HexTile t in this.tiles) { // tile
				if (t.walkable)
					graph.Add(t, this.GetNeighbours(t));
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
	}
}
