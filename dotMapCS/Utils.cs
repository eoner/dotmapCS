/*
 * Based on RacialDotMap project at  https://github.com/unorthodox123/RacialDotMap
 * */
using System;
using System.Collections.Generic;
using GlobalMapTiles;

namespace dotMapCS
{
    public class Utils
    {
        // from http://dominoc925.blogspot.com/2012/02/c-code-snippet-to-determine-if-point-is.html
        public static bool IsPointInPolygon(CoordinatePair[] polygon, CoordinatePair point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        public static CoordinatePair[] ParseCoords(String wkt)
        {
            List<CoordinatePair> coords = new List<CoordinatePair>();
            char[] seps = new char[] { '(', ')', ',' };
            char[] space = new char[] { ' ' };
            var cTokens = wkt.Split(seps, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < cTokens.Length; i++)
            {
                var tokens = cTokens[i].Split(space, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2) continue;
                coords.Add(new CoordinatePair()
                {
                    X = Convert.ToDouble(tokens[0]),
                    Y = Convert.ToDouble(tokens[1])
                });
            }

            return coords.ToArray();

        }
    }
}
