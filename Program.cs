﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LegislatureSim
{
    static class Constants
    {
        public static readonly Random rng = new Random();
        public static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        public static string[] adjectives;
        public static string[] nouns;

        public static float Distance(Tuple<float, float> t1, Tuple<float, float> t2)
        {
            return (float) Math.Sqrt(Math.Pow(t2.Item1-t1.Item1, 2) + Math.Pow(t2.Item2 - t1.Item2, 2));
        }

        public static string ToTitleCase(string str)
        {
            return Regex.Replace(str, @"\b[a-z]", delegate (Match m)
                {
                    return m.Value.ToUpper();
                }
            );
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            int numSeats = 100;
            int numParties = 3;

            using (Stream stream = Constants.assembly.GetManifestResourceStream("LegislatureSim.Resources.enAdjectives.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                Constants.adjectives = result.Split();
            }

            using (Stream stream = Constants.assembly.GetManifestResourceStream("LegislatureSim.Resources.enNouns.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                Constants.nouns = result.Split();
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

            var roundedCompassPts = from legislator in legislators select FloorTuple(legislator.compass);
            for (int y = -10; y < 10; y++)
            {
                for (int x = -10; x < 10; x++)
                {
                    if (roundedCompassPts.Contains(Tuple.Create((float) x,(float) y)))
                    {
                        Console.Write(" #");
                    } else
                    {
                        Console.Write(" ·");
                    }
                }
                Console.WriteLine();
            }

            while (true)
            {
                legislature.HoldSession();
                Console.ReadLine();
            }
        }

        private static Tuple<float, float> GenerateCompass()
        {
            float x = (float)Constants.rng.NextDouble() * 20;
            x -= 10;
            float y = (float)Constants.rng.NextDouble() * 20;
            y -= 10;
            return Tuple.Create(x, y);
        }

        private static string GenerateName(int maxLength, string prefix = "", string suffix = "")
        {
            string name = prefix;
            for (int letter = 0; letter < Constants.rng.Next(3, maxLength); letter++)
            {
                name += ((letter % 2 != 0) ? "bcdfghjklmnpqrstvwxyz"[Constants.rng.Next(21)] : "aeiou"[Constants.rng.Next(5)]).ToString();
            }
            name += suffix;
            return Constants.ToTitleCase(name);
        }

        private static Tuple<float, float> FloorTuple(Tuple<float, float> tuple)
        {
            return Tuple.Create((float)Math.Floor(tuple.Item1), (float)Math.Floor(tuple.Item2));
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
        private int passed = 0;
        private int failed = 0;
        private int proposed = 0;

        public Legislator[] legislators;

        public Legislature(Legislator[] legislators)
        {
            this.legislators = legislators;
        }

        public void HoldSession()
        {
            proposed++;
            Legislator advocate = this.legislators[Constants.rng.Next(this.legislators.Length)];
            var billCompass = Tuple.Create(advocate.compass.Item1 + Fuzziness(5), advocate.compass.Item2 + Fuzziness(5));
            var billName = GenerateBillName();

            Console.WriteLine(String.Format("=== Session {0} ===", proposed));
            Console.WriteLine(String.Format("{0} is introducing the {1}.", advocate, billName));

            var aye = 1; //1 for the advocate
            var nay = 0;
            var abstain = 0;
            foreach (Legislator legislator in this.legislators)
            {
                if (Constants.Distance(legislator.compass, billCompass) > 8 + Fuzziness(5))
                {
                    if (Constants.rng.Next() % 2 == 0) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        nay++;
                    } else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        abstain++;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    aye++;
                }
                Console.Write("#");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.Write(String.Format("\n{0} AYE {1} NAY {2} ABSTAIN, motion ", aye, nay, abstain));
            if (aye > nay)
            {
                passed++;
                Console.WriteLine("passes");
            } else
            {
                failed++;
                Console.WriteLine("fails");
            }

            Console.WriteLine();
            String statisticsPrintout = @"Legislature statistics:
Bill success percentage: {0}
Total bills proposed: {1}
Sessions wasted: {2}";
            Console.WriteLine(String.Format(statisticsPrintout, ((float)passed/(float)proposed)*100, proposed, failed));
        }

        private string GenerateBillName()
        {
            string adjective = Constants.adjectives[Constants.rng.Next(Constants.adjectives.Length)];
            string noun = Constants.nouns[Constants.rng.Next(Constants.nouns.Length)];
            return Constants.ToTitleCase(adjective  + " " + noun + " bill");
        }

        private float Fuzziness(float absoluteValue)
        {
            var a = (float) (Constants.rng.NextDouble()*absoluteValue);
            a = (Constants.rng.Next() % 2 == 0) ? -a : a;
            return a;
        }
    }
}
