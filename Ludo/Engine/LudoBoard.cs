using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ludo;
using Ludo.Engine;

namespace LudoRules
{
    /// <summary>
    /// We want to make sure that board contains methods and attributes
    /// to keep track of the actual state of of what's taking place
    /// and also services to change its state
    /// </summary>
    public class LudoBoard
    {       
        #region Fields
        private bool isActive = false;

        public int numOfActivePlayers;
        private const int numOfPlayers = 4;
        private const int numOfPiecesPerPlayer = 4;
        private const int numOfSquaresPerSide = 10;
        private const int numOfExitSquaresPerSide = 4;
        private const int numOfMaximumStepsPerLap = numOfSquaresPerSide * numOfPlayers;
        private const int numOfStepsToExit = numOfMaximumStepsPerLap + numOfExitSquaresPerSide;

        private LudoBoardTable ludoBoardTable;

        private Player[] players;
        private Piece[][] pieces;
        private Nest[] nests;
        private Square[][] squares;
        private Square[][] exitSquares;
        private Instructions instruction;
        #endregion



        #region Constructor
        /// <summary>
        /// By default, assume players are 4
        /// </summary>
        public LudoBoard() : this(4) { } 
        public LudoBoard(int numOfPlayers)
        {
            this.numOfActivePlayers = numOfPlayers;
            generateBoard();
            initializeDatabase();

            Debug.Write("\nBoard constructed, database initialized");
        }
        #endregion



        #region Properties
        public bool Active
        {
            get { return isActive;  }
            set { isActive = value;  }
        }

        /// <summary>
        /// The rule engine and the test engine will make good use of this information contained here
        /// </summary>
        public Dictionary<string, object> State
        {
            get
            {
                Dictionary<string, object> gameState = new Dictionary<string, object>();

                gameState.Add("board", this.ToString());
                gameState.Add("players", players);
                gameState.Add("pieces", pieces);
                gameState.Add("nests", nests);
                gameState.Add("squares", squares);
                gameState.Add("exitsquares", exitSquares);
                return gameState;
            }
        }
        public Instructions Instruction
        {
            get { return instruction;  }
            set { instruction = value; }
        }
        #endregion



        #region Methods
        private void generateBoard()
        {
            generatePlayersAndPieces();
            generateNests();
            generateSquares();
            Active = true;
        }
        private void generateNests()
        {
            nests = new Nest[numOfPlayers];

            for (int i = 0; i < numOfPlayers; i++)
            {
                nests[i] = new Nest((Colors)i, pieces[i]);
            }
        }
        private void generatePlayersAndPieces()
        {
            players = new Player[numOfPlayers];
            pieces = new Piece[numOfPlayers][];

            for (int i = 0; i < numOfPlayers; i++)
            {
                players[i] = new Player((Colors)i);
                generatePieces((Colors)i);
            }
        }
        private void generatePieces(Colors color)
        {
            pieces[(int)color] = new Piece[numOfPiecesPerPlayer];
            for (int i = 0; i < numOfPiecesPerPlayer; i++)
            {
                pieces[(int)color][i] = new Piece(color, i);
            }
        }
        private void generateSquares()
        {
            squares = new Square[numOfPlayers][];
            exitSquares = new ExitSquare[numOfPlayers][];

            for (int side = 0; side < numOfPlayers; side++)
            {
                squares[side] = new Square[numOfSquaresPerSide];
                for (int position = 0; position < numOfSquaresPerSide; position++)
                {
                    int squarePosition = position + (side * numOfSquaresPerSide);
                    squares[side][position] = new Square((Colors)side, squarePosition);
                }

                exitSquares[side] = new ExitSquare[numOfExitSquaresPerSide];
                for (int position = 0; position < numOfExitSquaresPerSide; position++)
                {
                    exitSquares[side][position] = new ExitSquare((Colors)side, position);
                }
            }
        }

        public void deActivatePiece(Piece piece)
        {
            piece.Active = false;
            piece.Position = -1;
        }
        private Piece popPieceFromNest(Colors color, int index)
        {
            return nests[(int)color].popPiece(index);
        }
        public void tryActivatePiece(Colors color, out bool isPieceActivated, int index)
        {
            Piece piece = popPieceFromNest(color, index);
            isPieceActivated = tryIntroducePiece(piece);
            piece.Active = isPieceActivated;

            if (isPieceActivated)  
            {
                players[(int)color].Active = true;
                piece.Position = 0 + ((int)color * numOfSquaresPerSide);
            }
        }
        /// <summary>
        /// By moving through the goal the piece gets killed
        /// </summary>
        /// <param name="piece"></param>
        private bool killPiece(Piece piece)
        {
            Square currentPosition = locateSquare(piece.Steps, piece.Position, piece.Color);
            currentPosition.Occupant = null;
            piece.Alive = false;
            piece.Active = false;
            piece.Position = 44; 
            piece.Steps = 44;
            return true;
        }
        private bool areAllPiecesDead(Colors color)
        {
            foreach (var piece in pieces[(int)color])
            { 
                if (piece.Alive) { return false;  }
            }
            return true;
        }
        private bool checkPlayerDeActivation(Colors color)
        {
            foreach (var piece in pieces[(int)color])
            {
                if (piece.Alive)
                {
                    return false;
                }
            }
            players[(int)color].Active = false;
            Instruction = Instructions.Victory;
            return true;
        }

        /// <summary>
        /// Here we need to consider 3 scenarios:
        /// - If we are about to roll the dice and have a chance at moving to the exit squares 
        /// - chance at moving through the goal
        /// - "normal" behaviour
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="steps"></param>
        /// <param name="isMoveSuccesful"></param>
        public void tryMovePiece(Piece piece, int steps, ref bool isMoveSuccesful)
        {
            int requestedSteps = piece.Steps + steps;

            // 3 scenarios //
            if (requestedSteps == numOfStepsToExit)
            {
                Debug.Write("\nBoard: Exiting");
                isMoveSuccesful = killPiece(piece);
                Instruction = Instructions.Exit;
                checkPlayerDeActivation(piece.Color);
            }
            // Almost quitting (stepping back)
            else if (requestedSteps > numOfStepsToExit) 
            {
                // We will have to step back x steps from the goal square (overflow)
                int newSteps = piece.Steps;
                int stepsToWalk = 0;

                Square currentPosition = locateSquare(piece.Steps, piece.Position, piece.Color);
                Debug.WriteLine("Missing target, calculating new Position");
                int newPosition  = calculateNewPositionAfterMissedTarget(piece.Steps, requestedSteps, 
                                                    ref stepsToWalk, ref newSteps, piece.Color);

                Square requestedPosition = locateSquare(newSteps, newPosition, piece.Color);
                Debug.WriteLine(requestedPosition.ToString());
                isMoveSuccesful = tryMove(currentPosition, requestedPosition, piece, stepsToWalk);
            }
            else
            {
                Square currentPosition = locateSquare(piece.Steps, piece.Position, piece.Color);
                Square requestedPosition = locateSquare(requestedSteps, piece.Position + steps, piece.Color);

                isMoveSuccesful = tryMove(currentPosition, requestedPosition, piece, steps);
            }
        }


        /// <summary>
        /// This is a collision detector. 
        /// You need to get an exact match with the dice in order to exit
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="requestedSteps"></param>
        /// <returns></returns>
        private int calculateNewPositionAfterMissedTarget(int currentPosition, int requestedSteps,
                                                        ref int stepsToWalk, ref int newSteps, Colors color)
        {
            int overflow = requestedSteps - numOfStepsToExit;
            newSteps = numOfStepsToExit - overflow;
            stepsToWalk = newSteps - currentPosition;
            int newRelativePosition = currentPosition + stepsToWalk;
            if (newRelativePosition < numOfMaximumStepsPerLap)
            {
                newRelativePosition -= ((int) color * numOfSquaresPerSide);
            }
            return newRelativePosition;
        }

        /// <summary>
        /// 2 scenarios; two types of squares
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private Square locateSquare(int steps, int position, Colors color)
        {
            if (steps >= numOfMaximumStepsPerLap) // look in exitSquares
            {
                int side = (int)color;
                int squareID = steps - (numOfPlayers * numOfSquaresPerSide); 
                return exitSquares[side][squareID]; 
            }
            else // look in normal squares
            {
                int side = position / numOfSquaresPerSide;
                int squareID = position - (side * numOfSquaresPerSide);
                if (side == 4) { side = 0; } // wrap around array
                return squares[side][squareID];
            }    
        }

        /// <summary>
        /// To introduce: move a player from nest to square
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        private bool tryIntroducePiece(Piece piece)
        {
            Square firstSquare = squares[(int)piece.Color][0];
            bool isIntroduced = tryMove(null, firstSquare, piece, 0);
            return isIntroduced;
        }

        /// <summary>
        /// We would like to achieve the following: 
        /// - replacing our last position with null
        /// - knocking pieces out or passing depending on collision with ourselves or others
        /// </summary>
        /// <param name="fromSquare"></param>
        /// <param name="toSquare"></param>
        /// <param name="piece"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        private bool tryMove(Square fromSquare, Square toSquare, Piece piece, int steps)
        {
            if (toSquare.Occupant == null)
            {
                Instruction = Instructions.Move;
                Debug.Write("\nBoard: Moving piece");
                Debug.Write("\nWalking: " + steps + " steps");
                move(piece, toSquare, fromSquare, steps);
                return true;
            }
            else
            {
                Piece occupyingPiece = toSquare.Occupant;
                Debug.Write("\nBoard: Square  is occupied");
                if (occupyingPiece.Color == piece.Color)
                {
                    Instruction = Instructions.CollisionWithSelf;
                    // Two pieces of the same color cannot occupy the same space (pass)
                    return false;
                }
                else
                {
                    // We will now deActivate the colliding piece, since it's not our own
                    // ... and insert our own piece
                    Instruction = Instructions.MoveAndKnockout;
                    Debug.Write("\nBoard: Knocking out piece ");
                    move(piece, toSquare, fromSquare, steps);
                    deActivatePiece(occupyingPiece);
                    if (nests[(int)occupyingPiece.Color].Count == 4)
                    { players[(int)occupyingPiece.Color].Active = false; }

                    return true;
                }
            }
        }
        private void move(Piece piece, Square toSquare, Square fromSquare, int steps)
        {
            if (fromSquare != null) { fromSquare.Occupant = null; }
            toSquare.Occupant = piece;
            piece.Steps += steps;
            piece.Position += steps;
        }

        public override string ToString()
        {
            return (Active) ? "Board is active" : "Board is not active";
        }         
        #endregion



        #region databaseMethods
        public void initializeDatabase()
        {
            using (var db = new LudoBoardContext())
            {
                Debug.WriteLine("\nPreparing DB");

                ludoBoardTable = new LudoBoardTable();

                List<PlayerTable> playerTables = new List<PlayerTable>();
                List<PieceTable> pieceTables = new List<PieceTable>();

                for (int color = 0; color < numOfPlayers; color++)
                {
                    for (int i = 0; i < numOfPiecesPerPlayer; i++)
                    {
                        var pieceTable = new PieceTable() { Color = color };
                        pieceTable.ID = i;
                        pieceTables.Add(pieceTable);
                    }


                    var playerTable = new PlayerTable() { Color = color };
                    playerTables.Add(playerTable);

                    #region NotUsed
                    //Debug.WriteLine("Creating pieces nestTable");
                    //var nestTable = new NestTable() { Color = color };
                    //for (int i = 0; i < numOfPiecesPerPlayer; i++)
                    //{
                    //    nestTable.Pieces.Add(i);
                    //    Debug.Write(i);
                    //}
                    //nestTables.Add(nestTable);
                    //for (int id = 0; id < numOfSquaresPerSide; id++)
                    //{
                    //    var squareTable = new SquareTable() { Color = color };
                    //    squareTable.ID = id;
                    //    squareTables.Add(squareTable);
                    //}
                    //for (int id = 0; id < numOfExitSquaresPerSide; id++)
                    //{
                    //    var exitSquareTable = new ExitSquareTable() { Color = color };
                    //    exitSquareTable.ID = id;
                    //    exitSquareTables.Add(exitSquareTable);
                    //}
                    #endregion

                }
                //
                ludoBoardTable.Pieces = pieceTables;
                ludoBoardTable.Players = playerTables;
            }
        }

        public void loadSavedBoard(out int playerTurn)
        {
            // set all fields to null first,
            // when loading a game and old values persist
            #region Nullify
            for (int color = 0; color < numOfPlayers; color++)
            {
                for (int i = 0; i < numOfSquaresPerSide; i++)
                {
                    squares[color][i].Occupant = null;
                }
                for (int i = 0; i < numOfExitSquaresPerSide; i++)
                {
                    exitSquares[color][i].Occupant = null;
                }
            }
            #endregion

            using (var db = new LudoBoardContext())
            {
                Debug.WriteLine("\nLoading DB");

                // reverse so we get the latest saved board
                var board = (from l in db.LudoBoardTables
                            orderby l.key descending 
                            select l).First();

                    playerTurn = board.PlayerTurn;

                    #region Pieces
                    foreach (var piece in board.Pieces)
                    {
                        int pieceID = piece.ID;
                        int pieceColor = piece.Color;
                        int piecePosition = piece.Position;

                        pieces[pieceColor][pieceID].Active = piece.IsActive;
                        pieces[pieceColor][pieceID].Alive = piece.IsAlive;
                        pieces[pieceColor][pieceID].Position = piece.Position;
                        pieces[pieceColor][pieceID].Steps = piece.Steps;


                        if (piecePosition > -1 && piecePosition <= 9)
                        {
                            squares[(int)Colors.Blue][piecePosition].Occupant = pieces[pieceColor][pieceID];
                        }
                        else if (piecePosition > 9 && piecePosition <= 19)
                        {
                            squares[(int)Colors.Green][piecePosition%10].Occupant = pieces[pieceColor][pieceID];
                        }
                        else if (piecePosition > 20 && piecePosition <= 29 )
                        {
                            squares[(int)Colors.Red][piecePosition%20].Occupant = pieces[pieceColor][pieceID];
                        }
                        else if (piecePosition > 30 && piecePosition < numOfMaximumStepsPerLap)
                        {
                            squares[(int)Colors.Yellow][piecePosition%30].Occupant = pieces[pieceColor][pieceID];
                        }
                        else if (piecePosition >= numOfMaximumStepsPerLap)
                        {
                            exitSquares[pieceColor][piecePosition % numOfMaximumStepsPerLap].Occupant = 
                                pieces[pieceColor][pieceID];
                        }
                    }
                    #endregion
                    #region Players
                    foreach (var player in board.Players)
                    {
                        int playerColor = player.Color;
                        players[player.Color].Active = player.IsActive;
                    }
                    #endregion
            }
        }

        public void saveBoard(int playerTurn)
        {
            using (var db = new LudoBoardContext())
            {
                Debug.WriteLine("\nSaving DB");

                // Board status
                ludoBoardTable.IsActive = this.Active;

                for (int color = 0; color < numOfPlayers; color++)
                {
                    // Players status
                    ludoBoardTable.Players[color].IsActive = players[color].Active;
                    ludoBoardTable.PlayerTurn = playerTurn;

                    for (int i = 0; i < numOfPiecesPerPlayer; i++)
                    {
                        int index = (color * numOfPlayers) + i;
                        ludoBoardTable.Pieces[index].IsAlive = pieces[color][i].Alive;
                        ludoBoardTable.Pieces[index].IsActive = pieces[color][i].Active;
                        ludoBoardTable.Pieces[index].Position = pieces[color][i].Position;
                        ludoBoardTable.Pieces[index].Steps = pieces[color][i].Steps;
                    }

                }
                //
                db.LudoBoardTables.Add(ludoBoardTable);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
