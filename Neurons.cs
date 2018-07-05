using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAnts
{
    static class StaticMethods
    {
        public static List<double> CloneList(List<double> toClone)
        {
            List<double> cloned = new List<double>();
            toClone.ForEach((item) =>
            {
                cloned.Add(item);
            });
            return cloned;
        }

        public static double Sigmoid(double signal, double shape)
        {
            double result = 1 / (1 + Math.Exp(-signal / shape));
            return result;
        }

        public static double Mean(List<double> val)
        {
            double s = 0;
            for (int i = 0; i < val.Count; i++)
                s += val.ElementAt(i);
            return s/val.Count;
        }
    }
    class Neuron
    {
        int numOfInputs;
        List<double> weights;
        private static readonly Random rand = new Random();

        public List<double> Weights
        {
            get
            {
                return weights;
            }

            set
            {
                weights = value;
            }
        }

        public Neuron(int numOfInputs)
        {
            this.Weights = new List<double>();
            this.numOfInputs = numOfInputs;

            for (int i = 0; i < numOfInputs + 1; i++)
                Weights.Add((rand.NextDouble()-0.5)*5);
        }

        public Neuron(int numOfInputs, List<double> weights, double treshold)
        {
            this.Weights = new List<double>();
            this.numOfInputs = numOfInputs;
            this.Weights = StaticMethods.CloneList(weights);

            this.Weights.Add(treshold);
        }

        public double calculateOutput(List<double> inputs)
        {
            double output = 0;

            if (inputs.Count != numOfInputs)
                return output;

            for (int i = 0; i < inputs.Count; i++)
            {
                output += (inputs.ElementAt(i) * Weights.ElementAt(i));
            }
            output += -1 * Weights.Last();

            output = StaticMethods.Sigmoid(output, 1);

            return output;
        }
    }

    class NeuronLayer
    {
        int numOfNeurons;
        List<Neuron> neurons;

        internal List<Neuron> Neurons
        {
            get
            {
                return neurons;
            }

            set
            {
                neurons = value;
            }
        }

        public NeuronLayer(int numOfNeurons, int numOfInputs)
        {
            this.numOfNeurons = numOfNeurons;
            this.Neurons = new List<Neuron>();
            
            for(int i = 0; i<numOfNeurons; i++)
            {
                Neurons.Add(new Neuron(numOfInputs));
            }
        }

        public List<double> calculateOutput(List<double> inputs)
        {
            List<double> output = new List<double>();

            foreach(Neuron neuron in Neurons)
            {
                output.Add(neuron.calculateOutput(inputs));
            }

            return output;
        }
    }

    class NeuralNetwork
    {
        int numOfInputs;
        int numOfOutputs;
        int numOfHiddenLayers;
        int numOfNeuronsPerLayer;

        List<NeuronLayer> layers;

        public NeuralNetwork(int numOfInputs, int numOfOutputs, int numOfHiddenLayers, int numOfNeuronsPerLayer){
            this.numOfInputs = numOfInputs;
            this.numOfOutputs = numOfOutputs;
            this.numOfHiddenLayers = numOfHiddenLayers;
            this.numOfNeuronsPerLayer = numOfNeuronsPerLayer;

            layers = new List<NeuronLayer>();
            for(int i=0; i<this.numOfHiddenLayers; i++)
            {
                layers.Add(new NeuronLayer(numOfNeuronsPerLayer, numOfInputs));
            }
            layers.Add(new NeuronLayer(this.numOfOutputs, numOfNeuronsPerLayer));
        }

        public List<double> calculateOutput(List<double> inputs)
        {
            List<double> output = new List<double>();
            List<double> input = new List<double>();

            input = StaticMethods.CloneList(inputs);

            foreach (NeuronLayer layer in layers)
            {
                output = layer.calculateOutput(input);
                input.Clear();
                //input = StaticMethods.CloneList(output);
                input = output;
            }

            output = StaticMethods.CloneList(input);

            return output;
        }

        public List<double> getWeights()
        {
            List<double> weights = new List<double>();

            foreach (NeuronLayer layer in layers)
                foreach (Neuron neuron in layer.Neurons)
                    foreach (double weight in neuron.Weights)
                        weights.Add(weight);


            //Debug.WriteLine("Weights: " + String.Join(" ", weights.ToArray()));

            return weights;
        }

        public void setWeights(List<double> weights)
        {
            int i = 0;

            foreach (NeuronLayer layer in layers)
                foreach (Neuron neuron in layer.Neurons)
                {
                    neuron.Weights = StaticMethods.CloneList(weights.GetRange(i, numOfInputs));
                    i += 4;
                }
        }
    }
}
