using AStar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarGame
{
	public class PathFinder : AStarSolver
	{

		public Dictionary<HexNode, List<HexNode>> nodeGraph { get; protected set; }
		private PathFinder(HexNode initial, PathExplorer ash)
			: base(initial, ash)
		{
			m_ash = ash;
			initial.bestCost = 0;

		}
		public PathFinder(Board board, HexNode initial, HexNode final)
			: this(initial, new PathExplorer(final))
		{
			nodeGraph = new Dictionary<HexNode, List<HexNode>>();
			foreach (HexTile t in board.tileMap)
			{
				List<HexNode> neighbours = new List<HexNode>();
			}
		}
		/*
		public bool AddToNodeGraph(HexTile tile)
		{
			bool success;
			List<HexNode> nodeNeighbours;
			success = nodeGraph.TryGetValue(tile, out nodeNeighbours);
			if (!success)
			{
				nodeGraph.Add(tile, tile.ToHexNode());
			}
			return success;
		}

		public HexNode GetTileNode(HexTile tile)
		{
			HexNode node;
			if (nodeGraph.TryGetValue(tile, out node) == false)
			{
				nodeGraph.Add(tile, tile.ToHexNode());
			}
			return node;
		}*/


	}

	public class HexNode : ASState
	{
		public override ASState ParentState
		{
			get { return m_parentState; }
			set
			{

				m_parentState = value;
				if (value != null)
					RefreshCosts();
			}

		}

		internal HexTile m_nodeTile;
		internal int bestCost = int.MaxValue;

		public int TotalCost { get; protected set; }

		public HexNode(HexTile tile):base(null)
		{
			m_nodeTile = tile;			
		}

		public int RefreshCosts()
		{
			return bestCost= ( ParentState != null ?
				((HexNode)ParentState).RefreshCosts() + m_nodeTile.FinalCost : m_nodeTile.FinalCost );
		}
		public override bool Equals(ASState otherState)
		{
			HexNode otherNode = otherState as HexNode;
			return this.m_nodeTile == otherNode.m_nodeTile || m_nodeTile.GridPosition.Equals(otherNode.m_nodeTile.GridPosition);
		}

		public override List<ASState> NeighbourStates()
		{

			List<ASState> neighbours = new List<ASState>();
			foreach (HexTile n in m_nodeTile.GetNeighbours() )
			{
				neighbours.Add(n.ToHexNode());
			}

			return neighbours;

		}
	}


	public class PathExplorer : AStarHeuristic
	{
		
		private HexNode m_wantedNode;

		public PathExplorer(HexNode goal)
		{
			m_wantedNode=goal;	
		}

		public override int EvaluateState(ASState candidateState)
		{
			HexNode candidateNode = candidateState as HexNode;
			return candidateNode.m_nodeTile.FinalCost;
			//return candidateState.m_nodeTile.FinalCost;
		}

		public override ASState EvaluateBest(List<ASState> candidateStates)
		{
			HexNode bestCandidate = (HexNode)candidateStates.ElementAt(0);
			int bestScore = EvaluateState(bestCandidate);

			foreach (HexNode candidate in candidateStates)
			{
				int current = EvaluateState(candidate);
				if (candidate != null && current < bestScore)
				{
					bestScore = current;
					bestCandidate = candidate;
				}
			}

			return bestCandidate;
		}

		public override int EvaluateFscore(ASState candidateState)
		{
			HexNode candidateNode = candidateState as HexNode;
			return candidateNode.TotalCost;
		}

		public override bool CheckStateForSolution(ASState candidateState)
		{
			return candidateState.Equals(m_wantedNode);
		}
	}

}
