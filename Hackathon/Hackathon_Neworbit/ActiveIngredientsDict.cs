using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackathon_Neworbit
{
    public class ActiveIngredientsDict
    {
        public ActiveIngredientsDict()
        {
            DictionaryData = new Dictionary<string, string>()
            {
                {"paracetamol", "headache, fever, cough" },
                {"aspirin", "headache, heart pain, high blood pressure" },
                {"phenylephrine hydrochloride", "cough, sore throat" },
                {"guaifenesin", "cough, sore throat" },
                {"caffeine", "cough, headache, fever, fatigue" },
                {"cetirizine hydrochloride", "hayfever, allergy, antihistamine" },
                {"fexofenadine hydrochloride", "hayfever, allergy, antihistamine" }
            };
        }

        public Dictionary<string, string> DictionaryData { get; }
    }
}
