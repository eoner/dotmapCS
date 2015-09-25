/*
 * Based on RacialDotMap project at  https://github.com/unorthodox123/RacialDotMap
 * */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalMapTiles;
using Newtonsoft.Json;

namespace dotMapCS
{
    class Program
    {
        static void Main(string[] args)
        {
            electionTiles();
            populationTiles();        
        }

        private static void electionTiles()
        {
            var features = new Feature[]
            {
                new Feature() { Name = "AKP", Color=Color.Gold },
                new Feature() { Name = "CHP", Color=Color.Red },
                new Feature() { Name = "MHP", Color=Color.Blue },
                new Feature() { Name = "HDP", Color=Color.DarkGreen },
                new Feature() { Name = "Diğer", Color=Color.Gray }
                //,
                //new Feature() { Name = "Geçersiz", Color=Color.Azure},
                //new Feature() { Name = "Oy Kullanmayanlar", Color=Color.Black },
            };

            DotMaker dotmaker = new DotMaker(features);

            string jsonString = File.ReadAllText(@"..\..\data\istanbulData.json");
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            foreach (var ilce in data["İstanbul"].Ilceler)
            {
                foreach (var mahalle in ilce.Value.Mahalleler)
                {
                    Console.WriteLine("Ilce: {1}, Mahalle: {0}", mahalle.Name, ilce.Name);
                    var polygon = Utils.ParseCoords(mahalle.Value.Boundary.Value);
                    var values = new int[]
                    {
                        mahalle.Value.Secimler["2015"].AKP,
                        mahalle.Value.Secimler["2015"].CHP,
                        mahalle.Value.Secimler["2015"].MHP,
                        mahalle.Value.Secimler["2015"].HDP,
                        mahalle.Value.Secimler["2015"].Diger+
                        mahalle.Value.Secimler["2015"].Gecersiz+
                        mahalle.Value.Secimler["2015"].OyKullanmayan

                    };
                    dotmaker.CreatePoints(polygon, values);
                }
            }

            dotmaker.SortPoints();
            dotmaker.WritePoints(@"..\..\data\istSecim.csv");

            //Console.WriteLine("Reading file...");
            //dotmaker.ReadPoints(@"..\..\data\istSecim.csv");

            for (int i = 7; i <= 14; i++)
                dotmaker.RenderTiles(i, @"..\..\demo\tiles\istSecim");

        }

        public static void populationTiles()
        {
            var features = new Feature[]
            {
                new Feature() { Name = "Nufus-18", Color=Color.Red },
                new Feature() { Name = "Nufus+18", Color=Color.Lime }
            };

            DotMaker dotmaker = new DotMaker(features);

            string jsonString = File.ReadAllText(@"..\..\data\istanbulData.json");
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            foreach (var ilce in data["İstanbul"].Ilceler)
            {
                foreach (var mahalle in ilce.Value.Mahalleler)
                {
                    Console.WriteLine("Ilce: {1}, Mahalle: {0}", mahalle.Name, ilce.Name);
                    var polygon = Utils.ParseCoords(mahalle.Value.Boundary.Value);
                    dotmaker.CreatePoints(polygon, new int[] { mahalle.Value.Demografi.NufusToplam - mahalle.Value.Demografi.NufusArti18, 
                        mahalle.Value.Demografi.NufusArti18 });
                }
            }

            dotmaker.SortPoints();
            dotmaker.WritePoints(@"..\..\data\istNufus.csv");

            //   Console.WriteLine("Reading file...");
            //   dotmaker.ReadPoints("istNufus.csv");

            for (int i = 7; i <= 14; i++)
                dotmaker.RenderTiles(i, @"..\..\data\istNufus.csv");
        }
    }
}
