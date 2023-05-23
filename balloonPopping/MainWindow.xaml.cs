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
using System.Text.RegularExpressions;



namespace balloonPopping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        bool SAG,SOL;
        int dartMoveSpeed = 20;
        double dartTurnSpeed = 2;
        int currentTime = 0;
        Random rnd = new Random();
        int dartTurn;
        DispatcherTimer gameTimer = new DispatcherTimer();
        DispatcherTimer countUpTimer = new DispatcherTimer();
        List<Rectangle> removeDart = new List<Rectangle>();
        List<Rectangle> removeBloons = new List<Rectangle>();
        ImageBrush backgroundImage = new ImageBrush();
        Rectangle dart = new Rectangle();
        Label Ceza,Timer,Aci;
        RotateTransform rotateTransform2;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += startGame;
            gameTimer.Tick += gameEvent;
            countUpTimer.Tick += timer;
        }

        private void startGame(object sender,RoutedEventArgs e){
            gameCanvas.Children.Clear();
            gameCanvas.Focus();

            Ceza = new Label(){
                FontSize = 20,
                Name = "Ceza",
                FontWeight = FontWeights.ExtraBold,
                Foreground = Brushes.Red,
            };
            Canvas.SetBottom(Ceza,24);
            gameCanvas.Children.Add(Ceza);

            Timer = new Label(){
                FontSize = 20,
                Content = "Süre: 00:00",
                Name = "Timer",
                FontWeight = FontWeights.ExtraBold,
            };
            Canvas.SetBottom(Timer,0);
            gameCanvas.Children.Add(Timer);

            Aci = new Label(){
                FontSize = 20,
                Content = "Açı: 90",
                Name = "Aci",
                FontWeight = FontWeights.ExtraBold,
            };
            Canvas.SetBottom(Aci,0);
            Canvas.SetRight(Aci,0);
            gameCanvas.Children.Add(Aci);


            backgroundImage.ImageSource =new BitmapImage(new Uri("./assets/backgroundImage.jpg",UriKind.Relative));
            gameCanvas.Background = backgroundImage;

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();

            countUpTimer.Interval = TimeSpan.FromMilliseconds(1000);
            countUpTimer.Start();

            rotateTransform2 = new RotateTransform(0);
            dartTurn = rnd.Next(1,6);
            currentTime = 0;
        
            ImageBrush dartColor = new ImageBrush();  
            dartColor.ImageSource = new BitmapImage(new Uri($"./assets/dart{dartTurn}.png",UriKind.Relative));
                
            dart.Width = 70;
            dart.Height = 70;
            dart.Tag = "dart";
            dart.Fill = dartColor; 
            dart.Name ="dart";
            
            rotateTransform2.CenterX = dart.Width/2;
            rotateTransform2.CenterY = dart.Height;
            dart.RenderTransform = rotateTransform2;
        
            Canvas.SetLeft(dart,(Application.Current.MainWindow.Width/2)-(dart.Width/2));
            Canvas.SetTop(dart,Application.Current.MainWindow.Height-150);
            gameCanvas.Children.Add(dart);

            int num;
            int mRow = 4;
            int mColumn =7;
            int spaceBetweenBloons = 20;

            for(int row = 0; row < mRow; row++){
                num= rnd.Next(1,6);
                for(int column =0;column <mColumn;column++){
                    num++;
                    if(num > 5){
                        num = 1;
                    }
                    
                    ImageBrush bloonColor = new ImageBrush();
                    bloonColor.ImageSource = new BitmapImage(new Uri($"./assets/balloon{num}.png",UriKind.Relative));
               
                    Rectangle bloon = new Rectangle{
                        Width = Application.Current.MainWindow.Width/mColumn - spaceBetweenBloons,
                        Height = 80,
                        Tag = "bloon",
                        Fill = bloonColor,//bloonColor
                        Name =$"bloon{num}"
                    };

                    Canvas.SetLeft(bloon,column * (bloon.Width+spaceBetweenBloons));
                    Canvas.SetTop(bloon,row*bloon.Height);
                    gameCanvas.Children.Add(bloon);
    
                }
    
            }

        }
        private void gameEvent(object sender,EventArgs e){
            //timer++;
            //Timer.Content = timer.ToString();
            if(SOL == true && rotateTransform2.Angle > -90){
                rotateTransform2.Angle -=dartTurnSpeed;
                dart.RenderTransform =rotateTransform2;
                Aci.Content = $"Açı:{90-rotateTransform2.Angle}";
            }
            if(SAG == true && rotateTransform2.Angle < 90){
                rotateTransform2.Angle +=dartTurnSpeed;
                dart.RenderTransform =rotateTransform2;
                Aci.Content = $"Açı:{90-rotateTransform2.Angle}";
            }
            
            foreach(var dart in gameCanvas.Children.OfType<Rectangle>()){
                if((string)dart.Tag == "newDart"){
                    
                    RotateTransform? rotation = dart.RenderTransform as RotateTransform;
                    double newDartxDirection,newDartyDirection;
                    newDartxDirection = ((90.0 - rotation.Angle) * Math.PI)/180;
                    newDartyDirection = ((90.0 - rotation.Angle) * Math.PI)/180;


                    Canvas.SetLeft(dart,Canvas.GetLeft(dart) + dartMoveSpeed*Math.Cos(newDartxDirection));
                    Canvas.SetTop(dart,Canvas.GetTop(dart) - dartMoveSpeed*Math.Sin(newDartyDirection));

                    //Rect newDartHitBox = new Rect(Canvas.GetLeft(dart),Canvas.GetTop(dart), dart.Width, dart.Height); // düzenleme lazım
                    Rect newDartHitBox = new Rect(Canvas.GetLeft(dart)+dart.Width/2 -1,Canvas.GetTop(dart), dart.Width-(dart.Width/2 -1)*2 ,dart.Height);
                
                    if(Canvas.GetLeft(dart)<-20 || Canvas.GetLeft(dart)>Application.Current.MainWindow.Width || Canvas.GetTop(dart)<-20){
                        removeDart.Add(dart);
                    }

                    foreach(var bloon in gameCanvas.Children.OfType<Rectangle>()){
                        if((string)bloon.Tag=="bloon"){
                            Rect bloonHitBox = new Rect(Canvas.GetLeft(bloon), Canvas.GetTop(bloon), bloon.Width, bloon.Height);
                            if(newDartHitBox.IntersectsWith(bloonHitBox)){
                                string bloonNum = Regex.Match((string)bloon.Name, @"\d+").Value;
                                string dartNum = Regex.Match((string)dart.Name, @"\d+").Value;
                           
                                if(bloonNum == dartNum){
                                    removeDart.Add(dart);
                                    removeBloons.Add(bloon);
                                }
                                else{
                                    removeDart.Add(dart);
                                    removeBloons.Add(bloon);
                                    Ceza.Content = "+00:10";
                                    currentTime += 10;
                                } 
                            }
                        }
                    }
                }
           
            }

            foreach(Rectangle dart in removeDart){
                gameCanvas.Children.Remove(dart);
            }
            foreach(Rectangle bloon in removeBloons){
                gameCanvas.Children.Remove(bloon);
            }
        }
        private void keyDown(object sender,KeyEventArgs e){
            if(e.Key == Key.Left){
                SOL = true;
            }
            if(e.Key == Key.Right){
                SAG = true;
            }
          
        }
        private void keyUp(object sender,KeyEventArgs e){

            if(e.Key == Key.Left){
                SOL = false;
            }
            if(e.Key == Key.Right){
                SAG = false;
            }
            if(e.Key == Key.Space){


                ImageBrush newDartColor = new ImageBrush();
                newDartColor.ImageSource =  new BitmapImage(new Uri($"./assets/dart{dartTurn}.png",UriKind.Relative));

                Rectangle newDart = new Rectangle{
                    Width = 70,
                    Height = 70,
                    Fill = newDartColor,//newDartColor
                    Tag = "newDart",
                    Name= $"newDart{dartTurn}"
                };
                
                dartTurn++;
                if(dartTurn > 5){
                    dartTurn = 1;
                }

                ImageBrush dartColor = new ImageBrush();
                dartColor.ImageSource =  new BitmapImage(new Uri($"./assets/dart{dartTurn}.png",UriKind.Relative));
                dart.Fill = dartColor;


                RotateTransform newRotateTransform = new RotateTransform(rotateTransform2.Angle);
                newRotateTransform.CenterX = newDart.Width/2;
                newRotateTransform.CenterY = newDart.Height;
                newDart.RenderTransform = newRotateTransform;

                double newDartxDirection,newDartyDirection;
                newDartxDirection = ((90.0 - rotateTransform2.Angle) * Math.PI)/180;
                newDartyDirection = ((90.0 - rotateTransform2.Angle) * Math.PI)/180;

                Canvas.SetLeft(newDart,Canvas.GetLeft(dart) + dart.Width*Math.Cos(newDartxDirection));
                Canvas.SetTop(newDart,Canvas.GetTop(dart) - dart.Height*Math.Sin(newDartyDirection));
            
                gameCanvas.Children.Add(newDart);
            }
        }
       
        private void timer(object sender,EventArgs e){
            string seconds,minutes;

            currentTime += 1;
            seconds = (currentTime%60).ToString();
            minutes = (currentTime/60).ToString();
            if(seconds.Length < 2){
                seconds = $"0{seconds}";
            }
            if(minutes.Length < 2){
                minutes = $"0{minutes}";
            }
            Timer.Content =$"Süre: {minutes}:{seconds}";

            int bloonCount = 0;
            foreach(var bloon in gameCanvas.Children.OfType<Rectangle>()){
                if((string)bloon.Tag=="bloon"){
                    bloonCount++;
                }
            }
            if(bloonCount < 1){
                countUpTimer.Stop();

                Label showEndTime = new Label();
                showEndTime.Content = $"Süre: {minutes}:{seconds}";
                showEndTime.Foreground = Brushes.Black;
                showEndTime.FontSize = 36;
                Canvas.SetLeft(showEndTime,(Application.Current.MainWindow.Width/2 - 100));
                Canvas.SetTop(showEndTime,(Application.Current.MainWindow.Height/2 -200));
                gameCanvas.Children.Add(showEndTime);

                Button restartGameBtn = new Button();
                restartGameBtn.Content ="Yeni Oyun";
                restartGameBtn.Foreground = Brushes.Black;
                restartGameBtn.FontSize = 36;
                restartGameBtn.Click += startGame;
                Canvas.SetLeft(restartGameBtn,(Application.Current.MainWindow.Width/2 - 90));
                Canvas.SetTop(restartGameBtn,(Application.Current.MainWindow.Height/2 -300));
                gameCanvas.Children.Add(restartGameBtn);
                

            }

            if((string)Ceza.Content!=""){
                Ceza.Content="";
            }
            
        }
    }
}
