using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Maths.NumericStrings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces
{
    public interface IMatchScorer
    {
        NumericStringManager NumericStringManager { get; set; }
        AttributeWeightingScheme AttributeWeightingScheme { get; set; }
        MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult);
        void ScoreAllMatchedFeatures(ParameterSet parameterSet, AttributeWeightingScheme attributeWeightingScheme, ReferenceSourceQueryResult referenceSourceQueryResult);
    }
}
