using GoRogue.Random;
// ReSharper disable IdentifierTypo

namespace Roguish.Map_Generation;

internal enum Wall
{
    Left,
    Right,
    Top,
    Bottom
}


internal class RectangularRoom
{
    public Rectangle Rect;
    public Point SuperGridCell;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Gets the top row. </summary>
    ///
    /// <value>	The top row. </value>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    internal int Top => Rect.Y;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Gets the left column. </summary>
    ///
    /// <value>	The left column. </value>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    internal int Left => Rect.X;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Gets the bottom row. </summary>
    ///
    /// <value>	The bottom row. </value>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    internal int Bottom => Rect.Y + Rect.Height - 1;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Gets the right column. </summary>
    ///
    /// <value>	The right column. </value>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    internal int Right => Rect.X + Rect.Width - 1;

    internal Point Location => Rect.Position;

    internal int Size(Dir dir)
    {
        return dir == Dir.Horiz ? Rect.Width : Rect.Height;
    }

    internal static RectangularRoom CreateUndirectional(Point location, int sizeInDir, int sizeInOtherDir, int superGridCoordInDir, int superGridCoordInOtherDir, Dir dir)
    {
        Point superGridCell;
        Rectangle rc;

        if (dir == Dir.Vert)
        {
            rc = new Rectangle(location.X, location.Y, sizeInOtherDir, sizeInDir);
            superGridCell = new Point(superGridCoordInOtherDir, superGridCoordInDir);
        }
        else
        {
            rc = new Rectangle(location.X, location.Y, sizeInDir, sizeInOtherDir);
            superGridCell = new Point(superGridCoordInDir, superGridCoordInOtherDir);
        }
        return new RectangularRoom() { Rect = rc, SuperGridCell = superGridCell };
    }

    public Area ToArea()
    {
        return new Area { Rect.Positions() };
    }

    public int SmallCoord(Dir dir)
    {
        return dir == Dir.Horiz ? Left : Top;
    }

    public int LargeCoord(Dir dir)
    {
        return dir == Dir.Horiz ? Right : Bottom;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Pick a spot on one of the walls of the room. </summary>
    ///
    /// <remarks>	
    /// This is a spot ON the wall - not outside it.  It thus lies within the confines of the room.
    /// Darrellp, 9/20/2011. 
    /// </remarks>
    ///
    /// <param name="wall">				The wall. </param>
    /// <param name="fIncludeCorners">	true to include corners in our consideration for a spot. </param>
    ///
    /// <returns>	Coordinates of the spot chosen. </returns>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    internal Point PickSpotOnWall(Wall wall, bool fIncludeCorners = false)
    {
        var rng = GlobalRandom.DefaultRNG;

        // Locals
        int column, row;

        // Our values get bumped to avoid corners if need be
        int cornerBump = fIncludeCorners ? 0 : 1;

        // If we're at the top or bottom
        if (wall == Wall.Top || wall == Wall.Bottom)
        {
            // Pick a random column
            int left = Left + cornerBump;
            int right = Right - cornerBump;
            column = rng.NextInt(left, right + 1);

            // Set row to top or bottom
            row = wall == Wall.Top ? Top : Bottom;
        }
        else
        {
            // Pick a random row
            int top = Top + cornerBump;
            int bottom = Bottom - cornerBump;
            row = rng.NextInt(top, bottom + 1);

            // Set column to left or right
            column = wall == Wall.Left ? Left : Right;
        }

        // Return our new spot
        return new Point(column, row);
    }
}