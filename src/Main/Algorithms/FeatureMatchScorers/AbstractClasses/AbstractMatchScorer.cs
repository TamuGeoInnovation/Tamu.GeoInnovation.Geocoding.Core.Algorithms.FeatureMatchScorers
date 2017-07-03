using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Maths.NumericStrings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

using System;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
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
                        //PAYTON:MATCHSCORE if less than streetlevel match - add matchscore penalty accordingly
                        switch (matchedFeature.ReferenceSourceType.ToString())
                        {
                            case "Census2000ConsolidatedCities":
                                matchedFeature.MatchScore += -50;
                                break;
                            case "Census2000Counties":
                                matchedFeature.MatchScore += -60;
                                break;
                            case "Census2000CountySubRegions":
                                matchedFeature.MatchScore += -60;
                                break;
                            case "Census2000Places":
                                matchedFeature.MatchScore += -50;
                                break;
                            case "Census2000States":
                                matchedFeature.MatchScore += -95;
                                break;
                            case "Census2008TigerLines":                                
                                break;
                            case "Census2008ZCTAs":
                                matchedFeature.MatchScore += -40;
                                break;
                            case "Census2010ConsolidatedCities":
                                matchedFeature.MatchScore += -50;
                                break;
                            case "Census2010Counties":
                                matchedFeature.MatchScore += -60;
                                break;
                            case "Census2010CountySubRegions":
                                matchedFeature.MatchScore += -60;
                                break;
                            case "Census2010Places":
                                matchedFeature.MatchScore += -50;
                                break;
                            case "Census2010States":
                                matchedFeature.MatchScore += -95;
                                break;
                            case "Census2016TigerLines":
                                break;
                            case "Census2015TigerLines":
                                break;
                            case "Census2010TigerLines":
                                break;
                            case "Census2010ZCTAs":
                                matchedFeature.MatchScore += -40;
                                break;
                            case "NavteqAddressPoints2016":
                                break;
                            case "NavteqAddressPoints2013":
                                break;
                            case "NavteqAddressPoints2012":
                                break;
                            case "NavteqStreets2008":
                                break;
                            case "NavteqStreets2012":
                                break;
                            case "ZipCodeDownloadZips2013":
                                matchedFeature.MatchScore += -40;
                                break;
                            case "USPSTigerZipPlus4":
                                matchedFeature.MatchScore += -40;
                                break;
                            default:                                
                                break;
                                //throw new Exception("Unexpected or unimplemented ReferenceSourceType: " + config.ReferenceSourceType);
                        }                        
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
