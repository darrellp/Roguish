// ReSharper disable IdentifierTypo

// ReSharper disable InvalidXmlDocComment
namespace Roguish
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Values that represent directions. </summary>
    ///
    /// <remarks>   There is LOTS of code which would be identical except for swapping out X/Column and
    ///             Y/Row coordinates.  Duplication is evil and I don't want it in the code if I can avoid
    ///             it.  If I used only fields for coordinates then I just have to make separate versions
    ///             of code which uses pt.X and pt.Y or use ?: operator everywhere.  To avoid this, I've
    ///             supplied a way of indexing the different coordinates which uses a single Dir argument
    ///             to an indexer rather than separate Row and Column fields.  So pt[Dir.Horiz] returns
    ///             the Column and pt[Dir.Vert] returns the row.  The direction can be passed into routines
    ///             and therefore implement both versions of the code without duplicating effort.
    ///             
    ///             None of this is exposed publicly right now.  It might makes sense to do so, but I don't
    ///             want to overly complicate things.
    ///             
    ///             Darrellp, 8/25/2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public enum Dir
    {
        Horiz,
        Vert
    }

    public static class PointExt
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the "other direction from a direction. </summary>
        ///
        /// <remarks>   Darrellp, 8/25/2016. </remarks>
        ///
        /// <param name="dir">  The direction. </param>
        ///
        /// <returns>   Dir.Horiz if dir is Dir.Vert, Dir.Vert if dir is Dir.Horiz. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static Dir OtherDirection(Dir dir)
        {
            return dir == Dir.Horiz ? Dir.Vert : Dir.Horiz;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Indexer to get or set a coordinate specified by dir.
        /// </summary>
        ///
        /// <param name="dir">  The coordinate to return - dir.Horiz for columns, dir.Vert for rows. </param>
        ///
        /// <returns>   The proper coordinate. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static int Get(this Point @this, Dir dir)
        {
            return dir == Dir.Horiz ? @this.X : @this.Y;
        }

        public static Point Set(this Point @this, Dir dir, int value)
        {
            return dir == Dir.Horiz ? new Point(value, @this.Y) : new Point(@this.X, value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Increments the proper coordinate. </summary>
        ///
        /// <remarks>   Darrellp, 8/25/2016. </remarks>
        ///
        /// <param name="dir">  The direction to increment. </param>
        ///
        /// <returns>   The incremented HVPoint. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static Point NextLarger(this Point @this, Dir dir)
        {
            var x = dir == Dir.Horiz ? @this.X + 1 : @this.X;
            var y = dir == Dir.Horiz ? @this.Y : @this.Y + 1;
            return new Point(x, y);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Decrements the proper coordinate. </summary>
        ///
        /// <remarks>   Darrellp, 8/25/2016. </remarks>
        ///
        /// <param name="dir">  The direction to decrement. </param>
        ///
        /// <returns>   The decremented HVPoint. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static Point NextSmaller(this Point @this, Dir dir)
        {
            var x = dir == Dir.Horiz ? @this.X - 1 : @this.X;
            var y = dir == Dir.Horiz ? @this.Y : @this.Y - 1;
            return new Point(x, y);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates a Point by specifying values relative to a direction. </summary>
        ///
        /// <remarks>   Darrellp, 8/25/2016. </remarks>
        ///
        /// <param name="par">  The parallel value - Column for Horz, Row for Vert. </param>
        /// <param name="perp"> The perpindicular value - Row for Horz, Column for Vert. </param>
        /// <param name="dir">  The direction to orient to. </param>
        ///
        /// <returns>   The new Point. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static Point CreateUndirectional(int par, int perp, Dir dir)
        {
            return dir == Dir.Horiz ? new Point(par, perp) : new Point(perp, par);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Neighbors of a position. </summary>
        ///
        /// <remarks>   These are just the direct horizontal and vertical neighbors of the
        ///             position.  Edges are taken into account so any returned neighbors are
        ///             guaranteed to lie in a rectangle from (0,0) to (width, height)
        ///             
        ///             Darrellp, 8/25/2016. </remarks>
        ///
        /// <param name="width">     Width of the map. </param>
        /// <param name="height">    Height of the map. </param>
        ///
        /// <returns>   An IEnumerable&lt;HVPoint&gt; of neighbors </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static IEnumerable<Point> Neighbors(this Point @this, int width, int height, bool f4Neighbors = true)
        {
            if (@this.X > 0)
            {
                yield return new Point(@this.X - 1, @this.Y);
            }
            if (@this.X < width - 1)
            {
                yield return new Point(@this.X + 1, @this.Y);
            }
            if (@this.Y > 0)
            {
                yield return new Point(@this.X, @this.Y - 1);
            }
            if (@this.Y < height - 1)
            {
                yield return new Point(@this.X, @this.Y + 1);
            }

            if (f4Neighbors)
            {
                yield break;
            }

            if (@this.X > 0 && @this.Y > 0)
            {
                yield return new Point(@this.X - 1, @this.Y - 1);
            }
            if (@this.X < width - 1 && @this.Y > 0)
            {
                yield return new Point(@this.X + 1, @this.Y - 1);
            }
            if (@this.X < width - 1 && @this.Y < height - 1)
            {
                yield return new Point(@this.X + 1, @this.Y + 1);
            }
            if (@this.X > 0 && @this.Y < height - 1)
            {
                yield return new Point(@this.X - 1, @this.Y + 1);
            }

        }
    }
}
