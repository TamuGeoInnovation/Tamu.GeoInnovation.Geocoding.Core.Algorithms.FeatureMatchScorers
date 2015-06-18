using System;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Implementations.EditDistanceScorers.LevenshteinEditDistanceScorers;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers
{

    public class FeatureMatchScorerManager
    {

        public IMatchScorer GetMatchScorer(FeatureMatchScorerType featureMatchScorerType, FeatureMatchScorerSubType featureMatchScorerSubType, ReferenceFeatureType referenceFeatureType)
        {
            IMatchScorer ret = null;

            switch (featureMatchScorerType)
            {
                case FeatureMatchScorerType.EditDistance:

                    switch (featureMatchScorerSubType)
                    {
                        case FeatureMatchScorerSubType.LevenshteinEditDistance:


                            switch (referenceFeatureType)
                            {
                                case ReferenceFeatureType.Dime:
                                    ret = new LevenshteinEditDistanceDimeScorer();
                                    break;
                                case ReferenceFeatureType.Nickle:
                                    ret = new LevenshteinEditDistanceNickleScorer();
                                    break;
                                case ReferenceFeatureType.Penny:
                                    ret = new LevenshteinEditDistancePennyScorer();
                                    break;

                                default:
                                    throw new Exception("Unexpected or unimplemented referenceFeatureType: " + referenceFeatureType);
                            }

                            break;
                        default:
                            throw new Exception("Unexpected or unimplemented featureMatchScorerSubType: " + featureMatchScorerSubType);
                    }

                    break;
                default:
                    throw new Exception("Unexpected or unimplemented FeatureMatchScorerType: " + featureMatchScorerType);
            }

            return ret;
        }
    }
}
