﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace LegislatureSim
{
    static class Constants
    {
        public static readonly Random rng = new Random();
        public static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public static float Distance(Tuple<float, float> t1, Tuple<float, float> t2)
        {
            return (float) Math.Sqrt(Math.Pow(t2.Item1-t1.Item1, 2) + Math.Pow(t2.Item2 - t1.Item2, 2));
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            int numSeats = 20;
            int numParties = 3;

            var a = Constants.assembly.GetManifestResourceNames();

            string[] adjectives;
            using (Stream stream = Constants.assembly.GetManifestResourceStream("LegislatureSim.Resources.enAdjectives.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                adjectives = result.Split();
            }

            string[] nouns;
            using (Stream stream = Constants.assembly.GetManifestResourceStream("LegislatureSim.Resources.enNouns.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                nouns = result.Split();
            }

            Party[] parties = new Party[numParties];
            for (int party = 0; party < numParties; party++)
            {
                string randomName = GenerateName(20, suffix: " Party");
                Tuple<float, float> randomCompass = GenerateCompass();
                parties[party] = new Party(randomName, randomCompass);
            }

            Legislator[] legislators = new Legislator[numSeats];
            for (int seat = 0; seat < numSeats; seat++)
            {
                string randomName = GenerateName(20, prefix: "Legislator ") + GenerateName(20, prefix: " ");
                Tuple<float, float> randomCompass = GenerateCompass();
                legislators[seat] = new Legislator(randomName, randomCompass, parties);
            }

            Legislature legislature = new Legislature(legislators);

            foreach (Legislator legislator in legislature.legislators)
            {
                Console.WriteLine(legislator);
            }

            Console.ReadLine();
        }

        private static Tuple<float, float> GenerateCompass()
        {
            return Tuple.Create((float)Constants.rng.NextDouble()*20, (float)Constants.rng.NextDouble()*20);
        }

        private static string GenerateName(int maxLength, string prefix = "", string suffix = "")
        {
            string name = prefix;
            for (int letter = 0; letter < Constants.rng.Next(3, maxLength); letter++)
            {
                string nextLetter = ((letter % 2 != 0) ? "bcdfghjklmnpqrstvwxyz"[Constants.rng.Next(21)] : "aeiou"[Constants.rng.Next(5)]).ToString();
                nextLetter = (letter == 0) ? nextLetter.ToUpper() : nextLetter;
                name += nextLetter;
            }
            name += suffix;
            return name;
        }
    }

    class Legislator
    {
        public string name;
        public Tuple<float, float> compass;
        private Party party;

        public Legislator(string name, Tuple<float, float> compass, Party[] partyChoices)
        {
            this.name = name;
            this.compass = compass;
            ChooseParty(partyChoices);
        }

        private void ChooseParty(Party[] partyChoices)
        {
            Party bestParty = new Independent(this.compass);
            float bestDistance = float.MaxValue;
            foreach (Party party in partyChoices)
            {
                float partyDistance = Constants.Distance(this.compass, party.compass);
                if (partyDistance < bestDistance && partyDistance < 5f)
                {
                    bestParty = party;
                    bestDistance = partyDistance;
                }
            }
            this.party = bestParty;
        }

        public override string ToString()
        {
            return this.name + " (" + this.party + ")";
        }
    }

    class Party
    {
        public string name;
        public Tuple<float, float> compass;

        public Party(string name, Tuple<float, float> compass)
        {
            this.name = name;
            this.compass = compass;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    class Independent : Party
    {
        public Independent(Tuple<float, float> compass) : base("Independent", compass)
        {
            this.compass = compass;
        }
    }

    class Legislature
    {
        public Legislator[] legislators;

        public Legislature(Legislator[] legislators)
        {
            this.legislators = legislators;
        }

        public void HoldSession()
        {
            Legislator advocate = this.legislators[Constants.rng.Next(this.legislators.Length)];
            foreach (Legislator legislator in this.legislators)
            {
                
            }
        }
    }
}