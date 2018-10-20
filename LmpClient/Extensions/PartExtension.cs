namespace LmpClient.Extensions
{
    public static class PartExtension
    {
        public static PartResource FindResource(this Part part, string resourceName)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < part.Resources.Count; i++)
            {
                if (part.Resources[i].resourceName == resourceName)
                    return part.Resources[i];
            }

            return null;
        }
        
        public static void AddCrew(this Part part, ProtoCrewMember crew)
        {
            part.protoModuleCrew.Add(crew);
            crew.RegisterExperienceTraits(part);
            if (!(part.internalModel != null) || !(part.internalModel.GetNextAvailableSeat() != null))
            {
                crew.seatIdx = -1;
                crew.seat = null;
            }
            else
            {
                part.internalModel.SitKerbalAt(crew, part.internalModel.GetNextAvailableSeat());
            }
            if (part.vessel != null)
            {
                part.vessel.CrewListSetDirty();
            }
        }

        public static void RemoveCrew(this Part part, ProtoCrewMember crew)
        {
            crew.UnregisterExperienceTraits(part);
            part.protoModuleCrew.Remove(crew);
            part.vessel.RemoveCrew(crew);
            if (part.internalModel != null)
            {
                part.internalModel.UnseatKerbal(crew);
            }
        }

        /// <summary>
        /// Makes a part immortal based on the parameter
        /// </summary>
        public static void SetImmortal(this Part part, bool immortal)
        {
            if (part == null) return;

            part.gTolerance = immortal ? double.PositiveInfinity : part.partInfo.partPrefab.gTolerance;
            part.maxPressure = immortal ? double.PositiveInfinity : part.partInfo.partPrefab.maxPressure;
            part.crashTolerance = immortal ? float.PositiveInfinity : part.partInfo.partPrefab.crashTolerance;

            if (part.rb)
            {
                part.rb.isKinematic = immortal;

                //Do not change this value as otherwise you can't grab ladders or right-click on vessels that are controlled by other players
                //part.rb.detectCollisions = !immortal;

                var buoyancy = part.GetComponent<PartBuoyancy>();
                if (buoyancy)
                {
                    buoyancy.enabled = !immortal;
                }

                var pqsPartCollider = part.GetComponent<PQS_PartCollider>();
                if (pqsPartCollider)
                {
                    pqsPartCollider.enabled = !immortal;
                }

                var collisionEnhancer = part.GetComponent<CollisionEnhancer>();
                if (collisionEnhancer)
                {
                    collisionEnhancer.enabled = !immortal;
                }
            }
            if (part.attachJoint)
            {
                if (immortal)
                {
                    part.attachJoint.SetUnbreakable(true, part.rigidAttachment);
                }
                else
                {
                    part.ResetJoints();
                }
            }

            //Do not set this as then you can't click on parts
            //part.SetDetectCollisions(!immortal);

            //Do not remove the colliders as then you can't dock
            //if (part.collider != null)
            //    part.collider.enabled = !immortal;
        }
    }
}
