using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Maths.NumericStrings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using System;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses
{
    public abstract class AbstractMatchScorer: IMatchScorer
    {
        #region Properties

        public NumericStringManager NumericStringManager { get; set; }
        public AttributeWeightingScheme AttributeWeightingScheme { get; set; }
        
        #endregion

        public AbstractMatchScorer()
        {
        }

        public AbstractMatchScorer(AttributeWeightingScheme attributeWeightingScheme)
        {
            AttributeWeightingScheme = attributeWeightingScheme;
        }

        public abstract MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult);


        public void ScoreAllMatchedFeatures(ParameterSet parameterSet, AttributeWeightingScheme attributeWeightingScheme, ReferenceSourceQueryResult referenceSourceQueryResult)
        {
            if (referenceSourceQueryResult.MatchedFeatureCandidateSet.MatchedFeatures.Count > 0)
            {
                MatchUnmatchScorer scorer = new MatchUnmatchScorer(attributeWeightingScheme);
                scorer.NumericStringManager = NumericStringManager;


                FeatureMatchScorerManager featureMatchScorerManager = new FeatureMatchScorerManager();
                IMatchScorer matchScorer = featureMatchScorerManager.GetMatchScorer(FeatureMatchScorerType.EditDistance, FeatureMatchScorerSubType.LevenshteinEditDistance, referenceSourceQueryResult.ReferenceFeatureType);
                matchScorer.NumericStringManager = NumericStringManager;

                int index = 0;
                foreach (MatchedFeature matchedFeature in referenceSourceQueryResult.MatchedFeatureCandidateSet.MatchedFeatures)
                {

                    try
                    {
                        matchedFeature.MatchScoreStartTimer = DateTime.Now;
                        matchedFeature.MatchScoreResult = scorer.GetMatchScore(parameterSet, parameterSet.StreetAddress, matchedFeature.MatchedFeatureAddress, matchedFeature.MatchedReferenceFeature, referenceSourceQueryResult, matchScorer);
                        matchedFeature.MatchScore = matchedFeature.MatchScoreResult.MatchScore;
                        matchedFeature.MatchScoreEndTimer = DateTime.Now;

                        matchedFeature.MatchTypeStartTimer = DateTime.Now;
                        matchedFeature.FeatureMatchTypes = FeatureMatchTypeManager.GetMatchType(matchedFeature, parameterSet.StreetAddress);
                        matchedFeature.MatchTypeEndTimer = DateTime.Now;

                        matchedFeature.Valid = true;
                    }

                    catch (Exception ex)
                    {
                        matchedFeature.Exception = ex;
                        matchedFeature.ExceptionOccurred = true;
                        matchedFeature.Error = ex.Message;
                    }

                    index++;
                }
            }
        }
    }
}
