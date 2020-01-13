using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Enums
{
    public enum EvaluationRuleCA
    {
        BOUNDS_ARE_WALLS,
        ROOMS_EXIST_IN_LEVEL,
        TILES_IN_ROOMS_ARE_CORRECT,
        CORRIDORS_NEXT_TO_BOUNDS
    }
}
