using System;
using System.Collections.Generic;
using DefaultNamespace;

namespace Utils
{
    public class RandomGeneration
    {
        private Random randomGenerator;
        
        public RandomGeneration()
        {
            randomGenerator = new Random();
        }

        public int GenerateUniformInt(int start, int end)
        {
            return randomGenerator.Next(start, end);
        }

        public T RandomChoose<T>(List<T> listOfValues)
        {
            var index = GenerateUniformInt(0, listOfValues.Count);
            return listOfValues[index];
        }
        
        public int GenerateBernoulli(float prob)
        {
            var value = randomGenerator.NextDouble();
            var outcome = value < prob ? 1 : 0;
            return outcome;
        }

        public int GenerateBinomial(int n, float prob)
        {
            int outcome = 0;
            for (int i = 0; i < n; i++)
            {
                outcome += GenerateBernoulli(prob);
            }

            return outcome;
        }
        
        public SpecimenEnum[] ShuffleArray(SpecimenEnum[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int i = randomGenerator.Next(n + 1);
                (array[i], array[n]) = (array[n], array[i]);
            }
            return array;
        }

    }
}