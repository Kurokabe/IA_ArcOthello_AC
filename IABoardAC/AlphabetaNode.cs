using System;
using System.Collections.Generic;
using System.Text;

namespace IABoardAC
{
    class AlphabetaNode
    {
        #region Properties
        public double Value { get; set; }
        public Tuple<int, int> Move { get; set; }
        #endregion

        #region Constructors
        public AlphabetaNode()
        {
            Move = new Tuple<int, int>(-1, -1);
        }

        public AlphabetaNode(double value) : this()
        {
            Value = value;
        }

        public AlphabetaNode(double value, Tuple<int, int> move)
        {
            Value = value;
            Move = move;
        }
        #endregion
    }
}
