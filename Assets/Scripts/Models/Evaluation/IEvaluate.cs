using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models.Evaluation
{
    public interface IEvaluate
    {
        EvaluationResult Evaluate(EvaluationRules rule);
    }
}
