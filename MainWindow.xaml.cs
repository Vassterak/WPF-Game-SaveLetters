using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace WPF_Game_SaveLetters
{

    public struct Letters //Each individial letter
    {
        public Label letter { get; }
        public bool aboard { get; set; }
        public bool isFalling { get; set; }

        public Letters(Label letter, bool aboard, bool isFalling)
        {
            this.letter = letter;
            this.aboard = aboard;
            this.isFalling = isFalling;
        }
    }

    public partial class MainWindow : Window
    {
        //Game object
        Rectangle leftSideShore = new Rectangle { Fill = Brushes.Green }; //left shore
        Rectangle rightSideShore = new Rectangle { Fill = Brushes.Green }; //right shore
        Rectangle bottomSideShore = new Rectangle { Fill = Brushes.Blue }; //water
        Rectangle boat = new Rectangle { Fill = Brushes.SaddleBrown };
        private bool boatIsMoving = false;

        //Timers
        DispatcherTimer physicsUpdate = new DispatcherTimer();
        DispatcherTimer spawnRate = new DispatcherTimer();

        //playerMovement
        private bool keyLeftDown = false;
        private bool keyRightDown = false;

        //Letters
        private int lettersRemoved = 0, lettersSaved = 0;
        private char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private List<Letters> lettersList = new List<Letters>();

        public MainWindow()
        {
            InitializeComponent();

            physicsUpdate.Tick += new EventHandler(physicsUpdate_Tick);
            physicsUpdate.Interval = new TimeSpan(0, 0, 0, 0, 20); //40ms => 50fps

            spawnRate.Tick += new EventHandler(spawnRate_Tick);
            spawnRate.Interval = new TimeSpan(0, 0, 0, 2); //every2 seconds

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateEnvironment();

            physicsUpdate.Start();
            spawnRate.Start();
        }

        private void GenerateEnvironment() //left + right shores + water
        {
            int fractionOfScreenWidth = 5; // one fift of width
            int fractionIfScreenHeight = 3; //one third of the height

            leftSideShore.Width = gameCanvas.ActualWidth / fractionOfScreenWidth;
            leftSideShore.Height = gameCanvas.ActualHeight / fractionIfScreenHeight;
            rightSideShore.Width = gameCanvas.ActualWidth / fractionOfScreenWidth;
            rightSideShore.Height = gameCanvas.ActualHeight / fractionIfScreenHeight;

            bottomSideShore.Width = gameCanvas.ActualWidth - (leftSideShore.Width + rightSideShore.Width);
            bottomSideShore.Height = gameCanvas.ActualHeight / (fractionOfScreenWidth -1) + 10; //10 is offset

            Canvas.SetLeft(leftSideShore, 0);
            Canvas.SetTop(leftSideShore, gameCanvas.ActualHeight - leftSideShore.Height);

            Canvas.SetLeft(rightSideShore, gameCanvas.ActualWidth - rightSideShore.Width);
            Canvas.SetTop(rightSideShore, gameCanvas.ActualHeight - leftSideShore.Height);

            Canvas.SetLeft(bottomSideShore, leftSideShore.Width);
            Canvas.SetTop(bottomSideShore, gameCanvas.ActualHeight - bottomSideShore.Height);

            gameCanvas.Children.Add(leftSideShore);
            gameCanvas.Children.Add(rightSideShore);
            gameCanvas.Children.Add(bottomSideShore);
            GenerateBoat();
        }

        private void GenerateBoat()
        {
            boat.Width = bottomSideShore.Width / 4;
            boat.Height = 50;

            Canvas.SetLeft(boat, leftSideShore.Width);
            Canvas.SetTop(boat, gameCanvas.ActualHeight - leftSideShore.Height);
            gameCanvas.Children.Add(boat);
        }

        private void physicsUpdate_Tick(object? sender, EventArgs e) //game physics timer
        {
            int boatSpeed = 10; //px per tick
            int letterSpeed = 2; //px per tick

            //Check if specific key is pressed on keyboard
            keyLeftDown = KeyBoardHoldCheck(Key.Left);
            keyRightDown = KeyBoardHoldCheck(Key.Right);

            if (keyLeftDown && !CollisionDetectRect(boat, leftSideShore))
            {
                Canvas.SetLeft(boat, Canvas.GetLeft(boat) - boatSpeed); //move by x pixels per physics_update (tick)
                boatIsMoving = true;
            }

            else if (keyRightDown && !CollisionDetectRect(boat, rightSideShore))
            {
                Canvas.SetLeft(boat, Canvas.GetLeft(boat) + boatSpeed);
                boatIsMoving = true;

            }

            //Move every letter in list
            for (int i = lettersList.Count -1; i >= 0; i--)
            {
                if (!lettersList[i].aboard)
                    Canvas.SetLeft(lettersList[i].letter, Canvas.GetLeft(lettersList[i].letter) + letterSpeed);

                if (!CollisionDetectRect(lettersList[i].letter, leftSideShore)) //When there is no groud on left side
                {
                    if (!CollisionDetectRect(lettersList[i].letter, boat) || lettersList[i].isFalling) //when there is no boat
                    {
                        Letters lt = lettersList[i]; //set it to permanent fall (this avoids clipping inside the boat)
                        lt.isFalling = true;
                        lettersList[i] = lt;

                        Canvas.SetTop(lettersList[i].letter, Canvas.GetTop(lettersList[i].letter) + 10); //move down by 10px
                    }

                    else //the letters are aboard
                    {
                        if (keyRightDown) //stop movement of numbers present on the platform
                        {
                            Letters lt = lettersList[i];
                            lt.aboard = true;
                            lettersList[i] = lt;
                            Canvas.SetLeft(lettersList[i].letter, Canvas.GetLeft(lettersList[i].letter) + boatSpeed);
                        }

                        else if (keyLeftDown && boatIsMoving)
                        {
                            Canvas.SetLeft(lettersList[i].letter, Canvas.GetLeft(lettersList[i].letter) - boatSpeed);
                        }

                    }
                }

                //When letter gets behind canvas it gets destroyed
                if (Canvas.GetLeft(lettersList[i].letter) > gameCanvas.ActualWidth)
                {
                    gameCanvas.Children.Remove(lettersList[i].letter);
                    lettersList.RemoveAt(i);
                    lettersSaved++;
                    labelDrowned.Content = "Počet zachráněných písmenek: " + lettersSaved; //change text in UI
                }

                //When letter gets behind canvas it gets destroyed (in water)
                else if (Canvas.GetTop(lettersList[i].letter) > gameCanvas.ActualHeight)
                {
                    gameCanvas.Children.Remove(lettersList[i].letter);
                    lettersList.RemoveAt(i);
                    lettersRemoved++;
                    
                    labelDrowned.Content = "Počet utopených písmenek: " + lettersRemoved; //change text in UI
                }
            }

            if ((lettersRemoved + lettersSaved) == alphabet.Length)
            {
                MessageBox.Show("The end");
                physicsUpdate.Stop();
            }

            boatIsMoving = false;
        }

        private void spawnRate_Tick(object? sender, EventArgs e) //SpawnRate Timer for letters
        {
            //Creating new label
            if ((lettersList.Count + lettersRemoved + lettersSaved) < alphabet.Length)
            {
                Label newLetter = new Label();
                newLetter.Content = alphabet[lettersList.Count + lettersRemoved + lettersSaved];
                newLetter.Background = Brushes.OrangeRed;
                newLetter.FontSize = 18;
                Canvas.SetLeft(newLetter, 0);
                Canvas.SetTop(newLetter, gameCanvas.ActualHeight - leftSideShore.Height - (newLetter.FontSize + 15)); //15px offset for maintaining collision state (one pixel in leftSideShore)

                gameCanvas.Children.Add(newLetter);
                lettersList.Add(new Letters(newLetter, false, false));
            }
            else 
                spawnRate.Stop();
        }

        private bool CollisionDetectRect(Shape shape1, Shape shape2) //shape X shape
        {
            Rect rect1 = new Rect(Canvas.GetLeft(shape1), Canvas.GetTop(shape1), shape1.Width, shape1.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(shape2), Canvas.GetTop(shape2), shape2.Width, shape2.Height);

            if (rect1.IntersectsWith(rect2))
                return true;
            else
                return false;
        }

        private bool CollisionDetectRect(Label label, Shape shape) //label X shape
        {
            Rect rect1 = new Rect();
            rect1.Location = new Point(Canvas.GetLeft(label), Canvas.GetTop(label));
            rect1.Height = label.ActualHeight;
            rect1.Width = label.ActualWidth;
            Rect rect2 = new Rect(Canvas.GetLeft(shape), Canvas.GetTop(shape), shape.Width, shape.Height);

            if (rect1.IntersectsWith(rect2))
                return true;
            else
                return false;
        }

        private bool KeyBoardHoldCheck(Key key)
        {
            if (Keyboard.IsKeyDown(key))
                return true;

            else
                return false;
        }
    }
}
