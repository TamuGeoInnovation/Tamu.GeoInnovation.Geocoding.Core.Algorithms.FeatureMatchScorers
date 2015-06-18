using System;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Maths.NumericStrings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers
{
    public class MatchUnmatchScorer
    {
        #region Properties

        public NumericStringManager NumericStringManager { get; set; }
        public AttributeWeightingScheme AttributeWeightingScheme { get; set; }

        #endregion

        public MatchUnmatchScorer()
        {
        }

        public MatchUnmatchScorer(AttributeWeightingScheme attributeWeightingScheme)
        {
            AttributeWeightingScheme = attributeWeightingScheme;
        }


        public MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult)
        {
            MatchScoreResult ret = new MatchScoreResult();

            try
            {

                FeatureMatchScorerManager featureMatchScorerManager = new FeatureMatchScorerManager();
                IMatchScorer matchScorer = featureMatchScorerManager.GetMatchScorer(FeatureMatchScorerType.EditDistance, FeatureMatchScorerSubType.LevenshteinEditDistance, referenceFeature.ReferenceFeatureType);

                ret = GetMatchScore(parameterSet, inputAddress, featureAddress, referenceFeature, referenceSourceQueryResult, matchScorer);

            }
            catch (Exception ex)
            {
                ret.Exception = ex;
                ret.ExceptionOccurred = true;
                ret.Error = ex.Message;
            }
            return ret;
        }

        public MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult, IMatchScorer matchScorer)
        {
            MatchScoreResult ret = new MatchScoreResult();

            try
            {
                if (matchScorer != null)
                {
                    matchScorer.AttributeWeightingScheme = AttributeWeightingScheme;

                    ret = matchScorer.GetMatchScore(parameterSet, inputAddress, featureAddress, referenceFeature, referenceSourceQueryResult);
                }
            }
            catch (Exception ex)
            {
                ret.Exception = ex;
                ret.ExceptionOccurred = true;
                ret.Error = ex.Message;
            }

            return ret;
        }
    }
}
