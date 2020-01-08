using System;
using System.Reflection;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLabComputePenaltyCity.Common.Utils.Strings;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses.EditDistanceScorers.LevenshteinEditDistanceScorers;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceFeatures;
using USC.GISResearchLab.Geocoding.Core.ReferenceDatasets.ReferenceSourceQueries;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Implementations.EditDistanceScorers.LevenshteinEditDistanceScorers
{
    public class LevenshteinEditDistanceNickleScorer : AbstractLevenshteinEditDistanceScorer
    {

        public LevenshteinEditDistanceNickleScorer()
        {
        }

        public LevenshteinEditDistanceNickleScorer(AttributeWeightingScheme attributeWeightingScheme)
        {
            AttributeWeightingScheme = attributeWeightingScheme;
        }

        public override MatchScoreResult GetMatchScore(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult)
        {
            Serilog.Log.Verbose(this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - entered");

            MatchScoreResult ret = new MatchScoreResult(AttributeWeightingScheme);
            ret.StartTimer();

            try
            {

                // need to experiment to see if using the edit distance of the whole string is better than using each individual part seperately

                //// the following computes the edit distance for the whole street address
                //string wholeInputAddress = StringUtils.ConcatArrayWithCharBetween(new string[] { inputAddress.PreDirectional, inputAddress.Name, inputAddress.Suffix, inputAddress.PostDirectional }, " ");
                //string wholeFeatureAddress = StringUtils.ConcatArrayWithCharBetween(new string[] { featureAddress.PreDirectional, featureAddress.Name, featureAddress.Suffix, featureAddress.PostDirectional }, " ");
                //double wholeStreetAddressPenalty = ComputeWeightedEditDistancePenalty(wholeInputAddress, wholeFeatureAddress, AttributeWeightingScheme.WeightPreDirectional + AttributeWeightingScheme.WeightName + AttributeWeightingScheme.WeightSuffix + AttributeWeightingScheme.WeightPostDirectional);


                // the following computes the edit distance for each portion of the street address 

                if (referenceFeature.AddressComponent == AddressComponents.All)
                {
                    MatchScoreNumberPenaltyResult numberPenalty = ComputePenaltyNumber(parameterSet, inputAddress, referenceFeature, referenceSourceQueryResult, AttributeWeightingScheme);

                    numberPenalty.AddressComponent = AddressComponents.Number;

                    ret.AddressDistance = numberPenalty.AddressDistance;
                    ret.AddressRangeResultType = numberPenalty.AddressRangeResultType;
                    ret.ParityResultType = numberPenalty.ParityResultType;

                    ret.PreferredAddressRangeResultType = numberPenalty.PreferredAddressRangeResultType;
                    ret.PreferredEndResultType = numberPenalty.PreferredEndResultType;
                    ret.AverageBlockSize = numberPenalty.AverageBlockSize;
                    ret.NumberOfBlocksAway = numberPenalty.NumberOfBlocksAway;
                    ret.AddressNumberTypeUsed = numberPenalty.AddressNumberTypeUsed;


                    //ret.AddressDistance = numberPenalty.OverallAddressDistance;
                    //ret.AddressRangeResultType = numberPenalty.OverallAddressRangeResultType;
                    //ret.ParityResultType = numberPenalty.OverallParityResultType;
                    //ret.PreferredAddressRangeResultType = numberPenalty.PreferredAddressRangeResultType;
                    //ret.PreferredEndResultType = numberPenalty.PreferredEndResultType;
                    //ret.AverageBlockSize = numberPenalty.AverageBlockSize;
                    //ret.NumberOfBlocksAway = numberPenalty.NumberOfBlocksAway;

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
                            cityPenalty.AdjustPenalty(cityPenalty.PenaltyValue * .1);
                        }
                    }

                    // if the city is correct but the zip is not, only take a 75% of the zip penalty because it's more likely that the zip is wrong but the city right
                    if (cityPenalty.PenaltyValue == 0)
                    {
                        if (zipPenalty.PenaltyValue > 0)
                        {
                            zipPenalty.AdjustPenalty(zipPenalty.PenaltyValue * .75);
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
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer();

            return ret;
        }

        public MatchScoreNumberPenaltyResult ComputePenaltyNumber(ParameterSet parameterSet, StreetAddress inputAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult, AttributeWeightingScheme attrbuteWeightingScheme)
        {
            Serilog.Log.Verbose(this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - entered");

            MatchScoreNumberPenaltyResult ret = new MatchScoreNumberPenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (referenceFeature.StreetAddressableGeographicFeature.GetType() != typeof(NickleStreet))
                {
                    throw new Exception("LevenshteinEditDistanceNickleScorer only works on type NickleStreet: " + referenceFeature.StreetAddressableGeographicFeature.GetType());
                }
                else
                {

                    // calculate the penalty for both address ranges of the feature (address range and house number) and use the smaller of the two
                    NickleStreet nickleStreet = (NickleStreet)referenceFeature.StreetAddressableGeographicFeature;
                    MatchScoreNumberPenaltyResult addressRangePenalty = ComputePenaltyNumberForTwoRanges(parameterSet, inputAddress, referenceFeature, referenceSourceQueryResult, attrbuteWeightingScheme, nickleStreet.AddressRangeMajor, nickleStreet.AddressRangeMinor, nickleStreet);
                    MatchScoreNumberPenaltyResult houseNumberPenalty = ComputePenaltyNumberForTwoRanges(parameterSet, inputAddress, referenceFeature, referenceSourceQueryResult, attrbuteWeightingScheme, nickleStreet.AddressRangeHouseNumberRangeMajor, nickleStreet.AddressRangeHouseNumberRangeMinor, nickleStreet);
                    FeatureMatchAddressRangePreferredAddressRangeResultType rangeTypeUsed = FeatureMatchAddressRangePreferredAddressRangeResultType.Unknown;

                    if (addressRangePenalty.PenaltyValue <= houseNumberPenalty.PenaltyValue)
                    {
                        ret = addressRangePenalty;
                        rangeTypeUsed = FeatureMatchAddressRangePreferredAddressRangeResultType.AddressRange;
                    }
                    else if (houseNumberPenalty.PenaltyValue < addressRangePenalty.PenaltyValue)
                    {
                        ret = houseNumberPenalty;
                        rangeTypeUsed = FeatureMatchAddressRangePreferredAddressRangeResultType.HouseNumber;
                    }

                    ret.PreferredAddressRangeResultType = rangeTypeUsed;
                    penalty = ret.PenaltyValue / 100.0;

                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyNumber: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }


        // given two address ranges for a street segment (address range and house number), figure out which to use and caluclate the within/outside penalty
        public MatchScoreNumberPenaltyResult ComputePenaltyNumberForTwoRanges(ParameterSet parameterSet, StreetAddress inputAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult, AttributeWeightingScheme attrbuteWeightingScheme, AddressRange addressRangeMajor, AddressRange addressRangeMinor, NickleStreet nickleStreet)
        {
            Serilog.Log.Verbose(this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - entered");

            // there are three types of cases for each address range of the reference feature (address range and house number)
            // (1) range major is a single number and the range minor has the actual variation 23-1 Main - 23-99 Main
            // -- the minor should be used to determine within/outside range, but the major has to be correct, use inputAddress.numberFractional 

            // (2) range major is a variation and the range minor is also a variation 23-1 Main - 29-5 Main
            // -- the major should be used to determine within/outside because the range of the minor is not konwn, ignore inputAddress.numberFractional 

            // (3) range major is a variation and the range minor empty 23 Main - 29 Main
            // -- the major should be used to determine within/outside because the minor is not appliable, inputAddress.numberFractional 

            // (4) range major is a single number and the range minor empty 23 Main - 23 Main
            // -- the minor should be used to determine within/outside range, but the major has to be correct, use inputAddress.numberFractional 

            MatchScoreNumberPenaltyResult ret = new MatchScoreNumberPenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {

                AddressRange addressRange = null;
                AddressNumberType addressNumberTypeUsed = AddressNumberType.Unknown;

                if (addressRangeMajor != null)
                {
                    if (addressRangeMinor != null)
                    {
                        if (addressRangeMajor.Size == 1) // Case (1): e.g., 23-1 Main - 23-5 Main
                        {
                            int inputAddressNumber = Convert.ToInt32(inputAddress.Number);
                            if (inputAddressNumber == addressRangeMajor.FromAddress) // input address must match the major range when major is a single number, e.g., 23-1 main is within 23-1 -> 23-9 main
                            {
                                addressRange = addressRangeMinor;
                                addressNumberTypeUsed = AddressNumberType.Fractional;
                            }
                        }
                        else // Case (2): use the major when major is not a single number even if it has a minor, e.g.,  23-1 Main -> 29-5 Main
                        {
                            addressRange = addressRangeMajor;
                            addressNumberTypeUsed = AddressNumberType.Number;
                        }
                    }
                    else // Case (3): use the major when the minor is null, e.g., 23 Main -> 29 Main
                    {
                        addressRange = addressRangeMajor;
                        addressNumberTypeUsed = AddressNumberType.Number;
                    }
                }

                if (addressRange != null)
                {
                    ret = ComputePenaltyNumberRange(parameterSet, inputAddress, addressRange, addressNumberTypeUsed, referenceSourceQueryResult, nickleStreet);
                    ret.AddressNumberTypeUsed = addressNumberTypeUsed;
                    penalty = ret.PenaltyValue / 100.0;
                }
                else
                {
                    ret.PenaltyRangeValue = AttributeWeightingScheme.ProportionalWeightNumber * 100.0;
                    ret.PenaltyParityValue = AttributeWeightingScheme.ProportionalWeightNumberParity * 100.0;
                    penalty = AttributeWeightingScheme.ProportionalWeightNumber;
                    penalty += AttributeWeightingScheme.ProportionalWeightNumberParity;
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyNumberForTwoRanges: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScoreNumberPenaltyResult ComputePenaltyNumberRange(ParameterSet parameterSet, StreetAddress inputAddress, AddressRange segmentRange, AddressNumberType addressNumberType, ReferenceSourceQueryResult referenceSourceQueryResult, NickleStreet nickleStreet)
        {
            Serilog.Log.Verbose(this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - entered");

            MatchScoreNumberPenaltyResult ret = new MatchScoreNumberPenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {

                bool isWithinRange = false;

                int inputAddressNumber = 0;

                if (addressNumberType == AddressNumberType.Number)
                {
                    if (!String.IsNullOrEmpty(inputAddress.Number))
                    {
                        if (StringUtils.IsInt(inputAddress.Number))
                        {
                            inputAddressNumber = Convert.ToInt32(inputAddress.Number);
                        }
                    }
                }
                else if (addressNumberType == AddressNumberType.Fractional)
                {
                    if (!String.IsNullOrEmpty(inputAddress.NumberFractional))
                    {
                        if (StringUtils.IsInt(inputAddress.NumberFractional))
                        {
                            inputAddressNumber = Convert.ToInt32(inputAddress.NumberFractional);
                        }
                    }
                }

                int inputAddressNumberEvenOdd = inputAddressNumber % 2;

                StreetNumberRangeParity inputAddressNumberParity = StreetNumberRangeParity.Unknown;
                if (inputAddressNumberEvenOdd == 0)
                {
                    inputAddressNumberParity = StreetNumberRangeParity.Even;
                }
                else
                {
                    inputAddressNumberParity = StreetNumberRangeParity.Odd;
                }

                if (segmentRange != null)
                {

                    // check the parity between the input address number and reference address range
                    if (segmentRange.StreetNumberRangeParity == StreetNumberRangeParity.Both)
                    {
                        ret.ParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                    }
                    else if (segmentRange.StreetNumberRangeParity == StreetNumberRangeParity.Unknown)
                    {
                        ret.ParityResultType = FeatureMatchAddressParityResultType.Unknown;
                    }
                    else if (segmentRange.StreetNumberRangeParity == StreetNumberRangeParity.None)
                    {
                        ret.ParityResultType = FeatureMatchAddressParityResultType.Unknown;
                    }
                    else
                    {
                        if (inputAddressNumberParity == segmentRange.StreetNumberRangeParity)
                        {
                            ret.ParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
                        }
                        else
                        {
                            ret.ParityResultType = FeatureMatchAddressParityResultType.IncorrectParity;
                        }
                    }


                    // setup which end is the to and which is the from
                    int rangeNumberFrom = 0;
                    int rangeNumberTo = 0;

                    if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
                    {
                        rangeNumberFrom = segmentRange.FromAddress;
                        rangeNumberTo = segmentRange.ToAddress;
                    }
                    else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
                    {
                        rangeNumberFrom = segmentRange.ToAddress;
                        rangeNumberTo = segmentRange.FromAddress;
                    }
                    else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
                    {
                        rangeNumberFrom = segmentRange.ToAddress;
                        rangeNumberTo = segmentRange.FromAddress;
                    }


                    // check if the input number is within the range
                    if (inputAddressNumber >= rangeNumberFrom && inputAddressNumber <= rangeNumberTo && (rangeNumberFrom > 0 || rangeNumberTo > 0))
                    {
                        isWithinRange = true;
                        ret.AddressRangeResultType = FeatureMatchAddressRangeResultType.WithinRange;
                        ret.AddressDistance = 0;
                    }
                    else
                    {

                        ret.AddressRangeResultType = FeatureMatchAddressRangeResultType.OutsideRange;

                        double fromDistance = Double.MaxValue;
                        double toDistance = Double.MaxValue;

                        if (rangeNumberFrom > 0)
                        {
                            fromDistance = Math.Abs(inputAddressNumber - rangeNumberFrom);
                        }

                        if (rangeNumberTo > 0)
                        {
                            toDistance = Math.Abs(inputAddressNumber - rangeNumberTo);
                        }


                        ret.AddressDistance = Math.Min(fromDistance, toDistance);


                        // set the preferred end of the address range to be the one that's closer to the input address
                        if (ret.AddressDistance != Double.MaxValue)
                        {
                            if (ret.AddressDistance == fromDistance)
                            {
                                if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
                                }
                                else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
                                }
                                else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
                                }
                            }
                            else
                            {
                                if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
                                }
                                else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
                                }
                                else if (segmentRange.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
                                {
                                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
                                }
                            }
                        }
                    }

                }


                if (!isWithinRange)
                {

                    double penaltyProportion = 1.0;

                    if (parameterSet.ShouldAllowNearbyMatches)
                    {
                        if (ret.AddressDistance != Double.MaxValue)
                        {
                            double blockSize = segmentRange.Size;
                            ret.AverageBlockSize = referenceSourceQueryResult.GetAverageBlockSize(nickleStreet.StreetName, nickleStreet.ZIP);
                            ret.NumberOfBlocksAway = 1.0 + (ret.AddressDistance / ret.AverageBlockSize);

                            if (ret.NumberOfBlocksAway < AttributeWeightingScheme.MaxBlocksAway)
                            {
                                penaltyProportion = ret.NumberOfBlocksAway / AttributeWeightingScheme.MaxBlocksAway;
                            }
                        }
                    }

                    ret.PenaltyRangeValue = AttributeWeightingScheme.ProportionalWeightNumber * 100.0;
                    penalty = AttributeWeightingScheme.ProportionalWeightNumber * penaltyProportion;

                }
                else // add a small fraction of penalty to account for size of address range - smaller range, less uncertainty -> less penalty
                {
                    try
                    {
                        if (AttributeWeightingScheme.ShouldTakeAddressRangeHouseUncertaintyPenalty)
                        {
                            double maxexactHouseUncertainty = AttributeWeightingScheme.ProportionalWeightNumberParity * .01; // use it as a maximum of 1% of the full penalty

                            double segmentSize = segmentRange.Size;
                            if (segmentSize <= 0)
                            {
                                segmentSize = 1;
                            }

                            double exactHouseUnceratinty = 1.0 / segmentSize;
                            double exactHouseProbability = 1.0 - exactHouseUnceratinty;
                            exactHouseProbability = exactHouseProbability * maxexactHouseUncertainty;
                            penalty += exactHouseProbability;
                        }
                    }
                    catch (Exception ex2)
                    {
                        ret.ExceptionOccurred = true;
                        ret.Error = "Exception in ComputePenaltyNumberRangeSize: " + ex2.Message;
                        ret.Exception = ex2;
                        Serilog.Log.Error(ex2, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                    }
                }

                ret.PenaltyRangeValue = penalty * 100.0;

                if (ret.ParityResultType != FeatureMatchAddressParityResultType.CorrectParity)
                {
                    penalty += AttributeWeightingScheme.ProportionalWeightNumberParity;
                    ret.PenaltyParityValue = AttributeWeightingScheme.ProportionalWeightNumberParity * 100.0;
                }

            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyNumberRange: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }



        //public MatchScoreNumberPenaltyResult ComputePenaltyNumber(ParameterSet parameterSet, StreetAddress inputAddress, IReferenceFeature referenceFeature, ReferenceSourceQueryResult referenceSourceQueryResult, AttributeWeightingScheme attrbuteWeightingScheme)
        //{
        //    MatchScoreNumberPenaltyResult ret = new MatchScoreNumberPenaltyResult();
        //    ret.StartTimer();

        //    double penalty = 0;

        //    try
        //    {
        //        if (referenceFeature.StreetAddressableGeographicFeature.GetType() != typeof(NickleStreet))
        //        {
        //            throw new Exception("LevenshteinEditDistanceNickleScorer only works on type NickleStreet: " + referenceFeature.StreetAddressableGeographicFeature.GetType());
        //        }
        //        else
        //        {
        //            NickleStreet nickleStreetAddress = (NickleStreet)referenceFeature.StreetAddressableGeographicFeature;


        //            // couple cases need to be checked for a addressNumber-addressFractional combinations on the input/reference features
        //            // (1) input number -> reference major range: input address has only addressNumber and there is no minorRange on the reference feature 
        //            //    (123 Main Street -> 101-199 Main street) 
        //            //    - Use the address range (major)
        //            //    - Average block size is derived from address range

        //            // (2) input number -> reference major range + minor range: input address has only addressNumber and there is a minorRange on the reference feature which is a more accurate range 
        //            //    (123 Main Street -> 101-199 Main street, 109-133 Main street) 
        //            //    - Use the more accurate range (minor)
        //            //    - Average block size is derived from more accurate range (minor)

        //            // (3) input number + fractional -> reference major range: input address has addressNumber and fractional and there is no minorRange on the reference feature which is a more accurate range 
        //            //    (123 1/2 Main Street -> 101-199 Main street) 
        //            //    - Drop the fractional and use the address range (major)
        //            //    - Average block size is derived from address range (major)

        //            // (4) input number + fractional -> reference major range + minor range: input address has addressNumber and fractional and there is a minorRange on the reference feature which is a more accurate range 
        //            //    (123 1/2 Main Street -> 101-199 Main street, 109-133 Main street) 
        //            //    - Drop the fractional and use the mora accurate range (minor)
        //            //    - Average block size is derived from more accurate range (minor)

        //            // (5) input number + house number -> reference major range + house number range: input address has addressNumber and houseNumber and there is no minorRange on the reference feature which is a more accurate range 
        //            //    (123-9 Main Street -> 101-199 Main street) 
        //            //    - Drop the house number and use the address range (major)
        //            //    - Average block size is derived from address range (major)

        //            // (6) input address has addressNumber and houseNumber and the address range is a range and there is a minorRange on the reference feature which is the house number range 
        //            //    (123-9 Main Street -> 101-199 Main street, 101-1 - 199-99 Main street) 
        //            //    - Drop the house number and use the address range (major)
        //            //    - Average block size is derived from address range (major)

        //            // (7) input address has addressNumber and houseNumber and the address range is a singleNumber and there is a minorRange on the reference feature which is the house number range 
        //            //    (123-9 Main Street -> 101-101 Main street, 101-1 - 101-99 Main street) 
        //            //    - Drop the address range and use the house number range (minor)
        //            //    - Average block size is derived from more accurate range (minor)




        //            bool isWithinAnyRange = false;

        //            int inputAddressNumber = Convert.ToInt32(inputAddress.Number);
        //            int inputAddressNumberEvenOdd = inputAddressNumber % 2;

        //            StreetNumberRangeParity inputAddressNumberParity = StreetNumberRangeParity.Unknown;
        //            if (inputAddressNumberEvenOdd == 0)
        //            {
        //                inputAddressNumberParity = StreetNumberRangeParity.Even;
        //            }
        //            else
        //            {
        //                inputAddressNumberParity = StreetNumberRangeParity.Odd;
        //            }

        //            if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor != null) // if there is a house number range on the street, use that
        //            {

        //                // if input address is not within the address range or has the wrong parity, check the house number range
        //                if (ret.AddressRangeMajorResultType != FeatureMatchAddressRangeResultType.WithinRange || ret.AddressRangeParityResultType != FeatureMatchAddressParityResultType.CorrectParity)
        //                {

        //                    if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.Both)
        //                    {
        //                        ret.AddressRangeHouseNumberParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                    }
        //                    else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.Unknown)
        //                    {
        //                        ret.AddressRangeHouseNumberParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                    }
        //                    else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.None)
        //                    {
        //                        ret.AddressRangeHouseNumberParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                    }
        //                    else
        //                    {
        //                        if (inputAddressNumberParity == nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeParity)
        //                        {
        //                            ret.AddressRangeHouseNumberParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                        }
        //                        else
        //                        {
        //                            ret.AddressRangeHouseNumberParityResultType = FeatureMatchAddressParityResultType.IncorrectParity;
        //                        }
        //                    }

        //                    // then check the house number range
        //                    int addressHouseNumberFrom = 0;
        //                    int addressHouseNumberTo = 0;

        //                    if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                    {
        //                        addressHouseNumberFrom = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.FromAddress;
        //                        addressHouseNumberTo = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.ToAddress;
        //                    }
        //                    else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                    {
        //                        addressHouseNumberFrom = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.ToAddress;
        //                        addressHouseNumberTo = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.FromAddress;
        //                    }
        //                    else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                    {
        //                        addressHouseNumberFrom = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.ToAddress;
        //                        addressHouseNumberTo = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.FromAddress;
        //                    }

        //                    if (inputAddressNumber >= addressHouseNumberFrom && inputAddressNumber <= addressHouseNumberTo && (addressHouseNumberFrom > 0 || addressHouseNumberTo > 0))
        //                    {
        //                        ret.AddressRangeHouseNumberMajorResultType = FeatureMatchAddressRangeResultType.WithinRange;
        //                        ret.AddressRangeHouseNumberDistance = 0;
        //                        isWithinAnyRange = true;

        //                        ret.OverallAddressDistance = 0;
        //                        ret.OverallAddressRangeResultType = ret.AddressRangeHouseNumberMajorResultType;
        //                        ret.OverallParityResultType = ret.AddressRangeHouseNumberParityResultType;
        //                        ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.HouseNumber;
        //                    }
        //                    else
        //                    {
        //                        ret.AddressRangeHouseNumberMajorResultType = FeatureMatchAddressRangeResultType.OutsideRange;


        //                        double fromDistance = Double.MaxValue;
        //                        double toDistance = Double.MaxValue;

        //                        if (addressHouseNumberFrom > 0)
        //                        {
        //                            fromDistance = Math.Abs(inputAddressNumber - addressHouseNumberFrom);
        //                        }

        //                        if (addressHouseNumberTo > 0)
        //                        {
        //                            toDistance = Math.Abs(inputAddressNumber - addressHouseNumberTo);
        //                        }


        //                        ret.AddressRangeHouseNumberDistance = Math.Min(fromDistance, toDistance);

        //                        if (ret.AddressRangeHouseNumberDistance != Double.MaxValue)
        //                        {
        //                            // set the end that's closer to the input address
        //                            if (ret.AddressRangeHouseNumberDistance == fromDistance)
        //                            {
        //                                if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeHouseNumberRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                {
        //                                    ret.AddressRangeHouseNumberPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else // otherwise use the address range if there is no house number range
        //            {

        //                if (nickleStreetAddress.AddressRangeMajor != null)
        //                {
        //                    if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeType == StreetNumberRangeType.Numeric)
        //                    {

        //                        // first check the parity of the address range
        //                        if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.Both)
        //                        {
        //                            ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                        }
        //                        else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.Unknown)
        //                        {
        //                            ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                        }
        //                        else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeParity == StreetNumberRangeParity.None)
        //                        {
        //                            ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                        }
        //                        else
        //                        {
        //                            if (inputAddressNumberParity == nickleStreetAddress.AddressRangeMajor.StreetNumberRangeParity)
        //                            {
        //                                ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                            }
        //                            else
        //                            {
        //                                ret.AddressRangeParityResultType = FeatureMatchAddressParityResultType.IncorrectParity;
        //                            }
        //                        }


        //                        // first check the address range
        //                        int addressRangeFrom = 0;
        //                        int addressRangeTo = 0;

        //                        if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                        {
        //                            addressRangeFrom = nickleStreetAddress.AddressRangeMajor.FromAddress;
        //                            addressRangeTo = nickleStreetAddress.AddressRangeMajor.ToAddress;
        //                        }
        //                        else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                        {
        //                            addressRangeFrom = nickleStreetAddress.AddressRangeMajor.ToAddress;
        //                            addressRangeTo = nickleStreetAddress.AddressRangeMajor.FromAddress;
        //                        }
        //                        else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                        {
        //                            addressRangeFrom = nickleStreetAddress.AddressRangeMajor.ToAddress;
        //                            addressRangeTo = nickleStreetAddress.AddressRangeMajor.FromAddress;
        //                        }

        //                        if (inputAddressNumber >= addressRangeFrom && inputAddressNumber <= addressRangeTo && (addressRangeFrom > 0 || addressRangeTo > 0))
        //                        {
        //                            ret.AddressRangeMajorResultType = FeatureMatchAddressRangeResultType.WithinRange;
        //                            ret.AddressRangeMajorDistance = 0;
        //                            isWithinAnyRange = true;

        //                            ret.OverallAddressDistance = 0;
        //                            ret.OverallAddressRangeResultType = ret.AddressRangeMajorResultType;
        //                            ret.OverallParityResultType = ret.AddressRangeParityResultType;
        //                            ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.AddressRange;
        //                        }
        //                        else
        //                        {
        //                            ret.AddressRangeMajorResultType = FeatureMatchAddressRangeResultType.OutsideRange;


        //                            double fromDistance = Double.MaxValue;
        //                            double toDistance = Double.MaxValue;


        //                            if (addressRangeFrom > 0)
        //                            {
        //                                fromDistance = Math.Abs(inputAddressNumber - addressRangeFrom);
        //                            }

        //                            if (addressRangeTo > 0)
        //                            {
        //                                toDistance = Math.Abs(inputAddressNumber - addressRangeTo);
        //                            }


        //                            ret.AddressRangeMajorDistance = Math.Min(fromDistance, toDistance);

        //                            if (ret.AddressRangeMajorDistance != Double.MaxValue)
        //                            {
        //                                // set the end that's closer to the input address
        //                                if (ret.AddressRangeMajorDistance == fromDistance)
        //                                {
        //                                    if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                    }
        //                                    else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                    }
        //                                    else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                    }
        //                                    else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                    }
        //                                    else if (nickleStreetAddress.AddressRangeMajor.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                    {
        //                                        ret.AddressRangePreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                    }
        //                                }
        //                            }
        //                        }







        //                        // if input address is not within the house number range or has the wrong parity, check the super range
        //                        if (ret.AddressRangeHouseNumberMajorResultType != FeatureMatchAddressRangeResultType.WithinRange || ret.AddressRangeHouseNumberParityResultType != FeatureMatchAddressParityResultType.CorrectParity)
        //                        {
        //                            if (nickleStreetAddress.AddressRangeSuper != null)
        //                            {
        //                                if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeParity == StreetNumberRangeParity.Both)
        //                                {
        //                                    ret.AddressRangeSuperParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeParity == StreetNumberRangeParity.Unknown)
        //                                {
        //                                    ret.AddressRangeSuperParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeParity == StreetNumberRangeParity.None)
        //                                {
        //                                    ret.AddressRangeSuperParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                                }
        //                                else
        //                                {
        //                                    if (inputAddressNumberParity == nickleStreetAddress.AddressRangeSuper.StreetNumberRangeParity)
        //                                    {
        //                                        ret.AddressRangeSuperParityResultType = FeatureMatchAddressParityResultType.CorrectParity;
        //                                    }
        //                                    else
        //                                    {
        //                                        ret.AddressRangeSuperParityResultType = FeatureMatchAddressParityResultType.IncorrectParity;
        //                                    }
        //                                }

        //                                int addressSuperFrom = 0;
        //                                int addressSuperTo = 0;

        //                                if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                {
        //                                    addressSuperFrom = nickleStreetAddress.AddressRangeSuper.FromAddress;
        //                                    addressSuperTo = nickleStreetAddress.AddressRangeSuper.ToAddress;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                {
        //                                    addressSuperFrom = nickleStreetAddress.AddressRangeSuper.ToAddress;
        //                                    addressSuperTo = nickleStreetAddress.AddressRangeSuper.FromAddress;
        //                                }
        //                                else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                {
        //                                    addressSuperFrom = nickleStreetAddress.AddressRangeSuper.ToAddress;
        //                                    addressSuperTo = nickleStreetAddress.AddressRangeSuper.FromAddress;
        //                                }

        //                                if (inputAddressNumber >= addressSuperFrom && inputAddressNumber <= addressSuperTo && (addressSuperFrom > 0 || addressSuperTo > 0))
        //                                {
        //                                    ret.AddressRangeSuperResultType = FeatureMatchAddressRangeResultType.WithinRange;
        //                                    ret.AddressSuperDistance = 0;
        //                                    isWithinAnyRange = true;

        //                                    ret.OverallAddressDistance = 0;
        //                                    ret.OverallAddressRangeResultType = ret.AddressRangeSuperResultType;
        //                                    ret.OverallParityResultType = ret.AddressRangeSuperParityResultType;
        //                                    ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.Super;
        //                                }
        //                                else
        //                                {
        //                                    ret.AddressRangeSuperResultType = FeatureMatchAddressRangeResultType.OutsideRange;

        //                                    double fromDistance = Double.MaxValue;
        //                                    double toDistance = Double.MaxValue;

        //                                    if (addressSuperFrom > 0)
        //                                    {
        //                                        fromDistance = Math.Abs(inputAddressNumber - addressSuperFrom);
        //                                    }

        //                                    if (addressSuperTo > 0)
        //                                    {
        //                                        toDistance = Math.Abs(inputAddressNumber - addressSuperTo);
        //                                    }

        //                                    ret.AddressSuperDistance = Math.Min(fromDistance, toDistance);

        //                                    if (ret.AddressSuperDistance != Double.MaxValue)
        //                                    {
        //                                        // set the end that's closer to the input address
        //                                        if (ret.AddressSuperDistance == fromDistance)
        //                                        {
        //                                            if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                            {
        //                                                ret.AddressRangeSuperPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                            }
        //                                            else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                            {
        //                                                ret.AddressRangeSuperPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.LowHi)
        //                                            {
        //                                                ret.AddressRangeSuperPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.HiEnd;
        //                                            }
        //                                            else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.HiLow)
        //                                            {
        //                                                ret.AddressRangeSuperPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                            }
        //                                            else if (nickleStreetAddress.AddressRangeSuper.StreetNumberRangeOrderType == StreetNumberRangeOrderType.SingleNumber)
        //                                            {
        //                                                ret.AddressRangeSuperPreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.LoEnd;
        //                                            }
        //                                        }
        //                                    }

        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }


        //            if (!isWithinAnyRange)
        //            {

        //                // weight the penalty by the number of blocks the address is away by
        //                ret.OverallAddressDistance = Math.Min(ret.AddressSuperDistance, Math.Min(ret.AddressRangeHouseNumberDistance, ret.AddressRangeMajorDistance));


        //                double maxBlocks = AttributeWeightingScheme.MaxBlocksAway;
        //                double blockSize = 0;
        //                double averageBlockSize = referenceSourceQueryResult.GetAverageBlockSize(nickleStreetAddress.Name, nickleStreetAddress.ZIP);
        //                double blocksAway = 0;
        //                double distance = ret.OverallAddressDistance;

        //                // if the distance is infinity, triple the output penalty
        //                if (Double.MaxValue == distance)
        //                {
        //                    blockSize = Double.MaxValue;
        //                    ret.OverallAddressRangeResultType = FeatureMatchAddressRangeResultType.OutsideRange;
        //                    ret.OverallParityResultType = FeatureMatchAddressParityResultType.Unknown;
        //                    ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.Unknown;
        //                    ret.PreferredEndResultType = FeatureMatchAddressRangePreferredEndResultType.Unknown;

        //                    penalty = AttributeWeightingScheme.ProportionalWeightNumber * 3.0;
        //                }
        //                else
        //                {

        //                    // pick the address range with the minimum distance and set the output parity, range types, and preffered endpoint to its values
        //                    if (distance == ret.AddressRangeMajorDistance)
        //                    {
        //                        blockSize = nickleStreetAddress.AddressRangeMajor.Size;
        //                        ret.OverallAddressRangeResultType = ret.AddressRangeMajorResultType;
        //                        ret.OverallParityResultType = ret.AddressRangeParityResultType;
        //                        ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.AddressRange;
        //                        ret.PreferredEndResultType = ret.AddressRangePreferredEndResultType;
        //                    }
        //                    else if (distance == ret.AddressRangeHouseNumberDistance)
        //                    {
        //                        blockSize = nickleStreetAddress.AddressRangeHouseNumberRangeMajor.Size;
        //                        ret.OverallAddressRangeResultType = ret.AddressRangeHouseNumberMajorResultType;
        //                        ret.OverallParityResultType = ret.AddressRangeHouseNumberParityResultType;
        //                        ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.HouseNumber;
        //                        ret.PreferredEndResultType = ret.AddressRangeHouseNumberPreferredEndResultType;
        //                    }
        //                    else if (distance == ret.AddressSuperDistance)
        //                    {
        //                        blockSize = nickleStreetAddress.AddressRangeSuper.Size;
        //                        ret.OverallAddressRangeResultType = ret.AddressRangeSuperResultType;
        //                        ret.OverallParityResultType = ret.AddressRangeSuperParityResultType;
        //                        ret.PreferredAddressRangeResultType = FeatureMatchAddressRangePreferredAddressRangeResultType.Super;
        //                        ret.PreferredEndResultType = ret.AddressRangeSuperPreferredEndResultType;
        //                    }


        //                    blocksAway = 1.0 + (distance / averageBlockSize);



        //                    ret.AverageBlockSize = averageBlockSize;
        //                    ret.NumberOfBlocksAway = blocksAway;

        //                    if (blocksAway >= maxBlocks)
        //                    {
        //                        penalty = AttributeWeightingScheme.ProportionalWeightNumber;
        //                    }
        //                    else
        //                    {
        //                        double proportionOfMaxBlocksAway = blocksAway / maxBlocks;
        //                        penalty = AttributeWeightingScheme.ProportionalWeightNumber * proportionOfMaxBlocksAway;
        //                    }
        //                }
        //            }

        //            ret.PenaltyRangeValue = penalty * 100.0;

        //            if (ret.OverallParityResultType != FeatureMatchAddressParityResultType.CorrectParity)
        //            {
        //                penalty += AttributeWeightingScheme.ProportionalWeightNumberParity;
        //                ret.PenaltyParityValue = AttributeWeightingScheme.ProportionalWeightNumberParity * 100.0;
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ret.ExceptionOccurred = true;
        //        ret.Error = "Exception in ComputePenaltyNumber: " + e.Message;
        //        ret.Exception = e;
        //    }

        //    ret.EndTimer(penalty);

        //    return ret;
        //}
    }
}