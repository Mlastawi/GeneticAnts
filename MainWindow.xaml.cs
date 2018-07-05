using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;
using System.Linq;

namespace GeneticAnts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rand = new Random();
        List<Ant> ants;
        Ant currentAnt;

        Rectangle r;

        private DispatcherTimer timer;

        private int cycleLength = 30, numOfCycles = 1000;
        private int cycles;
        private int generation;
        private int populationSize = 40;

        bool quickGen;

        public Point GetTopLeft(Point a, Point b, Point c, Point d)
        {
            double x = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
            double y = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));

            return new Point(x, y);
        }
        public Point GetBotRight(Point a, Point b, Point c, Point d)
        {
            double x = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
            double y = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));

            return new Point(x, y);
        }

        public Rect GetBounds(Ant of, Canvas from)
        {
            Point origin = new Point(of.Position.X + of.Width/2, of.Position.Y + of.Height/2);
            Point lt = new Point(of.Position.X, of.Position.Y);
            Point rt = new Point(of.Position.X + of.Width, of.Position.Y);

            Vector originToLT = lt - origin;
            Vector originToRT = rt - origin;
            Vector vec = rotateVector(originToLT, of.Angle);
            Vector vec2 = rotateVector(originToRT, of.Angle);

            Point newLT = origin + vec;
            Point newRB = origin - vec;

            Point newRT = origin + vec2;
            Point newLB = origin - vec2;

            Point boundsLT = GetTopLeft(newLT, newRB, newRT, newLB);
            Point boundsRB = GetBotRight(newLT, newRB, newRT, newLB);

            Rect bounds = new Rect(boundsLT, boundsRB);
            return bounds;
        }

        public Vector rotateVector(Vector input, double angle)
        {
            Vector output = new Vector();
            double angleRadian = Math.PI / 180 * angle;
            output.X = input.X * Math.Cos(angleRadian) - input.Y * Math.Sin(angleRadian);
            output.Y = input.X * Math.Sin(angleRadian) + input.Y * Math.Cos(angleRadian);

            return output;
        }

        public Size GetRotatedSize(Ant of, double angle)
        {
            Size size = new Size();
            size.Height = Math.Abs(of.Width * Math.Sin(Math.PI/180 * angle)) + Math.Abs(of.Height * Math.Cos(Math.PI/180*angle));
            size.Width = Math.Abs(of.Width * Math.Cos(Math.PI/180 * angle)) + Math.Abs(of.Height * Math.Sin(Math.PI/180*angle));

            label1.Content = "H: " + size.Height.ToString() + "\nW:" + size.Width.ToString() + "\nAng:" + angle.ToString();
            return size;
        }

        public Vector antOriginToPoint(Ant ant, Point point)
        {
            Vector aotp = point - new Point(ant.Position.X + ant.Width / 2, ant.Position.Y + ant.Height / 2);

            return aotp;
        }

        public void showAntInfo(Ant ant)
        {
            Vector antOTP = antOriginToPoint(ant, new Point(mainCanvas.Width / 2, mainCanvas.Height / 2));
            inputsLabel.Content = "OTP.X: " + antOTP.X.ToString() + "\nOTP.Y: " + antOTP.Y.ToString() + "\nFV.X: " + ant.FacingVector.X.ToString() + "\nFV.Y: " + ant.FacingVector.Y.ToString();

            Vector movement = move(ant, ant.ForceLeft, ant.ForceRight);

            label2.Content = "FL: " + ant.ForceLeft.ToString() + "\nFR: " + ant.ForceRight.ToString() + "\nMovX: " + movement.X.ToString() + "\nMovY: " + movement.Y.ToString();

            Rect bounds = GetBounds(ant, mainCanvas);

            Canvas.SetTop(r, bounds.Top);
            Canvas.SetLeft(r, bounds.Left);

            r.Height = bounds.Height;
            r.Width = bounds.Width;

            label.Content = "Top:" + bounds.Top.ToString() + "\n" + "Left:" + bounds.Left.ToString() + "\n" + "Right:" + bounds.Right.ToString() + "\n" + "Bottom:" + bounds.Bottom.ToString();

            label.Content += "\nAnt Top: " + ant.position.Y + "\nAnt Left: " + ant.position.X;

            double fitness = ant.Genome.fitness;
            genomeLabel.Content = "Fitness: " + fitness.ToString() + "\nBirth Place X: " + ant.BirthPoint.X.ToString() + "\nBirth Place Y: " + ant.BirthPoint.Y.ToString() + "\nClosest distance: " + ant.ClosestDistance + "\nBirth distance: " + (new Point(mainCanvas.Width / 2, mainCanvas.Height / 2) - ant.BirthPoint).Length;

            spiecesLabel.Content = ant.Spieces;
        }
        public MainWindow()
        {
            InitializeComponent();

            ants = new List<Ant>();
            currentAnt = null;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(cycleLength);
            timer.Tick += (s,e) => moveAnts();

            nextGenButton.Click += (s, e) => nextGen();

            r = new Rectangle();
            r.Stroke = Brushes.Black;
            mainCanvas.Children.Add(r);

            circle.Fill = Brushes.Black;
            Canvas.SetTop(circle, mainCanvas.Height/2 -circle.Height/2);
            Canvas.SetLeft(circle, mainCanvas.Width/2 - circle.Width/2);

            generation = 0;
            cycles = 0;
            //Canvas.SetTop(circle,0);
            //Canvas.SetLeft(circle,0);
            
        }


        //buttons
        private void button_Click(object sender, RoutedEventArgs e)
        {
            resetButton_Click(sender, e);

            for(int i=0; i<populationSize; i++)
            {
                Ant ant = new Ant();
                ant.Spieces = i;
                ant.body.Fill = new SolidColorBrush(Color.FromRgb((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));
                ant.Angle = rand.Next(360);

                mainCanvas.Children.Add(ant);

                Size size = GetRotatedSize(ant, ant.Angle);

                ant.Position = new Point(rand.Next(0, (int)(mainCanvas.ActualWidth - size.Width)), rand.Next(0, (int)(mainCanvas.ActualHeight - size.Height)));

                Canvas.SetLeft(ant, ant.Position.X);
                Canvas.SetTop(ant, ant.Position.Y);

                ant.BirthPoint = new Point(ant.Position.X + ant.Width / 2, ant.Position.Y + ant.Height / 2);

                ants.Add(ant);
            }          

            
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(Ant ant in ants)
                mainCanvas.Children.Remove(ant);
            ants.Clear();
            currentAnt = null;

            generation = 0;
            generationCounter.Content = generation;

            timer.Stop();
            cycles = Int32.MaxValue;
        }

        private void mainCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (Ant ant in ants)
            {
                Rect bounds = GetBounds(ant, mainCanvas);

                if (bounds.Contains(e.GetPosition(mainCanvas)))
                {
                    currentAnt = ant;
                    break;
                }
            }
            if(currentAnt != null)
                showAntInfo(currentAnt);
        }

        private void nextGen()
        {
            ants = ants.OrderByDescending( o => o.Genome.fitness).ToList();
        
            System.Diagnostics.Debug.Write("Generation: " + generation + " Fitness: ");
            foreach(Ant ant in ants)
                System.Diagnostics.Debug.Write(ant.Genome.fitness.ToString() + " ");

            System.Diagnostics.Debug.Write("\n");
            //killTheWeak(ants);
            //repopulate();
            reproduce();

            int i = 0;
            foreach(Ant ant in ants)
            {
                ant.Spieces = i;
                i++;

                Size size = GetRotatedSize(ant, ant.Angle);

                ant.Position = new Point(rand.Next(0, (int)(mainCanvas.ActualWidth - size.Width)), rand.Next(0, (int)(mainCanvas.ActualHeight - size.Height)));

                Canvas.SetLeft(ant, ant.Position.X);
                Canvas.SetTop(ant, ant.Position.Y);

                ant.BirthPoint = new Point(ant.Position.X + ant.Width / 2, ant.Position.Y + ant.Height / 2);
            }

            generation++;
            generationCounter.Content = generation;
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {

            if (!timer.IsEnabled)
            {
                cycles = 0;
                quickGen = false;
                timer.Start();
            }
        }
        private void quickGenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                quickGen = true;

                cycles = 0;
                while (cycles < numOfCycles)
                {
                    moveAnts();
                }

                foreach (Ant ant in ants)
                {
                    Point target = new Point(mainCanvas.Width / 2, mainCanvas.Height / 2);
                    ant.evaluateFitness(target);
                }

                nextGen();
            }
        }

        private void quick100Gen_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                quickGen = true;

                for (int i = 0; i < 100; i++)
                {
                    cycles = 0;
                    while (cycles < numOfCycles)
                    {
                        moveAnts();
                    }

                    foreach (Ant ant in ants)
                    {
                        Point target = new Point(mainCanvas.Width / 2, mainCanvas.Height / 2);
                        ant.evaluateFitness(target);
                    }

                    nextGen();
                }
            }
        }


        //moving ants
        private void moveAnts()
        {
            if (cycles >= numOfCycles)
            {
                timer.Stop();
                if(!quickGen)
                {
                    //double meanFitness = M
                }
                return;
            }
            else cycles++;

            foreach (Ant ant in ants)
            {
                Point target = new Point(mainCanvas.Width / 2, mainCanvas.Height / 2);
                Rect bounds = GetBounds(ant, mainCanvas);
                Size size = GetRotatedSize(ant, ant.Angle);
                
                Vector antOTP = antOriginToPoint(ant, target);

                if (antOTP.Length < ant.ClosestDistance)
                    ant.ClosestDistance = antOTP.Length;

                antOTP.Normalize();

                List<double> inputs = new List<double>();
                inputs.Add(antOTP.X);
                inputs.Add(antOTP.Y);
                inputs.Add(ant.FacingVector.X);
                inputs.Add(ant.FacingVector.Y);
                ant.analyze(inputs);

                rotate(ant, ant.ForceLeft, ant.ForceRight);
                Vector movement = move(ant, ant.ForceLeft, ant.ForceRight);

                if (bounds.Top > 0 && bounds.Top + size.Height < mainCanvas.ActualHeight)
                {
                    ant.position.Y += movement.Y;
                }
                else if (bounds.Top <= 0)
                    ant.position.Y -= bounds.Top;
                else ant.position.Y -= (bounds.Top + size.Height - mainCanvas.ActualHeight);

                if (bounds.Left > 0 && bounds.Left + size.Width < mainCanvas.ActualWidth)
                {
                    ant.position.X += movement.X;
                }
                else if (bounds.Left <= 0)
                    ant.position.X -= bounds.Left;
                else ant.position.X -= (bounds.Left + size.Width - mainCanvas.ActualWidth);

                Canvas.SetLeft(ant, ant.Position.X);
                Canvas.SetTop(ant, ant.Position.Y);

                /*if (bounds.Top > 0 && bounds.Top + size.Height < mainCanvas.ActualHeight)
                {
                    double newtop = ant.Position.Y + movement.Y;
                    Canvas.SetTop(ant, newtop);
                }
                else if (bounds.Top <= 0)
                    Canvas.SetTop(ant, -bounds.Top + ant.Position.Y);
                else Canvas.SetTop(ant, ant.Position.Y - (bounds.Top + size.Height - mainCanvas.ActualHeight));


                if (bounds.Left > 0 && bounds.Left + size.Width < mainCanvas.ActualWidth)
                {
                    double newleft = ant.Position.X + movement.X;
                    Canvas.SetLeft(ant, newleft);
                }
                else if (bounds.Left <= 0)
                    Canvas.SetLeft(ant, -bounds.Left + ant.Position.X);
                else Canvas.SetLeft(ant, ant.Position.X - (bounds.Left + size.Width - mainCanvas.ActualWidth));*/

                if (!quickGen)
                    ant.evaluateFitness(target);

                if(ant.Equals(currentAnt))
                    showAntInfo(ant);
            }
        }

        private void moveAntsQuick()
        {
            cycles++;

            foreach (Ant ant in ants)
            {
                Point target = new Point(mainCanvas.Width / 2, mainCanvas.Height / 2);
                Rect bounds = GetBounds(ant, mainCanvas);
                Size size = GetRotatedSize(ant, ant.Angle);

                Vector antOTP = antOriginToPoint(ant, target);

                if (antOTP.Length < ant.ClosestDistance)
                    ant.ClosestDistance = antOTP.Length;

                antOTP.Normalize();

                List<double> inputs = new List<double>();
                inputs.Add(antOTP.X);
                inputs.Add(antOTP.Y);
                inputs.Add(ant.FacingVector.X);
                inputs.Add(ant.FacingVector.Y);
                ant.analyze(inputs);

                Vector movement = move(ant, ant.ForceLeft, ant.ForceRight);
                rotate(ant, ant.ForceLeft, ant.ForceRight);

                if (bounds.Top > 0 && bounds.Top + size.Height < mainCanvas.ActualHeight)
                {
                    ant.position.Y += movement.Y;
                }
                else if (bounds.Top <= 0)
                    ant.position.Y -= bounds.Top;
                else ant.position.Y -= (bounds.Top + size.Height - mainCanvas.ActualHeight);


                if (bounds.Left > 0 && bounds.Left + size.Width < mainCanvas.ActualWidth)
                {
                    ant.position.X += movement.X;
                }
                else if (bounds.Left <= 0)
                     ant.position.X -= bounds.Left;
                else ant.position.X -= (bounds.Left + size.Width - mainCanvas.ActualWidth);
            }
        }
        public void rotate(Ant ant, double forceLeft, double forceRight)
        {
            double alfa = (forceLeft - forceRight) * 0.05;

            ant.Angle += alfa;
        }

        public Vector move(Ant ant, double forceLeft, double forceRight)
        {
            Vector movement = new Vector();

            double v = 0;
            double forceToVelocity = 0.05;

            if(forceLeft * forceRight > 0)
                if(forceLeft >0)
                    v = Math.Min(forceLeft, forceRight) * forceToVelocity;
                else if(forceLeft<0)
                    v = Math.Max(forceLeft, forceRight) * forceToVelocity;

            movement.X = ant.FacingVector.X * v;
            movement.Y = ant.FacingVector.Y * v;

            return movement;
        }

        
        //genetics
        private void killTheWeak(List<Ant> pop)
        {
            List<Ant> newPop = new List<Ant>();

            double coupons = 0;
            for(int i = 0; i< populationSize; i++)
            {
                coupons += pop.ElementAt(i).Genome.fitness;
            }

            for (int i = 0; i < populationSize / 2; i++)
            {
                if (pop.First().Genome.fitness != 0)
                {
                    double tmp = rand.NextDouble() * coupons;
                    for (int j = 0; j < pop.Count; j++)
                    {
                        Ant ant = pop.ElementAt(j);

                        if (tmp <= ant.Genome.fitness)
                        {
                            coupons -= ant.Genome.fitness;
                            newPop.Add(ant);
                            pop.Remove(ant);
                            break;
                        }
                        else
                        {
                            tmp -= ant.Genome.fitness;
                        }
                    }
                }
                else
                {
                    int tmp = rand.Next(pop.Count);
                    Ant ant = pop.ElementAt(tmp);

                    newPop.Add(ant);
                    pop.Remove(ant);
                }
            }


            foreach(Ant ant in pop)
            {
                mainCanvas.Children.Remove(ant);
            }

            System.Diagnostics.Debug.Write("New population: ");
            foreach (Ant ant in newPop)
                System.Diagnostics.Debug.Write(ant.Spieces.ToString() + " ");

            System.Diagnostics.Debug.Write("\n");

            pop.Clear();
            foreach (Ant ant in newPop)
                pop.Add(ant);
        }
        

        private void repopulate()
        {
            Ant[] newAnts;
            while(ants.Count < populationSize)
            {
                newAnts = ants.ElementAt(rand.Next(ants.Count)).reproduce(ants.ElementAt(rand.Next(ants.Count)));
                for(int i = 0; i<2; i++)
                {
                   newAnts[i].body.Fill = new SolidColorBrush(Color.FromRgb((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));

                    newAnts[i].Angle = rand.Next(360);

                    mainCanvas.Children.Add(newAnts[i]);

                    

                    ants.Add(newAnts[i]);
                }
                
            }
        }

        private void reproduce()
        {
            List<Ant> newPop = new List<Ant>();
            
            double coupons = 0;
            for (int i = 0; i < populationSize; i++)
            {
                coupons += ants.ElementAt(i).Genome.fitness;
            }

            for (int i = 0; i<populationSize/2; i++)
            {
                Ant firstParent = new Ant();
                Ant secondParent = new Ant();

                double tmpCoupons = coupons;
                double r = rand.NextDouble() * tmpCoupons;

                //selecting 1st parent
                for (int j = 0; j < ants.Count; j++)
                {
                    Ant ant = ants.ElementAt(j);

                    if (r <= ant.Genome.fitness)
                    {
                        tmpCoupons -= ant.Genome.fitness;
                        firstParent = ant;
                        break;
                    }
                    else
                    {
                        r -= ant.Genome.fitness;
                    }
                }

                //selecting 2nd parent
                List<Ant> sec = new List<Ant>();
                
                sec = ants.ToArray().ToList();
                sec.Remove(firstParent);

                r = rand.NextDouble() * tmpCoupons;

                for (int j = 0; j < sec.Count; j++)
                {
                    Ant ant = sec.ElementAt(j);

                    if (r <= ant.Genome.fitness)
                    {
                        secondParent = ant;
                        break;
                    }
                    else
                    {
                        r -= ant.Genome.fitness;
                    }
                }

                //crossover
                Ant[] newAnts;

                //System.Diagnostics.Debug.WriteLine("1st parent fitness: " + firstParent.Genome.fitness + " 2nd parent fitness: " + secondParent.Genome.fitness);
                newAnts = firstParent.reproduce(secondParent);
                for (int k = 0; k < 2; k++)
                {
                    newAnts[k].body.Fill = new SolidColorBrush(Color.FromRgb((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));

                    newAnts[k].Angle = rand.Next(360);

                    mainCanvas.Children.Add(newAnts[k]);
                    
                    newPop.Add(newAnts[k]);
                }
            }

            foreach (Ant ant in ants)
                mainCanvas.Children.Remove(ant);

            ants.Clear();
            ants = newPop;
        }
    }
}
