using System.Collections.Generic;
using System.Linq;

namespace CampaignEditor
{
    public static class MathFunctions
    { 

        public static T ArithmeticMean<T>(IEnumerable<T> array)
        {

            dynamic sum = 0.0M;
            if (array.Count() == 0)
                return sum;

            foreach (T number in array)
            {
                sum += number;
            }
            return sum / array.Count();
        }
    }
}
