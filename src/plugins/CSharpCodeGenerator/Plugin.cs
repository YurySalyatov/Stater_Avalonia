using Stater.Models;
using Stater.Plugin;

namespace CSharpCodeGenerator;

public class Plugin : ButtonPlugin
    {
        /// <summary>
        /// Эта функция вызывается по нажатию кнопки в Stater.
        /// </summary>
        /// <param name="_pluginParams"></param>
        /// <returns></returns>
        public override PluginOutput Start(PluginInput _pluginParams)
        {
            var res = PluginOutput.From("OK");
            // machine = _machines;

            pluginParams = _pluginParams;

            var machineList = _pluginParams.StateMachines;

            if (machineList.Count == 0)
            {
                return res;
            }
            
            // TODO-PRACTICE: DO file dialog
            WriteEvents(machineList, _pluginParams.pm.Info.GetWorkFolder());
            foreach (var machine in machineList)
            {
                path = machine.Name + ".cs";
                GenerateCode(machine);

            } 
            /*
            if (machine.Name == "1")
            {
                return 1;
            }
             * */
            return res;
        }

        public override PluginRetVal SilentStart(PluginParams _pluginParams)
        {
            return Start(_pluginParams);
        }

        private PluginParams pluginParams;

        private void WriteEvents(List<StateMachine> machineList, string _location)
        {
            var sw = new StreamWriter(_location + "\\Events.cs");
            var indent = 0;

            sw.WriteLine(DoIndent(indent) + "namespace " + pluginParams.pm.Info.Name);
            sw.WriteLine(DoIndent(indent) + "{");
            ++indent;

            sw.WriteLine(DoIndent(indent) + "public enum Events");
            sw.WriteLine(DoIndent(indent) + "{");
            ++indent;

            var evtList = new Dictionary<string, Event>();
            foreach (var machine in machineList)
            {
                foreach (var anEvent in machine.Events)
                {
                    //evtList.Add(anEvent);
                    //evtList.Add(anEvent.Name, anEvent);
                    evtList[anEvent.Name] = anEvent;
                    /*
                    if (anEvent != StaterV.Attributes.Event.CreateEpsilon())
                    {
                        sw.WriteLine(DoIndent(indent) + anEvent.SafeName + ",");
                    }
                     */
                }
            }

            foreach (KeyValuePair<string, Event> @event in evtList)
            {
                sw.WriteLine(DoIndent(indent) + @event.Value.SafeName + ",");
            }
            //sw.WriteLine(DoIndent(indent) + StaterV.Attributes.Event.CreateEpsilon().SafeName + ",");
            --indent;
            sw.WriteLine(DoIndent(indent) + "}");

            --indent;
            sw.WriteLine(DoIndent(indent) + "}");
            sw.Close();
        }

        //private StaterV.StateMachine.StateMachine machine;
        private string path;

        private const string statesEnumName = "States";
        //private const string transitionsEnumName = "Transitions";
        private const string eventsEnumName = "Events";
        private const string stateVariableName = "state";
        private const string eventArg = "_event";

        private string DoIndent(int count)
        {
            var res = "";
            for (int i = 0; i < count; i++)
            {
                res += "\t";
            }
            return res;
        }

        private void WriteConstructor(StreamWriter sr, int _indentCount, StateMachine machine)
        {
            var indentCount = _indentCount;
            sr.WriteLine(DoIndent(indentCount) + "public {0}()", machine.Name);
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;
            sr.WriteLine(DoIndent(indentCount) + "{0} = {1}.{2};", 
                stateVariableName, statesEnumName, 
                machine.StartState.TheAttributes.Name);
            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");
            //Запись стартового состояния в конструкторе.
        }

        private void WriteStateProcessor(StreamWriter sr, int _indentCount, StateMachine machine, bool withStrings)
        {
            var indentCount = _indentCount;

            if (withStrings)
            {
                sr.WriteLine(DoIndent(indentCount) + "public void ProcessEventStr(string {0})",
                    eventArg);
            }
            else
            {
                sr.WriteLine(DoIndent(indentCount) + "public void ProcessEvent(Events {0})",
                             eventArg);
            }
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;

            //switch по состояниям.
            sr.WriteLine(DoIndent(indentCount) + "switch ({0})", stateVariableName);
            sr.WriteLine(DoIndent(indentCount) + "{");
            foreach (var state in machine.States)
            {
                sr.WriteLine(DoIndent(indentCount) + "case {0}.{1}:",
                    statesEnumName, state.TheAttributes.Name);
                indentCount++;
                WriteEventSwitch(sr, indentCount, state, withStrings);
                sr.WriteLine(DoIndent(indentCount) + "break;");
                indentCount--;
            }
            sr.WriteLine(DoIndent(indentCount) + "}");

            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");
        }

        private void WriteActionsDeclarations(StreamWriter sr, int _indentCount, StateMachine machine)
        {
            var indentCount = _indentCount;
         
            //Создаем список выходных воздействий.
            HashSet<StaterV.Attributes.Action> actionSet = new HashSet<StaterV.Attributes.Action>();
            foreach (var state in machine.States)
            {
                PutActionsToSet(ref actionSet, state.TheAttributes.EntryActions);
                PutActionsToSet(ref actionSet, state.TheAttributes.ExitActions);

                foreach (var arrow in state.OutgoingArrows)
                {
                    var trans = arrow as Transition;
                    if (trans == null)
                    {
                        throw new StaterV.Exceptions.FormStructureException("From state " + state.TheAttributes.Name +
                        " doing out not transition!");
                    }
                    PutActionsToSet(ref actionSet, trans.TheAttributes.Actions);
                }
            }
            
            //Печатаем.
            foreach (var action in actionSet)
            {
                sr.WriteLine(DoIndent(indentCount) + "/// <summary>");
                sr.WriteLine(DoIndent(indentCount) + "///" + action.Comment);
                sr.WriteLine(DoIndent(indentCount) + "/// </summary>");
                sr.WriteLine(DoIndent(indentCount) + "public abstract void " + action.Name + "();");
            }
        }

        private void PutActionsToSet(ref HashSet<StaterV.Attributes.Action> actionSet, IEnumerable<StaterV.Attributes.Action> actions)
        {
            foreach (var action in actions)
            {
                actionSet.Add(action);
            }
        }

        /// <summary>
        /// Process one state.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="_indentCount"></param>
        /// <param name="state"></param>
        /// <param name="withStrings"></param>
        private void WriteEventSwitch(StreamWriter sr, int _indentCount, State state, bool withStrings)
        {
            var indentCount = _indentCount;

            string processFunction = (withStrings) ? "ProcessEventStr" : "ProcessEvent";

            sr.WriteLine(DoIndent(indentCount) + "switch ({0})", eventArg);
            sr.WriteLine(DoIndent(indentCount) + "{");
            foreach (var arr in state.OutgoingArrows)
            {
                var trans = arr as Transition;
                if (trans != null)
                {
                    if (trans.TheAttributes.TheEvent != null)
                    {
                        if (withStrings)
                        {
                            sr.WriteLine(DoIndent(indentCount) + "case \"{0}\":", 
                                trans.TheAttributes.TheEvent.Name);
                        }
                        else
                        {
                            sr.WriteLine(DoIndent(indentCount) + "case {0}.{1}:", eventsEnumName,
                                trans.TheAttributes.TheEvent.SafeName);
                        }
                        indentCount++;

                        //Сделать переход.
                        State st = (State)trans.End;
                        sr.WriteLine(DoIndent(indentCount) + "{0} = {1}.{2};",
                            stateVariableName, statesEnumName, st.TheAttributes.Name);
                        
                        //Выходные воздействия при выходе из состояния.
                        WriteActions(sr, indentCount, state.TheAttributes.ExitActions);

                        //Выходные воздействия на переходе.
                        WriteActions(sr, indentCount, trans.TheAttributes.Actions);

                        //Выходные воздействия при входе в состояние.
                        State newState = trans.End as State;
                        if (newState == null)
                        {
                            throw (new StaterV.Exceptions.InvalidDiagramException("End of transition from state " +
                            state.TheAttributes.Name + " is not a State!"));
                        }
                        WriteActions(sr, indentCount, newState.TheAttributes.EntryActions);

                        sr.WriteLine(DoIndent(indentCount) + "break;");
                        indentCount--;
                    }
                    else
                    {
                        throw (new Exception());
                    }
                }
                else
                {
                    throw (new Exception());
                }

            }

            //Вложенный автомат.
            if (state.TheAttributes.NestedMachines != null)
            {
                if (state.TheAttributes.NestedMachines.Count > 0)
                {
                    sr.WriteLine(DoIndent(indentCount) + "default:");
                    ++indentCount;
                    foreach (var machine in state.TheAttributes.NestedMachines)
                    {
                        //sr.WriteLine(DoIndent(indentCount) + "{0}_{1}.{2}({3});", 
                        //    state.TheAttributes.Name, machine, processFunction, eventArg);
                        sr.WriteLine(DoIndent(indentCount) + "{0}.{1}({2});", 
                            machine.Name, processFunction, eventArg);
                    }
                    sr.WriteLine(DoIndent(indentCount) + "break;");
                    --indentCount;

                }
            }

            sr.WriteLine(DoIndent(indentCount) + "}");
        }

        private void WriteActions(StreamWriter sr, int _indentCount, List<StaterV.Attributes.Action> actions)
        {           
            var indentCount = _indentCount;

            if (actions == null)
            {
                return;
            }

            foreach (var act in actions)
            {
                sr.WriteLine(DoIndent(indentCount) + act.Name + "();");
            }
        }

        private void GenerateCode(StateMachine machine)
        {
            StreamWriter sr = new StreamWriter(path);
            var indentCount = 0;

            sr.WriteLine(DoIndent(indentCount) + "namespace " + pluginParams.pm.Info.Name);
            sr.WriteLine(DoIndent(indentCount) + "{");
            ++indentCount;

            sr.WriteLine(DoIndent(indentCount) + "public abstract class " + machine.Name);
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;

            //Состояния.
            sr.WriteLine(DoIndent(indentCount) + "public enum " + statesEnumName);
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;
            foreach (var state in machine.States)
            {
                sr.WriteLine(DoIndent(indentCount) + state.TheAttributes.Name + ",");
            }
            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");

            //Переходы.
            sr.WriteLine(DoIndent(indentCount) + "public enum Transitions");
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;
            foreach (var transition in machine.Transitions)
            {
                sr.WriteLine(DoIndent(indentCount) + transition.TheAttributes.Name + ",");
            }
            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");

            //События.
            /*
            sr.WriteLine(DoIndent(indentCount) + "public enum Events");
            sr.WriteLine(DoIndent(indentCount) + "{");
            indentCount++;
            foreach (var anEvent in machine.Events)
            {
                sr.WriteLine(DoIndent(indentCount) + anEvent.SafeName + ",");
            }
            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");
             * */

            //Переменная, в которой хранится состояние.
            sr.WriteLine(DoIndent(indentCount) + "private {0} state;", statesEnumName);

            WriteNestedMachines(sr, indentCount, machine);

            WriteConstructor(sr, indentCount, machine);

            //Объявления выходных воздействий.
            WriteActionsDeclarations(sr, indentCount, machine);

            //Функция обработки событий.
            WriteStateProcessor(sr, indentCount, machine, false);

            WriteStateProcessor(sr, indentCount, machine, true);

            indentCount--;
            sr.WriteLine(DoIndent(indentCount) + "}");

            --indentCount;
            sr.WriteLine(DoIndent(indentCount) + "}");
            sr.Close();
        }

        #region definitions
        private void WriteNestedMachines(StreamWriter sr, int _indentCount, StateMachine machine)
        {
            int indentCount = _indentCount;
            foreach (var state in machine.States)
            {
                if (state.TheAttributes.NestedMachines == null)
                {
                    continue;
                }

                foreach (var nestedMachine in state.TheAttributes.NestedMachines)
                {
                    sr.WriteLine(DoIndent(indentCount) + "public {0} {1} {{ get; set; }}", nestedMachine.Type, nestedMachine.Name);
                }
            }
        }
        #endregion
    }