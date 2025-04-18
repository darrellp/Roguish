using GoRogue.MapGeneration;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Roguish.Map_Generation;

// ReSharper disable once InconsistentNaming
internal class RoomGenDAP : GenerationStep
{
    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;
    public int MinRoomHeight = 5;
    public int MinRoomWidth = 5;
    private readonly string? _rectRoomsComponentTag;
    public int SuperCellHeight = 15;
    public int SuperCellWidth = 15;
    private readonly string? _wallFloorComponentTag;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RoomGenDAP(
        string? name = null, 
        string? wallFloorComponentTag = "WallFloor",
        string? rectRoomsComponentTag = "RectRooms")
        : base(name)
    {
        _wallFloorComponentTag = wallFloorComponentTag;
        _rectRoomsComponentTag = rectRoomsComponentTag;
    }

    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        // Validate Configuration
        if (SuperCellWidth <= 0)
            throw new InvalidConfigurationException(this, nameof(SuperCellWidth),
                "The value must be greater than zero.");
        if (SuperCellHeight <= 0)
            throw new InvalidConfigurationException(this, nameof(SuperCellHeight),
                "The value must be greater than zero.");
        if (MinRoomWidth <= 0)
            throw new InvalidConfigurationException(this, nameof(MinRoomWidth),
                "The value must be greater than zero.");
        if (MinRoomHeight <= 0)
            throw new InvalidConfigurationException(this, nameof(MinRoomHeight),
                "The value must be greater than zero.");

        var wallFloor = new ArrayView<bool>(context.Width, context.Height);
        var rooms = LocateRooms(context, wallFloor);


        // Get or create/add a wall-floor context component
        context.GetFirstOrNew<ISettableGridView<bool>>(
            () => wallFloor,
            _wallFloorComponentTag
        );

        context.GetFirstOrNew(
            () => rooms,
            _rectRoomsComponentTag
        );

        yield return null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Locates the rooms on the map. </summary>
    /// <remarks>	Darrellp, 9/19/2011. </remarks>
    /// <param name="context">Map info</param>
    /// <param name="wallFloor">GridView to draw floor on</param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    private RectangularRoom[][] LocateRooms(GenerationContext context, ArrayView<bool> wallFloor)
    {
        // Max number of cells we can fit with at least base cell size
        var gridWidth = context.Width / SuperCellWidth;
        var gridHeight = context.Height / SuperCellHeight;

        // Size of cells we can manage total
        var baseRoomWidth = context.Width / gridWidth;
        var baseRoomHeight = context.Height / gridHeight;

        // Remainders for Bresenham algorithm
        var widthRemainder = context.Width % baseRoomWidth;
        var heightRemainder = context.Height % baseRoomHeight;

        // Tally for Bresenham
        var widthTally = gridWidth / 2;
        var heightTally = gridHeight / 2;

        // First column is on the left
        var gridColumn = 0;

        // Array of rooms in the grid
        var rooms = new RectangularRoom[gridWidth][];

        // For each grid column
        for (var superGridColumn = 0; superGridColumn < gridWidth; superGridColumn++)
        {
            // Reset the map row to 0
            var gridRow = 0;

            // Determine the current map column
            var currentWidth = baseRoomWidth;
            widthTally += widthRemainder;

            // Do we need to bump width ala Bresenham?
            if (widthTally >= gridWidth)
            {
                // Bump width
                currentWidth++;
                widthTally -= gridWidth;
            }

            // Create the row of rooms for this column
            rooms[superGridColumn] = new RectangularRoom[gridHeight];

            // For each row of the grid
            for (var superGridRow = 0; superGridRow < gridHeight; superGridRow++)
            {
                // Determine the current map row
                var currentHeight = baseRoomHeight;
                heightTally += heightRemainder;

                // Do we need to bump height ala Bresenham?
                if (heightTally >= gridHeight)
                {
                    // Bump height
                    currentHeight++;
                    heightTally -= gridHeight;
                }

                // Create a room in the grid cell
                var superGridLocation = new Point(superGridColumn, superGridRow);

                // Grid ocation of the Upper Left of the SuperGrid Cell that we'll create room in.
                var gridLocation = new Point(gridColumn, gridRow);

                // currentWidth and currentHeight here represent the width/height of the supergrid cell - not the room.
                var room = CreateRoomInCell(superGridLocation, gridLocation, currentWidth, currentHeight);
                var rect = room.Rect;
                rect = rect.WithSize(rect.Width - 2, rect.Height - 2)
                    .WithPosition(rect.Position + new Point(1, 1));
                foreach (var tileGridPos in rect.Positions()) wallFloor[tileGridPos] = true;

                // Place it in the grid
                rooms[superGridColumn][superGridRow] = room;

                // Advance the map row
                gridRow += currentHeight;
            }

            // Advance the map column
            gridColumn += currentWidth;
        }

        return rooms;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Creates a room in a grid cell. </summary>
    /// <remarks>	Darrellp, 9/20/2011. </remarks>
    /// <param name="superGridLocation">	The grid location of the cell. </param>
    /// <param name="gridLocation">	The cell location in the map. </param>
    /// <param name="cellWidth">	Width of the current cell. </param>
    /// <param name="cellHeight">	Height of the current cell. </param>
    /// <returns>	The newly created room. </returns>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    private RectangularRoom CreateRoomInCell(Point superGridLocation, Point gridLocation, int cellWidth, int cellHeight)
    {
        // Locals

        // Determine start and end columns
        RandomSpan(gridLocation.X + 1, gridLocation.X + cellWidth - 2, MinRoomWidth, out var startColumn,
            out var endColumn);

        // Determine start and end rows
        RandomSpan(gridLocation.Y + 1, gridLocation.Y + cellHeight - 2, MinRoomHeight, out var startRow,
            out var endRow);
        var rc = new Rectangle(startColumn, startRow, endColumn - startColumn + 1, endRow - startRow + 1);

        // Return newly created room
        var room = new RectangularRoom
        {
            Rect = rc,
            SuperGridCell = superGridLocation
        };

        return room;
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Create a random interval of length at least minWidth between start and end. </summary>
    /// <remarks>
    ///     The range for the interval is start through end inclusive.  The difference between the return
    ///     values will be at least minWidth, again inclusive. Darrellp, 9/18/2011.
    /// </remarks>
    /// <param name="start">		The start value of the range. </param>
    /// <param name="end">			The end value of the range. </param>
    /// <param name="minWidth">		Minimum width of the returned interval. </param>
    /// <param name="spanStart">	[out] The span start. </param>
    /// <param name="spanEnd">		[out] The span end. </param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    private void RandomSpan(int start, int end, int minWidth, out int spanStart, out int spanEnd)
    {
        if (minWidth > end - start)
            throw new InvalidConfigurationException(this, nameof(SuperCellWidth),
                "Width isn't wide enough to make span in RandomSpan");
        var val1 = _rng.NextInt(start, end + 1);
        var val2 = _rng.NextInt(start, end + 1);

        while (Math.Abs(val1 - val2) < minWidth)
        {
            val1 = _rng.NextInt(start, end + 1);
            val2 = _rng.NextInt(start, end + 1);
        }

        if (val1 < val2)
        {
            spanStart = val1;
            spanEnd = val2;
        }
        else
        {
            spanStart = val2;
            spanEnd = val1;
        }
    }
}