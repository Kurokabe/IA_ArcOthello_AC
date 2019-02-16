using IABoardAC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArcOthello_AC
{
    public class Board : IPlayable.IPlayable
    {
        #region IA Parameters
        private const int CORNER_BONUS = 50000;
        private const int WALL_MALUS = 8;
        private const int CORNER_GIVING_MALUS = 75;
        private const int EARLY_ROUNDS = 15;    // Number of rounds until the mobility is far less interesting
        private const int BLOCKING_OPPONENT = 0;
        private int roundNumber = 0;
        #endregion

        #region Properties
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        private ObservableCollection<ObservableCollection<Piece>> pieces;
        #endregion

        #region Indexer
        public ObservableCollection<ObservableCollection<Piece>> Pieces
        {
            get { return pieces; }
        }

        public Piece this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= GridHeight)
                    throw new ArgumentOutOfRangeException("row", row, "Invalid Row Index");
                if (col < 0 || col >= GridWidth)
                    throw new ArgumentOutOfRangeException("col", col, "Invalid Column Index");
                return pieces[col][row];
            }
        }
        #endregion

        #region Constructors and Initializers
        public Board()
        {
            this.GridWidth = 9;
            this.GridHeight = 7;
            this.Init();
        }

        public Board(int width, int height)
        {
            this.GridWidth = width;
            this.GridHeight = height;
            this.Init();
        }

        public Board(Board b)
        {
            this.GridWidth = b.GridWidth;
            this.GridHeight = b.GridHeight;
            Init(b.pieces);
        }
        
        /// <summary>
        /// Initializes a board's ObservableCollections (content) and default pieces.
        /// </summary>
        public void Init()
        {
            pieces = new ObservableCollection<ObservableCollection<Piece>>();
            for (int x = 0; x < GridWidth; x++)
            {
                ObservableCollection<Piece> col = new ObservableCollection<Piece>();
                for (int y = 0; y < GridHeight; y++)
                {
                    Piece p = new Piece(Team.None, x, y);
                    col.Add(p);
                }
                pieces.Add(col);
            }
            pieces[3][3].SetTeam(Team.White);
            pieces[3][4].SetTeam(Team.Black);
            pieces[4][3].SetTeam(Team.Black);
            pieces[4][4].SetTeam(Team.White);
        }

        /// <summary>
        /// Initializes a board's ObservableCollections (content) by copying given ObservableCollections.
        /// </summary>
        /// <param name="piecesToCopy">ObservableCollections to copy</param>
        public void Init(ObservableCollection<ObservableCollection<Piece>> piecesToCopy)
        {
            pieces = new ObservableCollection<ObservableCollection<Piece>>();
            for (int x = 0; x < GridWidth; x++)
            {
                ObservableCollection<Piece> col = new ObservableCollection<Piece>();
                for (int y = 0; y < GridHeight; y++)
                {
                    Piece p = new Piece(piecesToCopy[x][y]);
                    col.Add(p);
                }
                pieces.Add(col);
            }
        }
        #endregion

        #region Board control
        /// <summary>
        /// Poses the piece at given position.
        /// Throws exception if the position is invalid.
        /// </summary>
        /// <param name="row">y position on the board for the new piece</param>
        /// <param name="col">x position on the board for the new piece</param>
        /// <param name="p">piece to pose</param>
        public void SetPiece(int row, int col, Piece p)
        {
            if (row < 0 || row >= GridHeight)
                throw new ArgumentOutOfRangeException("row", row, "Invalid Row Index");
            if (col < 0 || col >= GridWidth)
                throw new ArgumentOutOfRangeException("col", col, "Invalid Column Index");
            pieces[col][row] = p;
        }

        /// <summary>
        /// Tries to play at given position and returns if the move is valid.
        /// </summary>
        /// <param name="row">y position on the board for the new piece</param>
        /// <param name="col">x position on the board for the new piece</param>
        /// <param name="team">team whose turn it is</param>
        /// <returns>True if the move is valid false otherwise</returns>
        public bool PosePiece(int row, int col, Team team)
        {
            if (pieces[col][row].Team == (team == Team.White ? 
                                          Team.WhitePreview : 
                                          Team.BlackPreview))
            {
                pieces[col][row].SetTeam(team);
                GetFlipPieceList(row, col, team).ForEach(p => p.Flip());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays given team's preview pieces on the board at valid positions.
        /// </summary>
        /// <param name="team">team whose turn it is</param>
        public void ShowPossibleMove(Team team)
        {
            Team preview = team == Team.Black ? Team.BlackPreview : Team.WhitePreview;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (GetFlipPieceList(y, x, team).Count() != 0 && pieces[x][y].Team == Team.None)
                        pieces[x][y].Team = preview;
                }
            }
        }
        
        /// <summary>
        /// Returns a list of pieces to flip for a piece of a given team posed at a given position.
        /// </summary>
        /// <param name="row">y position on the board</param>
        /// <param name="col">x position on the board</param>
        /// <param name="team">piece's team</param>
        /// <returns>list of pieces to flip</returns>
        private List<Piece> GetFlipPieceList(int row, int col, Team team)
        {
            return GetFlipPieceList(row, col, team, 1, 0)
                .Concat(GetFlipPieceList(row, col, team, -1, 0))
                .Concat(GetFlipPieceList(row, col, team, 1, 1))
                .Concat(GetFlipPieceList(row, col, team, -1, -1))
                .Concat(GetFlipPieceList(row, col, team, -1, 1))
                .Concat(GetFlipPieceList(row, col, team, 1, -1))
                .Concat(GetFlipPieceList(row, col, team, 0, 1))
                .Concat(GetFlipPieceList(row, col, team, 0, -1)).ToList();
        }

        /// <summary>
        /// Returns a list of pieces to flip for a piece of a given team posed at a given position.
        /// Checks only a given direction defined by incX / incY.
        /// </summary>
        /// <param name="row">y position on the board</param>
        /// <param name="col">x position on the board</param>
        /// <param name="team">piece's team</param>
        /// <param name="incX">x propagation direction</param>
        /// <param name="incY">y propagation direction</param>
        /// <returns>list of pieces to flip in the given direction</returns>
        private List<Piece> GetFlipPieceList(int row, int col, Team team, int incX, int incY)
        {
            List<Piece> flipPiece = new List<Piece>();

            Team enemyTeam = team == Team.Black ? Team.White : Team.Black;

            row += incY;
            col += incX;

            while (!IsSlotEmpty(row, col) && pieces[col][row].Team == enemyTeam)
            {
                flipPiece.Add(pieces[col][row]);
                row += incY;
                col += incX;
            }

            if (!IsSlotEmpty(row, col) && pieces[col][row].Team == team)
                return flipPiece;
            return new List<Piece>();
        }
        #endregion

        #region Board status
        /// <summary>
        /// Returns the number of valid moves for the given team.
        /// </summary>
        /// <param name="team">team whose turn it is</param>
        /// <returns></returns>
        public int NumberPossibleMove(Team team)
        {
            int count = 0;
            Team preview = team == Team.Black ? Team.BlackPreview : Team.WhitePreview;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    count += GetFlipPieceList(y, x, team).Count() != 0 && pieces[x][y].Team == Team.None ? 1 : 0;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns true if the slot at given position is empty.
        /// </summary>
        /// <param name="row">y position on the board</param>
        /// <param name="col">x position on the board</param>
        /// <returns>true if the slot is empty false otherwise</returns>
        public bool IsSlotEmpty(int row, int col)
        {
            return (col < 0 || col >= GridWidth ||
                    row < 0 || row >= GridHeight ||
                    pieces[col][row].Team == Team.None ||
                    pieces[col][row].Team == Team.BlackPreview ||
                    pieces[col][row].Team == Team.WhitePreview);
        }

        /// <summary>
        /// Returns true if a piece of the given team can be posed at given position.
        /// </summary>
        /// <param name="row">y position on the board</param>
        /// <param name="col">x position on the board</param>
        /// <param name="team">piece's team</param>
        /// <returns>true if the move is valid false otherwise</returns>
        public bool IsValid(int row, int col, Team team)
        {
            return col >= 0 && col < GridWidth && 
                   row >= 0 && row < GridHeight && 
                   pieces[col][row].Team == (team == Team.White ? Team.WhitePreview : Team.BlackPreview);
        }
        #endregion

        #region Preview controls
        /// <summary>
        /// Removes preview pieces from the board.
        /// </summary>
        public void ClearPreview()
        {
            foreach (ObservableCollection<Piece> row in Pieces)
            {
                foreach (Piece p in row)
                {
                    if (IsPreview(p))
                        p.Team = Team.None;
                }
            }
        }

        /// <summary>
        /// Returns true if the given piece is type of preview.
        /// </summary>
        /// <param name="p">piece to test</param>
        /// <returns>true if the given piece's type is preview false otherwise</returns>
        private bool IsPreview(Piece p)
        {
            return p.Team == Team.BlackPreview || p.Team == Team.WhitePreview;
        }
        #endregion

        #region IPlayable Implementation
        /// <summary>
        /// Returns the IA's name
        /// </summary>
        /// <returns>IA's name</returns>
        public string GetName()
        {
            return "Jack - CSS for the win";
        }

        /// <summary>
        /// Returns true if the move is valid for specified color
        /// </summary>
        /// <param name="column">value between 0 and 8</param>
        /// <param name="row">value between 0 and 6</param>
        /// <param name="isWhite"></param>
        /// <returns>true or false</returns>
        public bool IsPlayable(int column, int row, bool isWhite)
        {
            return IsValid(row, column, isWhite ? Team.White : Team.Black);
        }

        /// <summary>
        /// Will update the board status if the move is valid and return true
        /// Will return false otherwise (board is unchanged)
        /// </summary>
        /// <param name="column">value between 0 and 7</param>
        /// <param name="row">value between 0 and 7</param>
        /// <param name="isWhite">true for white move, false for black move</param>
        /// <returns></returns>
        public bool PlayMove(int column, int row, bool isWhite)
        {
            roundNumber++;
            ShowPossibleMove(isWhite ? Team.White : Team.Black);
            bool canPlay = PosePiece(row, column, isWhite ? Team.White : Team.Black);
            if (canPlay)
            {
                ClearPreview();
                ShowPossibleMove(isWhite ? Team.Black : Team.White);
            }
            return canPlay;
        }

        /// <summary>
        /// Asks the game engine next (valid) move given a game position
        /// The board assumes following standard move notation:
        /// 
        ///             A B C D E F G H I
        ///         [ ][0 1 2 3 4 5 6 7 8]     (first index)
        ///        1 0
        ///        2 1
        ///        3 2        X
        ///        4 3            X
        ///        5 4
        ///        6 5
        ///        7 6
        ///       
        ///          Column Line
        ///  E.g.:    D3, F4 game notation will map to {3,2} resp. {5,3}
        /// </summary>
        /// <param name="game">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="level">an integer value to set the level of the IA, 5 normally</param>
        /// <param name="whiteTurn">true if white players turn, false otherwise</param>
        /// <returns>The column and line indices. Will return {-1,-1} as PASS if no possible move </returns>
        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            // save board
            var savedBoard = GetBoard();
            // search for a valid move
            var node = Alphabeta(game,                  // given board
                                 5,                     // level
                                 1,                     // maximize first
                                 -int.MaxValue,         // default root score : Eval(game, whiteTurn)
                                 whiteTurn ? 0 : 1);    // current player's color
            // restore board
            RestoreBoard(savedBoard);
            // return move
            return node.Move;
        }

        /// <summary>
        /// Finds optimized move.
        /// </summary>
        /// <param name="gameRoot">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="level">an integer value to set the level of the IA, 5 normally</param>
        /// <param name="minOrMax">1 for maximize -1 for minimize</param>
        /// <param name="parentScore">parent's board fitness value</param>
        /// <param name="pieceSample">0 for white 1 for black</param>
        /// <param name="lastOp">Tuple of int : x and y for latest operations's position on the board</param>
        /// <param name="mobility">board's mobility factor</param>
        /// <returns></returns>
        private AlphabetaNode Alphabeta(int[,] gameRoot, int level, int minOrMax, int parentScore, 
                                        int pieceSample, Tuple<int, int> lastOp = null, int mobility = 0)
        {
            bool isWhite = pieceSample == 0;

            // find available operations
            var availableOps = GetOps(gameRoot, isWhite);

            if (level == 0 || IsFinal(gameRoot) || !availableOps.Any()) // handle leaves
                return new AlphabetaNode(Eval(gameRoot, 
                                              isWhite, 
                                              GetBonus(lastOp), 
                                              mobility,
                                              availableOps.Count == 0 ? 
                                                -minOrMax * BLOCKING_OPPONENT : 0));
            
            var currentNode = new AlphabetaNode(minOrMax * -int.MaxValue);
            foreach (var op in availableOps) // handle nodes
            {
                // apply the operator on the board
                int[,] newBoard = Apply(gameRoot, op, isWhite);

                // descend in the moves tree
                var branchResult = Alphabeta(newBoard, 
                                             level - 1, 
                                             -minOrMax, 
                                             currentNode.Value, 
                                             (pieceSample + 1) % 2, 
                                             op, 
                                             mobility + availableOps.Count * minOrMax);
                
                // compare branch results and current node value (current best move)
                int val = branchResult.Value;
                if (val * minOrMax > currentNode.Value * minOrMax)
                {
                    currentNode.Value = val;
                    currentNode.Move = op;
                    
                    // optimization
                    if (currentNode.Value * minOrMax > parentScore * minOrMax)
                        break;
                }
            }
            return currentNode;
        }

        //private int CalculateScore(int branchValue, int bonus, int mobility)
        //{
        //    int score = branchValue + bonus;

        //    double ratio = Math.Max(EARLY_ROUNDS - roundNumber, 1.0);

        //    score *= (int)(Math.Max(mobility, 1.0) * ratio);

        //    return score;
        //}

        /// <summary>
        /// Returns true if the board is final false otherwise
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <returns>true if the board is final false otherwise</returns>
        private bool IsFinal(int[,] board)
        {
            for (int y = 0; y < GridHeight; y++) // lines
            {
                for (int x = 0; x < GridWidth; x++) // columns
                {
                    if (board[x, y] == -1)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns board's fitness value.
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="whiteTurn">true if white players turn false otherwise</param>
        /// <param name="bonus">board's associated bonus / malus</param>
        /// <param name="mobility">board's mobility factor</param>
        /// <param name="blockedOpponentBonus">blocking opponent bonus / malus</param>
        /// <returns>board fitness value</returns>
        private int Eval(int[,] board, bool whiteTurn, int bonus = 0, int mobility = 0, int blockedOpponentBonus = 0)
        {
            // raw score
            int score = whiteTurn ? GetWhiteScore(board) : GetBlackScore(board);

            // add bonus / malus
            score += Math.Max(bonus, 0);

            // add blocked opponent bonus / malus
            score += Math.Max(blockedOpponentBonus, 0);

            // multiply by mobility factor
            //double ratio = Math.Max(EARLY_ROUNDS - roundNumber, 1.0);
            //score *= (int)(Math.Max(mobility, 1.0) * ratio);

            return score;
        }

        /// <summary>
        /// Computes and returns operation's bonuses and maluses.
        /// </summary>
        /// <param name="op">Tuple of int : x and y for operations's position on the board</param>
        /// <returns>sum of bonuse and maluses for given operation</returns>
        private int GetBonus(Tuple<int, int> op)
        {
            int bonus = 0;
            if (op != null)
            {
                if (IsInCorner(op))
                    bonus += CORNER_BONUS;
                if (IsInWall(op))
                    bonus -= WALL_MALUS;
                if (IsCornerGiving(op))
                    bonus -= CORNER_GIVING_MALUS;
            }
            return bonus;
        }

        /// <summary>
        /// Returns true if the given position is a weak spot around a board's corner.
        /// 
        ///         [ ][0 1 2 3 4 5 6 7 8] 
        ///          0    X           X
        ///          1  X X           X X
        ///          2        
        ///          3        
        ///          4
        ///          5 X X            X X
        ///          6   X            X 
        /// </summary>
        /// <param name="op">Tuple of int : x and y for operations's position on the board</param>
        /// <returns>true if the given position is a weak spot around a board's corner false otherwise</returns>
        private bool IsCornerGiving(Tuple<int, int> op)
        {
            if (op.Item1 == 0)                                          // x = 0
            {
                if (op.Item2 == 1 || op.Item2 == GridHeight - 2)        // y = 1 || 5
                    return true;
            }
            else if (op.Item1 == 1)                                     // x = 1
            {
                if (op.Item2 == 0 || op.Item2 == GridHeight - 1)        // y = 0 || 6
                    return true;
                else if (op.Item2 == 1 || op.Item2 == GridHeight - 2)   // y = 1 || 5
                    return true;
            }
            else if (op.Item1 == GridWidth - 1)                         // x = 8
            {
                if (op.Item2 == 1 || op.Item2 == GridHeight - 2)        // y = 1 || 5
                    return true;
            }
            else if (op.Item1 == GridWidth - 2)                         // x = 7
            {
                if (op.Item2 == 1 || op.Item2 == GridHeight - 2)        // y = 1 || 5
                    return true;
                else if (op.Item2 == 0 || op.Item2 == GridHeight - 1)   // y = 0 || 6
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given position is a board's wall.
        /// 
        ///         [ ][0 1 2 3 4 5 6 7 8] 
        ///          0  X X X X X X X X X 
        ///          1  X               X
        ///          2  X               X 
        ///          3  X               X        
        ///          4  X               X 
        ///          5  X               X 
        ///          6  X X X X X X X X X 
        /// </summary>
        /// <param name="op">Tuple of int : x and y for operations's position on the board</param>
        /// <returns>true if the given position is a board's wall false otherwise</returns>
        private bool IsInWall(Tuple<int, int> op)
        {
            if (!IsInCorner(op))
            {
                if (op.Item1 == 0 || op.Item1 == GridWidth - 1)     // x = 0 || 8
                    return true;
                if (op.Item2 == 0 || op.Item2 == GridHeight - 1)    // y = 0 || 6
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given position is a board's corner.
        /// 
        ///         [ ][0 1 2 3 4 5 6 7 8] 
        ///          0  X               X 
        ///          1  
        ///          2  
        ///          3  
        ///          4  
        ///          5  
        ///          6  X               X 
        /// </summary>
        /// <param name="op">Tuple of int : x and y for operations's position on the board</param>
        /// <returns>true if the given position is a board's corner false otherwise</returns>
        private bool IsInCorner(Tuple<int, int> op)
        {
            if (op.Item1 == 0)                                      // x = 0
            {
                if (op.Item2 == 0 || op.Item2 == GridHeight - 1)    // y = 0 || 6
                    return true;
            }
            else if (op.Item1 == GridWidth - 1)                     // x = 8
            {
                if (op.Item2 == 0 || op.Item2 == GridHeight - 1)    // y = 0 || 6
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns applicable operators for the given board.
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="whiteTurn">true if white players turn false otherwise</param>
        /// <returns>List of applicable operators</returns>
        private List<Tuple<int, int>> GetOps(int[,] board, bool whiteTurn)
        {
            Team currentTeam = GetTeam(whiteTurn);
            Team currentTeamPreview = currentTeam == Team.Black ? Team.BlackPreview : Team.WhitePreview;
            
            // convert board (int) to "our" board
            RestoreBoard(board);

            ShowPossibleMove(currentTeam);

            // search for available operations (current team's preview pieces on the board)
            List<Tuple<int, int>> ops = new List<Tuple<int, int>>();
            for (int y = 0; y < GridHeight; y++) // lines
            {
                for (int x = 0; x < GridWidth; x++) // columns
                {
                    if (pieces[x][y].Team == currentTeamPreview)
                        ops.Add(new Tuple<int, int>(x, y));
                }
            }
            return ops;
        }

        /// <summary>
        /// Change the values of our board (the observable collection of pieces) so that it matches the board in argument
        /// </summary>
        /// <param name="board">Two-dimensional array containing 0 for white, 1 for black and -1 for empty</param>
        private void RestoreBoard(int[,] board)
        {
            for (int y = 0; y < GridHeight; y++) // lines
            {
                for (int x = 0; x < GridWidth; x++) // columns
                {
                    if (board[x, y] == 0)
                        pieces[x][y].Team = Team.White;
                    else if (board[x, y] == 1)
                        pieces[x][y].Team = Team.Black;
                    else
                        pieces[x][y].Team = Team.None;
                }
            }
        }

        /// <summary>
        /// Return the team based on whether it is the white or black turn
        /// </summary>
        /// <param name="whiteTurn">True for white, false for black</param>
        /// <returns>Team.White if white turn, Team.Black otherwise</returns>
        private Team GetTeam(bool whiteTurn)
        {
            return whiteTurn ? Team.White : Team.Black;
        }

        /// <summary>
        /// Applies given move on the given board.
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <param name="op">Tuple of int : x and y for operations's position on the board</param>
        /// <param name="whiteTurn">true if white players turn false otherwise</param>
        /// <returns>modified 2D board with integer values</returns>
        private int[,] Apply(int[,] board, Tuple<int, int> op, bool whiteTurn)
        {
            // convert board (int) to "our" board
            RestoreBoard(board);
            // PosePiece() with op as position and whiteTurn for team
            PosePiece(op.Item2, op.Item1, GetTeam(whiteTurn));
            // convert back the board to int array
            return GetBoard();
        }

        /// <summary>
        /// Returns a reference to a 2D array with the board status
        /// </summary>
        /// <returns>The 7x9 tiles status</returns>
        public int[,] GetBoard()
        {
            int[,] boardInt = new int[GridWidth, GridHeight];
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    boardInt[x, y] = this[y, x].Team == Team.Black ? 1 :
                                     this[y, x].Team == Team.White ? 0 : -1;
                }
            }
            return boardInt;
        }

        /// <summary>
        /// Returns the number of white tiles on the board
        /// </summary>
        /// <returns>white player's score</returns>
        public int GetWhiteScore()
        {
            int whiteScore = 0;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    whiteScore += this[y, x].Team == Team.White ? 1 : 0;
                }
            }
            return whiteScore;
        }

        /// <summary>
        /// Returns the number of white tiles on the given board
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <returns>white player's score</returns>
        public int GetWhiteScore(int[,] board)
        {
            int whiteScore = 0;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    whiteScore += board[x, y] == 0 ? 1 : 0;
                }
            }
            return whiteScore;
        }

        /// <summary>
        /// Returns the number of black tiles on the board.
        /// </summary>
        /// <returns>black player's score</returns>
        public int GetBlackScore()
        {
            int blackScore = 0;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    blackScore += this[y, x].Team == Team.Black ? 1 : 0;
                }
            }
            return blackScore;
        }

        /// <summary>
        /// Returns the number of black tiles on the given board.
        /// </summary>
        /// <param name="board">a 2D board with integer values: 0 for white 1 for black and -1 for empty tiles. First index for the column, second index for the line</param>
        /// <returns>black player's score</returns>
        public int GetBlackScore(int[,] board)
        {
            int blackScore = 0;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    blackScore += board[x, y] == 1 ? 1 : 0;
                }
            }
            return blackScore;
        }
        #endregion
    }
}
