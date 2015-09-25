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

namespace dotMapCS
{
    public class Feature
    {
        public string Name { get; set; }
        public Color Color { get; set; }
    }

    public class Person
    {
        public CoordinatePair Pos { get; set; }
        public int FeatureIndex { get; set; }
    }

    public class DotMaker
    {
        protected GlobalMercator merc = new GlobalMercator();
        protected Random random = new Random(DateTime.Now.Millisecond);
        // a list of quadkey:Person tuples to hold generated dots
        protected List<Tuple<string, Person>> people = new List<Tuple<string, Person>>();
        protected double A = 1000.0;
        protected string tileDir = "tiles";
        protected int tileW = 256, tileH = 256;

        protected Feature[] features=null;

        public DotMaker(Feature[] features)
        {
            this.features = features;
        }

        public void CreatePoints(CoordinatePair[] polygon, int[] featureValues)
        {
            if (featureValues.Length != features.Length) throw new ArgumentException("featureValues");

            var bounds = getBounds(polygon);
            int total = featureValues.Sum();
            Console.WriteLine("Total dots: {0}", total);
            
            for (int k=0;k<features.Length;k++)
            {
                for (int i = 0; i < featureValues[k]; i++)
                {
                    if (i % 10000 == 0) Console.Write(".");
                    CoordinatePair point = null;
                    while (true)
                    {
                        point = new CoordinatePair()
                        {
                            X = createRandomValue(bounds[0], bounds[2]),
                            Y = createRandomValue(bounds[1], bounds[3])
                        };

                        if (Utils.IsPointInPolygon(polygon, point)) break;
                    }

                    var mPoint = merc.LatLonToMeters(point.Y, point.X);
                    var tileAddress = merc.MetersToTile(mPoint.X, mPoint.Y, 21);
                    var quadkey = merc.QuadTree(tileAddress.X, tileAddress.Y, 21);
                    var person = new Person()
                    {
                        Pos = mPoint,
                        FeatureIndex = k
                    };
                    people.Add(Tuple.Create(quadkey, person));
                }

            }
            Console.WriteLine("");
        }

        public void SortPoints()
        {
            Console.WriteLine("Sorting quadkeys...");
            people.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }

        public void WritePoints(string filename)
        {
            using (StreamWriter file = File.CreateText(filename))
            {
                foreach (var person in people)
                {
                    file.WriteLine("{0},{1},{2},{3}", person.Item1, person.Item2.Pos.X, person.Item2.Pos.Y,person.Item2.FeatureIndex);
                }
            }
        }

        public void ReadPoints(string filename)
        {
            using (StreamReader file = File.OpenText(filename))
            {
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine();
                    var tokens = line.Split(',');
                    var person=new Person()
                    {
                        Pos=new CoordinatePair()
                        {
                            X = Convert.ToDouble(tokens[1]),
                            Y = Convert.ToDouble(tokens[2])
                        },
                        FeatureIndex=tokens.Length == 3 ? 0 : Convert.ToInt32(tokens[3])
                    };
                    people.Add(Tuple.Create(tokens[0],person));
                }
            }
        }

        public void RenderTiles(int zoomLevel,string tileDir)
        {
            this.tileDir = tileDir;
            Console.WriteLine("Rendering level: " + zoomLevel.ToString());

            string quadKey = string.Empty;
            Graphics g = null; 
            Bitmap bmp=null;
            TileAddress tmsTile=null;
            
            Pen p = new Pen(Brushes.Black,1);
            p.Width = pointWeight(zoomLevel);
            
            foreach (var dot in people)
            {
                float px = (float)(dot.Item2.Pos.X / A);
                float py = (float)(dot.Item2.Pos.Y / A);
                String newQuadKey = dot.Item1.Substring(0, zoomLevel);

                if (!newQuadKey.Equals(quadKey))
                {
                    Console.Write(".");

                    //finish up the last tile
                    if (g!=null) 
                    {
                   //   saveImage(g,bmp,tmsTile,level);
                      saveImage(g, bmp, quadKey);
                    }

                    tmsTile = merc.QuadTreeToTile(newQuadKey, newQuadKey.Length);

                    // create new graphics
                    if (g != null) { g.Dispose(); g = null; }
                    if (bmp != null) { bmp.Dispose(); bmp = null; }
                    createGraphics(out bmp, out g);

                    var bounds = merc.TileBounds(tmsTile.X, tmsTile.Y, zoomLevel);
                    double tile_ll = bounds.West / A;
                    double tile_bb = bounds.South / A;
                    double tile_rr = bounds.East / A;
                    double tile_tt = bounds.North / A;

                    double xscale = tileW / (tile_rr - tile_ll);
                    double yscale = tileH/ (tile_tt - tile_bb);
                    double scale = Math.Min(xscale, yscale);

                    g.ScaleTransform((float)scale, (float)-scale);
                    g.TranslateTransform(-(float)tile_ll, -(float)tile_tt); 
                    
                    quadKey = newQuadKey;
                    
                }
                // draw point
                p.Color = Color.FromArgb(pointTransparency(zoomLevel), features[dot.Item2.FeatureIndex].Color);
               // g.DrawRectangle(p, px, py, p.Width,p.Width);
                g.DrawEllipse(p, px, py, p.Width, p.Width);
            }

            saveImage(g, bmp, quadKey);

            if (g != null) g.Dispose();
            if (bmp != null) bmp.Dispose();
            Console.WriteLine("");
        }

        protected void createGraphics(out Bitmap bmp,out Graphics g)
        {
            bmp = new Bitmap(tileW,tileH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }

        protected void saveImage(Graphics g,Bitmap bmp,TileAddress tile,int level)
        {
            g.Flush();
            string filename=Path.Combine(tileDir,string.Format("{0}\\{1}\\{2}.png",level,tile.X,tile.Y));
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            bmp.Save(filename);
        }

        protected void saveImage(Graphics g, Bitmap bmp, string quadKey)
        {
            g.Flush();
            var level = quadKey.Length;
            var tile = merc.QuadTreeToTile(quadKey, level);
            string filename = Path.Combine(tileDir, string.Format("{0}\\{1}\\{2}.png", level, tile.X, tile.Y));
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            bmp.Save(filename);
        }

        float pointWeight(int level)
        {
            switch (level)
            {
                case 4:
                    return 0.01f;
                case 5:
                    return 0.01f;
                case 6:
                    return 0.01f;
                case 7:
                    return 0.01f;
                case 8:
                    return 0.01f;
                case 9:
                    return 0.01f;
                case 10:
                    return 0.015f;
                case 11:
                    return 0.03f;
                case 12:
                    return 0.03f;
                case 13:
                    return 0.03f;
                case 14:
                    return 0.015f;
                case 15:
                    return 0.015f;
                default:
                    return 0.015f;
            }
        }

        int pointTransparency(int level)
        {
            switch (level)
            {
                case 4:
                    return 153;
                case 5:
                    return 153;
                case 6:
                    return 179;
                case 7:
                    return 179;
                case 8:
                    return 204;
                case 9:
                    return 204;
                case 10:
                    return 230;
                case 11:
                    return 230;
                case 12:
                    return 255;
                case 13:
                    return 255;
                default:
                    return 255;
            }
        }

        protected double[] getBounds(CoordinatePair[] polygon)
        {
            double ll = Double.MaxValue;
            double bb = Double.MaxValue;
            double rr = Double.MinValue;
            double tt = Double.MinValue;

            foreach (CoordinatePair p in polygon)
            {
                ll = Math.Min(ll, p.X);
                rr = Math.Max(rr, p.X);
                bb = Math.Min(bb, p.Y);
                tt = Math.Max(tt, p.Y);
            }

            return new double[] { ll, bb, rr, tt };
        }

        protected double createRandomValue(double low, double high)
        {
            return low + random.NextDouble() * (high - low);
        }


    }



}
