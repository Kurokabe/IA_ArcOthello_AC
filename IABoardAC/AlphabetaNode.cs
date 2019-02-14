using System;
using System.Collections.Generic;
using System.Text;

namespace IABoardAC
{
    class AlphabetaNode
    {
        #region Properties
        public int Value { get; set; }
        public Tuple<int, int> Move { get; set; }
        public int Mobility { get; set; }
        #endregion

        #region Constructors
        public AlphabetaNode()
        {
            Move = new Tuple<int, int>(-1, -1);
            Mobility = 1;
        }

        public AlphabetaNode(int value) : this()
        {
            Value = value;
        }

        public AlphabetaNode(int value, Tuple<int, int> move)
        {
            Value = value;
            Move = move;
        }
        #endregion
    }
}
