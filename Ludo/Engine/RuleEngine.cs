using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dice = System.Int32;
using piece = System.Int32;

namespace LudoRules
{
    /// Ludo.LudoRules
    /// <author> Per Jonsson, Hannah Börjesson </author>
    /// Innovativ Programmering, Linköpings Universitet
    /// TDDD49: Lab 2
    /// <summary>
    /// Laborationsbeskrivning:
    /// "Bygg en regelmotor som verifierar olika drag och håller ordning på spelets tillstånd.
    /// Regelmotorn ska vara isolerad mot resten av systemet.
    /// Du ska enhetstesta din regelmotor med enhetstestprojekt i Visual Studio."
    /// </summary>
    public class RuleEngine
    {
        #region Fields
        private int numOfPlayers;
        private int numOfPiecesPerPlayer;
        private LudoBoard ludoBoard;
        private bool isActive;
        #endregion



        #region Constructor
        public RuleEngine()
        {
            ActiveGame = true;
            setupBoard();
            this.numOfPlayers = ludoBoard.numOfActivePlayers;
            this.numOfPiecesPerPlayer = ludoBoard.numOfActivePlayers;
        }
        #endregion


        
        #region Properties

        public Dictionary<string, object> State
        {
            get { return ludoBoard.State; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ActiveGame 
        {
            get
            { 
                return this.isActive; 
            }
            set
            {
                this.isActive = value;

                string status = "\nRules: Game is: ";
                status += (this.isActive) ? "active" : "not active";
                Debug.Write(status);
            }
        }
        #endregion



        #region Methods
        public GameState loadGame()
        {
            ludoBoard.loadSavedBoard();
            GameState gameState = new GameState();
            Piece[][] pieces = (Piece[][]) ludoBoard.State["pieces"];
            return updateGameState(gameState, pieces);
        }
        public void saveGame()
        {
            // TODO: spara vems tur det är
            ludoBoard.saveBoard();
        }
        private void setupBoard()
        {
            ludoBoard = new LudoBoard();
        }

        /// <summary>
        /// This is the method that UI will call in order to get a status update
        /// </summary>
        /// <param name="gameEvent"></param>
        /// <returns></returns>
        public GameState parseEvent(GameEvent gameEvent)
        {
            Colors playerID = gameEvent.Player; 
            piece chosenPieceID = gameEvent.Piece;
            Piece[][] pieces = (Piece[][]) ludoBoard.State["pieces"];
            Piece chosenPiece = pieces[(int)playerID][chosenPieceID];
            dice dice = gameEvent.Dice;
            Player[] players = (Player[]) ludoBoard.State["players"];
            Player player = players[(int)playerID];
            GameState gameState = new GameState();
            bool isPieceActivated = false;

            Nest[] nests = (Nest[]) ludoBoard.State["nests"];

            Debug.Write("\nRules: Deciding action for player: " + player.PlayerID + ", with piece: " + 
                        chosenPiece.PieceID + ", player rolled: " + dice);

            if (!chosenPiece.Active) 
            { 
                isPieceActivated = tryActivate(playerID, dice, chosenPieceID);
                Debug.Write(String.Format("\nRules: Tried activating new piece: {0}", isPieceActivated));
                if (ludoBoard.Instruction != Instructions.CollisionWithSelf) 
                {
                    ludoBoard.Instruction = (isPieceActivated) ? Instructions.Introduce : Instructions.NotIntroduce;
                }
            }
            else
            {
                Debug.Write("\nRules: Trying to move piece.");
                bool hasPieceMoved = tryMove(chosenPiece, dice);
            }
            Debug.WriteLine("\n Instruction = " + gameState.Instruction);
            return updateGameState(gameState, pieces);
        }


        private GameState updateGameState(GameState gameState, Piece[][] pieces)
        {
            gameState.Pieces = exportPieces(pieces);
            gameState.Instruction = exportInstruction();
            return gameState;
        }

        private Instructions exportInstruction()
        {
            Instructions instruction = (Instructions) ludoBoard.Instruction;
            ludoBoard.Instruction = Instructions.NotIntroduce; // reset
            return instruction;
        }
        private List<int[]> exportPieces(Piece[][] pieces)
        {
            List<int[]> piecesList = new List<int[]>();

                for (int color = 0; color < numOfPlayers; color++)
                {
                    for (int index = 0; index < numOfPiecesPerPlayer; index++)
                    {
                        int piecePosition = pieces[color][index].Position;
                        int pieceSteps = pieces[color][index].Steps;
                        int[] pieceInfo = { piecePosition, color, pieceSteps };
                        piecesList.Add( pieceInfo );   
                    }
                }
            return piecesList;
        }

        /// <summary>
        /// tryActivate(Piece)
        /// </summary>
        /// <param name="dice"></param>
        /// <returns></returns>
        private bool tryActivate(Colors playerID, dice dice, int chosenPieceID)
        {
            bool isPieceActivated;

            switch (dice)
            {
                case 6:
                    ludoBoard.tryActivatePiece(playerID, out isPieceActivated, chosenPieceID);
                    break;
                default:
                    isPieceActivated = false;
                    break;
            }
            return isPieceActivated;
        }

        /// <summary>
        ///  tryMove(piece)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="dice"></param>
        private bool tryMove(Piece piece, dice dice)
        {
            bool isMoveSuccesful = false;
            ludoBoard.tryMovePiece(piece, dice, ref isMoveSuccesful); 
            string debug = (isMoveSuccesful) ? "Rules: Move succesful" : "Rules: Move not succesful";
            Debug.WriteLine("\n" + debug);
            return isMoveSuccesful;
        }
        #endregion
    }
}
