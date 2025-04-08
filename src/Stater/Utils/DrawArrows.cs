using Avalonia;
using Stater.Models;

namespace Stater.Utils;

public abstract record DrawArrows(
    State Start,
    State End,
    Transition Transition
)
{
    
}