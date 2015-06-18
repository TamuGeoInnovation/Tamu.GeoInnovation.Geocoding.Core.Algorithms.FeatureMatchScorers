using System.Collections;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.Interfaces.EditDistanceScorers;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchScorers.AbstractClasses.EditDistanceScorers
{
    public abstract class AbstractEditDistanceScorer : AbstractMatchScorer, IEditDistanceScorer
    {

        #region Properties
        public Hashtable EditDistanceHashtable { get; set; }

        #endregion

        public AbstractEditDistanceScorer()
        {
            EditDistanceHashtable = new Hashtable();
        }

        public AbstractEditDistanceScorer(AttributeWeightingScheme attributeWeightingScheme)
            : base(attributeWeightingScheme)
        {
            EditDistanceHashtable = new Hashtable();
        }

        public abstract double ComputeWeightedEditDistancePenalty(string inputString, string referenceString, double fullWeight);
    }
}
