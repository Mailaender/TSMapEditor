﻿using System;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models.MapFormat;
using TSMapEditor.Rendering;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A cell on the map with additional logic properties for the map editor.
    /// </summary>
    public class MapTile : IsoMapPack5Tile
    {
        private const int SubCellCount = 5;

        public MapTile() { }

        public MapTile(byte[] data) : base(data) { }

        /// <summary>
        /// The cached image for this tile.
        /// This should be cleared when the tile's terrain is changed.
        /// </summary>
        public TileImage TileImage { get; set; }
        public TerrainObject TerrainObject { get; set; }
        public Structure Structure { get; set; }
        public Unit Vehicle { get; set; }
        public Aircraft Aircraft { get; set; }
        public Infantry[] Infantry { get; set; } = new Infantry[SubCellCount];
        public TileImage PreviewTileImage { get; set; }
        public int PreviewSubTileIndex { get; set; }
        public int PreviewLevel { get; set; } = -1;

        public Overlay Overlay { get; set; }
        public Smudge Smudge { get; set; }

        public Waypoint Waypoint { get; set; }

        public CellTag CellTag { get; set; }

        /// <summary>
        /// A list of objects that graphically overlap with this tile.
        /// When this tile is re-drawn, all the objects in the list should
        /// be re-drawn as well.
        /// </summary>
        public List<AbstractObject> OverlapList { get; set; }

        /// <summary>
        /// The number of the screen refresh iteration when
        /// this map tile was last rendered. If this value matches the current
        /// rendering iteration, there is no need to draw this tile.
        /// </summary>
        public ulong LastRefreshIndex;


        public void ShiftPosition(int x, int y)
        {
            X += (short)x; 
            Y += (short)y;

            // If we have overlay and/or a smudge, also move their position
            if (Overlay != null)
                Overlay.Position += new Point2D(x, y);

            if (Smudge != null)
                Smudge.Position += new Point2D(x, y);
        }

        public void AddObjectsToList(List<AbstractObject> objects)
        {
            if (Structure != null)
                objects.Add(Structure);

            if (Vehicle != null)
                objects.Add(Vehicle);
        }

        public void AddInfantry(Infantry infantry)
        {
            Infantry[(int)infantry.SubCell] = infantry;
        }

        public void DoForAllInfantry(Action<Infantry> action)
        {
            for (int i = 0; i < Infantry.Length; i++)
            {
                if (Infantry[i] != null)
                    action(Infantry[i]);
            }
        }

        public SubCell GetFreeSubCellSpot()
        {
            if (GetInfantryFromSubCellSpot(SubCell.Bottom) == null)
                return SubCell.Bottom;

            if (GetInfantryFromSubCellSpot(SubCell.Left) == null)
                return SubCell.Left;

            if (GetInfantryFromSubCellSpot(SubCell.Right) == null)
                return SubCell.Right;

            return SubCell.None;
        }

        public Infantry GetInfantryFromSubCellSpot(SubCell subCell)
        {
            return Infantry[(int)subCell];
        }

        public Infantry GetFirstInfantry()
        {
            for (int i = 0; i < Infantry.Length; i++)
            {
                if (Infantry[i] != null)
                    return Infantry[i];
            }

            return null;
        }

        public bool HasTechno()
        {
            return Structure != null || Vehicle != null || Aircraft != null || Array.Exists(Infantry, inf => inf != null);
        }

        public bool HasTechnoThatPassesCheck(Predicate<TechnoBase> predicate)
        {
            return GetFirstTechnoThatPassesCheck(predicate) != null;
        }

        public TechnoBase GetFirstTechnoThatPassesCheck(Predicate<TechnoBase> predicate)
        {
            if (Structure != null && predicate(Structure))
                return Structure;

            if (Vehicle != null && predicate(Vehicle))
                return Vehicle;

            if (Aircraft != null && predicate(Aircraft))
                return Aircraft;

            return Array.Find(Infantry, inf => inf != null && predicate(inf));
        }

        public TechnoBase GetTechno()
        {
            if (Structure != null)
                return Structure;

            if (Vehicle != null)
                return Vehicle;

            if (Aircraft != null)
                return Aircraft;

            return Array.Find(Infantry, inf => inf != null);
        }

        public GameObject GetObject()
        {
            GameObject obj = GetTechno();
            if (obj != null)
                return obj;

            if (TerrainObject != null)
                return TerrainObject;

            return null;
        }

        /// <summary>
        /// Determines whether a specific game object can be assigned to this tile.
        /// </summary>
        public bool CanAddObject(GameObject gameObject)
        {
            switch (gameObject.WhatAmI())
            {
                case RTTIType.Aircraft:
                    return Aircraft == null;
                case RTTIType.Building:
                    return Structure == null || Structure == gameObject;
                case RTTIType.Unit:
                    return Vehicle == null;
                case RTTIType.Infantry:
                    return GetFreeSubCellSpot() != SubCell.None;
                case RTTIType.Terrain:
                    return TerrainObject == null;
            }

            return false;
        }

        public bool ContainsObject(AbstractObject abstractObject)
        {
            switch (abstractObject.WhatAmI())
            {
                case RTTIType.Aircraft:
                    return Aircraft == abstractObject;
                case RTTIType.Terrain:
                    return TerrainObject == abstractObject;
                case RTTIType.Building:
                    return Structure == abstractObject;
                case RTTIType.Unit:
                    return Vehicle == abstractObject;
                case RTTIType.Infantry:
                    return Array.Exists(Infantry, inf => inf == abstractObject);
                case RTTIType.Overlay:
                    return Overlay == abstractObject;
                case RTTIType.Smudge:
                    return Smudge == abstractObject;
                case RTTIType.Waypoint:
                    return Waypoint == abstractObject;
            }

            return false;
        }

        public bool IsClearGround()
        {
            return TileIndex == 0;
        }

        /// <summary>
        /// Returns a value that tells whether the cell has a harvestable resource (tiberium or ore) on it.
        /// </summary>
        public bool HasTiberium()
        {
            return Overlay != null && Overlay.OverlayType != null && Overlay.OverlayType.Tiberium;
        }

        public void ChangeTileIndex(int newTileIndex, byte newSubTileIndex)
        {
            TileImage = null;
            TileIndex = newTileIndex;
            SubTileIndex = newSubTileIndex;
        }

        public Point2D CoordsToPoint() => new Point2D(X, Y);
    }
}
