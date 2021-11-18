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
        public bool shouldMove { get; set; }
        public bool aboard { get; set; }

        public Letters(Label letter, bool shouldMove, bool aboard)
        {
            this.letter = letter;
            this.shouldMove = shouldMove;
            this.aboard = aboard;
        }
    }

    public partial class MainWindow : Window
    {
        //Game object
        Rectangle leftSideShore = new Rectangle { Fill = Brushes.Green }; //left shore
        Rectangle rightSideShore = new Rectangle { Fill = Brushes.Green }; //right shore
        Rectangle bottomSideShore = new Rectangle { Fill = Brushes.Blue }; //water
        Rectangle boat = new Rectangle { Fill = Brushes.SaddleBrown };

        //Timers
        DispatcherTimer physicsUpdate = new DispatcherTimer();
        DispatcherTimer spawnRate = new DispatcherTimer();

        //playerMovement
        private bool keyLeftDown = false;
        private bool keyRightDown = false;

        //Letters
        private int lettersRemoved = 0;
        private char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private List<Letters> lettersList = new List<Letters>();

        public MainWindow()
        {
            InitializeComponent();

            physicsUpdate.Tick += new EventHandler(physicsUpdate_Tick);
            physicsUpdate.Interval = new TimeSpan(0, 0, 0, 0, 20); //40ms => 50fps

            spawnRate.Tick += new EventHandler(spawnRate_Tick);
            spawnRate.Interval = new TimeSpan(0, 0, 0, 1); //every2 seconds

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
            int boatSpeed = 8; //px per tick
            int letterSpeed = 2; //px per tick

            //Check if specific key is pressed on keyboard
            keyLeftDown = KeyBoardHoldCheck(Key.Left);
            keyRightDown = KeyBoardHoldCheck(Key.Right);

            if (keyLeftDown)
            {
                if (!CollisionDetectRect(boat, leftSideShore))
                    Canvas.SetLeft(boat, Canvas.GetLeft(boat) - boatSpeed); //move by x pixels per physics_update (tick)

            }

            else if (keyRightDown)
            {
                if (!CollisionDetectRect(boat, rightSideShore))
                    Canvas.SetLeft(boat, Canvas.GetLeft(boat) + boatSpeed);
            }

            //Move every letter in list
            for (int i = lettersList.Count -1; i >= 0; i--)
            {
                Canvas.SetLeft(lettersList[i].letter, Canvas.GetLeft(lettersList[i].letter) + letterSpeed);

                if (Canvas.GetLeft(lettersList[i].letter) > gameCanvas.ActualWidth - 50)
                {
                    gameCanvas.Children.Remove(lettersList[i].letter);
                    lettersList.RemoveAt(i);
                    lettersRemoved++;
                }
            }

            //foreach (var item in lettersList) 
            //{
            //    Canvas.SetLeft(item.letter, Canvas.GetLeft(item.letter) + letterSpeed);

            //    if (Canvas.GetLeft(item.letter) > gameCanvas.ActualWidth - 50)
            //        lettersList.Remove(item); //Cannot loop trought and then delete without causing exeption
            //}
        }

        private void spawnRate_Tick(object? sender, EventArgs e) //SpawnRate Timer
        {
            //Creating new label
            if ((lettersList.Count + lettersRemoved) < alphabet.Length)
            {
                Label newLetter = new Label();
                newLetter.Content = alphabet[lettersList.Count + lettersRemoved];
                newLetter.Background = Brushes.OrangeRed;
                newLetter.FontSize = 18;
                Canvas.SetLeft(newLetter, 0);
                Canvas.SetTop(newLetter, gameCanvas.ActualHeight - leftSideShore.Height - (newLetter.FontSize + 16));

                gameCanvas.Children.Add(newLetter);
                lettersList.Add(new Letters(newLetter, true, false));
            }
            else 
                spawnRate.Stop();

        }

        private bool CollisionDetectRect(Rectangle shape1, Rectangle shape2)
        {
            Rect rect1 = new Rect(Canvas.GetLeft(shape1), Canvas.GetTop(shape1), shape1.Width, shape1.Height);
            Rect rect2 = new Rect(Canvas.GetLeft(shape2), Canvas.GetTop(shape2), shape2.Width, shape2.Height);

            if (rect2.IntersectsWith(rect1))
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
