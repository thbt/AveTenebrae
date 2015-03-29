﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WarGame;


namespace WarGame {
	class Board : ATComponent {

		public HexTile[,] tileMap {get; protected set;}
		public readonly int width, height;
		//public int[,] map;
		public Dictionary<HexTile, List<HexTile>> tileGraph;
		public Board(ATGame game) : base(game) {

			int n = int.MaxValue;
			int[,] tmpMap = new int[,] 
            {
                {1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1},
                {1,1,1,1,1,n,1},
                {1,1,n,1,1,n,1},
                {1,1,n,1,1,n,1},
                {1,1,n,1,1,1,1},
            };

			/* +---> x = upperbound1
			 * |
			 * v
			 * y = upperbound0
			 */

			width = tmpMap.GetUpperBound(1) + 1;
			height = tmpMap.GetUpperBound(0) + 1;
			Console.WriteLine("{0},{1}", width, height);
			tileMap = new HexTile[height, width];

			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					//à reviser?
					tileMap[y, x] = new HexTile(atGame, x, y,tmpMap[y, x]);
					atGame.Components.Add(tileMap[y, x]);
				}
			}
		}
		
		public List<HexTile> GetNeighbours(HexTile tile) {


			

			List<HexTile> neighbours = new List<HexTile>();

			Point p = tile.Position;

			bool xParity = tile.Position.X % 2 != 0; // true : pair, false : impair
			// Pour une tile (X,Y) avec x pair, les voisins sont :
			if (xParity) { // pair
				if (p.X > 0 && p.Y > 0) neighbours.Add(tileMap[p.X - 1, p.Y - 1]);
				if (p.Y > 0) neighbours.Add(tileMap[p.X, p.Y - 1]);
				if (p.X < width-1 && p.Y > 0) neighbours.Add(tileMap[p.X + 1, p.Y - 1]);
				if (p.X < width-1) neighbours.Add(tileMap[p.X + 1, p.Y]);
				if (p.X > 0) neighbours.Add(tileMap[p.X - 1, p.Y]);
				if (p.Y < height-1) neighbours.Add(tileMap[p.X, p.Y + 1]);
			}// Pour une tile (X,Y) avec x impair, the neighbors are:
			else { // impair
				if (p.X > 0) neighbours.Add(tileMap[p.X - 1, p.Y]);
				if (p.Y > 0) neighbours.Add(tileMap[p.X, p.Y - 1]);
				if (p.X < width-1) neighbours.Add(tileMap[p.X + 1, p.Y]);
				if (p.X < width-1 && p.Y < height-1) neighbours.Add(tileMap[p.X + 1, p.Y + 1]);
				if (p.Y < height-1) neighbours.Add(tileMap[p.X, p.Y + 1]);
				if (p.X > 0 && p.Y < height-1) neighbours.Add(tileMap[p.X - 1, p.Y + 1]);
			}

			return neighbours;
		}

		public List<HexTile> GetTileList() {
			List<HexTile> tileList = new List<HexTile>();

			foreach (HexTile t in tileMap) {
				tileList.Add(t);
			}

			return tileList;
		}

		public void CreateGraph() {
			tileGraph = new Dictionary<HexTile,List<HexTile>>(width * height);
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
	}
}
