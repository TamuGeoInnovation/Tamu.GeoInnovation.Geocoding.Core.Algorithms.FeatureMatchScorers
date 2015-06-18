using System;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses.EditDistanceScorers.LevenshteinEditDistanceScorers;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Implementations.EditDistanceScorers.LevenshteinEditDistanceScorers
{
    public class LevenshteinEditDistanceDimeScorer : AbstractLevenshteinEditDistanceScorer
    {
        
        public LevenshteinEditDistanceDimeScorer()
        {
        }

        public LevenshteinEditDistanceDimeScorer(AttributeWeightingScheme attributeWeightingScheme)
        {
            AttributeWeightingScheme = attributeWeightingScheme;
        }


        public override MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult)
        {
            throw new NotImplementedException();
        }
    }
}
