using System;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.GeographicFeatures.AddressPoints;
using USC.GISResearchLab.Common.Utils.Numbers;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses.EditDistanceScorers.LevenshteinEditDistanceScorers;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures.Implementations;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Implementations.EditDistanceScorers.LevenshteinEditDistanceScorers
{
    public class LevenshteinEditDistancePennyScorer : AbstractLevenshteinEditDistanceScorer
    {
        public LevenshteinEditDistancePennyScorer()
        {
        }

        public LevenshteinEditDistancePennyScorer(AttributeWeightingScheme attributeWeightingScheme)
            : base(attributeWeightingScheme)
        { }

        public override MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult)
        {

            MatchScoreResult ret = new MatchScoreResult(AttributeWeightingScheme);
            ret.StartTimer();

            try
            {

                if (referenceFeature.AddressComponent == AddressComponents.All)
                {

                    MatchScoreNumberPenaltyResult numberPenalty = ComputePenaltyNumber(parameterSet, inputAddress, referenceFeature, referenceSourceQueryResult, AttributeWeightingScheme);
                    numberPenalty.AddressComponent = AddressComponents.Number;
                    ret.AddressDistance = numberPenalty.OverallAddressDistance;
                    ret.ParityResultType = numberPenalty.OverallParityResultType;
                    ret.MatchScorePenaltyResults.Add(numberPenalty);

                    if (numberPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = numberPenalty.Exception;
                        ret.Error = numberPenalty.Error;
                    }


                    MatchScorePenaltyResult preDirectionalPenalty = ComputePenaltyPreDirectional(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPreDirectional);
                    preDirectionalPenalty.AddressComponent = AddressComponents.PreDirectional;
                    ret.MatchScorePenaltyResults.Add(preDirectionalPenalty);

                    if (preDirectionalPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = preDirectionalPenalty.Exception;
                        ret.Error = preDirectionalPenalty.Error;
                    }

                    MatchScorePenaltyResult preQualifierPenalty = ComputePenaltyPreQualifier(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPreQualifier);
                    preQualifierPenalty.AddressComponent = AddressComponents.PreQualifier;
                    ret.MatchScorePenaltyResults.Add(preQualifierPenalty);

                    if (preQualifierPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = preQualifierPenalty.Exception;
                        ret.Error = preQualifierPenalty.Error;
                    }

                    MatchScorePenaltyResult preTypePenalty = ComputePenaltyPreType(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPreType);
                    preTypePenalty.AddressComponent = AddressComponents.PreType;
                    ret.MatchScorePenaltyResults.Add(preTypePenalty);

                    if (preTypePenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = preTypePenalty.Exception;
                        ret.Error = preTypePenalty.Error;
                    }

                    MatchScorePenaltyResult preArticlePenalty = ComputePenaltyPreArticle(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPreArticle);
                    preArticlePenalty.AddressComponent = AddressComponents.PreArticle;
                    ret.MatchScorePenaltyResults.Add(preArticlePenalty);

                    if (preArticlePenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = preArticlePenalty.Exception;
                        ret.Error = preArticlePenalty.Error;
                    }

                    MatchScorePenaltyResult namePenalty = ComputePenaltyName(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightName);
                    namePenalty.AddressComponent = AddressComponents.StreetName;
                    ret.MatchScorePenaltyResults.Add(namePenalty);

                    if (namePenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = namePenalty.Exception;
                        ret.Error = namePenalty.Error;
                    }

                    MatchScorePenaltyResult postArticlePenalty = ComputePenaltyPostArticle(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPostArticle);
                    postArticlePenalty.AddressComponent = AddressComponents.PostArticle;
                    ret.MatchScorePenaltyResults.Add(postArticlePenalty);

                    if (postArticlePenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = postArticlePenalty.Exception;
                        ret.Error = postArticlePenalty.Error;
                    }

                    MatchScorePenaltyResult suffixPenalty = ComputePenaltySuffix(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightSuffix);
                    suffixPenalty.AddressComponent = AddressComponents.Suffix;
                    ret.MatchScorePenaltyResults.Add(suffixPenalty);

                    if (suffixPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = suffixPenalty.Exception;
                        ret.Error = suffixPenalty.Error;
                    }

                    MatchScorePenaltyResult postQualifierPenalty = ComputePenaltyPostQualifier(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPostQualifier);
                    postQualifierPenalty.AddressComponent = AddressComponents.PostQualifier;
                    ret.MatchScorePenaltyResults.Add(postQualifierPenalty);

                    if (postQualifierPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = postQualifierPenalty.Exception;
                        ret.Error = postQualifierPenalty.Error;
                    }

                    MatchScorePenaltyResult postDirectionalPenalty = ComputePenaltyPostDirectional(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightPostDirectional);
                    postDirectionalPenalty.AddressComponent = AddressComponents.PostDirectional;
                    ret.MatchScorePenaltyResults.Add(postDirectionalPenalty);

                    if (postDirectionalPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = postDirectionalPenalty.Exception;
                        ret.Error = postDirectionalPenalty.Error;
                    }

                }

                MatchScorePenaltyResult zipPenalty = null;
                if (referenceFeature.AddressComponent == AddressComponents.All || referenceFeature.AddressComponent == AddressComponents.Zip || referenceFeature.AddressComponent == AddressComponents.ZipPlus4)
                {
                    zipPenalty = ComputePenaltyZip(parameterSet, inputAddress.ZIP, featureAddress, AttributeWeightingScheme.ProportionalWeightZip);
                    zipPenalty.AddressComponent = AddressComponents.Zip;

                    if (zipPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = zipPenalty.Exception;
                        ret.Error = zipPenalty.Error;
                    }
                }


                MatchScorePenaltyResult zipPlus4Penalty = null;
                if (referenceFeature.AddressComponent == AddressComponents.All || referenceFeature.AddressComponent == AddressComponents.Zip || referenceFeature.AddressComponent == AddressComponents.ZipPlus4)
                {
                    zipPlus4Penalty = ComputePenaltyZipPlus4(parameterSet, inputAddress.ZIPPlus4, featureAddress, AttributeWeightingScheme.ProportionalWeightZipPlus4);
                    zipPlus4Penalty.AddressComponent = AddressComponents.ZipPlus4;
                    ret.MatchScorePenaltyResults.Add(zipPlus4Penalty);

                    if (zipPlus4Penalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = zipPlus4Penalty.Exception;
                        ret.Error = zipPlus4Penalty.Error;
                    }
                }


                MatchScorePenaltyResult cityPenalty = null;
                if (referenceFeature.AddressComponent == AddressComponents.All || referenceFeature.AddressComponent == AddressComponents.City)
                {
                    //TASK:X7-T1 Added variable to parameterSet allow for not using alias table (10/9/18)
                    cityPenalty = ComputePenaltyCity(parameterSet, inputAddress, featureAddress, AttributeWeightingScheme.ProportionalWeightCity);                    
                    cityPenalty.AddressComponent = AddressComponents.City;

                    if (cityPenalty.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = cityPenalty.Exception;
                        ret.Error = cityPenalty.Error;
                    }
                }

               

                if (zipPenalty != null && cityPenalty != null)
                {
                    // if the zip is correct but the city is not, only take a 10th of the city penalty because it's more likely that the city is wrong but the zip right
                    if (zipPenalty.PenaltyValue == 0)
                    {
                        if (cityPenalty.PenaltyValue > 0)
                        {
                            cityPenalty.AdjustPenalty(cityPenalty.PenaltyValue / 10.0);
                        }
                    }

                    // if the city is correct but the zip is not, only take a 10th of the zip penalty because it's more likely that the zip is wrong but the city right
                    if (cityPenalty.PenaltyValue == 0)
                    {
                        if (zipPenalty.PenaltyValue > 0)
                        {
                            zipPenalty.AdjustPenalty(zipPenalty.PenaltyValue / 10.0);
                        }
                    }

                    ret.MatchScorePenaltyResults.Add(zipPenalty);
                    ret.MatchScorePenaltyResults.Add(cityPenalty);
                }
                else
                {
                    if (zipPenalty != null)
                    {
                        ret.MatchScorePenaltyResults.Add(zipPenalty);
                    }

                    if (cityPenalty != null)
                    {
                        ret.MatchScorePenaltyResults.Add(cityPenalty);
                    }
                }

                if (referenceFeature.AddressComponent == AddressComponents.All || referenceFeature.AddressComponent == AddressComponents.State)
                {
                    MatchScorePenaltyResult statePenality = ComputePenaltyState(parameterSet, inputAddress.State, featureAddress, AttributeWeightingScheme.ProportionalWeightState);
                    statePenality.AddressComponent = AddressComponents.State;
                    ret.MatchScorePenaltyResults.Add(statePenality);

                    if (statePenality.ExceptionOccurred)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Exception = statePenality.Exception;
                        ret.Error = statePenality.Error;
                    }

                }


            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in GetMatchScore: " + e.Message;
                ret.Exception = e;
            }

            ret.EndTimer();

            return ret;
        }

        public MatchScoreNumberPenaltyResult ComputePenaltyNumber(ParameterSet parameterSet, StreetAddress inputAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult, AttributeWeightingScheme attrbuteWeightingScheme)
        {
            MatchScoreNumberPenaltyResult ret = new MatchScoreNumberPenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {

                if (referenceFeature.GetType() != typeof(PennyReferenceFeature))
                {
                    throw new Exception("LevenshteinEditDistancePennyScorer only works on type PennyReferenceFeature: " + referenceFeature.GetType());
                }
                else
                {
                    PennyAddressPoint pennyAddressPoint = (PennyAddressPoint)referenceFeature.StreetAddressableGeographicFeature;

                    if ((String.Compare(inputAddress.Number, pennyAddressPoint.Number, true) == 0) && ((String.Compare(inputAddress.NumberFractional,pennyAddressPoint.NumberFractional,true)==0)) || (inputAddress.NumberFractional=="" && pennyAddressPoint.NumberFractional == null))
                    {
                        ret.OverallAddressDistance = 0;
                        ret.OverallParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    else if ((String.Compare(inputAddress.Number, pennyAddressPoint.Number, true) == 0) && (String.Compare(inputAddress.NumberFractional, pennyAddressPoint.NumberFractional, true) == 1))
                    {
                        ret.OverallAddressDistance = 0;
                        penalty = AttributeWeightingScheme.ProportionalWeightNumberParity;
                        ret.OverallParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    //PAYTON:NY-ADDRESS SCORING The following three else if statements are added for NY and HI to account for hyphenated street numbers entered as one concatenated number
                    else if (String.Compare(inputAddress.Number + inputAddress.NumberFractional, pennyAddressPoint.Number + pennyAddressPoint.NumberFractional, true) == 0 && (pennyAddressPoint.State == "NY" || pennyAddressPoint.State == "HI"))
                    {
                        ret.OverallAddressDistance = 0;
                        penalty = AttributeWeightingScheme.ProportionalWeightNumberParity;
                        ret.OverallParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    //PAYTON:NY-ADDRESS SCORING 7-16-17
                    else if (String.Compare(inputAddress.Number+inputAddress.NumberFractional, pennyAddressPoint.Number, true) == 0 && (pennyAddressPoint.State == "NY" || pennyAddressPoint.State == "HI"))
                    {
                        ret.OverallAddressDistance = 0;
                        penalty = AttributeWeightingScheme.ProportionalWeightNumberParity;
                        ret.OverallParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    else if (String.Compare(inputAddress.Number+inputAddress.NumberFractional, pennyAddressPoint.Number + '0' + pennyAddressPoint.NumberFractional, true) == 0 && (pennyAddressPoint.State == "NY" || pennyAddressPoint.State == "HI"))
                    {
                        ret.OverallAddressDistance = 0;
                        penalty = AttributeWeightingScheme.ProportionalWeightNumberParity * 2;
                        ret.OverallParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    else
                    {
                        if (NumberUtils.isNumber(inputAddress.Number) && NumberUtils.isNumber(pennyAddressPoint.Number))
                        {
                            int inputAddressNumber = Convert.ToInt32(inputAddress.Number);
                            int referenceAddressNumber = Convert.ToInt32(pennyAddressPoint.Number);

                            int inputAddressNumberEvenOdd = inputAddressNumber % 2;
                            int referenceAddressNumberEvenOdd = referenceAddressNumber % 2;

                            if (inputAddressNumberEvenOdd == referenceAddressNumberEvenOdd)
                            {
                                ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                            }
                            else
                            {
                                ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.IncorrectParity;
                            }

                            double distance = Math.Abs(inputAddressNumber - referenceAddressNumber);
                            ret.OverallAddressDistance = distance;


                            double maxHousesAway = AttributeWeightingScheme.MaxBlocksAway;
                            double housesPerBlock = 100;
                            double housesAway = 0;

                            housesAway = 1.0 + (distance / housesPerBlock);


                            ret.AverageBlockSize = housesPerBlock;
                            ret.NumberOfBlocksAway = housesAway;

                            if (housesAway >= maxHousesAway)
                            {
                                penalty = AttributeWeightingScheme.ProportionalWeightNumber;
                            }
                            else
                            {
                                double proportionOfMaxBlocksAway = housesAway / maxHousesAway;
                                penalty = AttributeWeightingScheme.ProportionalWeightNumber * proportionOfMaxBlocksAway;
                            }

                            if (ret.OverallParityResultType != FeatureMatchAddressParityResultType.CorrectParity)
                            {
                                penalty += AttributeWeightingScheme.ProportionalWeightNumberParity;
                                ret.PenaltyParityValue = AttributeWeightingScheme.ProportionalWeightNumberParity * 100.0;
                            }
                        }
                        else
                        {
                            penalty = AttributeWeightingScheme.ProportionalWeightNumber;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyNumber: " + e.Message;
                ret.Exception = e;
            }

            ret.EndTimer(penalty);

            return ret;
        }
    }
}
