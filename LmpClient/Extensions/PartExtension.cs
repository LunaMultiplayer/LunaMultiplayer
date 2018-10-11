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

        public static void SetImmortal(this Part part, bool immortal)
        {
            if (part == null) return;

            part.gTolerance = immortal ? double.PositiveInfinity : 50;
            part.maxPressure = immortal ? double.PositiveInfinity : 4000;
            part.crashTolerance = immortal ? float.PositiveInfinity : 9f;

            if (part.rb)
            {
                part.rb.isKinematic = immortal;
                part.rb.detectCollisions = !immortal;
                if (part.GetComponent<PartBuoyancy>())
                {
                    part.GetComponent<PartBuoyancy>().enabled = !immortal;
                }
                if (part.GetComponent<PQS_PartCollider>())
                {
                    part.GetComponent<PQS_PartCollider>().enabled = !immortal;
                }
                if (part.GetComponent<CollisionEnhancer>())
                {
                    part.GetComponent<CollisionEnhancer>().enabled = !immortal;
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
            if (part.collisionEnhancer)
            {
                part.collisionEnhancer.OnTerrainPunchThrough = immortal ? CollisionEnhancerBehaviour.DO_NOTHING : CollisionEnhancerBehaviour.EXPLODE;
            }

            //Do not set this as then you can't click on parts
            //part.SetDetectCollisions(!immortal);

            //Do not remove the colliders as then you can't dock
            //if (part.collider != null)
            //    part.collider.enabled = !immortal;
        }
    }
}
