using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Roguish
{
    class RectangularRoom
    {
        public Rectangle Position;
        public Point SuperGridCell;
    }

    internal class RoomGenDAP : GenerationStep
    {
        public readonly int SuperCellWidth = 20;
        public readonly int SuperCellHeight = 10;
        public readonly int MinRoomWidth = 5;
        public readonly int MinRoomHeight = 5;
        public readonly int PctMergeChance = 50;
        public readonly int PctDoorChance = 50;
        public readonly string? WallFloorComponentTag;
        public readonly string? RectRoomsComponentTag;


        public IEnhancedRandom RNG = GlobalRandom.DefaultRNG;

        public RoomGenDAP(string? name = null, string? wallFloorComponentTag = "WallFloor", string? rectRoomsComponentTag = "RectRooms")
            : base(name)
        {
            WallFloorComponentTag = wallFloorComponentTag;
            RectRoomsComponentTag = rectRoomsComponentTag;
        }

        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Validate Configuration
            if (SuperCellWidth <= 0)
                throw new InvalidConfigurationException(this, nameof(SuperCellWidth),
                    $"The value must be greater than zero.");
            if (SuperCellHeight <= 0)
                throw new InvalidConfigurationException(this, nameof(SuperCellHeight),
                    $"The value must be greater than zero.");
            if (MinRoomWidth <= 0)
                throw new InvalidConfigurationException(this, nameof(MinRoomWidth),
                    $"The value must be greater than zero.");
            if (MinRoomHeight <= 0)
                throw new InvalidConfigurationException(this, nameof(MinRoomHeight),
                    $"The value must be greater than zero.");
            if (PctMergeChance <= 0)
                throw new InvalidConfigurationException(this, nameof(PctMergeChance),
                    $"The value must be greater than zero.");
            if (PctDoorChance <= 0)
                throw new InvalidConfigurationException(this, nameof(PctDoorChance),
                    $"The value must be greater than zero.");
            if (PctMergeChance >= 100)
                throw new InvalidConfigurationException(this, nameof(PctMergeChance),
                    $"The value must be less than 100.");
            if (PctDoorChance >= 100)
                throw new InvalidConfigurationException(this, nameof(PctDoorChance),
                    $"The value must be less than 100.");

            var wallFloor = new ArrayView<bool>(context.Width, context.Height);
            var rooms = LocateRooms(context, wallFloor);


            // Get or create/add a wall-floor context component
            // var wallFloorContext = context.GetFirstOrNew<ISettableGridView<bool>>(
            context.GetFirstOrNew<ISettableGridView<bool>>(
                () => wallFloor,
                WallFloorComponentTag
            );

            // var roomsContext = context.GetFirstOrNew(
            context.GetFirstOrNew(
                () => rooms,
                RectRoomsComponentTag
            );

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Locates the rooms on the map. </summary>
        /// 
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        /// <param name="context"></param>
        /// <param name="wallFloor"></param>
        /// <param name="map">	The map to be excavated. </param>
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
                    int currentHeight = baseRoomHeight;
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

                    foreach (var tileGridPos in room.Position.Positions())
                    {
                        wallFloor[tileGridPos] = true;
                    }

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
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ///
        /// <param name="superGridLocation">	The grid location of the cell. </param>
        /// <param name="gridLocation">	The cell location in the map. </param>
        /// <param name="cellWidth">	Width of the current cell. </param>
        /// <param name="cellHeight">	Height of the current cell. </param>
        ///
        /// <returns>	The newly created room. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private RectangularRoom CreateRoomInCell(Point superGridLocation, Point gridLocation, int cellWidth, int cellHeight)
        {
            // Locals
            int startRow, startColumn, endRow, endColumn;

            // Determine start and end columns
            RandomSpan(gridLocation.X + 1, gridLocation.X + cellWidth - 2, MinRoomWidth, out startColumn, out endColumn);

            // Determine start and end rows
            RandomSpan(gridLocation.Y + 1, gridLocation.Y + cellHeight - 2, MinRoomHeight, out startRow, out endRow);
            var mapLocation = new Point(startColumn, startRow);
            var rc = new Rectangle(startColumn, startRow, endColumn - startColumn + 1, endRow - startRow + 1);

            // Return newly created room
            RectangularRoom room = new RectangularRoom()
            {
                Position = rc,
                SuperGridCell = superGridLocation,
            };

            return room;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Create a random interval of length at least minWidth between start and end. </summary>
        ///
        /// <remarks>	
        /// The range for the interval is start through end inclusive.  The difference between the return
        /// values will be at least minWidth, again inclusive. Darrellp, 9/18/2011. 
        /// </remarks>
        ///
        /// <param name="start">		The start value of the range. </param>
        /// <param name="end">			The end value of the range. </param>
        /// <param name="minWidth">		Minimum width of the returned interval. </param>
        /// <param name="spanStart">	[out] The span start. </param>
        /// <param name="spanEnd">		[out] The span end. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void RandomSpan(int start, int end, int minWidth, out int spanStart, out int spanEnd)
        {
            if (minWidth > end - start)
            {
                throw new InvalidConfigurationException(this, nameof(SuperCellWidth),
                    "Width isn't wide enough to make span in RandomSpan");
            }
            var val1 = RNG.NextInt(start, end + 1);
            var val2 = RNG.NextInt(start, end + 1);

            while (Math.Abs(val1 - val2) < minWidth)
            {
                val1 = RNG.NextInt(start, end + 1);
                val2 = RNG.NextInt(start, end + 1);
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
}
