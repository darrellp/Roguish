using GoRogue.Random;
using ShaiRandom.Generators;

namespace Roguish.Map_Generation
{
    internal class GridConnections
    {
        private readonly bool[,] _connectsRight;
        private readonly bool[,] _connectsDown;
        private int _freeConnectionCount;
        private readonly int _superGridWidth;
        private readonly int _superGridHeight;

        // ReSharper disable once InconsistentNaming
        private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

        #region Internal Properties
        internal Point FirstRoomConnected { get; private set; }
        internal Point LastRoomConnected { get; private set; }
        #endregion



        #region Constructor
        internal GridConnections(int superGridWidth, int superGridHeight)
        {
            _superGridWidth = superGridWidth;
            _superGridHeight = superGridHeight;
            _connectsRight = new bool[superGridWidth - 1, superGridHeight];
            _connectsDown = new bool[superGridWidth, superGridHeight - 1];
            _freeConnectionCount = (superGridWidth - 1) * superGridHeight + superGridWidth * (superGridHeight - 1);
        }
        #endregion


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Ensure that all rooms are connected. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void ConnectCells()
        {
            // locals
            var unconnectedCellList = new List<Point>();
            var isConnected = new bool[_superGridWidth, _superGridHeight];

            // Initialize to all cleared connections
            ClearConnections();

            // For each column in grid
            for (var iColumn = 0; iColumn < _superGridWidth; iColumn++)
            {
                // For each row in grid
                for (var iRow = 0; iRow < _superGridHeight; iRow++)
                {
                    // Mark the grid cell as unconnected
                    unconnectedCellList.Add(new Point(iColumn, iRow));
                }
            }

            // Allocate a list of connected cells
            var connectedCells = new List<Point>(unconnectedCellList.Count);

            // Pick out an unconnected cell as the first one to connect
            var cellIndex = _rng.NextInt(unconnectedCellList.Count);
            var connectedCell = unconnectedCellList[cellIndex];
            isConnected[connectedCell.X, connectedCell.Y] = true;

            // Add it to the connected list
            connectedCells.Add(connectedCell);
            FirstRoomConnected = connectedCell;

            // Remove it from the unconnected list
            unconnectedCellList.RemoveAt(cellIndex);

            // While there are unconnected cells
            while (unconnectedCellList.Count != 0)
            {
                // Pick a random connected cell
                var connectedCellIndex = _rng.NextInt(connectedCells.Count);
                connectedCell = connectedCells[connectedCellIndex];

                // Get the totally unconnected neighbors it's not connected with
                var unconnectedNeighbors = UnconnectedNeighbors(connectedCell)
                    .Where(pt => !isConnected[pt.X, pt.Y]).ToList();

                // Are there any such neighbors?
                if (unconnectedNeighbors.Count != 0)
                {
                    // Pick a random such neighbor
                    int neighborIndex = _rng.NextInt(unconnectedNeighbors.Count);
                    Point neighborLocation = unconnectedNeighbors[neighborIndex];
                    LastRoomConnected = neighborLocation;

                    // Connect it to our already connected cell
                    Connect(connectedCell, neighborLocation);

                    // Take the neighbor off the unconnected list
                    unconnectedCellList.Remove(neighborLocation);

                    // Add it to the connected list
                    connectedCells.Add(neighborLocation);

                    // and mark it as connected
                    isConnected[neighborLocation.X, neighborLocation.Y] = true;
                }
                else
                {
                    // Take the connected cell off the connected list
                    // It doesn't do us any good any more since it's a dead end
                    connectedCells.RemoveAt(connectedCellIndex);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Add random connections to the rooms. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ///
        /// <param name="connectionCount">	Number of random connections to add. </param>
        /// <param name="connections">		The room connections. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void AddRandomConnections(int connectionCount, GridConnections connections)
        {
            // For the number of random connections to be added
            for (var iConnection = 0; iConnection < connectionCount; iConnection++)
            {
                // Add a random connection...
                connections.MakeRandomConnection();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Makes a random connection in the grid. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void MakeRandomConnection()
        {
            // Pick a random index for the new connection to be made
            int connectionIndex = _rng.NextInt(_freeConnectionCount);

            foreach (var info in Connections.Where(info => !info.IsConnected && connectionIndex-- == 0))
            {
                info.SetValue(true);
                break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Clears all connections. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void ClearConnections()
        {
            foreach (var info in Connections)
            {
                info.SetValue(false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Return the unconnected neighbors of a position. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="cellLocation">	The cell location whose unconnected neighbors are desired. </param>
        ///
        /// <returns>	The unconnected neighbors as an IEnumerable. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal IEnumerable<Point> UnconnectedNeighbors(Point cellLocation)
        {
            return cellLocation.Neighbors(_superGridWidth, _superGridHeight).Where(crd => !IsConnected(crd, cellLocation));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns information on all the connections in the grid. </summary>
        ///
        /// <value>	ConnectionInfo for each of the locations in the grid </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal IEnumerable<ConnectionInfo> Connections
        {
            get
            {
                // For each row
                for (int iRow = 0; iRow < _superGridHeight - 1; iRow++)
                {
                    // For each column
                    for (int iColumn = 0; iColumn < _superGridWidth; iColumn++)
                    {
                        yield return new ConnectionInfo(this, new Point(iColumn, iRow), Dir.Vert, _connectsDown[iColumn, iRow]);
                    }
                }

                // For each row
                for (int iRow = 0; iRow < _superGridHeight; iRow++)
                {
                    // For each column
                    for (int iColumn = 0; iColumn < _superGridWidth - 1; iColumn++)
                    {
                        yield return new ConnectionInfo(this, new Point(iColumn, iRow), Dir.Horiz, _connectsRight[iColumn, iRow]);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Connects two adjacent locations. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="pt1">	The first coordinate. </param>
        /// <param name="pt2">	The second coordinate. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void Connect(Point pt1, Point pt2)
        {
            // Are they valid neighbors?
            if (CheckCoordinates(ref pt1, ref pt2, out var fVertical))
            {
                // If vertical
                if (fVertical)
                {
                    // Make the vertical connection
                    ConnectVertical(pt1.X, pt1.Y);
                }
                else
                {
                    // Make the horizontal connection
                    ConnectHorizontal(pt1.Y, pt1.X);
                }
            }
        }

        #region Setting connections
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Connects two adacent cells vertically. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="column">		The column the cells occupy. </param>
        /// <param name="higherRow">	The row of the higher cell - i.e., the row with the smaller row index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void ConnectVertical(int column, int higherRow)
        {
            _connectsDown[column, higherRow] = true;
            _freeConnectionCount--;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Connects two adacent cells horizontally. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="row">				The row the cells occupy. </param>
        /// <param name="leftmostColumn">	The column of the leftmost cell. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void ConnectHorizontal(int row, int leftmostColumn)
        {
            _connectsRight[leftmostColumn, row] = true;
            _freeConnectionCount--;
        }

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Query if two positions are connected. </summary>
        ///
        /// <remarks>	Darrellp, 9/19/2011. </remarks>
        ///
        /// <param name="pt1">	The first coordinate. </param>
        /// <param name="pt2">	The second coordinate. </param>
        ///
        /// <returns>	true if connected, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal bool IsConnected(Point pt1, Point pt2)
        {
            bool ret = false;

            // Are they valid neighbors?
            if (CheckCoordinates(ref pt1, ref pt2, out var fVertical))
            {
                // Choose the correct connection array based on fVertical
                bool[,] connectArray = fVertical ? _connectsDown : _connectsRight;

                // See if they're actually connected
                ret = connectArray[pt1.X, pt1.Y];
            }
            return ret;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Check coordinates to see if they're neighbors. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ///
        /// <param name="pt1">		[in,out] The first coordinate - upon return, the coordinate with the smallest value. </param>
        /// <param name="pt2">		[in,out] The second coordinate - upon return, the coordinate with the largest value. </param>
        /// <param name="fVertical">	[out] Returns as true if the positions are vertical neighbors. </param>
        ///
        /// <returns>	true if the locations are neighbors - false otherwise. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool CheckCoordinates(ref Point pt1, ref Point pt2, out bool fVertical)
        {
            // If they're in the same column then they're vertical so set the fVertical flag
            fVertical = pt1.X == pt2.X;

            // Are they in different rows and different columns?
            if (!fVertical && pt1.Y != pt2.Y)
            {
                // return false - they're not neighbors
                return false;
            }

            // Determine if they need to be swapped
            var fSwap = fVertical ? pt2.Y < pt1.Y : pt2.X < pt1.X;

            // Do they?
            if (fSwap)
            {
                // Swap them
                (pt1, pt2) = (pt2, pt1);
            }

            // return true if their differing coordinates differ by one
            var coordinateDifference = fVertical ? pt2.Y - pt1.Y : pt2.X - pt1.X;
            return coordinateDifference == 1;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Information about connections. </summary>
        ///
        /// <remarks>	Darrellp, 9/20/2011. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal struct ConnectionInfo(GridConnections connections, Point location, Dir dir, bool isConnected)
        {
            #region Properties
            internal Point Location { get; } = location;
            internal Dir Dir => dir;
            internal bool IsConnected { get; private set; } = isConnected;
            #endregion

            #region Modification
            internal void SetValue(bool isConnectedNew)
            {
                // If we haven't changed the value, then there's nothing to do
                if (isConnectedNew == IsConnected)
                {
                    return;
                }

                if (dir == Dir.Vert)
                {
                    connections._connectsDown[Location.X, Location.Y] = isConnectedNew;
                }
                else
                {
                    connections._connectsRight[Location.X, Location.Y] = isConnectedNew;
                }
                connections._freeConnectionCount += IsConnected ? 1 : -1;
                IsConnected = isConnectedNew;
            }
            #endregion
        }

    }
}

