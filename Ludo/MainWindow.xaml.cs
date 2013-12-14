
using LudoRules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;

namespace Ludo
{
    /// <summary>
    /// // <author> Per Jonsson, Hannah Börjesson </author>
    /// Innovativ Programmering, Linköpings Universitet
    /// TDDD49
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        Boolean hasGameStarted = false;
        Boolean hasChosenPiece = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<Piece> pieces;
        private int chosenPieceID;
        private const int numOfPiecesPerPlayer = 4;
        private const int numOfPlayers = 4;
        private LudoRules.Colors chosenPieceColor;

        private int currentTurn = 0;
        private string turn = "";

        private RuleEngine ruleEngine;
        private GameState gameState; 
        private GameEvent gameEvent;

        private string piecesStatus;

        private const int blueStartingPosition = 1;
        private const int redStartingPosition = 11; 
        private const int yellowStartingPosition = 21;
        private const int greenStartingPosition = 31;
        private int[] colorStartingPositions = {blueStartingPosition, redStartingPosition, 
                                               yellowStartingPosition, greenStartingPosition};

        #region Positions
        public Dictionary<int, List<int>> nestPositions = new Dictionary<int, List<int>>(){
                {0, new List<int> {1,1}}, {1, new List<int> {2,1}}, {2, new List<int> {1,2}}, {3, new List<int> {2,2}}, 
                {4, new List<int> {8,1}}, {5, new List<int> {9,1}}, {6, new List<int> {8,2}}, {7, new List<int> {9,2}},
                {8, new List<int> {8,8}}, {9, new List<int> {9,8}}, {10, new List<int> {8,9}}, {11, new List<int> {9,9}}, 
                {12, new List<int> {1,8}}, {13, new List<int> {2,8}}, {14, new List<int> {1,9}}, {15, new List<int> {2,9}}
                };

        
        public Dictionary<int, List<int>> squarePositions = new Dictionary<int, List<int>>(){
                {1, new List<int> {0,4}}, {2, new List<int> {1,4}}, {3, new List<int> {2,4}}, {4, new List<int> {3,4}},       
                {5, new List<int> {4,4}}, {6, new List<int> {4,3}}, {7, new List<int> {4,2}}, {8, new List<int> {4,1}},
                {9, new List<int> {4,0}}, {10, new List<int> {5,0}}, 

                {11, new List<int> {6,0}}, {12, new List<int> {6,1}},               
                {13, new List<int> {6,2}}, {14, new List<int> {6,3}}, {15, new List<int> {6,4}}, {16, new List<int> {7,4}},
                {17, new List<int> {8,4}}, {18, new List<int> {9,4}}, {19, new List<int> {10,4}}, {20, new List<int> {10,5}},   
                                                                                           
                {21, new List<int> {10,6}}, {22, new List<int> {9,6}}, {23, new List<int> {8,6}}, {24, new List<int> {7,6}},                                               
                {25, new List<int> {6,6}}, {26, new List<int> {6,7}}, {27, new List<int> {6,8}}, {28, new List<int> {6,9}},
                {29, new List<int> {6,10}}, {30, new List<int> {5,10}}, 
                
                {31, new List<int> {4,10}}, {32, new List<int> {4,9}},                                                                                           
                {33, new List<int> {4,8}}, {34, new List<int> {4,7}}, {35, new List<int> {4,6}}, {36, new List<int> {3,6}},                                               
                {37, new List<int> {2,6}}, {38, new List<int> {1,6}}, {39, new List<int> {0,6}}, {40, new List<int> {0,5}},
             
                // Red exit squares
                {41, new List<int> {5,1}},
                {42, new List<int> {5,2}},
                {43, new List<int> {5,3}},
                {44, new List<int> {5,4}},

                // Yellow
                {45, new List<int> {9,5}},
                {46, new List<int> {8,5}},
                {47, new List<int> {7,5}},
                {48, new List<int> {6,5}},

                // Green
                {49, new List<int> {5,9}},
                {50, new List<int> {5,8}},
                {51, new List<int> {5,7}},
                {52, new List<int> {5,6}},

                // Orange
                {53, new List<int> {1,5}},
                {54, new List<int> {2,5}},
                {55, new List<int> {3,5}},
                {56, new List<int> {4,5}},          
        };
        #endregion  
        #endregion



        #region Properties
        public List<Piece> Pieces
        {
            get { return pieces; }
            set { pieces = value; OnPropertyChanged("Pieces"); }
        }
        public string PlayerTurn
        {
            get { return turn; }
            set { turn = value; OnPropertyChanged("PlayerTurn"); }
        }
        public int ChosenPieceID
        {
            get { return chosenPieceID; }
            set { chosenPieceID = value; } 
        }
        public LudoRules.Colors ChosenPieceColor
        {
            get { return chosenPieceColor; }
            set { chosenPieceColor = value; } 
        }
        public string PiecesStatus
        {
            get { return piecesStatus;  }
            set { piecesStatus = value; OnPropertyChanged("PiecesStatus"); }
        }
        #endregion



        #region Methods
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            chooseTalkingBubble("/images/notstarted.png");
        }

        protected void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        void StartGame(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
        private void StartGame()
        {
            ruleEngine = new RuleEngine();
            gameState = new GameState();
            gameEvent = new GameEvent();
            pieces = new List<Piece>();
            hasGameStarted = true;
            createTurnString(ref turn);
            PlayerTurn = turn;
            gameEvent.Player = LudoRules.Colors.Blue; // || savedPlayer
            chooseTalkingBubble("/images/started.png");
            initializePieces();
        }     
        void SaveGame(object sender, RoutedEventArgs e)
        {
            // TODO:
            /*
              ruleEngine.saveGame()
             * spara vems tur det är? skicka in i saveGame
             */ 
        }
        void LoadGame(object sender, RoutedEventArgs e)
        {
            // TODO:
            /*
             StartGame();
             gameState = ruleEngine.loadGame()
             changePieces(gameState);
             *   
                    resetInGameValues();
                    gameEvent.Player = switchTurn(); 
                    createTurnString(ref turn);
                    PlayerTurn = turn;
             */
        }
        void QuitGame(object sender, RoutedEventArgs e)
        {
            // TODO:
            /*
             Hur gör man det??
             */ 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RollDiceAndHandleEvents(object sender, RoutedEventArgs e)
        {
            if (hasGameStarted)
            {
                // Force player to choose the right piece
                if (!hasChosenPiece)
                {
                    chooseTalkingBubble("/images/choosepieceid.png");
                }
                else if (hasChosenPiece && (ChosenPieceColor != gameEvent.Player))
                {
                    chooseTalkingBubble("/images/chooserightpieceid.png");
                }
                // Piece now chosen,try rolling the dice
                else
                {
                    Dice newDice = new Dice();
                    int diceRoll = newDice.rollDice(); 
                    showDice("/images/dice" + diceRoll.ToString() + ".jpg");

                    // Let the rule engine do its magic and update the game state, then update GUI
                    gameState = parseNewEvent(diceRoll);
                    showGameState(gameState);
                    changePieces(gameState);
                    showInstructions(diceRoll);

                    // Prepare next turn
                    resetInGameValues();
                    gameEvent.Player = switchTurn(); 
                    createTurnString(ref turn);
                    PlayerTurn = turn;
                } 
            }
        }

        private void showGameState(LudoRules.GameState gameState)
        {
            int i = 0;
            string status = String.Empty;

            foreach (var pieceInfo in gameState.Pieces)
            {
                var piecePosition = pieceInfo[0];
                var pieceColor = pieceInfo[1];
                var pieceSteps = pieceInfo[2];


               status += String.Format("\nPiece: {0}. Position: {1}, color: {2}, steps: {3}",
                    ++i, piecePosition, pieceColor, pieceSteps);
            }
            piecesStatus = status;
            PiecesStatus = piecesStatus;
        }

        private GameState parseNewEvent(int diceRoll)
        {
            gameEvent.Dice = diceRoll;
            gameEvent.Piece = ChosenPieceID;
            return ruleEngine.parseEvent(gameEvent);
        }

        void ScaleUp(object sender, MouseEventArgs e)
        {
            Image img = e.Source as Image;
            img.Height = img.ActualHeight * 1.2;
        }
        void ScaleDown(object sender, MouseEventArgs e)
        {
            Image img = e.Source as Image;
            img.Height /=  1.2;
        }
        private void changePieces(GameState gameState)
        {
            int x, y;
            int pieceIndexInNest = 0;

            foreach (var pieceInfo in gameState.Pieces)
            {
                var piecePosition = pieceInfo[0];
                var pieceColor = pieceInfo[1];
                var pieceSteps = pieceInfo[2];

                if (piecePosition == -1) // in nest
                {
                    x = nestPositions[pieceIndexInNest][0];
                    y = nestPositions[pieceIndexInNest][1];
                }
                else if (piecePosition == 44) // exited, nowhere
                {
                    x = 0; // TODO: göm undan pjäser ...
                    y = 0; //
                }
                else // in squares
                {
                    int addPos = 0;

                    if (pieceSteps >= 40) // in exit squares
                    {
                        switch (pieceColor)
                        {
                            case (int) LudoRules.Colors.Blue:
                                addPos += 12;
                                break;
                            case (int) LudoRules.Colors.Red:
                                addPos += 0;
                                break;
                            case (int) LudoRules.Colors.Yellow:
                                addPos += 4;
                                break;
                            case (int) LudoRules.Colors.Green:
                                addPos += 8;
                                break;
                        }
                        x = squarePositions[pieceSteps + 1 + addPos][0];
                        y = squarePositions[pieceSteps + 1 + addPos][1];
                    }
                    else
                    {
                        x = squarePositions[piecePosition + 1 + addPos][0];
                        y = squarePositions[piecePosition + 1 + addPos][1];
                    }
                    
                }

                pieces[pieceIndexInNest].X = x;
                pieces[pieceIndexInNest].Y = y;
                pieceIndexInNest++;
            }
            // Change Property
            Pieces = pieces;
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChoosePiece(object sender, RoutedEventArgs e)
        {
            Image img = e.Source as Image;
            ChosenPieceID = img.Name[img.Name.Length - 1] - 49; // fetch id (0-3)       
            ChosenPieceColor = getPieceColor(img);
            hasChosenPiece = true;
        }
        private LudoRules.Colors getPieceColor(Image img)
        {
            string name = img.Name;

            if (name.StartsWith("blue"))
            {
                return LudoRules.Colors.Blue;
            }
            else if (name.StartsWith("lavender"))
            {
                return LudoRules.Colors.Red;
            }
            else if (name.StartsWith("lemon"))
            {
                return LudoRules.Colors.Yellow;
            }
            else // (name.StartsWith("green"))
            {
                return LudoRules.Colors.Green;
            }
        }

        private void createTurnString(ref string turnString)
        {
            switch (gameEvent.Player)
            {
                case LudoRules.Colors.Blue:
                    turnString = "Ananasens tur";
                    break;
                case LudoRules.Colors.Red:
                    turnString = "Körsbärets tur";
                    break;
                case LudoRules.Colors.Yellow:
                    turnString = "Bananens tur";
                    break;
                case LudoRules.Colors.Green:
                    turnString = "Jordgubbens tur";
                    break;
            }
        }
        private LudoRules.Colors switchTurn()
        {
            // Increment or wrap around
            if (++currentTurn > 3)
            {
                currentTurn = 0;
            }
            return (LudoRules.Colors)currentTurn;
        }

        private void chooseTalkingBubble(string uri)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(uri, UriKind.Relative));
            bubble.Source = bitmapImage;
        }
        private void showDice(string uri)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(uri, UriKind.Relative));
            dice.Source = bitmapImage;
        }
        private void showInstructions(int diceRoll)
        {
            switch (gameState.Instruction)
            {
                case Instructions.Introduce:
                    chooseTalkingBubble("/images/activated.png");
                    break;
                case Instructions.NotIntroduce:
                    chooseTalkingBubble("/images/pass.png");
                    break;
                case Instructions.Move:
                    chooseTalkingBubble("/images/bubble" + diceRoll.ToString() + ".png");
                    break;
                case Instructions.MoveAndKnockout:
                    chooseTalkingBubble("/images/knock.png");
                    break;
                case Instructions.CollisionWithSelf:
                    chooseTalkingBubble("/images/collision.png");
                    break;
                case Instructions.Exit:
                    chooseTalkingBubble("/images/exit.png");
                    break;
                case Instructions.Victory:
                    chooseTalkingBubble("/images/victory.png");
                    break;
                default:
                    break;
            }
        }

        private void resetInGameValues()
        {
            hasChosenPiece = false;
        }

        private void initializePieces()
        {
            Piece blue1 = new Piece("ananas", 1, 1);
            pieces.Add(blue1);

            Piece blue2 = new Piece("ananas", 2, 1);
            pieces.Add(blue2);
            Piece blue3 = new Piece("ananas", 1, 2);
            pieces.Add(blue3);
            Piece blue4 = new Piece("ananas", 2, 2);
            pieces.Add(blue4);

            Piece lavender1 = new Piece("cherry", 8, 1);
            pieces.Add(lavender1);
            Piece lavender2 = new Piece("cherry", 9, 1);
            pieces.Add(lavender2);
            Piece lavender3 = new Piece("cherry", 8, 2);
            pieces.Add(lavender3);
            Piece lavender4 = new Piece("cherry", 9, 2);
            pieces.Add(lavender4);

            Piece lemon1 = new Piece("banan", 8, 8);
            pieces.Add(lemon1);
            Piece lemon2 = new Piece("banan", 9, 8);
            pieces.Add(lemon2);
            Piece lemon3 = new Piece("banan", 8, 9);
            pieces.Add(lemon3);
            Piece lemon4 = new Piece("banan", 9, 9);
            pieces.Add(lemon4);

            Piece green1 = new Piece("strawberry", 1, 8);
            pieces.Add(green1);
            Piece green2 = new Piece("strawberry", 2, 8);
            pieces.Add(green2);
            Piece green3 = new Piece("strawberry", 1, 9);
            pieces.Add(green3);
            Piece green4 = new Piece("strawberry", 2, 9);
            pieces.Add(green4);

            // change Property
            Pieces = pieces;

            //Show images of pieces on the board 
            BitmapImage blueImg1 = blue1.getImage();
            blueStart1.Source = blueImg1;
            BitmapImage blueImg2 = blue2.getImage();
            blueStart2.Source = blueImg2;
            BitmapImage blueImg3 = blue3.getImage();
            blueStart3.Source = blueImg3;
            BitmapImage blueImg4 = blue4.getImage();
            blueStart4.Source = blueImg4;

            BitmapImage lavenderImg1 = lavender1.getImage();
            lavenderStart1.Source = lavenderImg1;
            BitmapImage lavenderImg2 = lavender2.getImage();
            lavenderStart2.Source = lavenderImg2;
            BitmapImage lavenderImg3 = lavender3.getImage();
            lavenderStart3.Source = lavenderImg3;
            BitmapImage lavenderImg4 = lavender4.getImage();
            lavenderStart4.Source = lavenderImg4;

            BitmapImage lemonImg1 = lemon1.getImage();
            lemonStart1.Source = lemonImg1;
            BitmapImage lemonImg2 = lemon2.getImage();
            lemonStart2.Source = lemonImg2;
            BitmapImage lemonImg3 = lemon3.getImage();
            lemonStart3.Source = lemonImg3;
            BitmapImage lemonImg4 = lemon4.getImage();
            lemonStart4.Source = lemonImg4;

            BitmapImage greenImg1 = green1.getImage();
            greenStart1.Source = greenImg1;
            BitmapImage greenImg2 = green2.getImage();
            greenStart2.Source = greenImg2;
            BitmapImage greenImg3 = green3.getImage();
            greenStart3.Source = greenImg3;
            BitmapImage greenImg4 = green4.getImage();
            greenStart4.Source = greenImg4;
        }
        #endregion
    }
}