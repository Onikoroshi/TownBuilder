//using System;
//using UnityEngine;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using AssetFramework;
//using ProjectAutomata;
//using Random = UnityEngine.Random;
//
//public class UrbanPlanningOffice : CityOffice
//{
//    [Serializable]
//    private class RoadCon
//    {
//        public int TileIndex;
//        public Direction Direction;
//        public List<RoadCon> Connections;
//        public int DistFromParent;
//
//        public RoadCon(int ti, Direction d, int dist)
//        {
//            TileIndex = ti;
//            Direction = d;
//            Connections = new List<RoadCon>();
//            DistFromParent = dist;
//        }
//    }
//
//    private struct PotentialBuildLoc : IEquatable<PotentialBuildLoc>
//    {
//        public int Tile;
//        public Direction TowardsRoad;
//
//        public PotentialBuildLoc(int t, Direction dir)
//        {
//            Tile = t;
//            TowardsRoad = dir;
//        }
//
//
//        public static bool operator ==(PotentialBuildLoc l1, PotentialBuildLoc l2)
//        {
//            return l1.Tile == l2.Tile;
//        }
//
//        public static bool operator !=(PotentialBuildLoc l1, PotentialBuildLoc l2)
//        {
//            return !(l1 == l2);
//        }
//
//        public bool Equals(PotentialBuildLoc other)
//        {
//            return other.Tile == Tile;
//        }
//    }
//
//    private readonly List<RoadCon> _allRoadCons = new List<RoadCon>();
//    private readonly List<RoadCon> _openRoadCons = new List<RoadCon>();
//    private readonly List<RoadCon> _toBeRemoved = new List<RoadCon>();
//    private readonly List<RoadCon> _toBeAdded = new List<RoadCon>();
//    private readonly List<PotentialBuildLoc> _possibleBuildLocations = new List<PotentialBuildLoc>();
//
//    private readonly List<int> _visited = new List<int>();
//
//    [Header("Road Generation")] public float StepSpeed = .1f;
//    [Range(0, 1)] public float ChanceOffSplitting;
//    public int MinDistFromLastSplit;
//    public int WantedHousePositions;
//    public int WantedHouseCount;
//
//    public bool DrawGizmos;
//    public bool DebugBuildingSteps = false;
//
//    [AssetFrameworkObjectProperty(typeof (GameData), "buildingsSerialized")] public Building ResidentialBuilding;
//
//    [AssetFrameworkObjectProperty(typeof (GameData), "buildingsSerialized")] public Building CommercialBuilding;
//
//    [AssetFrameworkObjectProperty(typeof (GameData), "cityAttributesSerialized")] public CityAttribute
//        CommercialCapacityAttribute;
//
//    [AssetFrameworkObjectProperty(typeof (GameData), "cityAttributesSerialized")] public CityAttribute
//        ResidentialCapacityAttribute;
//
//    private CityMechanics _mechanics;
//    public int Buildings = 50;
//    public void Start()
//    {
//        MainOffice = GetComponent<MainOffice>();
//        _mechanics = MainOffice.CityMechanics;
//        var startIndex = MainOffice.City.tradingCenterRoadConnectionNode;
//        _openRoadCons.Add(new RoadCon(startIndex, Direction.South, 0));
//        _allRoadCons.AddRange(_openRoadCons);
//        
//        _mechanics.SetAttributeWanted(CommercialCapacityAttribute, Random.Range(25, 55));
//
//        if (!DebugBuildingSteps)
//        {
//            while (Buildings > 0)
//            {
//                AllStep();
//            }
//        }
//    }
//
//    void Update()
//    {
//        if (DebugBuildingSteps)
//        {
//            if (Input.GetKey(KeyCode.K))
//            {
//                AllStep();
//            }
//        }
//    }
//
//    public override List<CityBill> ProposeCityActions()
//    {
//        var actions = new List<CityBill>();
//        return actions;
//        //if (_possibleBuildLocations.Count > 0)
//        //{
//        //    var buildCommercial = new CityBill("Build Commercial House");
//        //    var buildResidential = new CityBill("Build Residential House");
//
//        //    var com = CommercialBuilding.GetComponent<CityCommercial>();
//        //    var res = ResidentialBuilding.GetComponent<CityResidential>();
//
//        //    //Build commercial initialization
//        //    buildCommercial.AddEffect(MainOffice.CommercialCapacityAttribute, com.commercialCapacity);
//        //    buildCommercial.AddEffect(MainOffice.MoneyAttribute, -CommercialBuilding.BaseCost);
//        //    buildCommercial.ActionSelectedCallback = () => BuildCommercial(com.commercialCapacity);
//
//        //    //Build residential initialization
//        //    buildResidential.AddEffect(MainOffice.ResidentialCapacityAttribute, res.populationCapacity);
//        //    buildResidential.AddEffect(MainOffice.MoneyAttribute, -ResidentialBuilding.BaseCost);
//        //    buildResidential.ActionSelectedCallback = () => BuildResidential(res.populationCapacity);
//
//        //    actions.Add(buildCommercial);
//        //    actions.Add(buildResidential);
//        //}
//        return actions;
//    }
//
//    void BuildCommercial(int neededCap)
//    {
//        for (int i = 0; i < _possibleBuildLocations.Count; i++)
//        {
//            var potential = _possibleBuildLocations[i];
//            if (neededCap <= 0)
//            {
//                _possibleBuildLocations.RemoveRange(0, i);
//                return;
//            }
//            if (BuildingManager.instance.GetBuilding(potential.Tile) != null)
//                continue;
//
//            var coords = Tile.GetCoordinates(potential.Tile);
//            var b = Building.Create(CommercialBuilding, coords.x, coords.y, potential.TowardsRoad);
//
//            _mechanics.Buildings.Add(b);
//
//            var res = b.GetComponent<CityCommercial>();
//            _mechanics.ChangeAttributeValue(GameData.CityAttributes.GetObjectByName("CommercialCapacity"), res.commercialCapacity);
//
//            neededCap -= res.commercialCapacity;
//        }
//        _possibleBuildLocations.Clear();
//    }
//
//    void BuildResidential(int neededCap)
//    {
//        for (int i = 0; i < _possibleBuildLocations.Count; i++)
//        {
//            var potential = _possibleBuildLocations[i];
//            if (neededCap <= 0)
//            {
//                _possibleBuildLocations.RemoveRange(0, i);
//                return;
//            }
//            if (BuildingManager.instance.GetBuilding(potential.Tile) != null)
//                continue;
//
//            var coords = Tile.GetCoordinates(potential.Tile);
//            var b = Building.Create(ResidentialBuilding, coords.x, coords.y, potential.TowardsRoad);
//
//            _mechanics.Buildings.Add(b);
//
//            var res = b.GetComponent<CityResidential>();
//            _mechanics.ChangeAttributeValue(GameData.CityAttributes.GetObjectByName("ResidentialCapacity"), res.populationCapacity);
//
//            neededCap -= res.populationCapacity;
//        }
//        _possibleBuildLocations.Clear();
//    }
//
//    //Tick function. Follow all open roads one step
//    [ContextMenu("All step")]
//    public void AllStep()
//    {
//        Buildings--;
//        if (Buildings < 0)
//            return;
//
//        foreach (var con in _openRoadCons)
//            Step(con);
//
//        foreach (var a in _toBeAdded)
//        {
//            _openRoadCons.Add(a);
//            _allRoadCons.Add(a);
//            
//            RoadManager.instance.PlaceAndConnect(a.TileIndex, 0);
//        }
//
//        _toBeAdded.Clear();
//
//        foreach (var r in _toBeRemoved)
//            _openRoadCons.Remove(r);
//        _toBeRemoved.Clear();
//        
//        if(Random.Range(0f, 1f) > 0.25f)
//            BuildResidential(10);
//        else
//            BuildCommercial(10);
//    }
//
//    [ContextMenu("Reset")]
//    public void Reset()
//    {
//        _allRoadCons.Clear();
//        _openRoadCons.Clear();
//        _toBeAdded.Clear();
//        _toBeRemoved.Clear();
//        _possibleBuildLocations.Clear();
//        CancelInvoke("AllStep");
//        Start();
//    }
//
//    void Step(RoadCon connection)
//    {
//        if (_possibleBuildLocations.Count >= WantedHousePositions)
//            return;
//        _toBeRemoved.Add(connection);
//
//        var a = RoadManager.instance.astar.GetGraphNode(connection.TileIndex);
//
//        foreach (var con in a.connections)
//        {
//            var connectionDirection = Tile.GetDirection(connection.TileIndex, con);
//            var invertedDir = (Direction) Tile.InvertDirection(connectionDirection);
//            if (invertedDir == connection.Direction)
//                continue;
//
//            PlaceNewCon(connection, con, (Direction) Tile.GetDirection(connection.TileIndex, con));
//        }
//        
//        var split = Random.Range(0f, 1f) < ChanceOffSplitting;
//        var nextSplit = HasSplitOrTurn(connection.TileIndex, connection.Direction, MinDistFromLastSplit);
//
//        if (split && connection.DistFromParent >= MinDistFromLastSplit && nextSplit >= MinDistFromLastSplit)
//        {
//            connection.DistFromParent = -1;
//            Split(connection);
//            return;
//        }
//
//        var coords = Tile.GetCoordinates(connection.TileIndex);
//        var nextpos = Tile.Move(coords.x, coords.y, (int) connection.Direction);
//
//        PlaceNewCon(connection, nextpos, connection.Direction);
//    }
//
//    private int HasSplitOrTurn(int index, Direction dir, int maxSteps)
//    {
//        int count = 0;
//        for (int i = 0; i < maxSteps; i++)
//        {
//            var a = RoadManager.instance.astar.GetGraphNode(index);
//
//            if (a.connections.Length == 1)
//                return maxSteps;
//
//            if (a.connections.Length > 2)
//                return count;
//
//            if (a.connections.Length == 2)
//            {
//                var dir1 = (Direction) Tile.GetDirection(a.connections[0], index);
//                var dir2 = (Direction) Tile.GetDirection(index, a.connections[1]);
//                if (dir1 != dir2)
//                    return count;
//            }
//            var c = Tile.GetCoordinates(index);
//            index = Tile.Move(c.x, c.y, (int) dir);
//            count++;
//        }
//
//        return count;
//    }
//
//    #region RoadConnection
//
//    void Split(RoadCon connection)
//    {
//        var tc = Tile.GetCoordinates(connection.TileIndex);
//        var forwardPos = Tile.Move(tc.x, tc.y, (int) connection.Direction);
//        var existing = GetRoadCon(forwardPos);
//        if (existing != null)
//        {
//            existing.Connections.Add(connection);
//            RemovePossibleLocation(connection.TileIndex);
//            return;
//        }
//
//        AddPossibleLocation(forwardPos, (Direction) Tile.InvertDirection((byte) connection.Direction));
//
//        Direction dir1;
//        Direction dir2;
//        var neighbors = GetTileNeighbors(connection.TileIndex, connection.Direction, out dir1, out dir2);
//
//        PlaceNewCon(connection, neighbors.x, dir1);
//        PlaceNewCon(connection, neighbors.y, dir2);
//
//        RemovePossibleLocation(neighbors.x);
//        RemovePossibleLocation(neighbors.y);
//    }
//
//    RoadCon PlaceNewCon(RoadCon parentConnection, int tile, Direction dir)
//    {
//        if (!RoadManager.instance.At(tile) && !RoadManager.instance.CanBuild(tile))
//            return null;
//
//        var existing = GetRoadCon(tile);
//        if (existing != null)
//        {
//            existing.Connections.Add(parentConnection);
//            RemovePossibleLocation(tile);
//
//            return null;
//        }
//
//        if (Snap(parentConnection, tile, dir))
//            return null;
//
//        if (_visited.Contains(tile))
//            return null;
//
//        _visited.Add(tile);
//
//        Direction dir1;
//        Direction dir2;
//        var neighbors = GetTileNeighbors(tile, dir, out dir1, out dir2);
//        //Swap directions, since we need the buildings to point towards the road
//        AddPossibleLocation(neighbors.x, dir2);
//        AddPossibleLocation(neighbors.y, dir1);
//        RemovePossibleLocation(tile);
//        
//        var newcon = new RoadCon(tile, dir, parentConnection.DistFromParent + (parentConnection.Direction == dir ? 1 : 0));
//        _toBeAdded.Add(newcon);
//        parentConnection.Connections.Add(newcon);
//
//        return newcon;
//    }
//
//    bool Snap(RoadCon parentConnection, int tile, Direction dir)
//    {
//        Direction dir1;
//        Direction dir2;
//
//        var neighbors = GetTileNeighbors(tile, dir, out dir1, out dir2);
//
//        var pos1Existing = GetRoadCon(neighbors.x);
//        var pos2Existing = GetRoadCon(neighbors.y);
//
//        if (pos1Existing != null || pos2Existing != null)
//        {
//            var newcon = new RoadCon(tile, dir, parentConnection.DistFromParent + 1);
//            parentConnection.Connections.Add(newcon);
//
//            if (pos1Existing != null)
//            {
//                pos1Existing.Connections.Add(newcon);
//                RemovePossibleLocation(newcon.TileIndex);
//            }
//            if (pos2Existing != null)
//            {
//                pos2Existing.Connections.Add(newcon);
//                RemovePossibleLocation(newcon.TileIndex);
//            }
//            return true;
//        }
//        return false;
//    }
//
//    void AddPossibleLocation(int tile, Direction dir)
//    {
//        if (World.IsBlocked(tile, BlockReason.CantBuild) || World.IsWater(tile) || World.GetBiome(tile) != Biome.Grass)
//            return;
//
//        if (RoadManager.instance.astar.GetGraphNode(tile).connections.Length > 0)
//            return;
//
//        var loc = new PotentialBuildLoc(tile, dir);
//        if (!_possibleBuildLocations.Contains(loc))
//            _possibleBuildLocations.Add(loc);
//    }
//
//    void RemovePossibleLocation(int tile)
//    {
//        for (int index = 0; index < _possibleBuildLocations.Count; index++)
//        {
//            var p = _possibleBuildLocations[index];
//            if (p.Tile == tile)
//            {
//                _possibleBuildLocations.RemoveAt(index);
//                return;
//            }
//        }
//    }
//
//    /// <summary>
//    /// returns the two tiles that are neighbors to the tile
//    /// The two tiles will be parallel with the direction. So if direction is north,
//    /// it would return the east and west tiles.
//    /// </summary>
//    /// <param name="tile"></param>
//    /// <param name="direction"></param>
//    /// <returns></returns>
//    Int2 GetTileNeighbors(int tile, Direction direction, out Direction dir1, out Direction dir2)
//    {
//        var tc = Tile.GetCoordinates(tile);
//        var result = new Int2();
//        if (direction == Direction.North || direction == Direction.South)
//        {
//            result.x = Tile.Move(tc.x, tc.y, (int) Direction.East);
//            result.y = Tile.Move(tc.x, tc.y, (int) Direction.West);
//
//            dir1 = Direction.East;
//            dir2 = Direction.West;
//
//            return result;
//        }
//
//        result.x = Tile.Move(tc.x, tc.y, (int) Direction.North);
//        result.y = Tile.Move(tc.x, tc.y, (int) Direction.South);
//
//        dir1 = Direction.North;
//        dir2 = Direction.South;
//
//        return result;
//    }
//
//    RoadCon GetRoadCon(int index)
//    {
//        foreach (var con in _allRoadCons)
//        {
//            if (con.TileIndex == index)
//                return con;
//        }
//        return null;
//    }
//
//    #endregion
//
//    #region Gizmos
//
//    public void OnDrawGizmos()
//    {
//        if (!DrawGizmos)
//            return;
//        if (_allRoadCons.Count > 1)
//        {
//            Gizmos.color = Color.cyan;
//            var currentCon = _allRoadCons[0];
//            for (int i = 1; i < _allRoadCons.Count; i++)
//            {
//                var nextCon = _allRoadCons[i];
//                var currentConPos = World.Pos(currentCon.TileIndex);
//                foreach (var con in currentCon.Connections)
//                {
//                    var conpos = World.Pos(con.TileIndex);
//                    Gizmos.DrawLine(currentConPos, conpos);
//                }
//                currentCon = nextCon;
//            }
//        }
//
//        Gizmos.color = Color.red;
//        foreach (var open in _openRoadCons)
//        {
//            Gizmos.DrawSphere(World.Pos(open.TileIndex), .2f);
//        }
//
//        Gizmos.color = Color.white;
//        foreach (var pos in _possibleBuildLocations)
//        {
//            var p = World.Pos(pos.Tile);
//            Gizmos.DrawCube(p, Vector3.one);
//        }
//    }
//
//    #endregion
//}
//
///*
//
//        private int GetHeuristicBestExpansionDirection(int x, int y, byte[] freeDirections, out int freeStreetSpots)
//        {
//            return GetHeuristicBestExpansionDirection(Tile.GetIndex(x, y), freeDirections, out freeStreetSpots);
//        }
//        /// <summary>
//        /// Performs heuristic checks on which free direction is the best for building the road.
//        /// The following checks are performed:
//        /// - How much space is in this direction, this is used directly to weight.
//        /// 
//        /// Returns the index of the best expansion direction and -1 in case of an error
//        /// </summary>
//        private int GetHeuristicBestExpansionDirection(int index, byte[] freeDirections, out int freeStreetSpots)
//        {
//            if (freeDirections.Length == 0)
//            {
//                freeStreetSpots = 0;
//                return -1;
//            }
//
//            // TODO: Add free building spots along road check
//            int mostFreeStreetSpots = 0;
//            int mostFreeSpotsId = -1;
//            for (int i = 0; i < freeDirections.Length; i++)
//            {
//                int iIndex = index;
//                int dir = Tile.dir4[freeDirections[i]];
//                int spots = 0;
//
//                // Count free spots
//                for (int j = 0; j < heuristicCheckRange; j++)
//                {
//                    iIndex += dir;
//
//                    if (WithinCityBorder(iIndex) && RoadManager.instance.CanBuild(iIndex) && !World.IsBlocked(iIndex, BlockReason.Slope))
//                    {
//                        spots++;
//                    }
//                    else
//                        break;
//                }
//
//                if (mostFreeStreetSpots <= spots)
//                {
//                    mostFreeStreetSpots = spots;
//                    mostFreeSpotsId = i;
//                }
//            }
//
//            freeStreetSpots = mostFreeStreetSpots;
//            return mostFreeSpotsId;
//        }
//*/