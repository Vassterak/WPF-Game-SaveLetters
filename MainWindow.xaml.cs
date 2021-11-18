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
        bool keyLeftDown = false;
        bool keyRightDown = false;

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

            physicsUpdate.Start(); //start timer
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
            int speed = 10; //10px per tick

            //Check if specific key is pressed on keyboard
            keyLeftDown = KeyBoardHoldCheck(Key.Left);
            keyRightDown = KeyBoardHoldCheck(Key.Right);

            if (keyLeftDown)
            {
                if (!CollisionDetectRect(boat, leftSideShore))
                    Canvas.SetLeft(boat, Canvas.GetLeft(boat) - speed); //move by x pixels per physics_update (tick)

            }
            else if (keyRightDown)
            {
                if (!CollisionDetectRect(boat, rightSideShore))
                    Canvas.SetLeft(boat, Canvas.GetLeft(boat) + speed);
            }
        }

        private void spawnRate_Tick(object? sender, EventArgs e) //SpawnRate Timer
        {
            throw new NotImplementedException();
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

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Left) //player is pressing left key
        //    {
        //        keyLeftDown = true;
        //        keyRightDown = false;
        //    }

        //    else if (e.Key == Key.Right) //player is pressing right key
        //    {
        //        keyRightDown = true;
        //        keyLeftDown = false;
        //    }
        //    else
        //    {
        //        keyRightDown = false;
        //        keyLeftDown = false;
        //    }
        //}

        private bool KeyBoardHoldCheck(Key key)
        {
            if (Keyboard.IsKeyDown(key))
                return true;

            else
                return false;
        }
    }
}
