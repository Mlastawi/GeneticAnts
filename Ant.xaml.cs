using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GeneticAnts
{
    /// <summary>
    /// Interaction logic for Ant.xaml
    /// </summary>
    public partial class Ant : UserControl
    {
        private readonly static double mutationImpact = 0.3;
        private readonly static double mutationRate = 0.1;

        private int spieces;

        private double angle;
        private double forceLeft;
        private double forceRight;
        private Vector facingVector;

        Point birthPoint;
        public Point position;
        double closestDistance;

        private Chromosome genome;
        private NeuralNetwork brain;

        public Ant()
        {
            InitializeComponent();
            angle = 0;
            double radAngle = Math.PI / 180 * angle;
            facingVector = new Vector(Math.Sin(radAngle), -Math.Cos(radAngle));

            Spieces = 0;

            BirthPoint = new Point(0, 0);
            Position = new Point(0, 0);
            ClosestDistance = Double.MaxValue;

            grid.RenderTransformOrigin = new Point(0.5, 0.5);
            this.rotateTransform.Angle = angle;
            this.Width = head.Width;
            this.Height = head.Height + body.Height;

            this.ForceLeft = 0;
            this.ForceRight = 0;

            this.brain = new NeuralNetwork(4, 2, 1, 4);
            this.Genome = new Chromosome(brain.getWeights());

            body.SizeChanged += (s, arg) => { this.Height = head.Height + body.Height; };
            
        }

        public double Angle
        {
            get
            {
                return angle;
            }

            set
            {

                angle = value;
                double radAngle = Math.PI / 180 * angle;
                FacingVector = new Vector(Math.Sin(radAngle), -Math.Cos(radAngle));
                this.rotateTransform.Angle = value;
            }
        }

        public Vector FacingVector
        {
            get
            {
                return facingVector;
            }

            set
            {
                facingVector = value;
            }
        }

        public double ForceLeft
        {
            get
            {
                return forceLeft;
            }

            set
            {
                forceLeft = value;
                if (forceLeft > 50)
                    forceLeft = 50;
                else if (forceLeft < -50)
                    forceLeft = -50;
            }
        }

        public double ForceRight
        {
            get
            {
                return forceRight;
            }

            set
            {
                forceRight = value;
                if (forceRight > 50)
                    forceRight = 50;
                else if (forceRight < -50)
                    forceRight = -50;
            }
        }

        public Point BirthPoint
        {
            get
            {
                return birthPoint;
            }

            set
            {
                birthPoint = value;
            }
        }

        public double ClosestDistance
        {
            get
            {
                return closestDistance;
            }

            set
            {
                closestDistance = value;
            }
        }

        public int Spieces
        {
            get
            {
                return spieces;
            }

            set
            {
                spieces = value;
            }
        }

        internal Chromosome Genome
        {
            get
            {
                return genome;
            }

            set
            {
                genome = value;
            }
        }

        public Point Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public void analyze(List<double> inputs)
        {
            List<double> results = new List<double>();
            results = brain.calculateOutput(inputs);

            ForceLeft = (results.ElementAt(0) -0.5)*100;
            ForceRight = (results.ElementAt(1) -0.5)*100;
        }

        public void applyChromosome(Chromosome chromo)
        {
            this.brain.setWeights(chromo.NeuralWeights);
        }

        public double evaluateFitness(Point target)
        {
            Genome.evaluateFitness(this.birthPoint, target, this.closestDistance);
            return Genome.fitness;
        }


        public Ant[] reproduce(Ant otherParent)
        {
            Chromosome[] chromos = this.genome.crossOver(otherParent.Genome);

            Ant child1 = new Ant();
            Ant child2 = new Ant();

            chromos[0].mutate(mutationRate, mutationImpact);
            chromos[1].mutate(mutationRate, mutationImpact);

            child1.brain.setWeights(chromos[0].NeuralWeights);
            child2.brain.setWeights(chromos[1].NeuralWeights);
            

            Ant[] children = { child1, child2 };

            return children;
        }
    }
}
