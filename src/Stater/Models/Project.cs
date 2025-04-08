using System.Collections.Generic;
using System.Xml.Serialization;

namespace Stater.Models;

public record Project(
    [property: XmlAttribute("name")] string Name,
    [property: XmlAttribute("Location")] string? Location
)
{
    public Project() : this("Project", null)
    {
    }
};

public record ExportProject(
    [property: XmlElement("Project")] Project? Project,
    [property: XmlElement("StateMachines")] List<StateMachine>? StateMachines
)
{
    public ExportProject() : this(null, null)
    {
    }
}