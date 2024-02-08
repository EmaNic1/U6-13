using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace U6_13
{
    /// <summary>
    /// CLASS IS FOR STORING PLAYER DATA
    /// </summary>
    class Handlers
    {
        private string nameSurname { get; set; }
        private int yearsOld { get; set; }
        private double weight { get; set; }
        private string playingPosition { get; set; }

        public Handlers()
        {
            nameSurname = "";
            yearsOld = 0;
            weight = 0;
            playingPosition = "";
        }

        public void Put(string nameSurname, int yearsOld, 
            double weight, string playingPosition)
        {
            this.nameSurname = nameSurname;
            this.yearsOld = yearsOld;
            this.weight = weight;
            this.playingPosition = playingPosition;
        }

        public string TakeNameSurname() { return nameSurname; }

        public int TakeYears() { return yearsOld; }

        public double TakeWeight() { return weight; }

        public string TakePlayingPosition() { return playingPosition; }

        public override string ToString()
        {
            string line;
            line = String.Format("|{0,-18}|{1,16:d}|{2,10:f2}|{3,-20}|", 
                nameSurname, yearsOld, weight, playingPosition);
            return line;
        }
    }

    /// <summary>
    /// CONTEINER CLASS IS FOR STORING PLAYER DATA
    /// </summary>
    class Matrix
    {
        const int CMaxLine = 100;
        const int CMaxColumns = 100;
        private Handlers[] hadlers;
        private int[,] H;
        public int n { get; set; }
        public int m { get; set; }

        public Matrix()
        {
            n = 0;
            m = 0;
            hadlers = new Handlers[CMaxLine];
            H = new int[CMaxLine, CMaxColumns];
        }

        /// <summary>
        /// Returns the object at the specified index
        /// </summary>
        /// <param name="nr">Number</param>
        /// <returns>Returns the object at the specified index</returns>
        public Handlers Take(int nr) { return hadlers[nr]; }

        /// <summary>
        /// Adds a new object array and increases 
        /// the size of the array by one
        /// </summary>
        /// <param name="ob">Object</param>
        public void Add(Handlers ob) { hadlers[n++] = ob; }

        /// <summary>
        /// Replaces an element of the order matrix
        /// </summary>
        /// <param name="i">Line</param>
        /// <param name="j">Colunm</param>
        /// <param name="points">New orde</param>
        public void Put(int i, int j, int points) { H[i, j] = points; }

        /// <summary>
        ///  Returns an element of the array
        /// </summary>
        /// <param name="i">Line</param>
        /// <param name="j">Column</param>
        /// <returns>Returns an element of the array</returns>
        public int Take(int i, int j) { return H[i, j]; }

        /// <summary>
        /// Deletes two elements
        /// </summary>
        /// <param name="points">Points array</param>
        /// <param name="points1">First point index</param>
        /// <param name="points2">Second point index</param>
        public void Delete(int[] points, int points1, int points2)
        {
            hadlers[points1] = hadlers[n - 1];
            hadlers[points2] = hadlers[n - 2];

            points[points1] = points[n - 1];
            points[points2] = points[n - 2];

            n = n - 2;
        }
    }

    internal class Program
    {
        const string PD = "Duomenys.txt";
        const string RZ = "Rezultatai.txt";

        static void Main(string[] args)
        {
            Matrix hadlers = new Matrix();
            string teamName, coachName;
            Read(ref hadlers, PD, out teamName, out coachName);
            if(File.Exists(RZ))
                File.Delete(RZ);
            Print(hadlers, RZ, "Informacija apie zaidejus:", teamName, coachName);

            //Prints how many points every player has
            int[] points = new int[hadlers.n];//points array 
            PointsByPlayers(hadlers, points);
            PrintEarnedPoints(hadlers, RZ, points, "Kiek kievienas zaidejas" +
                " surinko tasku:");

            //Prints two most points 
            if (hadlers.n >= 2)
                PrintsMostEarnedPoints(hadlers, RZ, points);
            else
                using (var fr = File.AppendText(RZ))
                {
                    fr.WriteLine("Nera bent dvieju zaideju sarase.");
                    fr.WriteLine();
                }

            //Deletes two leats points
            int points1, points2;
            int maziausiai = LeastPointsByPlayers(hadlers, points, 
                out points1, out points2);
            if (points1 > -1 && points2 > -1)
            {
                hadlers.Delete(points, points1, points2);
                PrintEarnedPoints(hadlers, RZ, points, "Sarasas, po atleistu zaideju " +
                    "su maziausiais taskais:");
            }
            else
                using (var fr = File.AppendText(RZ))
                {
                    fr.WriteLine("Nera zaideju, su dviem maziausiais taskais.");
                }
        }

        /// <summary>
        /// Reads all data from the data file
        /// </summary>
        /// <param name="hadlers">Conteiner</param>
        /// <param name="fv">Data file name</param>
        static void Read(ref Matrix hadlers, string fv, out string teamName, 
            out string coachName)
        {
            using (StreamReader reader = new StreamReader(fv))
            {
                string line;
                string[] parts;
                //reads first data line
                line = reader.ReadLine();
                string[] part = line.Split(';');
                teamName = part[0];
                coachName = part[1];
                int playerNumber = int.Parse(part[2]);
                int gameNumber = int.Parse(part[3]);
                //reads the following lines
                for (int i = 0; i < playerNumber; i++)
                {
                    line = reader.ReadLine();
                    parts = line.Split(';');
                    string nameSurname = parts[0];
                    int yearsOld = int.Parse(parts[1]);
                    double weight = double.Parse(parts[2]);
                    string gamePosition = parts[3];
                    Handlers hand = new Handlers();
                    hand.Put(nameSurname, yearsOld, weight, gamePosition);
                    hadlers.Add(hand);
                }
                //read the matrix(points)
                hadlers.n = playerNumber;
                hadlers.m = gameNumber;
                for (int i = 0; i < hadlers.n; i++)
                {
                    line = reader.ReadLine();
                    parts = line.Split(' ');
                    for (int j = 0; j < hadlers.m; j++)
                    {
                        int points = int.Parse(parts[j]);
                        hadlers.Put(i, j, points);
                    }
                }
            }
        }

        /// <summary>
        /// Prints all the data from data file
        /// </summary>
        /// <param name="hadlers">Conteiner</param>
        /// <param name="fv">Results file name</param>
        /// <param name="name">Inscription above the table</param>
        static void Print(Matrix hadlers, string fv, string name,
            string teamName, string coachName)
        {
            using (var fr = File.AppendText(fv))
            {
                fr.WriteLine("Komandos pavadinimas: {0}", teamName);
                fr.WriteLine("Komandos treneris: {0}", coachName);
                fr.WriteLine("Zaideju skaicius: {0}", hadlers.n);
                fr.WriteLine("Suzaista zaidimu: {0}", hadlers.m);
                fr.WriteLine();
                fr.WriteLine(name);
                fr.WriteLine("|------------------------------------" +
                    "-------------------------------|");
                fr.WriteLine("|  Pavarde Vardas  |  Gimimo metai  |" +
                    "  Svoris  |  Zaidimo pozicija  |");
                fr.WriteLine("|------------------------------------" +
                    "-------------------------------|");
                for (int i = 0; i < hadlers.n; i++)
                    fr.WriteLine("{0}", hadlers.Take(i).ToString());
                fr.WriteLine("|-------------------------------------" +
                    "------------------------------|");
                fr.WriteLine();
                fr.WriteLine("Zaideju pelnyti taskai:");
                for (int i = 0; i < hadlers.n; i++)
                {
                    fr.Write("{0,-19}|", hadlers.Take(i).TakeNameSurname());
                    for (int j = 0; j < hadlers.m; j++)
                    {
                        fr.Write("{0,2:d}", hadlers.Take(i, j));
                        fr.Write("|");
                    }

                    fr.WriteLine();
                }
                fr.WriteLine();
            }
        }

        /// <summary>
        /// Counts how many points each player has 
        /// and puts the points into an array
        /// </summary>
        /// <param name="hadlers">Conteiner name</param>
        /// <param name="points">Points array</param>
        static void PointsByPlayers(Matrix hadlers, int[] points)
        {
            for (int i = 0; i < hadlers.n; i++)
            {
                int count = 0;
                for (int j = 0; j < hadlers.m; j++)
                    count += hadlers.Take(i, j);
                points[i] = count;
            }
        }

        /// <summary>
        /// Prints how many points each player has
        /// </summary>
        /// <param name="hadlers">Conteiner</param>
        /// <param name="fv">Results file name</param>
        /// <param name="points">Points array</param>
        ///<param name="name">Inscription above the table</param>
        static void PrintEarnedPoints(Matrix hadlers, string fv, 
            int[] points, string name)
        {
            using (var fr = File.AppendText(fv))
            {
                fr.WriteLine(name);
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                fr.WriteLine("|  Pavarde Vardas  |  Gimimo metai  |" +
                    "  Svoris  |  Zaidimo pozicija  |  Surinkti taskai  |");
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                for (int i = 0; i < hadlers.n; i++)
                    fr.WriteLine("{0} {1,18:d}|", 
                        hadlers.Take(i).ToString(), points[i]);
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                fr.WriteLine();
            }
        }

        /// <summary>
        /// Finds two players, that has the highes points
        /// </summary>
        /// <param name="hadlers">Conteiner</param>
        /// <param name="points">Points array</param>
        /// <param name="points1">First highes point</param>
        /// <param name="points2">Second highes point</param>
        /// <returns>Two players with highes points(name)</returns>
        static bool MostPointsByPlayers(Matrix hadlers, int[] points,
            out int points1, out int points2)
        {
            points1 = -1;
            points2 = -1;
            if (hadlers.n < 2)
                return false;
            if (points[0] > points[1])
            {
                points1 = points[0];
                points2 = points[1];
            }
            else
            {
                points1 = points[1];
                points2 = points[0];
            }
            for (int i = 2; i < hadlers.n; i++)
            {
                if (points[i] > points1)
                {
                    points2 = points1;
                    points1 = points[i];
                }
                else if (points[i] > points2)
                {
                    points2 = points[i];
                }
            }
            return true;
        }

        /// <summary>
        /// Prints two players, that has the highes points
        /// </summary>
        /// <param name="hadlers">Conteiner name</param>
        /// <param name="fv">Results file</param>
        /// <param name="points">Points array</param>
        static void PrintsMostEarnedPoints(Matrix hadlers, string fv, int[] points)
        {
            using (var fr = File.AppendText(fv))
            {
                int points1, points2;
                bool mostEarnedPoints = MostPointsByPlayers(hadlers, points,
                    out points1, out points2);
                fr.WriteLine("Du zaidejai, surinke daugiausiai tasku:");
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                fr.WriteLine("|  Pavarde Vardas  |  Gimimo metai  |" +
                    "  Svoris  |  Zaidimo pozicija  |  Surinkti taskai  |");
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                for (int i = 0; i < hadlers.n; i++)
                {
                    if (points1 == points[i])
                        fr.WriteLine("{0} {1,18:d}|", 
                            hadlers.Take(i).ToString(), points1);

                    else if (points2 == points[i])
                        fr.WriteLine("{0} {1,18:d}|", 
                            hadlers.Take(i).ToString(), points2);
                }
                fr.WriteLine("|------------------------------------" +
                    "---------------------------------------------------|");
                fr.WriteLine();
            }
        }

        /// <summary>
        /// Finds two players, that has the least points
        /// </summary>
        /// <param name="hadlers">Conteiner</param>
        /// <param name="points">Points array</param>
        /// <param name="points1">First highes point</param>
        /// <param name="points2">Second highes point</param>
        /// <returns>Two players with least points(idnex)</returns>
        static int LeastPointsByPlayers(Matrix hadlers, int[] points, 
            out int index1, out int index2)
        {
            index1 = -1;
            index2 = -1;
            if (hadlers.n < 2)
                return -1;
            if (points[0] < points[1])
            {
                index1 = 0;
                index2 = 1;
            }
            else
            {
                index1 = 1;
                index2 = 0;
            }
            for (int i = 2; i < hadlers.n; i++)
            {
                if (points[i] < points[index1])
                {
                    index2 = index1;
                    index1 = i;
                }
                else if (points[i] < points[index2])
                {
                    index2 = i;
                }
            }
            return 0;
        }
    }
}
