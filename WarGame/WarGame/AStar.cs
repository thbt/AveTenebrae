using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AStar
{

    public abstract class AStarHeuristic
    {
		public abstract int EvaluateState(ASState candidateState);
        public abstract ASState EvaluateBest(List<ASState> candidateStates);
		public abstract int EvaluateFscore(ASState candidateState);
        public abstract bool CheckStateForSolution(ASState candidateState);

    }

    public abstract class ASState
    {
        
        //public int Value { get; protected set; }
		protected int m_depth=0;
		public int Depth { get { return m_depth; } protected set { m_depth = value; } }
		protected ASState m_parentState;
		public virtual ASState ParentState
		{
			get { return m_parentState; } 
			set { 
			m_parentState = value;
			if ( value != null )
				Depth = m_parentState.Depth + 1;
			}
		
		}
   
        public ASState(ASState parentState)
        {
            ParentState = parentState;
			if (parentState != null)
				Depth = CalcDepth(0, 1024);
			else
				Depth = 0;
            //throw new NotImplementedException();
        }

        public virtual List<ASState> NeighbourStates() {
            throw new NotImplementedException();
        }

        public abstract bool Equals(ASState otherState);

		public int CalcDepth(int level, int limit) {
			if (ParentState != null) return ParentState.CalcDepth(++level, limit);
            else return level;

        }
 
    }


	/**--------------------------------------------------------------------
	 * Classe de l'algo central
	 * 
	 **/


    public class AStarSolver
    {
        public enum SearchType { ST_1STFOUND };
		public int DepthLimit=42;

		protected SearchType stopCondition = SearchType.ST_1STFOUND;
		protected AStarHeuristic m_ash;
        protected ASState m_initialState;
        //private ASState m_wantedState;

		protected List<ASState> m_OpenStates, m_ClosedStates;


        public AStarSolver(ASState initial, AStarHeuristic ash)
        {
            m_ash = ash;
            m_initialState = initial;

            m_OpenStates = new List<ASState>();
            m_ClosedStates = new List<ASState>();

            m_OpenStates.Add(initial);
        }

        public ASState SearchSolution()
        {
            bool solutionFound = false;           

            while ( /*!solutionFound &&*/ m_OpenStates.Count > 0 )
            {


                ASState m = PopBestFromOpen();
				Console.WriteLine(m);
				if (m_ash.CheckStateForSolution(m))
				{
					solutionFound = true;
					return m;
				}
				else if (m.Depth > DepthLimit)
				{
					return null;
				}

				m_ClosedStates.Add(m);				

                foreach (ASState currentNeighbour in m.NeighbourStates() )
                {

                    ASState koFound = null, kcFound = null;

                    koFound = FindOccurenceInList(currentNeighbour,m_OpenStates, false);
					//if (koFound != null)
                    {
						kcFound = FindOccurenceInList(currentNeighbour, m_ClosedStates, true);
                         if (kcFound != null)
                        {
                            //continue;
                        }
                        else
                        {
                            if (koFound == null)
                            {
                                currentNeighbour.ParentState = m;
                                m_OpenStates.Add(currentNeighbour);

                                
                            }
                            else
                            {
								koFound.ParentState = m;
								m_OpenStates.Add(koFound);   
                            }


                        }
                    }
	
                }
            }
			Console.WriteLine("Solution not found");
            return null;
        }

		//cherche les occurences et s'occupe des ajouts/suppression dans la liste des "ouverts"
        public ASState FindOccurenceInList(ASState instance, List<ASState> list, bool onlyBetter)
        {
            //petit (d)
			bool found = false;
			List<ASState> removables = new List<ASState>();
            ASState k = null;

			for (int i = 0; /*!found &&*/list.Count > 0 && i < list.Count; i++)
            {
				k = list.ElementAt<ASState>(i);

                if (instance.Equals(k))                   
                {
					if (!onlyBetter || m_ash.EvaluateFscore(k) < m_ash.EvaluateFscore(instance))
					{
						return k;
					}
                    else
                    {
						return instance;
                    }

                }
					
            }
            return null;
        }

        private ASState PopBestFromOpen()
        {
            ASState best=m_ash.EvaluateBest(m_OpenStates);

            if (best != null)
            {
				//Console.WriteLine("No best state found");
                m_OpenStates.Remove(best);
            }
            else
            {
                return null;
            }
            return best;
        }

    }

}