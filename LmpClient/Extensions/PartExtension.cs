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
    }
}
