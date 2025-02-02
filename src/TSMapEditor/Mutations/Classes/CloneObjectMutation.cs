﻿using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that clones a game object on the map.
    /// </summary>
    public class CloneObjectMutation : Mutation
    {
        public CloneObjectMutation(IMutationTarget mutationTarget, IMovable movable, Point2D clonePosition) : base(mutationTarget)
        {
            this.objectToClone = (AbstractObject)movable;
            if (!objectToClone.IsTechno())
                throw new NotSupportedException(nameof(CloneObjectMutation) + " only supports cloning Technos!");

            this.clonePosition = clonePosition;
        }

        private AbstractObject objectToClone;
        private Point2D clonePosition;
        private AbstractObject placedClone;

        private void CloneObject()
        {
            var clone = objectToClone.Clone();

            switch (clone.WhatAmI())
            {
                case RTTIType.Aircraft:
                    var aircraft = (Aircraft)clone;
                    aircraft.Position = clonePosition;
                    MutationTarget.Map.PlaceAircraft(aircraft);
                    break;
                case RTTIType.Building:
                    var building = (Structure)clone;
                    building.Position = clonePosition;
                    MutationTarget.Map.PlaceBuilding(building);
                    break;
                case RTTIType.Unit:
                    var unit = (Unit)clone;
                    unit.Position = clonePosition;
                    MutationTarget.Map.PlaceUnit(unit);
                    break;
                case RTTIType.Infantry:
                    var infantry = (Infantry)clone;
                    infantry.Position = clonePosition;
                    infantry.SubCell = Map.GetTile(clonePosition).GetFreeSubCellSpot();
                    MutationTarget.Map.PlaceInfantry(infantry);
                    break;
            }

            placedClone = clone;

            MutationTarget.AddRefreshPoint(clonePosition);
        }

        public override void Perform()
        {
            CloneObject();
        }

        public override void Undo()
        {
            switch (objectToClone.WhatAmI())
            {
                case RTTIType.Aircraft:
                    Map.RemoveAircraft(clonePosition);
                    break;
                case RTTIType.Building:
                    Map.RemoveBuilding(clonePosition);
                    break;
                case RTTIType.Unit:
                    Map.RemoveUnit(clonePosition);
                    break;
                case RTTIType.Infantry:
                    Map.RemoveInfantry((Infantry)placedClone);
                    break;
            }
        }
    }
}
