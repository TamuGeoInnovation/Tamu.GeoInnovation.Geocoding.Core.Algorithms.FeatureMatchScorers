namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces.EditDistanceScorers
{
    public interface IEditDistanceScorer
    {
        double ComputeWeightedEditDistancePenalty(string inputString, string referenceString, double fullWeight);
    }
}
