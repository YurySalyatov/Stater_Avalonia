using System.Collections.Generic;
using Stater.Models;

namespace Stater.Plugin;

public record PluginInput(
    Project Project,
    StateMachine StateMachine,
    List<StateMachine> StateMachines,
    
    IProjectManager ProjectManager
);