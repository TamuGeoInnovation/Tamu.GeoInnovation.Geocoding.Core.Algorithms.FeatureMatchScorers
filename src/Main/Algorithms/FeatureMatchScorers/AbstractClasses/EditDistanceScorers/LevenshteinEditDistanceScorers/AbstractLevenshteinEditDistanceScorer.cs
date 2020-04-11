using System;
using System.Collections;
using System.Reflection;
using System.Text;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.Directionals;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.PostQualifiers;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.PreArticles;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.PreQualifiers;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.PreTypes;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.DeliveryAddressLines.PrimaryStreetAddresses;
using USC.GISResearchLab.AddressProcessing.Core.Standardizing.StandardizedAddresses.Lines.LastLines;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.StringComparators.EditDistances;
using USC.GISResearchLab.Common.Core.TextEncodings.Soundex;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.MatchScoreResults;
using USC.GISResearchLab.Geocoding.Core.Queries.Parameters;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses.EditDistanceScorers.LevenshteinEditDistanceScorers
{
    public abstract class AbstractLevenshteinEditDistanceScorer : AbstractEditDistanceScorer
    {

        public Hashtable StringToNumberCache { get; set; }


        public AbstractLevenshteinEditDistanceScorer() : base()
        {
            StringToNumberCache = new Hashtable();
        }

        public AbstractLevenshteinEditDistanceScorer(AttributeWeightingScheme attributeWeightingScheme)
            : base(attributeWeightingScheme)
        {
            StringToNumberCache = new Hashtable();
        }

        public MatchScorePenaltyResult ComputePenaltyPreDirectional(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                string inputPreDirectional = inputAddress.PreDirectional;
                string featurePreDirectional = featureAddress.PreDirectional;
                string featurePreDirectionalAlias = featureAddress.AliasPreDirectional;
                string inputPostDirectional = inputAddress.PostDirectional;
                string featurePostDirectional = featureAddress.PostDirectional;
                string featurePostDirectionalAlias = featureAddress.AliasPostDirectional;

                if (DirectionalUtils.isDirectional(inputPreDirectional))
                {
                    inputPreDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(inputPreDirectional);
                }

                if (DirectionalUtils.isDirectional(featurePreDirectional))
                {
                    featurePreDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePreDirectional);
                }
                else if (DirectionalUtils.isDirectional(featurePreDirectionalAlias))
                {
                    featurePreDirectionalAlias = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePreDirectionalAlias);
                }

                if (DirectionalUtils.isDirectional(inputPostDirectional))
                {
                    inputPostDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(inputPostDirectional);
                }

                if (DirectionalUtils.isDirectional(featurePostDirectional))
                {
                    featurePostDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePostDirectional);
                }
                else if (DirectionalUtils.isDirectional(featurePostDirectionalAlias))
                {
                    featurePostDirectionalAlias = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePostDirectionalAlias);
                }

                if (String.Compare(inputPreDirectional, featurePreDirectional, true) != 0)
                {

                    // if the error is an ommission from the input string or reference data 
                    // only take half the full weight after 
                    // checking that pre/post were not reversed

                    if (String.IsNullOrEmpty(inputPreDirectional) || String.IsNullOrEmpty(featurePreDirectional)) // one or the other or both are null
                    {
                        if (String.IsNullOrEmpty(inputPreDirectional) && String.IsNullOrEmpty(featurePreDirectional)) // both are null
                        {
                            penalty = 0;
                        }
                        else if (String.IsNullOrEmpty(inputPreDirectional)) // input pre is null (reference pre is not null)
                        {
                            if (!String.IsNullOrEmpty(inputPostDirectional)) // check that the input post was not used by accident
                            {

                                // if the refernce post is null, the input post may have been used instead of the input pre
                                if (String.IsNullOrEmpty(featurePostDirectional))
                                {

                                    // compare the values inputPost vs referncePre

                                    if (String.Compare(inputPostDirectional, featurePreDirectional, true) == 0) // if they are the same, take 10% of the wieght
                                    {
                                        penalty = (fullWeight * .1);
                                    }
                                    else // otherwise take the full weight
                                    {
                                        penalty = fullWeight;
                                    }
                                }
                                else // the reference has a pre and a post, take the full weight
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else // input pre and post are empty, use full or proportional weight
                            {
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PreDirectional))
                                {
                                    // if attribute relaxation is allowed and is allowed on this attribute take half the weight
                                    penalty = (fullWeight * .5);
                                }

                                else
                                {
                                    // if the error is an inccorrect value in the input string take the full weight
                                    penalty = fullWeight;
                                }
                            }

                        }
                        else if (String.IsNullOrEmpty(featurePreDirectional)) // reference pre is null (input pre is not null)
                        {
                            if (!String.IsNullOrEmpty(featurePostDirectional)) // check that the feaure post was not used by accident
                            {

                                // if the input post is null, the refernce post may have been used instead of the reference pre
                                if (String.IsNullOrEmpty(inputPostDirectional))
                                {
                                    // compare the values referencePost vs inputPre
                                    if (String.Compare(featurePostDirectional, inputPreDirectional, true) == 0) // if they are the same take 10% of the weight
                                    {
                                        penalty = (fullWeight * .1);
                                    }
                                    else // otherwise take the full weight
                                    {
                                        penalty = fullWeight;
                                    }
                                }
                                else // the input has a pre and a post, take the full weight
                                {
                                    penalty = fullWeight;
                                }

                            }
                            else if (!String.IsNullOrEmpty(featurePreDirectionalAlias)) // the feature pre directional may be on the feature alias instead of the feature
                            {
                                // compare the values inputPre vs referncePreAlias

                                if (String.Compare(inputPreDirectional, featurePreDirectionalAlias, true) == 0) // if they are the same, take 10% of the wieght
                                {
                                    penalty = (fullWeight * .1);
                                }
                                else // otherwise take the full weight
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else // reference pre and post are empty, use full or proportional weight
                            {
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PreDirectional))
                                {
                                    // if attribute relaxation is allowed and is allowed on this attribute take half the weight
                                    penalty = (fullWeight * .5);
                                }

                                else
                                {
                                    // if the error is an inccorrect value in the input string take the full weight
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                    else
                    {
                        // if the error is an inccorrect value in the input string take the full weight or partial weight if partially correct based on if the first or second char matches

                        if (inputPreDirectional.Length == 2 && featurePreDirectional.Length == 1) // 123 NW main -> 123 N Main && 123 NW main -> 123 W Main
                        {
                            if (inputPreDirectional.Contains(featurePreDirectional))
                            {
                                if (inputPreDirectional.IndexOf(featurePreDirectional) == 0) // 123 NW main -> 123 N Main - take 50% of weight
                                {
                                    penalty = (fullWeight * .5);
                                }
                                else if (inputPreDirectional.IndexOf(featurePreDirectional) == 1) // 123 NW main -> 123 W Main - take 75% of weight
                                {
                                    penalty = (fullWeight * .75);
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                        else if (inputPreDirectional.Length == 1 && featurePreDirectional.Length == 2) // 123 N main -> 123 NW Main && 123 W main -> 123 NW Main
                        {
                            if (featurePreDirectional.Contains(inputPreDirectional))
                            {
                                if (featurePreDirectional.IndexOf(inputPreDirectional) == 0) // 123 N main -> 123 NW Main  - take 50% of weight
                                {
                                    penalty = (fullWeight * .5);
                                }
                                else if (featurePreDirectional.IndexOf(inputPreDirectional) == 1) // 123 W main -> 123 NW Main - take 75% of weight
                                {
                                    penalty = (fullWeight * .75);
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                        else
                        {
                            penalty = fullWeight;
                        }
                    }
                }
                else // if the inputPre == featurePre, might have to take small penalty if inputPre is null and inputPre != aliasPre
                {
                    if (!String.IsNullOrEmpty(inputPreDirectional) && !String.IsNullOrEmpty(featurePreDirectionalAlias))
                    {
                        if (String.Compare(inputPreDirectional, featurePreDirectionalAlias, true) != 0)
                        {
                            if (String.IsNullOrEmpty(inputPreDirectional)) // input and feature are empty, alias is not: 123 PCH -> 123 PCH | 123 W PCH
                            {
                                penalty = (fullWeight * .1);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPreDirectionional: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPostDirectional(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                string inputPreDirectional = inputAddress.PreDirectional;
                string featurePreDirectional = featureAddress.PreDirectional;
                string featurePreDirectionalAlias = featureAddress.AliasPreDirectional;
                string inputPostDirectional = inputAddress.PostDirectional;
                string featurePostDirectional = featureAddress.PostDirectional;
                string featurePostDirectionalAlias = featureAddress.AliasPostDirectional;

                if (DirectionalUtils.isDirectional(inputPreDirectional))
                {
                    inputPreDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(inputPreDirectional);
                }

                if (DirectionalUtils.isDirectional(featurePreDirectional))
                {
                    featurePreDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePreDirectional);
                }
                else if (DirectionalUtils.isDirectional(featurePreDirectionalAlias))
                {
                    featurePreDirectionalAlias = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePreDirectionalAlias);
                }

                if (DirectionalUtils.isDirectional(inputPostDirectional))
                {
                    inputPostDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(inputPostDirectional);
                }

                if (DirectionalUtils.isDirectional(featurePostDirectional))
                {
                    featurePostDirectional = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePostDirectional);
                }
                else if (DirectionalUtils.isDirectional(featurePostDirectionalAlias))
                {
                    featurePostDirectionalAlias = DirectionalUtils.getDirectionalOfficialAbbreviation(featurePostDirectionalAlias);
                }

                if (String.Compare(inputPostDirectional, featurePostDirectional, true) != 0)
                {

                    // if the error is an ommission from the input string or reference data 
                    // only take half the full weight after 
                    // checking that pre/post were not reversed

                    if (String.IsNullOrEmpty(inputPostDirectional) || String.IsNullOrEmpty(featurePostDirectional)) // one or the other or both are null
                    {

                        if (String.IsNullOrEmpty(inputPostDirectional) && String.IsNullOrEmpty(featurePostDirectional)) // both are null
                        {
                            penalty = 0;
                        }
                        else if (String.IsNullOrEmpty(inputPostDirectional)) // input post is null (feaure post is not null)
                        {
                            if (!String.IsNullOrEmpty(inputPreDirectional)) // check that the input prewas not used by accident
                            {

                                // if the refernce pre is null, the input pree may have been used instead of the input post
                                if (String.IsNullOrEmpty(featurePreDirectional))
                                {

                                    // compare the values inputPre vs referncePost

                                    if (String.Compare(inputPreDirectional, featurePostDirectional, true) == 0) // if they are the same, take 10% of the wieght
                                    {
                                        penalty = (fullWeight * .1);
                                    }
                                    else // otherwise take the full weight
                                    {
                                        penalty = fullWeight;
                                    }
                                }
                                else // the reference has a pre and a post, take the full weight
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else // input pre and post are empty, use full or proportional weight
                            {
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PostDirectional))
                                {
                                    // if attribute relaxation is allowed and is allowed on this attribute take half the weight
                                    penalty = (fullWeight * .5);
                                }
                                else
                                {
                                    // if the error is an inccorrect value in the input string take the full weight
                                    penalty = fullWeight;
                                }
                            }
                        }
                        else if (String.IsNullOrEmpty(featurePostDirectional)) // feature post is null (input post is not null)
                        {
                            if (!String.IsNullOrEmpty(featurePreDirectional)) // check that the feaure pre was not used by accident
                            {

                                // if the input pre is null, the refernce pre may have been used instead of the reference post
                                if (String.IsNullOrEmpty(inputPreDirectional))
                                {
                                    // compare the values referencePrePost vs inputPost
                                    if (String.Compare(featurePreDirectional, inputPostDirectional, true) == 0) // if they are the same take 10% of the weight
                                    {
                                        penalty = (fullWeight * .1);
                                    }
                                    else // otherwise take the full weight
                                    {
                                        penalty = fullWeight;
                                    }
                                }
                                else // the input has a pre and a post, take the full weight
                                {
                                    penalty = fullWeight;
                                }

                            }
                            else if (!String.IsNullOrEmpty(featurePostDirectionalAlias)) // the feature post directional may be on the feature alias instead of the feature
                            {
                                // compare the values inputPost vs referncePostAlias

                                if (String.Compare(inputPostDirectional, featurePostDirectionalAlias, true) == 0) // if they are the same, take 10% of the wieght
                                {
                                    penalty = (fullWeight * .1);
                                }
                                else // otherwise take the full weight
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else // reference pre and post are empty, use full or proportional weight
                            {
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PostDirectional))
                                {
                                    // if attribute relaxation is allowed and is allowed on this attribute take half the weight
                                    penalty = (fullWeight * .5);
                                }

                                else
                                {
                                    // if the error is an inccorrect value in the input string take the full weight
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                    else
                    {
                        // if the error is an inccorrect value in the input string take the full weight or partial weight if partially correct based on if the first or second char matches

                        if (inputPostDirectional.Length == 2 && featurePostDirectional.Length == 1) // 123 NW main -> 123 N Main && 123 NW main -> 123 W Main
                        {
                            if (inputPostDirectional.Contains(featurePostDirectional))
                            {
                                if (inputPostDirectional.IndexOf(featurePostDirectional) == 0) // 123 NW main -> 123 N Main - take 50% of weight
                                {
                                    penalty = (fullWeight * .5);
                                }
                                else if (inputPostDirectional.IndexOf(featurePostDirectional) == 1) // 123 NW main -> 123 W Main - take 75% of weight
                                {
                                    penalty = (fullWeight * .75);
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }

                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                        else if (inputPostDirectional.Length == 1 && featurePostDirectional.Length == 2) // 123 N main -> 123 NW Main && 123 W main -> 123 NW Main
                        {
                            if (featurePostDirectional.Contains(inputPostDirectional))
                            {
                                if (featurePostDirectional.IndexOf(inputPostDirectional) == 0) // 123 N main -> 123 NW Main  - take 50% of weight
                                {
                                    penalty = (fullWeight * .5);
                                }
                                else if (featurePostDirectional.IndexOf(inputPostDirectional) == 1) // 123 W main -> 123 NW Main - take 75% of weight
                                {
                                    penalty = (fullWeight * .75);
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                        else
                        {
                            penalty = fullWeight;
                        }
                    }
                }
                else // if the inputPost == featurePost, might have to take small penalty if inputPost is null and inputPost != aliasPost
                {
                    if (!String.IsNullOrEmpty(inputPostDirectional) && !String.IsNullOrEmpty(featurePostDirectionalAlias))
                    {
                        if (String.Compare(inputPostDirectional, featurePostDirectionalAlias, true) != 0)
                        {
                            if (String.IsNullOrEmpty(inputPostDirectional)) // input and feature are empty, alias is not: 123 PCH -> 123 PCH | 123 PCH W
                            {
                                penalty = (fullWeight * .1);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPostDirectiononal: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPreArticle(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.PreArticle, featureAddress.PreArticle, true) != 0)
                {
                    if (!String.IsNullOrEmpty(inputAddress.PreArticle) || !String.IsNullOrEmpty(featureAddress.PreArticle))
                    {
                        // if the error is an ommission from the input string or reference data  only take half the full weight 
                        if (String.IsNullOrEmpty(inputAddress.PreArticle) || String.IsNullOrEmpty(featureAddress.PreArticle))
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PreArticle))
                            {
                                penalty = (fullWeight / 2);
                            }

                            else
                            {
                                // if the error is an inccorrect value in the input string take the full weight
                                penalty = fullWeight;
                            }
                        }
                        else
                        {

                            PreArticleMatcher matcher = new PreArticleMatcher();

                            if (matcher.HasMatch(0, inputAddress.PreArticle) || matcher.HasMatch(0, featureAddress.PreArticle))
                            {
                                if (matcher.HasMatch(0, inputAddress.PreArticle) && matcher.HasMatch(0, featureAddress.PreArticle))
                                {
                                    string inputPreArticleOfficial = matcher.GetLongestMatch(0, inputAddress.PreArticle).OfficialAbbreviation;
                                    string featurePreArticleOfficial = matcher.GetLongestMatch(0, featureAddress.PreArticle).OfficialAbbreviation;

                                    if (String.Compare(inputPreArticleOfficial, featurePreArticleOfficial, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(inputPreArticleOfficial, featurePreArticleOfficial, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPreArticle: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPostArticle(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.PostArticle, featureAddress.PostArticle, true) != 0)
                {
                    if (!String.IsNullOrEmpty(inputAddress.PostArticle) || !String.IsNullOrEmpty(featureAddress.PostArticle))
                    {
                        // if the error is an ommission from the input string or reference data  only take half the full weight 
                        if (String.IsNullOrEmpty(inputAddress.PostArticle) || String.IsNullOrEmpty(featureAddress.PostArticle))
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PostArticle))
                            {
                                penalty = (fullWeight / 2);
                            }

                            else
                            {
                                // if the error is an inccorrect value in the input string take the full weight
                                penalty = fullWeight;
                            }
                        }
                        else
                        {

                            PreArticleMatcher matcher = new PreArticleMatcher();

                            if (matcher.HasMatch(0, inputAddress.PostArticle) || matcher.HasMatch(0, featureAddress.PostArticle))
                            {
                                if (matcher.HasMatch(0, inputAddress.PostArticle) && matcher.HasMatch(0, featureAddress.PostArticle))
                                {
                                    string inputPostArticleOfficial = matcher.GetLongestMatch(0, inputAddress.PostArticle).OfficialAbbreviation;
                                    string featurePostArticleOfficial = matcher.GetLongestMatch(0, featureAddress.PostArticle).OfficialAbbreviation;

                                    if (String.Compare(inputPostArticleOfficial, featurePostArticleOfficial, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(inputPostArticleOfficial, featurePostArticleOfficial, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPostArticle: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPreType(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.PreType, featureAddress.PreType, true) != 0)
                {
                    if (!String.IsNullOrEmpty(inputAddress.PreType) || !String.IsNullOrEmpty(featureAddress.PreType))
                    {
                        // if the error is an ommission from the input string or reference data  only take half the full weight 
                        if (String.IsNullOrEmpty(inputAddress.PreType) || String.IsNullOrEmpty(featureAddress.PreType))
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PreType))
                            {
                                penalty = (fullWeight / 2);
                            }

                            else
                            {
                                // if the error is an inccorrect value in the input string take the full weight
                                penalty = fullWeight;
                            }
                        }
                        else
                        {

                            PreTypeMatcher matcher = new PreTypeMatcher();

                            if (matcher.HasMatch(0, inputAddress.PreType) || matcher.HasMatch(0, featureAddress.PreType))
                            {
                                if (matcher.HasMatch(0, inputAddress.PreType) && matcher.HasMatch(0, featureAddress.PreType))
                                {
                                    string inputPreTypeOfficial = matcher.GetLongestMatch(0, inputAddress.PreType).OfficialAbbreviation;
                                    string featurePreTypeOfficial = matcher.GetLongestMatch(0, featureAddress.PreType).OfficialAbbreviation;

                                    if (String.Compare(inputPreTypeOfficial, featurePreTypeOfficial, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(inputPreTypeOfficial, featurePreTypeOfficial, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPreType: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPreQualifier(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.PreQualifier, featureAddress.PreQualifier, true) != 0)
                {
                    if (!String.IsNullOrEmpty(inputAddress.PreQualifier) || !String.IsNullOrEmpty(featureAddress.PreQualifier))
                    {
                        // if the error is an ommission from the input string or reference data  only take half the full weight 
                        if (String.IsNullOrEmpty(inputAddress.PreQualifier) || String.IsNullOrEmpty(featureAddress.PreQualifier))
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PreQualifier))
                            {
                                penalty = (fullWeight / 2);
                            }

                            else
                            {
                                // if the error is an inccorrect value in the input string take the full weight
                                penalty = fullWeight;
                            }
                        }
                        else
                        {

                            PreQualifierMatcher matcher = new PreQualifierMatcher();

                            if (matcher.HasMatch(0, inputAddress.PreQualifier) || matcher.HasMatch(0, featureAddress.PreQualifier))
                            {
                                if (matcher.HasMatch(0, inputAddress.PreQualifier) && matcher.HasMatch(0, featureAddress.PreQualifier))
                                {
                                    string inputPreQualifierOfficial = matcher.GetLongestMatch(0, inputAddress.PreQualifier).OfficialAbbreviation;
                                    string featurePreQualifierOfficial = matcher.GetLongestMatch(0, featureAddress.PreQualifier).OfficialAbbreviation;

                                    if (String.Compare(inputPreQualifierOfficial, featurePreQualifierOfficial, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(inputPreQualifierOfficial, featurePreQualifierOfficial, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPreQualifier: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyPostQualifier(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.PostQualifier, featureAddress.PostQualifier, true) != 0)
                {
                    if (!String.IsNullOrEmpty(inputAddress.PostQualifier) || !String.IsNullOrEmpty(featureAddress.PostQualifier))
                    {
                        // if the error is an ommission from the input string or reference data  only take half the full weight 
                        if (String.IsNullOrEmpty(inputAddress.PostQualifier) || String.IsNullOrEmpty(featureAddress.PostQualifier))
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PostQualifier))
                            {
                                penalty = (fullWeight / 2);
                            }

                            else
                            {
                                // if the error is an inccorrect value in the input string take the full weight
                                penalty = fullWeight;
                            }
                        }
                        else
                        {

                            PostQualifierMatcher matcher = new PostQualifierMatcher();

                            if (matcher.HasMatch(0, inputAddress.PostQualifier) || matcher.HasMatch(0, featureAddress.PostQualifier))
                            {
                                if (matcher.HasMatch(0, inputAddress.PostQualifier) && matcher.HasMatch(0, featureAddress.PostQualifier))
                                {
                                    string inputPostQualifierOfficial = matcher.GetLongestMatch(0, inputAddress.PostQualifier).OfficialAbbreviation;
                                    string featurePostQualifierOfficial = matcher.GetLongestMatch(0, featureAddress.PostQualifier).OfficialAbbreviation;

                                    if (String.Compare(inputPostQualifierOfficial, featurePostQualifierOfficial, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(inputPostQualifierOfficial, featurePostQualifierOfficial, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyPostQualifier: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyName(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputAddress.StreetName, featureAddress.StreetName, true) != 0)
                {
                    // if both of them are numbers, compare the numeric values and return full weight if not equivalent
                    if ((inputAddress.NameIsNumericAbbreviation || inputAddress.NameIsNumberWords || inputAddress.NameIsNumber) && (featureAddress.NameIsNumericAbbreviation || featureAddress.NameIsNumberWords || featureAddress.NameIsNumber))
                    {
                        string inputAddressNumberString = "";
                        string featureAddressNumberString = "";

                        int inputAddressNumber = -1;
                        int featureAddressNumber = -1;

                        if (StringToNumberCache.Contains(inputAddress.StreetName.ToLower()))
                        {
                            inputAddressNumber = (int)StringToNumberCache[inputAddress.StreetName.ToLower()];
                        }
                        else
                        {
                            if (inputAddress.NameIsNumericAbbreviation)
                            {
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddress.StreetName, true);
                                inputAddressNumber = NumericStringManager.GetNumberPartOfNumericAbbreviation(inputAddress.StreetName);
                            }
                            else if (inputAddress.NameIsNumberWords)
                            {
                                inputAddressNumber = NumericStringManager.WordsToInteger(inputAddress.StreetName);
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                            }
                            else if (inputAddress.NameIsNumber)
                            {
                                inputAddressNumber = Convert.ToInt32(inputAddress.StreetName);
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                            }

                            StringToNumberCache.Add(inputAddress.StreetName.ToLower(), inputAddressNumber);
                        }

                        if (StringToNumberCache.Contains(featureAddress.StreetName.ToLower()))
                        {
                            featureAddressNumber = (int)StringToNumberCache[featureAddress.StreetName.ToLower()];
                        }
                        else
                        {
                            if (featureAddress.NameIsNumericAbbreviation)
                            {
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddress.StreetName, true);
                                featureAddressNumber = NumericStringManager.GetNumberPartOfNumericAbbreviation(featureAddress.StreetName);
                            }
                            else if (featureAddress.NameIsNumberWords)
                            {
                                featureAddressNumber = NumericStringManager.WordsToInteger(featureAddress.StreetName);
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                            }
                            else if (featureAddress.NameIsNumber)
                            {
                                featureAddressNumber = Convert.ToInt32(featureAddress.StreetName);
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                            }
                            StringToNumberCache.Add(featureAddress.StreetName.ToLower(), featureAddressNumber);
                        }


                        if (inputAddressNumber > 0 && featureAddressNumber > 0)
                        {
                            if (inputAddressNumber != featureAddressNumber)
                            {
                                penalty = fullWeight;
                            }
                        }
                        else
                        {
                            penalty = fullWeight;
                        }
                    }
                    else if (inputAddress.NameIsNumericAbbreviation || featureAddress.NameIsNumericAbbreviation || inputAddress.NameIsNumberWords || featureAddress.NameIsNumberWords || inputAddress.NameIsNumber || featureAddress.NameIsNumber) // if one of them is a number compare the spelled out number text edit distances
                    {
                        string inputAddressNumberString = "";
                        string featureAddressNumberString = "";

                        int inputAddressNumber = -1;
                        int featureAddressNumber = -1;

                        if (StringToNumberCache.Contains(inputAddress.StreetName.ToLower()))
                        {
                            inputAddressNumber = (int)StringToNumberCache[inputAddress.StreetName.ToLower()];
                        }
                        else
                        {
                            if (inputAddress.NameIsNumericAbbreviation)
                            {
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddress.StreetName, true);
                                inputAddressNumber = NumericStringManager.GetNumberPartOfNumericAbbreviation(inputAddress.StreetName);
                            }
                            else if (inputAddress.NameIsNumberWords)
                            {
                                inputAddressNumber = NumericStringManager.WordsToInteger(inputAddress.StreetName);
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                            }
                            else if (inputAddress.NameIsNumber)
                            {
                                inputAddressNumber = Convert.ToInt32(inputAddress.StreetName);
                                inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                            }
                            else
                            {
                                inputAddressNumberString = inputAddress.StreetName;
                            }

                            StringToNumberCache.Add(inputAddress.StreetName.ToLower(), inputAddressNumber);
                        }

                        if (StringToNumberCache.Contains(featureAddress.StreetName.ToLower()))
                        {
                            featureAddressNumber = (int)StringToNumberCache[featureAddress.StreetName.ToLower()];
                        }
                        else
                        {
                            if (featureAddress.NameIsNumericAbbreviation)
                            {
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddress.StreetName, true);
                                featureAddressNumber = NumericStringManager.GetNumberPartOfNumericAbbreviation(featureAddress.StreetName);
                            }
                            else if (featureAddress.NameIsNumberWords)
                            {
                                featureAddressNumber = NumericStringManager.WordsToInteger(featureAddress.StreetName);
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                            }
                            else if (featureAddress.NameIsNumber)
                            {
                                featureAddressNumber = Convert.ToInt32(featureAddress.StreetName);
                                featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                            }
                            else
                            {
                                featureAddressNumberString = featureAddress.StreetName;
                            }

                            StringToNumberCache.Add(featureAddress.StreetName.ToLower(), featureAddressNumber);
                        }


                        if (inputAddressNumber > 0 && featureAddressNumber > 0)
                        {
                            if (inputAddressNumber != featureAddressNumber)
                            {
                                penalty = fullWeight;
                            }
                        }
                        else
                        {
                            if (inputAddressNumber > 0)
                            {
                                if (inputAddress.NameIsNumericAbbreviation)
                                {
                                    inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddress.StreetName, true);
                                }
                                else if (inputAddress.NameIsNumberWords)
                                {
                                    inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                                }
                                else if (inputAddress.NameIsNumber)
                                {
                                    inputAddressNumberString = NumericStringManager.IntegerToWords(inputAddressNumber, true);
                                }
                            }
                            else
                            {
                                inputAddressNumberString = inputAddress.StreetName;
                            }

                            if (featureAddressNumber > 0)
                            {
                                if (featureAddress.NameIsNumericAbbreviation)
                                {
                                    featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddress.StreetName, true);
                                }
                                else if (featureAddress.NameIsNumberWords)
                                {
                                    featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                                }
                                else if (featureAddress.NameIsNumber)
                                {
                                    featureAddressNumberString = NumericStringManager.IntegerToWords(featureAddressNumber, true);
                                }
                            }
                            else
                            {
                                featureAddressNumberString = featureAddress.StreetName;
                            }

                            if (new PreTypeMatcher().HasMatch(0, featureAddress.StreetName)) // need to check if either the reference has a pre-type because reference feature may not be parsed
                            {
                                if (!String.IsNullOrEmpty(inputAddress.PreType))
                                {
                                    string preTypeAndName = inputAddress.PreType + " " + inputAddress.StreetName;
                                    if (String.Compare(preTypeAndName, featureAddress.StreetName, true) != 0)
                                    {
                                        penalty = ComputeWeightedEditDistancePenalty(preTypeAndName, featureAddress.StreetName, fullWeight);
                                    }
                                }
                                else
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddressNumberString, featureAddressNumberString, fullWeight);
                            }
                        }
                    }
                    else // if neither of them are a number, compare the edit distances after making sure that pre-type, suffix, and saint errors in the reference feature attributes are accounted for
                    {

                        if (new PreTypeMatcher().HasMatch(0, featureAddress.StreetName)) // need to check if either the input or reference has a pret-type because parsing could have been messed up or the reference feature may not be parsed
                        {
                            if (!String.IsNullOrEmpty(inputAddress.PreType))
                            {
                                string preTypeAndName = inputAddress.PreType + " " + inputAddress.StreetName;
                                if (String.Compare(preTypeAndName, featureAddress.StreetName, true) != 0)
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(preTypeAndName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        else if (new PreArticleMatcher().HasMatch(0, featureAddress.StreetName)) // need to check if either the input or reference has a pre-Article because parsing could have been messed up or the reference feature may not be parsed
                        {
                            if (!String.IsNullOrEmpty(inputAddress.PreType))
                            {
                                string preArticleAndName = inputAddress.PreArticle + " " + inputAddress.StreetName;
                                if (String.Compare(preArticleAndName, featureAddress.StreetName, true) != 0)
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(preArticleAndName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        else if (new PostQualifierMatcher().HasMatch(0, featureAddress.StreetName)) // need to check if either the input or reference has a post-Article because parsing could have been messed up or the reference feature may not be parsed
                        {
                            if (!String.IsNullOrEmpty(inputAddress.PostArticle))
                            {
                                string postArticleAndName = inputAddress.StreetName + " " + inputAddress.PostArticle;
                                if (String.Compare(postArticleAndName, featureAddress.StreetName, true) != 0)
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(postArticleAndName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        else if (new PostQualifierMatcher().HasMatch(0, inputAddress.StreetName)) // need to check if either the input or reference has a post-Article because parsing could have been messed up or the reference feature may not be parsed
                        {
                            if (!String.IsNullOrEmpty(featureAddress.PostArticle))
                            {
                                string postArticleAndName = featureAddress.StreetName + " " + featureAddress.PostArticle;
                                if (String.Compare(postArticleAndName, inputAddress.StreetName, true) != 0)
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(postArticleAndName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        else if (!String.IsNullOrEmpty(parameterSet.StreetAddressInitialParse.PostQualifier)) // need to check if either the originally-parsed address has a post-Article because parsing could have been messed up or the reference feature may not be parsed
                        {
                            // this penalty is caused when an input address has a post qualifier, but the reference does not.
                            // SOURCE_NAVTEQ_ADDRESSPOINTS_2016, for example, does not have post qualifiers, and it has not been processed/parsed to create them. And, when converting to USPS 28, the post qual is appended to Street Name
                            // ex: 1236 AHTANUM RIDGE BUSINESS PARK yakima WA 98903
                            string inputAddressWithoutPostQualifier = inputAddress.StreetName.Replace(parameterSet.StreetAddressInitialParse.PostQualifier, "");
                            double tempPenalty = ComputeWeightedEditDistancePenalty(inputAddressWithoutPostQualifier, featureAddress.StreetName, fullWeight);

                            // if there is a perfect match without the post qual, return a small penalty
                            if (tempPenalty == 0)
                            {
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.PostQualifier))
                                {
                                    // penalize zero if relaxation is allowed on post qualifiers
                                    penalty = tempPenalty;
                                }
                                else
                                {
                                    // penalize half of full weight if post qualifier relaxation is not allowed
                                    penalty = (AttributeWeightingScheme.ProportionalWeightPostQualifier / 2);
                                }
                            }
                            else // if there is a not a perfect match without the post qual, return normal penalty
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        else if (featureAddress.StreetName.ToUpper().StartsWith("SAINT") || inputAddress.StreetName.ToUpper().StartsWith("SAINT")) // need to check if either the input or reference is a SAINT because parsing could have been messed up or the reference feature may not be parsed
                        {
                            if (featureAddress.StreetName.ToUpper().StartsWith("SAINT") && !inputAddress.StreetName.ToUpper().StartsWith("SAINT"))
                            {
                                if (inputAddress.StreetName.ToUpper().StartsWith("ST"))
                                {
                                    string featureName = featureAddress.StreetName.ToUpper().Replace("SAINT", "ST");
                                    penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureName, fullWeight);
                                }
                                else
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else if (!featureAddress.StreetName.ToUpper().StartsWith("SAINT") && inputAddress.StreetName.ToUpper().StartsWith("SAINT"))
                            {
                                if (featureAddress.StreetName.ToUpper().StartsWith("ST"))
                                {
                                    string featureName = featureAddress.StreetName.ToUpper().Replace("ST", "SAINT");
                                    penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureName, fullWeight);
                                }
                                else
                                {
                                    penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                                }
                            }
                            else
                            {
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                        }
                        // else if the feature address is more than one word and/or contains a suffix, treat it special as the parsing may have screwed up or the input/reference feature may contain a suffix
                        else if (StreetSuffixUtils.containsStreetSuffix(featureAddress.StreetName))
                        {

                            // first split up the input name and normalize the suffix part if there is one
                            string normalizedinputNameAndSuffix = "";
                            string[] inputParts = inputAddress.StreetName.Split(' ');
                            foreach (string part in inputParts)
                            {
                                if (StreetSuffixUtils.isStreetSuffix(part))
                                {
                                    if (!String.IsNullOrEmpty(normalizedinputNameAndSuffix))
                                    {
                                        normalizedinputNameAndSuffix += " ";
                                    }

                                    normalizedinputNameAndSuffix += StreetSuffixUtils.getStreetSuffixOfficialAbbreviation(part);
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(normalizedinputNameAndSuffix))
                                    {
                                        normalizedinputNameAndSuffix += " ";
                                    }

                                    normalizedinputNameAndSuffix += part;
                                }
                            }

                            // then do the same for the reference feature
                            string normalizedReferenceNameAndSuffix = "";
                            string[] referenceParts = featureAddress.StreetName.Split(' ');
                            foreach (string part in referenceParts)
                            {
                                if (StreetSuffixUtils.isStreetSuffix(part))
                                {
                                    if (!String.IsNullOrEmpty(normalizedReferenceNameAndSuffix))
                                    {
                                        normalizedReferenceNameAndSuffix += " ";
                                    }

                                    normalizedReferenceNameAndSuffix += StreetSuffixUtils.getStreetSuffixOfficialAbbreviation(part);
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(normalizedReferenceNameAndSuffix))
                                    {
                                        normalizedReferenceNameAndSuffix += " ";
                                    }

                                    normalizedReferenceNameAndSuffix += part;
                                }
                            }

                            // first compare the normalized versions (input name v. reference name)
                            if (String.Compare(normalizedinputNameAndSuffix, normalizedReferenceNameAndSuffix, true) != 0)
                            {
                                // if the normalized versions are not the same, use them to calculate the penalty
                                penalty = ComputeWeightedEditDistancePenalty(normalizedinputNameAndSuffix, normalizedReferenceNameAndSuffix, fullWeight);
                            }

                            if (penalty > 0)
                            {
                                // try comparing the input name + suffix v. reference name
                                if (!String.IsNullOrEmpty(inputAddress.Suffix))
                                {
                                    string inputNamePlusSuffix = inputAddress.StreetName + " " + inputAddress.Suffix;
                                    double penaltyNamePlusSuffix = ComputeWeightedEditDistancePenalty(inputNamePlusSuffix, normalizedReferenceNameAndSuffix, fullWeight);

                                    if (penaltyNamePlusSuffix < penalty)
                                    {
                                        penalty = penaltyNamePlusSuffix;
                                    }

                                }
                            }

                        }
                        else
                        {
                            // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.StreetName))
                            {
                                // if the names are just plain different but soundex the same, weight the error cost by the edit distance
                                penalty = ComputeWeightedEditDistancePenalty(inputAddress.StreetName, featureAddress.StreetName, fullWeight);
                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyName: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltySuffix(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                string inputSuffix = inputAddress.Suffix;
                string featureSuffix = featureAddress.Suffix;

                if (StreetSuffixUtils.isStreetSuffix(inputAddress.Suffix))
                {
                    inputSuffix = StreetSuffixUtils.getStreetSuffixOfficialAbbreviation(inputSuffix);
                }

                if (StreetSuffixUtils.isStreetSuffix(featureAddress.Suffix))
                {
                    featureSuffix = StreetSuffixUtils.getStreetSuffixOfficialAbbreviation(featureSuffix);
                }

                if (String.Compare(inputSuffix, featureSuffix, true) != 0)
                {
                    // if the error is an ommission from the input string or reference data  only take half of the full weight
                    if (String.IsNullOrEmpty(inputSuffix) || String.IsNullOrEmpty(featureSuffix))
                    {
                        // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                        if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.Suffix))
                        {
                            penalty = (fullWeight / 2);

                        }
                        else
                        {
                            // if the error is an inccorrect value in the input string take the full weight
                            penalty = fullWeight;
                        }
                    }
                    else
                    {
                        // if the error is an inccorrect value in the input string take the full weight
                        penalty = fullWeight;
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltySuffix: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyZipPlus4(ParameterSet parameterSet, string inputZipPlus4, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;
            double alternatePenalty = fullWeight;

            try
            {
                if (String.Compare(inputZipPlus4, featureAddress.ZIPPlus4, true) != 0)
                {

                    if (String.IsNullOrEmpty(inputZipPlus4) || String.IsNullOrEmpty(featureAddress.ZIPPlus4) || String.Compare(inputZipPlus4, "0000", true) == 0) // if the error is an ommission from the input string or reference data only take half of the full weight
                    {
                        //Update 10/20/16 - Baldridge - no longer charging penalty for ommission errors 
                        //if (String.IsNullOrEmpty(inputZipPlus4) && String.IsNullOrEmpty(featureAddress.ZIPPlus4)) // if they are both null, zero penalty
                        //{
                        //    // do nothing, penalty will stay zero
                        //}
                        //else
                        //{
                        //    // if attribute relaxation is allowed and is allowed on this attribute take .2 of the weight, otherwise subtract the whole weight

                        //    if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.ZipPlus4))
                        //    {
                        //        penalty = (fullWeight * .2);
                        //    }
                        //    else
                        //    {
                        //        penalty = fullWeight;
                        //    }
                        //}
                    }
                    else
                    {
                        // if the error is an inccorrect value in the input string take the full weight
                        penalty = fullWeight;
                    }
                }

            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyZip: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            if (alternatePenalty < penalty)
            {
                penalty = alternatePenalty;
                ret.UsingAlternateZipCode = true;
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyZip(ParameterSet parameterSet, string inputZip, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;
            double alternatePenalty = fullWeight;

            try
            {
                if (String.Compare(inputZip, featureAddress.ZIP, true) != 0)
                {

                    if (String.IsNullOrEmpty(inputZip) || String.IsNullOrEmpty(featureAddress.ZIP) || String.Compare(inputZip, "99999", true) == 0) // if the error is an ommission from the input string or reference data only take half of the full weight
                    {
                        if (String.IsNullOrEmpty(inputZip) && String.IsNullOrEmpty(featureAddress.ZIP)) // if they are both null, zero penalty
                        {
                            // do nothing, penalty will stay zero
                        }
                        else
                        {
                            // if attribute relaxation is allowed and is allowed on this attribute take .2 of the weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.Zip))
                            {
                                penalty = (fullWeight * .2);
                            }
                            else
                            {
                                penalty = fullWeight;
                            }
                        }
                    }
                    else
                    {
                        // if the error is an inccorrect value in the input string take the positional proportion weight
                        penalty = ComputePenaltyZipPositionProportional(parameterSet, inputZip, featureAddress, fullWeight);
                    }
                }

                if (!String.IsNullOrEmpty(featureAddress.ZIPAlternate))
                {
                    if (String.Compare(featureAddress.ZIP, featureAddress.ZIPAlternate, true) != 0)
                    {
                        if (String.Compare(inputZip, featureAddress.ZIPAlternate, true) != 0)
                        {

                            // if the error is an ommission from the input string or reference data only take half of the full weight
                            if (String.IsNullOrEmpty(inputZip) || String.IsNullOrEmpty(featureAddress.ZIPAlternate))
                            {
                                // if attribute relaxation is allowed and is allowed on this attribute take half the weight, otherwise subtract the whole weight
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.Zip))
                                {
                                    alternatePenalty = (fullWeight / 2);
                                }
                                else
                                {
                                    // if the error is an inccorrect value in the input string take the positional proportion weight
                                    alternatePenalty = ComputePenaltyZipPositionProportional(parameterSet, inputZip, featureAddress, fullWeight);
                                }
                            }
                            else
                            {
                                // if the error is an inccorrect value in the input string take the positional proportion weight
                                alternatePenalty = ComputePenaltyZipPositionProportional(parameterSet, inputZip, featureAddress, fullWeight);
                            }
                        }
                        else
                        {
                            alternatePenalty = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyZip: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            if (alternatePenalty < penalty)
            {
                penalty = alternatePenalty;
                ret.UsingAlternateZipCode = true;
            }

            ret.EndTimer(penalty);

            return ret;
        }

        public double ComputePenaltyZipPositionProportional(ParameterSet parameterSet, string inputZip, StreetAddress featureAddress, double fullWeight)
        {
            double ret = 0;
            try
            {
                double errorIndex = 0;

                int inputLength = inputZip.Length;
                int referenceLength = featureAddress.ZIP.Length;

                int minLength = Math.Min(inputLength, referenceLength);

                for (int i = 0; i < minLength; i++)
                {
                    if (inputZip[i] != featureAddress.ZIP[i])
                    {
                        errorIndex = Convert.ToDouble(i);
                        break;
                    }
                }

                double proportion = (errorIndex / 5);
                double normalized = Convert.ToDouble(1.0) - proportion;
                double proportionalWeight = fullWeight * normalized;

                ret = proportionalWeight;

            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyZipPositionProportional: " + e.Message, e);
            }

            return ret;
        }

        public MatchScorePenaltyResult ComputePenaltyState(ParameterSet parameterSet, string inputState, StreetAddress featureAddress, double fullWeight)
        {
            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;

            try
            {
                if (String.Compare(inputState, featureAddress.State, true) != 0)
                {
                    if (StateUtils.isState(inputState) && StateUtils.isState(featureAddress.State))
                    {
                        inputState = StateUtils.getStateOfficialAbbreviation(inputState);
                        string featureState = StateUtils.getStateOfficialAbbreviation(featureAddress.State);

                        if (String.Compare(inputState, featureState, true) != 0)
                        {
                            penalty = fullWeight;
                        }
                    }
                    else
                    {
                        penalty = fullWeight;
                    }
                }
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyState: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            ret.EndTimer(penalty);

            return ret;
        }
        //BUG:X7-74 Payton- Added code to account for periods in names such as St. Paul. Eventually we need to account for this in the address normalization
        //public MatchScorePenaltyResult ComputePenaltyCity(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        //{
        //    MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
        //    ret.StartTimer();

        //    double penalty = 0;
        //    double alternatePenalty = fullWeight;
        //    string inCity = inputAddress.City.Replace(".", string.Empty); 
        //    string featCity = featureAddress.City.Replace(".", string.Empty); 

        //    try
        //    {
        //        if (String.Compare(inputAddress.City, featureAddress.City, true) != 0)
        //        {
        //            if(!CityUtils.isValidAlias(inputAddress.City,featureAddress.City,inputAddress.State))
        //            {
        //                if (!String.IsNullOrEmpty(inputAddress.City))
        //                {

        //                    if (!String.IsNullOrEmpty(featureAddress.City))
        //                    {
        //                        if (StateUtils.isState(inputAddress.City) || StateUtils.isState(featureAddress.City)) // if the city is a state name 'NY, NY' compare both the expanded versions
        //                        {
        //                            penalty = ComputePenaltyCityStateWord(parameterSet, inputAddress.City, featureAddress.City, fullWeight);
        //                        }
        //                        else if (featureAddress.City.IndexOf(' ') > 0 || featureAddress.City.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
        //                        {
        //                            penalty = ComputePenaltyCityMultiWord(parameterSet, inputAddress, featureAddress, fullWeight);
        //                        }
        //                        else
        //                        {
        //                            penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
        //                        }

        //                        // if the full penalty has been applied by comparing the input city name against the refernce city name, force it to compare against the mcd, county sub, and county
        //                        if (penalty == fullWeight)
        //                        {
        //                            penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight, false);
        //                        }

        //                    }
        //                    else
        //                    {
        //                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // aatribute relaxation is allowed take half the weight for an ommission error, otherwise subtract the whole weight
        //                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.City))
        //                {
        //                    penalty = (fullWeight / 2);
        //                }
        //                else
        //                {
        //                    penalty = fullWeight;
        //                }
        //            }



        //            if (!String.IsNullOrEmpty(featureAddress.CityAlternate))
        //            {
        //                if (String.Compare(featureAddress.City, featureAddress.CityAlternate, true) != 0)
        //                {
        //                    if (String.Compare(inputAddress.City, featureAddress.CityAlternate, true) != 0)
        //                    {
        //                        if (StateUtils.isState(inputAddress.City) || StateUtils.isState(featureAddress.CityAlternate)) // if the city is a state name 'NY, NY' compare both the expanded versions
        //                        {
        //                            alternatePenalty = ComputePenaltyCityStateWord(parameterSet, inputAddress.City, featureAddress.CityAlternate, fullWeight);
        //                        }
        //                        else if (featureAddress.CityAlternate.IndexOf(' ') > 0 || featureAddress.CityAlternate.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
        //                        {
        //                            alternatePenalty = ComputePenaltyCityAlternateMultiWord(parameterSet, inputAddress.City, featureAddress.CityAlternate, fullWeight);
        //                        }
        //                        else
        //                        {
        //                            alternatePenalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress.City, featureAddress.CityAlternate, fullWeight);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        alternatePenalty = 0;
        //                    }
        //                }
        //            }
        //        }
        //        //PAYTON:ALIASTABLE  
        //        //TASK:X7-T1 Added variable to allow for not using alias table (10/9/18)
        //        if (penalty > 0 && parameterSet.ShouldUseAliasTable) //If there is a penalty, check to see if the input city is a valid alias
        //        {
        //            bool isValidAlias = CityUtils.isValidAlias(inputAddress.City, featureAddress.City, inputAddress.State);
        //            if(isValidAlias)
        //            {
        //                penalty = 0;
        //            }
        //        }
        //    }            
        //    catch (Exception e)
        //    {
        //        ret.ExceptionOccurred = true;
        //        ret.Error = "Exception in ComputePenaltyCity: " + e.Message;
        //        ret.Exception = e;
        //    }

        //    if (alternatePenalty < penalty)
        //    {
        //        penalty = alternatePenalty;
        //        ret.UsingAlternateCityName = true;
        //    }

        //    ret.EndTimer(penalty);

        //    return ret;
        //}

        public MatchScorePenaltyResult ComputePenaltyCity(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            Serilog.Log.Verbose(this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - entered");

            MatchScorePenaltyResult ret = new MatchScorePenaltyResult();
            ret.StartTimer();

            double penalty = 0;
            double alternatePenalty = fullWeight;


            try
            {

                // Dan - 2019-01-11 - No longer apply penalty for missing input/feature city (to match what happens with ZIP)
                if (!String.IsNullOrEmpty(inputAddress.City) && !String.IsNullOrEmpty(featureAddress.City))
                {


                    string inCity = inputAddress.City.Replace(".", string.Empty);
                    string featCity = featureAddress.City.Replace(".", string.Empty);
                    string[] parts = inCity.Split(' ');

                    if (String.Compare(inCity, featCity, true) != 0)
                    {
                        if (parameterSet.ShouldUseAliasTable)
                        {
                            if (!CityUtils.isValidAlias(inCity, featCity, inputAddress.State))
                            {
                                if (!String.IsNullOrEmpty(inCity))
                                {

                                    if (!String.IsNullOrEmpty(featCity))
                                    {
                                        if (StateUtils.isState(inCity) || StateUtils.isState(featCity)) // if the city is a state name 'NY, NY' compare both the expanded versions
                                        {
                                            penalty = ComputePenaltyCityStateWord(parameterSet, inCity, featCity, fullWeight);
                                        }
                                        else if (featCity.IndexOf(' ') > 0 || featCity.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
                                        {
                                            penalty = ComputePenaltyCityMultiWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                        }
                                        else
                                        {
                                            penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                        }

                                        // if the full penalty has been applied by comparing the input city name against the refernce city name, force it to compare against the mcd, county sub, and county
                                        if (penalty == fullWeight)
                                        {
                                            penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight, false);
                                        }

                                    }
                                    else
                                    {
                                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                    }
                                }
                                // aatribute relaxation is allowed take half the weight for an ommission error, otherwise subtract the whole weight
                                if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.City))
                                {
                                    penalty = (fullWeight / 2);
                                }
                                else
                                {
                                    penalty = fullWeight;
                                }
                            }
                            else
                            {
                                //do nothing - since it's a valid alias no penalty is assessed
                            }
                        }
                        else //if not using alias table then don't need to do alias check
                        {
                            if (!String.IsNullOrEmpty(inCity))
                            {

                                if (!String.IsNullOrEmpty(featCity))
                                {
                                    if (StateUtils.isState(inCity) || StateUtils.isState(featCity)) // if the city is a state name 'NY, NY' compare both the expanded versions
                                    {
                                        penalty = ComputePenaltyCityStateWord(parameterSet, inCity, featCity, fullWeight);
                                    }
                                    else if (featCity.IndexOf(' ') > 0 || featCity.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
                                    {
                                        penalty = ComputePenaltyCityMultiWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                    }
                                    else
                                    {
                                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                    }

                                    // if the full penalty has been applied by comparing the input city name against the refernce city name, force it to compare against the mcd, county sub, and county
                                    if (penalty == fullWeight)
                                    {
                                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight, false);
                                    }

                                }
                                else
                                {
                                    penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(featureAddress.CityAlternate))
                        {
                            if (String.Compare(featCity, featureAddress.CityAlternate, true) != 0)
                            {
                                if (String.Compare(inCity, featureAddress.CityAlternate, true) != 0)
                                {
                                    if (StateUtils.isState(inCity) || StateUtils.isState(featureAddress.CityAlternate)) // if the city is a state name 'NY, NY' compare both the expanded versions
                                    {
                                        alternatePenalty = ComputePenaltyCityStateWord(parameterSet, inCity, featureAddress.CityAlternate, fullWeight);
                                    }
                                    else if (featureAddress.CityAlternate.IndexOf(' ') > 0 || featureAddress.CityAlternate.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
                                    {
                                        alternatePenalty = ComputePenaltyCityAlternateMultiWord(parameterSet, inCity, featureAddress.CityAlternate, fullWeight);
                                    }
                                    else
                                    {
                                        alternatePenalty = ComputePenaltyCitySingleWord(parameterSet, inCity, featureAddress.CityAlternate, fullWeight);
                                    }
                                }
                                else
                                {
                                    alternatePenalty = 0;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string message = MethodBase.GetCurrentMethod().DeclaringType + " " + MethodBase.GetCurrentMethod().Name + " - inputAddress.City or featureAddress.city and/is null";
                    Serilog.Log.Verbose(message);
                }
                //BUG:X7-74 If no match is found but input or feature city contains pre or post qualifers, remove and try match again.
                //else
                //{
                //    string[] inParts = inCity.Split(' ');
                //    string[] featParts = featCity.Split(' ');
                //    string[] qualifiers = { "OLDE", "OLD", "OLE", "OL", "TOWNSHIP", "TWNSHIP", "BOROUGH", "TWP", "TOWN", "TOWNE","CITY","BORO","TWN","TWNE" };
                //    StringBuilder sbin = new StringBuilder();
                //    StringBuilder sbfeat = new StringBuilder();
                //    foreach(var part in inParts)
                //    {
                //        bool add = true;
                //        foreach (var qualifier in qualifiers)
                //        {
                //            if (part.ToUpper() == qualifier)
                //            {
                //                add=false;
                //            }                            
                //        }
                //        if(add)
                //        {
                //            sbin.Append(part);
                //        }
                //    }
                //    foreach (var part in featParts)
                //    {
                //        bool add = true;
                //        foreach (var qualifier in qualifiers)
                //        {
                //            if (part.ToUpper() == qualifier)
                //            {
                //                add = false;
                //            }
                //        }
                //        if (add)
                //        {
                //            sbfeat.Append(part);
                //        }
                //    }

                //    string inCityMod = sbin.ToString();
                //    string featCityMod = sbfeat.ToString();
                //    //inCity = inputAddress.City.Replace(" Old ", string.Empty).Replace(" Olde ", string.Empty).Replace(" Ol ", string.Empty).Replace(" Ole ", string.Empty).Replace(" Township ", string.Empty).Replace(" TWNSP ", string.Empty).Replace(" Borough ", string.Empty);
                //    //featCity = featureAddress.City.Replace(" Old ", string.Empty).Replace(" Olde ", string.Empty).Replace(" Ol ", string.Empty).Replace(" Ole ", string.Empty).Replace(" Township ", string.Empty).Replace(" TWNSP ", string.Empty).Replace(" Borough ", string.Empty);

                //    if (String.Compare(inCityMod, featCityMod, true) != 0)
                //    {
                //        if (!CityUtils.isValidAlias(inCityMod, featCityMod, inputAddress.State))
                //        {
                //            if (!String.IsNullOrEmpty(inCityMod))
                //            {

                //                if (!String.IsNullOrEmpty(featCityMod))
                //                {
                //                    if (StateUtils.isState(inCityMod) || StateUtils.isState(featCityMod)) // if the city is a state name 'NY, NY' compare both the expanded versions
                //                    {
                //                        penalty = ComputePenaltyCityStateWord(parameterSet, inCityMod, featCityMod, fullWeight);
                //                    }
                //                    else if (featCityMod.IndexOf(' ') > 0 || featCityMod.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
                //                    {
                //                        penalty = ComputePenaltyCityMultiWord(parameterSet, inputAddress, featureAddress, fullWeight);
                //                    }
                //                    else
                //                    {
                //                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                //                    }

                //                    // if the full penalty has been applied by comparing the input city name against the refernce city name, force it to compare against the mcd, county sub, and county
                //                    if (penalty == fullWeight)
                //                    {
                //                        penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight, false);
                //                    }

                //                }
                //                else
                //                {
                //                    penalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            // aatribute relaxation is allowed take half the weight for an ommission error, otherwise subtract the whole weight
                //            if (parameterSet.ShouldUseRelaxation && parameterSet.RelaxableAttributes.Contains(AddressComponents.City))
                //            {
                //                penalty = (fullWeight / 2);
                //            }
                //            else
                //            {
                //                penalty = fullWeight;
                //            }
                //        }
                //        if (!String.IsNullOrEmpty(featureAddress.CityAlternate))
                //        {
                //            if (String.Compare(featCityMod, featureAddress.CityAlternate, true) != 0)
                //            {
                //                if (String.Compare(inCityMod, featureAddress.CityAlternate, true) != 0)
                //                {
                //                    if (StateUtils.isState(inCityMod) || StateUtils.isState(featureAddress.CityAlternate)) // if the city is a state name 'NY, NY' compare both the expanded versions
                //                    {
                //                        alternatePenalty = ComputePenaltyCityStateWord(parameterSet, inCityMod, featureAddress.CityAlternate, fullWeight);
                //                    }
                //                    else if (featureAddress.CityAlternate.IndexOf(' ') > 0 || featureAddress.CityAlternate.IndexOf('-') > 0) // if this is a multi-word city try each word and take the best score
                //                    {
                //                        alternatePenalty = ComputePenaltyCityAlternateMultiWord(parameterSet, inCityMod, featureAddress.CityAlternate, fullWeight);
                //                    }
                //                    else
                //                    {
                //                        alternatePenalty = ComputePenaltyCitySingleWord(parameterSet, inCityMod, featureAddress.CityAlternate, fullWeight);
                //                    }
                //                }
                //                else
                //                {
                //                    alternatePenalty = 0;
                //                }
                //            }
                //        }

                //    }
                // }                    
                //PAYTON:ALIASTABLE  
                //TASK:X7-T1 Added variable to allow for not using alias table (10/9/18)
                //Doing this in the actual penalty code above now
                //if (penalty > 0 && parameterSet.ShouldUseAliasTable) //If there is a penalty, check to see if the input city is a valid alias
                //{
                //    bool isValidAlias = CityUtils.isValidAlias(inCity , featCity, inputAddress.State);
                //    if (isValidAlias)
                //    {
                //        penalty = 0;
                //    }
                //}
            }
            catch (Exception e)
            {
                ret.ExceptionOccurred = true;
                ret.Error = "Exception in ComputePenaltyCity: " + e.Message;
                ret.Exception = e;
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
            }

            if (alternatePenalty < penalty)
            {
                penalty = alternatePenalty;
                ret.UsingAlternateCityName = true;
            }

            ret.EndTimer(penalty);

            return ret;
        }
        public double ComputePenaltyCityStateWord(ParameterSet parameterSet, string inputCity, string referenceCity, double fullWeight)
        {
            double ret = 0;

            try
            {
                StreetAddress inputAddressCity = new StreetAddress();
                if (StateUtils.isState(inputCity))
                {
                    inputAddressCity.City = StateUtils.getStateOfficialFullName(inputCity);
                }
                else
                {
                    inputAddressCity.City = inputCity;
                }

                StreetAddress featureAddressCity = new StreetAddress();
                if (StateUtils.isState(referenceCity))
                {
                    featureAddressCity.City = StateUtils.getStateOfficialFullName(referenceCity);
                }
                else
                {
                    featureAddressCity.City = referenceCity;
                }

                ret = ComputePenaltyCitySingleWord(parameterSet, inputAddressCity, featureAddressCity, fullWeight);

            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyCityStateWord: " + e.Message, e);
            }

            return ret;
        }
        //BUG:X7-74 Payton- Added code to account for periods in names such as St. Paul. Eventually we need to account for this in the address normalization
        //public double ComputePenaltyCityMultiWord(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        //{
        //    double ret = 0;

        //    try
        //    {

        //        // if there is an alternate name inside of the name "San Boneventura (Ventura)", run them both and pick the one that is less penalty
        //        if (featureAddress.City.IndexOf('(') > 0 && featureAddress.City.IndexOf(')') > 0)
        //        {

        //            int parenStart = featureAddress.City.IndexOf('(');
        //            int parenEnd = featureAddress.City.IndexOf(')');
        //            int parenLength = parenEnd - parenStart;

        //            string cityPart1 = featureAddress.City.Substring(0, parenStart);

        //            if (parenEnd != featureAddress.City.Length)
        //            {
        //                cityPart1 += featureAddress.City.Substring(parenEnd + 1);
        //            }

        //            string cityPart2 = featureAddress.City.Substring(parenStart + 1, parenLength - 1);

        //            StreetAddress cityPart1Address = new StreetAddress();
        //            cityPart1Address.City = cityPart1;

        //            StreetAddress cityPart2Address = new StreetAddress();
        //            cityPart2Address.City = cityPart2;

        //            double penalty1 = ComputePenaltyCityMultiWord(parameterSet, inputAddress, cityPart1Address, fullWeight);
        //            double penalty2 = ComputePenaltyCityMultiWord(parameterSet, inputAddress, cityPart2Address, fullWeight);

        //            if (penalty1 < penalty2)
        //            {
        //                ret = penalty1;
        //            }
        //            else
        //            {
        //                ret = penalty2;
        //            }

        //        }
        //        else
        //        {

        //            string[] cityParts = null;
        //            if (featureAddress.City.IndexOf(' ') > 0)
        //            {
        //                cityParts = featureAddress.City.Split(' ');
        //            }

        //            if (featureAddress.City.IndexOf('-') > 0)
        //            {
        //                cityParts = featureAddress.City.Split('-');
        //            }

        //            double bestPenalty = fullWeight;

        //            bool hasExactMatch = false;

        //            int numberOfWrongWords = 0;
        //            foreach (string cityPart in cityParts)
        //            {
        //                StreetAddress cityAddress = new StreetAddress();
        //                cityAddress.City = cityPart;

        //                double currentPenalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, cityAddress, fullWeight);
        //                if (currentPenalty != 0)
        //                {
        //                    numberOfWrongWords++;
        //                }

        //                if (currentPenalty < bestPenalty)
        //                {
        //                    bestPenalty = currentPenalty;
        //                    if (bestPenalty == 0)
        //                    {
        //                        hasExactMatch = true;
        //                        break;
        //                    }
        //                }
        //            }

        //            ret = bestPenalty;


        //            // if there was an exact match in there somewhere, take of 10% for each of the wrong words
        //            if (hasExactMatch)
        //            {
        //                if (numberOfWrongWords > 0)
        //                {
        //                    ret += (.1 * Convert.ToDouble(numberOfWrongWords)) * fullWeight;
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Exception in ComputePenaltyCityMultiWord: " + e.Message, e);
        //    }

        //    return ret;
        //}
        public double ComputePenaltyCityMultiWord(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            double ret = 0;
            string inCity = inputAddress.City.Replace(".", string.Empty);
            string featCity = featureAddress.City.Replace(".", string.Empty);
            string[] parts = inCity.Split(' ');
            string[] inParts = inCity.Split(' ');
            string[] featParts = featCity.Split(' ');
            string[] qualifiers = { "OLDE", "OLD", "OLE", "OL", "TOWNSHIP", "TWNSHIP", "BOROUGH", "TWP", "TOWN", "TOWNE", "CITY", "BORO", "TWN", "TWNE" };
            StringBuilder sbin = new StringBuilder();
            StringBuilder sbfeat = new StringBuilder();
            int partsCount = inParts.Length;
            foreach (var part in inParts)
            {
                bool add = true;
                foreach (var qualifier in qualifiers)
                {
                    if (part.ToUpper() == qualifier)
                    {
                        add = false;
                    }
                }
                if (add)
                {
                    if (partsCount > 1)
                    {
                        sbin.Append(part + " ");
                    }
                    else
                    {
                        sbin.Append(part);
                    }

                }
                partsCount = partsCount - 1;
            }
            partsCount = featParts.Length;
            foreach (var part in featParts)
            {
                bool add = true;
                foreach (var qualifier in qualifiers)
                {
                    if (part.ToUpper() == qualifier)
                    {
                        add = false;
                    }
                }
                if (add)
                {
                    if (partsCount > 1)
                    {
                        sbfeat.Append(part + " ");
                    }
                    else
                    {
                        sbfeat.Append(part);
                    }

                }
                partsCount = partsCount - 1;
            }

            string inCityMod = sbin.ToString().TrimEnd();
            string featCityMod = sbfeat.ToString().TrimEnd();
            try
            {
                //BUG:T7-47 Since updating qualifiers - test full cityname once again to ensure it still does not match
                if (String.Compare(inCityMod, featCityMod, true) != 0)
                {
                    // if there is an alternate name inside of the name "San Boneventura (Ventura)", run them both and pick the one that is less penalty
                    if (featCityMod.IndexOf('(') > 0 && featCityMod.IndexOf(')') > 0)
                    {

                        int parenStart = featCityMod.IndexOf('(');
                        int parenEnd = featCityMod.IndexOf(')');
                        int parenLength = parenEnd - parenStart;

                        string cityPart1 = featCityMod.Substring(0, parenStart);

                        if (parenEnd != featCityMod.Length)
                        {
                            cityPart1 += featCityMod.Substring(parenEnd + 1);
                        }

                        string cityPart2 = featCityMod.Substring(parenStart + 1, parenLength - 1);

                        StreetAddress cityPart1Address = new StreetAddress();
                        cityPart1Address.City = cityPart1;

                        StreetAddress cityPart2Address = new StreetAddress();
                        cityPart2Address.City = cityPart2;

                        double penalty1 = ComputePenaltyCityMultiWord(parameterSet, inputAddress, cityPart1Address, fullWeight);
                        double penalty2 = ComputePenaltyCityMultiWord(parameterSet, inputAddress, cityPart2Address, fullWeight);

                        if (penalty1 < penalty2)
                        {
                            ret = penalty1;
                        }
                        else
                        {
                            ret = penalty2;
                        }

                    }
                    else
                    {

                        string[] cityParts = null;
                        if (featCityMod.IndexOf(' ') > 0)
                        {
                            cityParts = featCityMod.Split(' ');
                        }

                        if (featCityMod.IndexOf('-') > 0)
                        {
                            cityParts = featCityMod.Split('-');
                        }
                        else
                        {
                            cityParts = new string[] { featCityMod };
                        }
                        double bestPenalty = fullWeight;

                        bool hasExactMatch = false;

                        int numberOfWrongWords = 0;
                        foreach (string cityPart in cityParts)
                        {
                            StreetAddress cityAddress = new StreetAddress();
                            cityAddress.City = cityPart;

                            double currentPenalty = ComputePenaltyCitySingleWord(parameterSet, inputAddress, cityAddress, fullWeight);
                            if (currentPenalty != 0)
                            {
                                numberOfWrongWords++;
                            }

                            if (currentPenalty < bestPenalty)
                            {
                                bestPenalty = currentPenalty;
                                if (bestPenalty == 0)
                                {
                                    hasExactMatch = true;
                                    break;
                                }
                            }
                        }

                        ret = bestPenalty;


                        // if there was an exact match in there somewhere, take of 10% for each of the wrong words
                        if (hasExactMatch)
                        {
                            if (numberOfWrongWords > 0)
                            {
                                ret += (.1 * Convert.ToDouble(numberOfWrongWords)) * fullWeight;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyCityMultiWord: " + e.Message, e);
            }

            return ret;
        }
        public double ComputePenaltyCityAlternateMultiWord(ParameterSet parameterSet, string inputCity, string featureCity, double fullWeight)
        {
            double ret = 0;

            try
            {
                string[] cityParts = null;
                if (featureCity.IndexOf(' ') > 0)
                {
                    cityParts = featureCity.Split(' ');
                }

                if (featureCity.IndexOf('-') > 0)
                {
                    cityParts = featureCity.Split('-');
                }

                double bestPenalty = fullWeight;

                foreach (string cityPart in cityParts)
                {

                    double currentPenalty = ComputePenaltyCitySingleWord(parameterSet, inputCity, cityPart, fullWeight);
                    if (currentPenalty < bestPenalty)
                    {
                        bestPenalty = currentPenalty;
                        if (bestPenalty == 0)
                        {
                            break;
                        }
                    }
                }

                ret = bestPenalty;

            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyCityAlternateMultiWord: " + e.Message, e);
            }

            return ret;
        }

        public double ComputePenaltyCitySingleWord(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight)
        {
            return ComputePenaltyCitySingleWord(parameterSet, inputAddress, featureAddress, fullWeight, true);

        }

        public double ComputePenaltyCitySingleWord(ParameterSet parameterSet, StreetAddress inputAddress, StreetAddress featureAddress, double fullWeight, bool checkReferenceCity)
        {
            double ret = 0;

            try
            {

                if (!String.IsNullOrEmpty(inputAddress.City) || !String.IsNullOrEmpty(featureAddress.City))
                {
                    if ((!String.IsNullOrEmpty(featureAddress.City)) && checkReferenceCity)
                    {
                        ret = ComputePenaltyCitySingleWord(parameterSet, inputAddress.City, featureAddress.City, fullWeight);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(featureAddress.ConsolidatedCity))
                        {
                            // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                            {
                                // if reference city is blank, but consolidated city is not then use a minimum of 1/3 the weight unless the input=refernce
                                double penalty = ComputeWeightedEditDistancePenalty(inputAddress.City, featureAddress.ConsolidatedCity, fullWeight);
                                if (penalty > 0)
                                {
                                    if (penalty > (fullWeight * .3))
                                    {
                                        ret = penalty;
                                    }
                                    else
                                    {
                                        ret = (fullWeight * .3);
                                    }
                                }
                                else // if the penalty was zero (meaning there was an exact match between the input city and the consolidated city) apply a 10% penalty
                                {
                                    ret = (fullWeight * .1);
                                }
                            }
                            else
                            {
                                ret = fullWeight;
                            }
                        }
                        else if (!String.IsNullOrEmpty(featureAddress.MinorCivilDivision))
                        {
                            // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                            {
                                // if reference city is blank, but Minor civil division is not then use a minimum of 1/3 the weight unless the input=refernce
                                double penalty = ComputeWeightedEditDistancePenalty(inputAddress.City, featureAddress.MinorCivilDivision, fullWeight);
                                if (penalty > 0)
                                {
                                    if (penalty > (fullWeight * .3))
                                    {
                                        ret = penalty;
                                    }
                                    else
                                    {
                                        ret = (fullWeight * .3);
                                    }
                                }
                                else // if the penalty was zero (meaning there was an exact match between the input city and the mcd) apply a 10% penalty
                                {
                                    ret = (fullWeight * .1);
                                }
                            }
                            else
                            {
                                ret = fullWeight;
                            }

                        }
                        else if (!String.IsNullOrEmpty(featureAddress.CountySubregion))
                        {
                            // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                            {
                                // if reference city is blank, but County SubRegion is not then use a minimum of 1/3 the weight unless the input=refernce
                                double penalty = ComputeWeightedEditDistancePenalty(inputAddress.City, featureAddress.CountySubregion, fullWeight);
                                if (penalty > 0)
                                {
                                    if (penalty > (fullWeight * .3))
                                    {
                                        ret = penalty;
                                    }
                                    else
                                    {
                                        ret = (fullWeight * .3);
                                    }
                                }
                                else // if the penalty was zero (meaning there was an exact match between the input city and the county sub region) apply a 10% penalty
                                {
                                    ret = (fullWeight * .1);
                                }
                            }
                            else
                            {
                                ret = fullWeight;
                            }
                        }
                        else if (!String.IsNullOrEmpty(featureAddress.County))
                        {
                            // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                            if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                            {
                                // if reference city is blank, but County is not then use a minimum of 2/3 the weight
                                double penalty = ComputeWeightedEditDistancePenalty(inputAddress.City, featureAddress.County, fullWeight);
                                if (penalty > 0)
                                {
                                    if (penalty > (fullWeight * .75))
                                    {
                                        ret = penalty;
                                    }
                                    else
                                    {
                                        ret = (fullWeight * .75);
                                    }
                                }
                                else // if the penalty was zero (meaning there was an exact match between the input city and the county) apply a 10% penalty
                                {
                                    ret = (fullWeight * .25);
                                }
                            }
                            else
                            {
                                ret = fullWeight;
                            }
                        }
                        else
                        {
                            //PAYTON:CITY
                            //What is the purpose of this?? If the reference data set does not have a city, this returns 0 penalty - default to always use for now
                            if (!checkReferenceCity)
                            {
                                ret = fullWeight;
                            }
                            //Adding else to always return fullWeight here
                            {
                                ret = fullWeight;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyCitySingleWord: " + e.Message, e);
            }

            return ret;
        }

        public double ComputePenaltyCitySingleWord(ParameterSet parameterSet, string inputCity, string referenceCity, double fullWeight)
        {
            double ret = 0;

            try
            {

                if (!String.IsNullOrEmpty(inputCity) && !String.IsNullOrEmpty(referenceCity))
                {
                    // if the error is an ommission from the input string
                    if (String.IsNullOrEmpty(inputCity) || String.IsNullOrEmpty(referenceCity))
                    {
                        // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                        if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                        {
                            ret = ComputeWeightedEditDistancePenalty(inputCity, referenceCity, fullWeight);
                        }
                        else
                        {
                            ret = (fullWeight / 2);
                        }
                    }
                    else
                    {

                        // if soundex is allowed and is allowed on this attribute take the edit distance weight, otherwise subtract the whole weight
                        if (parameterSet.ShouldUseSoundex && parameterSet.SoundexAttributes.Contains(AddressComponents.City))
                        {
                            ret = ComputeWeightedEditDistancePenalty(inputCity, referenceCity, fullWeight);
                        }
                        else
                        {
                            ret = fullWeight;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, this.GetType().Name + " " + MethodBase.GetCurrentMethod().Name + " - errored out");
                throw new Exception("Exception in ComputePenaltyCitySingleWord: " + e.Message, e);
            }

            return ret;
        }

        public override double ComputeWeightedEditDistancePenalty(string inputString, string referenceString, double fullWeight)
        {
            double ret = 0;

            if (String.IsNullOrEmpty(inputString) && String.IsNullOrEmpty(referenceString))
            {
                ret = 0;
            }
            else
            {
                if (String.IsNullOrEmpty(inputString) || String.IsNullOrEmpty(referenceString))
                {
                    ret = fullWeight;
                }
                else
                {

                    string inputStringCleaned = inputString.Replace(" ", "").Replace("-", "");
                    string referenceStringCleaned = referenceString.Replace(" ", "").Replace("-", "");

                    if (String.Compare(inputStringCleaned, referenceStringCleaned, true) != 0)
                    {

                        // if either of the strings are shorter than four chars use full weight, otherwise use the edit distance proportional weight
                        if (inputStringCleaned.Length < 4 || referenceStringCleaned.Length < 4)
                        {
                            ret = fullWeight;
                        }
                        else
                        {
                            string inputStringSoundex = SoundexEncoder.ComputeEncodingOld(inputStringCleaned);
                            string referenceStringSoundex = SoundexEncoder.ComputeEncodingOld(referenceStringCleaned);
                            if (String.Compare(inputStringSoundex, referenceStringSoundex, true) == 0)
                            {
                                int editDistance = Int32.MaxValue;

                                if (EditDistanceHashtable.Contains(inputStringCleaned.ToLower()))
                                {
                                    Hashtable comparisonTable = (Hashtable)EditDistanceHashtable[inputStringCleaned.ToLower()];

                                    if (comparisonTable.Contains(referenceStringCleaned.ToLower()))
                                    {
                                        editDistance = (int)comparisonTable[referenceStringCleaned.ToLower()];
                                    }
                                    else
                                    {
                                        editDistance = LevenshteinEditDistance.EditDistance(inputStringCleaned.ToLower(), referenceStringCleaned.ToLower());
                                        comparisonTable.Add(referenceStringCleaned.ToLower(), editDistance);
                                    }
                                }
                                else
                                {
                                    Hashtable comparisonTable = new Hashtable();
                                    editDistance = LevenshteinEditDistance.EditDistance(inputStringCleaned.ToLower(), referenceStringCleaned.ToLower());
                                    comparisonTable.Add(referenceStringCleaned.ToLower(), editDistance);
                                    EditDistanceHashtable.Add(inputStringCleaned.ToLower(), comparisonTable);
                                }


                                if (editDistance > inputStringCleaned.Length)
                                {
                                    editDistance = inputStringCleaned.Length;
                                }

                                double percentDifference = (double)editDistance / (double)inputStringCleaned.Length;
                                double partialWeight = percentDifference * fullWeight;
                                ret = partialWeight;

                            }
                            else
                            {
                                ret = fullWeight;
                            }
                        }
                    }
                    else
                    {
                        ret = 0;
                    }
                }
            }

            return ret;
        }

        //public class PenaltyCodeResult
        //{
        //    //These ints will be scores for each section - lower the score the less severe the penalty i.e. 0 indicates no penalty 9 is max penalty
        //    #region Penalty Variables
        //    public int matchScore { get; set; }     //penalty based on matchscore: 0(100),1(90-100),2(80-90),3(70-80)..etc
        //    public int inputType { get; set; } // PO Box, RR, Zipcode only etc
        //    public int zip { get; set; } //0-zip matched,1-matched by ambiguous,2-didn't match
        //    public int city { get; set; } //0-city matched,1-city matched to alias,2-city soundex matched,3-city not matched at all
        //    public int soundexPenalty { get; set; } //soundex penalty - edit distance/length of word       
        //    public int zipPenalty { get; set; } //0-zip matched,1-1st digit from right different,2-2nd digit from right different etc       
        //    public int directionals { get; set; } //pre-post directionals - 0-no error,1-missing pre input,2-missing pre ref,3-missing post input,4-missing post ref
        //    public int qualifiers { get; set; } //pre-post qualifiers - 0-no error,1-missing pre input,2-missing pre ref,3-missing post input,4-missing post ref
        //    public int distance { get; set; }     //will assign penalty depending on average distance between all results      
        //    public int censusBlocks { get; set; }     //census blocks matched 0,didn't match 1
        //    public int censusTracts { get; set; }     //census Tracts matched 0,didn't match 1      
        //    public int county { get; set; }     //penalty based on how many counties are different 0-all counties match,9-all counties different       

        //    #endregion

        //    public PenaltyCodeResult()
        //    {
        //        matchScore = 0;
        //        inputType = 0;
        //        zip = 0;
        //        city = 0;
        //        soundexPenalty = 0;
        //        zipPenalty = 0;
        //        directionals = 0;
        //        qualifiers = 0;
        //        distance = 0;
        //        censusBlocks = 0;
        //        censusTracts = 0;
        //        county = 0;
        //    }
        //}
    }
}
