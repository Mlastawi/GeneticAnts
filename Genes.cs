using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GeneticAnts
{
    public class Chromosome
    {
        public double fitness;
        List<double> neuralWeights;
        private static readonly Random rand = new Random();

        public List<double> NeuralWeights
        {
            get
            {
                return neuralWeights;
            }

            set
            {
                neuralWeights = value;
            }
        }

        public Chromosome()
        {
            fitness = 0;
            NeuralWeights = new List<double>();
        }

        public Chromosome(List<double> weights)
        {
            fitness = 0;
            NeuralWeights = new List<double>();
            NeuralWeights = StaticMethods.CloneList(weights);

        }

        public void evaluateFitness(Point birthPoint, Point target, double closestDistance)
        {
            Vector birthDistance = (target - birthPoint);

            fitness = (birthDistance.Length - closestDistance) / birthDistance.Length;
        }

        public Chromosome[] crossOver(Chromosome otherChromo)
        {
            int crossPoint = rand.Next(this.neuralWeights.Count);
            //int crossPoint = 48;

            List<double> n1 = this.NeuralWeights.GetRange(0, crossPoint);
            n1.AddRange(otherChromo.NeuralWeights.GetRange(crossPoint, otherChromo.NeuralWeights.Count - crossPoint));

            List<double> n2 = otherChromo.NeuralWeights.GetRange(0, crossPoint);
            n2.AddRange(this.NeuralWeights.GetRange(crossPoint, this.NeuralWeights.Count - crossPoint));

            Chromosome chromo1 = new Chromosome(n1);
            Chromosome chromo2 = new Chromosome(n2);

            Chromosome[] chromos = { chromo1, chromo2 };

            //System.Diagnostics.Debug.Write();
            return chromos;
        }

        public void mutate(double mutationRate, double mutationImpact)
        {
            for(int i=0; i<NeuralWeights.Count; i++)
            {
                if (rand.NextDouble() < mutationRate)
                    neuralWeights.Insert(i, neuralWeights.ElementAt(i) +(rand.NextDouble() - 0.5) * 2);
            }
        }
    }
}
