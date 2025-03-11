using GoRogue.MapGeneration;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
// ReSharper disable IdentifierTypo

namespace Roguish.Map_Generation
{
    // ReSharper disable once InconsistentNaming
    public class RoomConnectDAP : GenerationStep
    {
        public string? WallFloorComponentTag;
        public string? RectRoomsComponentTag;
        public string? TunnelsComponentTag;
        public string? AreasComponentTag;
        public string? WallsComponentTag;

        public int PctMergeChance = 50;
        private GridConnections? _connections;
        private GridConnections? _merges;

        private ISettableGridView<bool>? _wallFloor;
        private ISettableGridView<bool>? _walls;
        RectangularRoom[][] _rooms = [];
        private Dictionary<RectangularRoom, Area> _roomAreas;

        private int SuperGridWidth => _rooms.Length;
        private int SuperGridHeight => _rooms[0].Length;

        // ReSharper disable once InconsistentNaming
        public IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

        public RoomConnectDAP(string? name = null, string? wallFloorComponentTag = "WallFloor",
            string? tunnelsComponentTag = "Tunnels",
            string? rectRoomsComponentTag = "RectRooms",
            string? areasComponentTag = "Areas",
            string? wallsComponentTag = "Walls")
            : base(name,
                (typeof(IGridView<bool>), wallFloorComponentTag),
                (typeof(RectangularRoom[][]), rectRoomsComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            RectRoomsComponentTag = rectRoomsComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
            AreasComponentTag = areasComponentTag;
            WallsComponentTag = wallsComponentTag;

            _roomAreas = new Dictionary<RectangularRoom, Area>();
        }

        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get or create/add the various context components
            _wallFloor = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);
            _rooms = context.GetFirst<RectangularRoom[][]>(RectRoomsComponentTag);
            _roomAreas = new Dictionary<RectangularRoom, Area>();

            // Create our connections objects
            _connections = new GridConnections(SuperGridWidth, SuperGridHeight);
            _merges = new GridConnections(SuperGridWidth, SuperGridHeight);
            _walls = new ArrayView<bool>(context.Width, context.Height);


            foreach (var room in _rooms.SelectMany(roomRow => roomRow))
            {
                // Excavate the room
                _roomAreas[room] = room.ToArea();
            }


            // Determine which rooms should be connected
            DetermineRoomConnections();

            // Determine which rooms should be merged and resize those rooms
            DetermineRoomMerges();

            // For each room, excavate it
            foreach (var room in _rooms.SelectMany(roomRow => roomRow))
            {
                // Excavate the room
                ExcavateRoom(room);
            }

            // Excavate between the rooms
            ExcavateRoomConnections();

            // Do any cleanup
            PostProcess(context);

            // Get or create/add a walls context component
            context.GetFirstOrNew(
                () => _walls,
                WallsComponentTag
            );

            var areas = new HashSet<Area>(_roomAreas.Values).ToArray();
            context.GetFirstOrNew(
                () => areas,
                AreasComponentTag
            );

            yield return null;
        }

        private void PostProcess(GenerationContext context)
        {
            CompleteWalls(context);
        }

        private void CompleteWalls(GenerationContext context)
        {
            for (var iRow = 0; iRow < context.Height; iRow++)
            {
                // For each Column
                for (var iColumn = 0; iColumn < context.Width; iColumn++)
                {
                    // Is there a no floor here?
                    if (!_wallFloor![iColumn, iRow])
                    {
                        continue;
                    }

                    var pos = new Point(iColumn, iRow);
                    // Get the neighboring off map locations
                    var offmapLocations = pos.Neighbors(context.Width, context.Height, false)
                        .Where(p => !_wallFloor[p]);

                    // For each neighboring off map location
                    foreach (var location in offmapLocations)
                    {
                        // Turn it into stone wall
                        _walls![location] = true;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate all corridors on the map. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateRoomConnections()
        {
            // For every connection
            foreach (var info in _connections!.Connections.Where(ci => ci.IsConnected))
            {
                // Get the pertinent information
                var location1 = info.Location;
                var location2 = info.Location.NextLarger(info.Dir);
                var room1 = RoomAt(location1)!;
                var room2 = RoomAt(location2)!;

                // Are the rooms to be merged?
                if (_merges!.IsConnected(location1, location2))
                {
                    // merge them
                    ExcavateMerge(room1, room2, info.Dir);
                }
                else
                {
                    // Build a corridor between them
                    ExcavateCorridor(room1, room2, info.Dir);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate a merge between two rooms. </summary>
        ///
        /// <remarks>	
        /// Names are named as though dir was vertical and dirOther horizontal. Darrellp, 9/22/2011. 
        /// </remarks>
        ///
        /// <param name="topRoom">		The top room. </param>
        /// <param name="bottomRoom">	The bottom room. </param>
        /// <param name="dir">			The dir. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateMerge(RectangularRoom topRoom, RectangularRoom bottomRoom, Dir dir)
        {
            // Get the opposite direction
            var dirOther = PointExt.OtherDirection(dir);

            // Get the appropriate coordinates
            var topRoomsLeft = topRoom.Location.Get(dirOther);
            var topRoomsRight = topRoomsLeft + topRoom.Size(dirOther) - 1;
            var bottomRoomsLeft = bottomRoom.Location.Get(dirOther);
            var bottomRoomsRight = bottomRoomsLeft + bottomRoom.Size(dirOther) - 1;

            // Get the high and low points of the overlap
            var overlapLeft = Math.Max(topRoomsLeft, bottomRoomsLeft) + 1;
            var overlapRight = Math.Min(topRoomsRight, bottomRoomsRight) - 1;

            // Create our new merged generic room
            var groomTop = _roomAreas[topRoom];
            var groomBottom = _roomAreas[bottomRoom];
            groomTop = Area.GetUnion(groomTop, groomBottom);
            _roomAreas[topRoom] = groomTop;
            _roomAreas[bottomRoom] = groomTop;

            // Get the location we're going to start the clearing at
            var topRoomsBottom = topRoom.Location.Get(dir) + topRoom.Size(dir) - 1;
            var currentLocation = PointExt.CreateUndirectional(topRoomsBottom, overlapLeft, dir);

            // For each spot along the overlap
            for (var iCol = overlapLeft; iCol <= overlapRight; iCol++)
            {
                // Clear out the two walls of the abutting rooms
                currentLocation = currentLocation.Set(dirOther, iCol);
                _wallFloor![currentLocation] = true;
                currentLocation = currentLocation.Set(dir, topRoomsBottom + 1);
                _wallFloor![currentLocation] = true;
                currentLocation = currentLocation.Set(dir, topRoomsBottom);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavates between two rooms. </summary>
        ///
        /// <remarks>	Variables named from the perspective of dir being vertical.  Darrellp, 9/18/2011. </remarks>
        ///
        /// <param name="roomTop">		The first room. </param>
        /// <param name="roomBottom">	The second room. </param>
        /// <param name="dir">			The direction to excavate in. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateCorridor(RectangularRoom roomTop, RectangularRoom roomBottom, Dir dir)
        {
            // Get the entrances to each room
            GetEntrances(roomTop, roomBottom, dir, out var topEntrance, out var bottomEntrance);

            // Allocate the generic room
            Area corridorArea = new Area();

            // Excavate a connection between the two rooms
            CreateBend(dir, topEntrance, bottomEntrance, corridorArea);

            // Put the exits in the appropriate generic rooms
            //Area groomTop = _roomAreas[roomTop];
            //Area groomBottom = _roomAreas[roomBottom];


            //// Should we put a door in the top room?
            //if (_rng.NextInt(100) < _pctDoorChance)
            //{
            //    // Place the door
            //    map[topEntrance].Terrain = TerrainType.Door;
            //}

            //// Should we put a door in the bottom room?
            //if (_rng.NextInt(100) < _pctDoorChance)
            //{
            //    // Place the door
            //    map[bottomEntrance].Terrain = TerrainType.Door;
            //}
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate between connected rooms in a grid connection. </summary>
        ///
        /// <remarks>	Variables named from the perspective that dir is vertical Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="dir">				The direction the merge will take place in. </param>
        /// <param name="topEntrance">		The small coordinate entrance. </param>
        /// <param name="bottomEntrance">	The large coordinate entrance. </param>
        /// <param name="groom">			The room being prepared for this corridor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void CreateBend(Dir dir, Point topEntrance, Point bottomEntrance, Area groom)
        {
            // locals
            var otherDir = PointExt.OtherDirection(dir);
            var startRow = topEntrance.Get(dir);
            var endRow = bottomEntrance.Get(dir);
            var startColumn = topEntrance.Get(otherDir);
            var endColumn = bottomEntrance.Get(otherDir);

            // Determine bend location
            var bendRow = _rng.NextInt(startRow + 1, endRow);

            // Excavate the bend between the two rooms
            ExcavateBend(startColumn, endColumn, startRow, endRow, bendRow, groom, dir);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate bend. </summary>
        ///
        /// <remarks>	
        /// "Perpindicular" here refers to the coordinate perpindicular to the orientation of the two
        /// rooms.  If the rooms are oriented vertically then perpindicular refers to the horizontal (x)
        /// coordinate wo that startPerpindicular is the starting column. If they're oriented vertically,
        /// startPerpindicular is the starting row.  Parallel refers to the coordinate parallel to the
        /// orientation.  Bend is always in the perpindicular coordinate. Unidirectional but named as
        /// though dir was vertical.  Darrellp, 9/18/2011. 
        /// </remarks>
        ///
        /// <param name="startColumn">	The start perpindicular. </param>
        /// <param name="endColumn">	The end perpindicular. </param>
        /// <param name="startRow">		The start parallel. </param>
        /// <param name="endRow">		The end parallel. </param>
        /// <param name="bend">			The bend coordinate. </param>
        /// <param name="groom">		The room being prepared for this corridor. </param>
        /// <param name="dir">			The direction the bend is supposed to run. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateBend(int startColumn, int endColumn, int startRow, int endRow, int bend, Area groom, Dir dir)
        {
            var otherDir = PointExt.OtherDirection(dir);

            // Create corridor to the bend
            ExcavateCorridorRun(startColumn, startRow, bend, groom, dir);

            // Create the cross corridor at the bend
            ExcavateCorridorRun(bend, startColumn, endColumn, groom, otherDir);

            // Create the corridor from the bend to the destination
            ExcavateCorridorRun(endColumn, bend, endRow, groom, dir);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate a straight corridor run either vertically or horizontally. </summary>
        ///
        /// <remarks>	
        /// Excavates from (startParallel, perpindicular) to (endParallel, perpindicular) inclusive if
        /// fVertical.  If not fVertical, swap coordinates.  start and end parallel coordinates do not
        /// have to be in numerical order.  This is a unidirectional function but, as usual, names are
        /// named as though dir was vertical.  Darrellp, 9/19/2011. 
        /// </remarks>
        ///
        /// <param name="column">	The perpindicular coordinate. </param>
        /// <param name="endRow1">	The starting parallel coordinate. </param>
        /// <param name="endRow2">	The ending parallel coordinate. </param>
        /// <param name="groom">	The room being prepared for this corridor. </param>
        /// <param name="dir">		The direction of the corridor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateCorridorRun(int column, int endRow1, int endRow2, Area groom, Dir dir)
        {
            // We work with small and large coords rather than start and end
            var startRow = Math.Min(endRow1, endRow2);
            var endRow = Math.Max(endRow1, endRow2);

            // Create the starting location
            var currentLocation = PointExt.CreateUndirectional(startRow, column, dir);

            // For each row in the run
            for (var iRow = startRow; iRow <= endRow; iRow++)
            {
                // Place our terrain
                currentLocation = currentLocation.Set(dir, iRow);
                _wallFloor![currentLocation] = true;
                groom.Add(currentLocation);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Determine entrances for two connected rooms in a grid connection. </summary>
        ///
        /// <remarks>	Variables named from the perspective that dir is vertical.  Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="room1">			The first room. </param>
        /// <param name="room2">			The second room. </param>
        /// <param name="dir">				The direction we're excavating. </param>
        /// <param name="topEntrance">		[out] The small coordinate entrance. </param>
        /// <param name="bottomEntrance">	[out] The large coordinate entrance. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void GetEntrances(
            RectangularRoom room1,
            RectangularRoom room2,
            Dir dir,
            out Point topEntrance,
            out Point bottomEntrance)
        {
            // Determine room order in the orientation direction
            RectangularRoom topRoom = room1;
            RectangularRoom bottomRoom = room2;
            int iGrid1 = room1.SuperGridCell.Get(dir);
            int iGrid2 = room2.SuperGridCell.Get(dir);

            // Is the coordinate for room 1 less than room 2?
            if (iGrid1 > iGrid2)
            {
                // Set large coordinate to room1, small to room2
                bottomRoom = room1;
                topRoom = room2;
            }

            // Determine entrances for each room
            topEntrance = topRoom.PickSpotOnWall(dir == Dir.Vert ? Wall.Bottom : Wall.Right);
            bottomEntrance = bottomRoom.PickSpotOnWall(dir == Dir.Vert ? Wall.Top : Wall.Left);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Excavate a room, putting walls on the border. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="room">	The room to carve out of the map. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ExcavateRoom(RectangularRoom room)
        {
            // For each column in the room
            for (var iX = room.Left + 1; iX < room.Right; iX++)
            {
                // For each row in the room
                for (var iY = room.Top + 1; iY < room.Bottom; iY++)
                {
                    // Place the appropriate terrain
                    _wallFloor![iX, iY] = true;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Connects the rooms into a network. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void DetermineRoomConnections()
        {
            // Connect all the grid cells
            _connections!.ConnectCells();

            // Add some random connections
            _connections!.AddRandomConnections(Math.Min(_rooms.Length, _rooms[0].Length), _connections);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Merge rooms. </summary>
        ///
        /// <remarks>	
        /// This doesn't actually excavate the two rooms - it just resizes them so that they will merge
        /// properly and marks them down to be merged in the merge grid structure.  Darrellp, 9/25/2011. 
        /// </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void DetermineRoomMerges()
        {
            // For each connection
            foreach (var connectionInfo in _connections!.Connections.Where(ci => ci.IsConnected))
            {
                // Is it a connection that we wish to merge?
                if (_rng.NextInt(100) < PctMergeChance)
                {
                    // Retrieve it's location
                    var superGridLocation = connectionInfo.Location;

                    // and merge the rooms
                    MergeTwoRooms(superGridLocation, connectionInfo.Dir);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Checks whether a grid location is within the bounds of our grid. </summary>
        ///
        /// <remarks>	Darrellp, 9/30/2011. </remarks>
        ///
        /// <param name="superGridLocation">	The grid location. </param>
        ///
        /// <returns>	true if it's in bounds, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool WithinGrid(Point superGridLocation)
        {
            var column = superGridLocation.X;
            var row = superGridLocation.Y;
            return column >= 0 && column < SuperGridWidth &&
                   row >= 0 && row < SuperGridHeight;
        }


        RectangularRoom? RoomAt(Point location)
        {
            return WithinGrid(location) ? _rooms[location.X][location.Y] : null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Merge two rooms. </summary>
        ///
        /// <remarks>	Named as though dir was vertical.  Darrellp, 9/22/2011. </remarks>
        ///
        /// <param name="topRoomsGridLocation">	The location of the top room in the grid. </param>
        /// <param name="dir">					The direction of the merge. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void MergeTwoRooms(Point topRoomsGridLocation, Dir dir)
        {
            // Get grid coordinates of the other room
            var bottomRoomsGridLocation = topRoomsGridLocation.NextLarger(dir);
            var otherDir = PointExt.OtherDirection(dir);

            // Retrieve both rooms
            var topRoom = RoomAt(topRoomsGridLocation)!;
            var bottomRoom = RoomAt(bottomRoomsGridLocation)!;

            // Are the rooms unmergable?
            if (!CheckMergeForOverlap(topRoom, bottomRoom, otherDir))
                // Return and don't merge them - use normal corridors
                return;

            // Remove their generic counterparts
            //_mapRoomToGenericRooms.Remove(topRoom);
            //_mapRoomToGenericRooms.Remove(bottomRoom);

            // Get their current coordinates, etc.
            var topRoomsBottom = topRoom.LargeCoord(dir);
            var topRoomsTop = topRoom.SmallCoord(dir);
            var bottomRoomsTop = bottomRoom.SmallCoord(dir);
            var bottomRoomsHeight = bottomRoom.Size(dir);
            var topRoomsWidth = topRoom.Size(otherDir);
            var bottomRoomsWidth = bottomRoom.Size(otherDir);

            // Pick a random spot between the rooms to merge them
            // This will be the new inside coord of the small coordinate room
            var mergeRow = _rng.NextInt(topRoomsBottom, bottomRoomsTop);

            // Determine all the new coordinates
            var topRoomsNewHeight = mergeRow - topRoomsTop + 1;
            var bottomRoomsNewHeight = bottomRoomsTop - mergeRow + bottomRoomsHeight - 1;
            var bottomRoomsLocation = bottomRoom.Location;
            bottomRoomsLocation = bottomRoomsLocation.Set(dir, mergeRow + 1);

            // Create our new expanded rooms
            var roomTopNew = RectangularRoom.CreateUndirectional(
                topRoom.Location,
                topRoomsNewHeight,
                topRoomsWidth,
                topRoomsGridLocation.Get(dir),
                topRoomsGridLocation.Get(otherDir),
                dir);
            var roomBottomNew = RectangularRoom.CreateUndirectional(
                bottomRoomsLocation,
                bottomRoomsNewHeight,
                bottomRoomsWidth,
                bottomRoomsGridLocation.Get(dir),
                bottomRoomsGridLocation.Get(otherDir),
                dir);

            // Install the new rooms
            SetRoomAt(topRoomsGridLocation, roomTopNew);
            SetRoomAt(bottomRoomsGridLocation, roomBottomNew);

            // Create the new generic rooms
            // We don't create the single merged generic room until we excavate because we don't know
            // the layout until then.
            _roomAreas[roomTopNew] = roomTopNew.ToArea();
            _roomAreas[roomBottomNew] = roomBottomNew.ToArea();

            // Mark this in our merges structure
            _merges!.Connect(topRoomsGridLocation, bottomRoomsGridLocation);
        }

        private void SetRoomAt(Point superGridLocation, RectangularRoom room)
        {
            _rooms[superGridLocation.X][superGridLocation.Y] = room;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Check a merge for overlap between the merging rooms. </summary>
        ///
        /// <remarks>	
        /// Names are named as though dir was vertical and dirOther horizontal. Darrellp, 9/25/2011. 
        /// </remarks>
        ///
        /// <param name="topRoom">		The top room. </param>
        /// <param name="bottomRoom">	The bottom room. </param>
        /// <param name="dirOther">		The dir other. </param>
        ///
        /// <returns>	true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool CheckMergeForOverlap(RectangularRoom topRoom, RectangularRoom bottomRoom, Dir dirOther)
        {
            // Get our neighboring rooms in the dirOther direction
            var topLeft = RoomAt(topRoom.SuperGridCell.NextSmaller(dirOther));
            var topRight = RoomAt(topRoom.SuperGridCell.NextLarger(dirOther));
            var bottomLeft = RoomAt(bottomRoom.SuperGridCell.NextSmaller(dirOther));
            var bottomRight = RoomAt(bottomRoom.SuperGridCell.NextLarger(dirOther));

            // Ensure that we overlap with our merge target and not with anything else
            return CheckOverlap(topRoom, bottomRoom, dirOther) &&
                   !CheckOverlap(topLeft, bottomRoom, dirOther) &&
                   !CheckOverlap(topRight, bottomRoom, dirOther) &&
                   !CheckOverlap(topRoom, bottomLeft, dirOther) &&
                   !CheckOverlap(topRoom, bottomRight, dirOther);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Check overlap between two rooms. </summary>
        ///
        /// <remarks>	Darrellp, 9/30/2011. </remarks>
        ///
        /// <param name="topRoom">		The top room. </param>
        /// <param name="bottomRoom">	The bottom room. </param>
        /// <param name="dirOther">		The direction to check overlap along. </param>
        ///
        /// <returns>	true if they overlap, false if they don't. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool CheckOverlap(RectangularRoom? topRoom, RectangularRoom? bottomRoom, Dir dirOther)
        {
            if (topRoom == null || bottomRoom == null)
            {
                return false;
            }

            // Get the appropriate coordinates
            var topRoomsLeft = topRoom.Location.Get(dirOther);
            var topRoomsRight = topRoomsLeft + topRoom.Size(dirOther) - 1;
            var bottomRoomsLeft = bottomRoom.Location.Get(dirOther);
            var bottomRoomsRight = bottomRoomsLeft + bottomRoom.Size(dirOther) - 1;

            // Get the high and low points of the overlap
            var overlapLeft = Math.Max(topRoomsLeft, bottomRoomsLeft) + 1;
            var overlapRight = Math.Min(topRoomsRight, bottomRoomsRight) - 1;

            // return true if they overlap
            return overlapLeft < overlapRight;
        }
    }
}
