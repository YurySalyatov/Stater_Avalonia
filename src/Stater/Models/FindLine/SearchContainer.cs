using System;
using Stater.Models.Editors;

namespace Stater.Models.FindLine;

public class SearchContainer
{
    public EditorTypeEnum EditorType;
    public int StartPos;
    public int EndPos;
    public bool IsDescription = false;
    public Guid Guid { get; set; }
    public Guid StateMachineGuid;
}