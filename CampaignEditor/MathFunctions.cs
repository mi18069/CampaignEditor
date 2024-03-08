using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public static class MathFunctions
    { 

        public static T ArithmeticMean<T>(IEnumerable<T> array)
        {

            dynamic sum = 0.0;
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
