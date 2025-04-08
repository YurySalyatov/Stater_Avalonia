using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SpinVeriff.GUI;
using StaterV;
using StaterV.Attributes;
using StaterV.PluginManager;
using StaterV.StateMachine;

// TODO: practice NEED


namespace SpinVeriff
{
    public class SpinPlugin : ButtonPlugin
    {
        private const string ReportName = "\\report.txt";
        private const string ConOutputName = "\\con_output.txt";
        private string conOutputFullPath;
        private Options options;
        private string report;
        public const string Spin = "spin623.exe";
        private List<StateMachine> machineList;
        private List<AutomatonExecution> executionsList;
        public List<AutomatonExecution> ExecutionsList
        {
            get { return executionsList; }
        }

        private List<string> LTL;
        private ModelGenerator modelGenerator;
        private bool silent = false;

        public const string DefaultModelFile = "spinModel.ltl";
        private string modelFile = DefaultModelFile;

        public override PluginRetVal Start(PluginParams pluginParams)
        {
            report = "";
            Params = pluginParams;
            PluginRetVal res = new PluginRetVal();
            res.signal = PluginRetVal.Signal.OK;
            StartOptionsForm();
            
            return res;
        }

        public override PluginRetVal SilentStart(PluginParams pluginParams)
        {
            silent = true;
            PluginRetVal res = new PluginRetVal();
            res.signal = PluginRetVal.Signal.OK;
            GUI.OptionsLogic ol = new OptionsLogic();
            Params = pluginParams;
            var wd = Params.pm.Info.GetWorkFolder();
            ol.TryReadOptions(wd);
            options = ol.GetOptions();
            LTL = LTLConverter.SetLTLExt(options.FormulaeLTL);
            ol.TrySaveState(wd);
            if (options.OnlyScriptsMode)
            {
                return CreateScripts();
            }

            res = StartAction();
            return res;
        }

        private PluginRetVal StartOptionsForm()
        {
            GUI.OptionsLogic ol = new OptionsLogic();
            var res = ol.Start(Params.pm.Info.GetWorkFolder());
            options = ol.GetOptions();

            LTL = LTLConverter.SetLTLExt(options.FormulaeLTL);

            PluginRetVal retVal = new PluginRetVal();
            switch (res)
            {
                case OptionsLogic.Result.OK:
                    retVal = StartAction();
                    break;
                case OptionsLogic.Result.Cancel:
                    retVal.signal = PluginRetVal.Signal.OK;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return retVal;
        }

        private PluginRetVal StartAction()
        {
            PluginRetVal res;
            if (PrepareModel(out res)) return res;

            conOutputFullPath = Params.pm.Info.GetWorkFolder() + ConOutputName;

            WriteNewFile(conOutputFullPath, "Verification output:\n");

            if (CreatePan())
            {
                CompilePan();
                for (int iFormula = 0; iFormula < LTL.Count; ++iFormula)
                {
                    ExecPan(iFormula);
                }
            }
            else
            {
                //OpenNotepad(Params.pm.Info.GetWorkFolder() + ReportName);
                res.signal = PluginRetVal.Signal.Error;
            }
            WriteReport();
            return res;
        }

        private PluginRetVal CreateScripts()
        {
            PluginRetVal res;
            if (PrepareModel(out res)) return res;

            using (StreamWriter sw = new StreamWriter(Params.pm.Info.GetWorkFolder() + "\\run.sh"))
            {
                sw.WriteLine(@"#!/bin/bash");
                sw.WriteLine("rm spinModel.ltl.trail pan.*");
                sw.WriteLine(@"spin -a spinModel.ltl");
                sw.WriteLine(@"gcc pan.c " + GetCmpSymbol() + "-o pan");
                for (int i = 0; i < LTL.Count; ++i)
                {
                    sw.WriteLine("rm spinModel.ltl.trail");
                    sw.WriteLine(@"./pan -m10000000 -a -n -N f" + i);

                    //for counter-example
                    sw.WriteLine(@"./pan -r -N f" + i);

                    sw.WriteLine("if [ -f spinModel.ltl.trail ]; then");
                    sw.WriteLine("\texit");
                    sw.WriteLine("fi");
                }
            }
            using (StreamWriter sw = new StreamWriter(Params.pm.Info.GetWorkFolder() + "\\run.bat"))
            {
                sw.WriteLine(@"spin623 -a spinModel.ltl");
                sw.WriteLine(@"gcc pan.c " + GetCmpSymbol() + "-o pan");
                for (int i = 0; i < LTL.Count; ++i)
                {
                    sw.WriteLine("del spinModel.ltl.trail");
                    sw.WriteLine(@"pan -m10000000 -a -n -N f" + i);

                    //for counter-example
                    sw.WriteLine(@"pan -r -N f" + i);
                    sw.WriteLine("if exist spinModel.ltl.trail goto 1");
                }
                sw.WriteLine(":1");
                sw.WriteLine("copy nul > done");
            }
            return res;
        }

        private bool PrepareModel(out PluginRetVal res)
        {
            report = "";
            res = new PluginRetVal();
            res.signal = PluginRetVal.Signal.OK;

            machineList = Params.pm.ExportStateMachines();
            if (machineList == null || machineList.Count == 0)
            {
                return true;
            }

            AddForkMachines();

            modelGenerator = new ModelGenerator();
            List<string> model;
            bool valid = modelGenerator.CreateModel(this, machineList, options, ExecutionsList, out model);
            if (!valid)
            {
                res.signal = PluginRetVal.Signal.Error;
                res.message = modelGenerator.ResultMessage;
            }

            else
            {
                string path = Params.pm.Info.GetWorkFolder() + "\\" + modelFile;
                StreamWriter sw = new StreamWriter(path);
                for (int i = 0; i < model.Count; i++)
                {
                    sw.WriteLine(model[i]);
                }
                sw.Close();
            }
            return false;
        }

        private void AddForkMachines()
        {
            executionsList = new List<AutomatonExecution>();
            foreach (var m in machineList)
            {
                foreach (var state in m.States)
                {
                    foreach (var execution in state.TheAttributes.EntryExecutions)
                    {
                        //options.EnteredObjects.Add(
                        //    new Options.ObjectName {Type = execution.Type, Name = execution.Name});
                        ExecutionsList.Add(execution);
                    }
                }
            }
        }

        private void WriteNewFile(string path, string text)
        {
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(text);
            sw.Close();
        }

        private void AddToFile(string path, string text)
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(text);
            sw.Close();
        }

        private void WriteReport()
        {
            string path = Params.pm.Info.GetWorkFolder() + ReportName;
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(report);
            sw.Close();

            //Открыть блокнот.
            if (!silent)
            {
                OpenNotepad(path);
            }
        }

        private static void OpenNotepad(string path)
        {
            Process notepad = new Process();
            notepad.StartInfo.FileName = "notepad";
            notepad.StartInfo.Arguments = path;
            notepad.Start();

            //notepad.WaitForExit();
            //notepad.Close();
        }

        private void ExecPan(int iFormula)
        {
            Process pan = new Process();
            pan.StartInfo.FileName = Params.pm.Info.GetWorkFolder() + "\\pan.exe";
            pan.StartInfo.WorkingDirectory = Params.pm.Info.GetWorkFolder();
            pan.StartInfo.Arguments = "-m10000000 -a -n -I -N f" + iFormula;
            pan.StartInfo.RedirectStandardOutput = true;
            pan.StartInfo.UseShellExecute = false;
            pan.Start();

            string panOutput = pan.StandardOutput.ReadToEnd();
            //string panOutput = "";
            pan.WaitForExit();
            pan.Close();

            AddToFile(conOutputFullPath, panOutput);

            ParsePanOutput(panOutput, iFormula);
        }

        private void ParsePanOutput(string panOutput, int iFormula)
        {
            // /*
            int iError = panOutput.IndexOf("errors: ");
            int errStart = iError + 8;//Начало 
            int errEnd = panOutput.IndexOfAny(new[] {' ', '\n', '\r'}, errStart);
            string e = panOutput.Substring(errStart, errEnd - errStart);
            // * */
            int numErrors;// = int.Parse(panOutput.Substring(errStart, errEnd-errStart));
            if (!int.TryParse(panOutput.Substring(errStart, errEnd - errStart), out numErrors))
            {
                numErrors = 0;
            }
            //int numErrors = 1;
            report += iFormula + ". " + LTL[iFormula] + "\r\n";
            if (numErrors == 0)
            {
                //Ура! Ошибок нет!
                report += "Verification successful!";
            }
            else
            {
                report += "There are errors. Counterexample:\n";
                GetTrail(iFormula);
            }
            report += "\r\n\r\n";
        }

        private void GetTrail(int iFormula)
        {
            /*
            Process spin = new Process();
            spin.StartInfo.FileName = Spin;
            spin.StartInfo.Arguments = "-t " + Params.pm.Info.GetWorkFolder() + "\\spinModel.ltl";
            spin.StartInfo.RedirectStandardOutput = true;
            spin.StartInfo.UseShellExecute = false;
            spin.StartInfo.WorkingDirectory = Params.pm.Info.GetWorkFolder();
            spin.Start();
            var spinOutput = spin.StandardOutput.ReadToEnd();
            spin.WaitForExit();
            spin.Close();
             */

            Process pan = new Process();
            pan.StartInfo.FileName = Params.pm.Info.GetWorkFolder() + "\\pan.exe";
            pan.StartInfo.WorkingDirectory = Params.pm.Info.GetWorkFolder();
            pan.StartInfo.Arguments = "-r -N f" + iFormula;
            pan.StartInfo.RedirectStandardOutput = true;
            pan.StartInfo.UseShellExecute = false;
            pan.Start();
            var panOutput = pan.StandardOutput.ReadToEnd();
            AddToFile(conOutputFullPath, panOutput);

            ParseTrail(panOutput);
        }

        private void ParseTrail(string trail)
        {
            var tokens = trail.Split('\n');
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            try
            {
                foreach (var t in tokens)
                {
                    if (t.IndexOf("Never claim") != 0)
                    {
                        var line = ProcessToken(t);
                        if (line != "")
                        {
                            sb.AppendLine(line);
                        }
                        //sb.Append('\n');
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            report += sb.ToString();
        }

        private string ProcessToken(string t)
        {
            string res = "";
            string token = t.Trim();
            if (ParseMachine(token, out res))
            {
                return res;
            }
            if (ParseEnvironment(token, out res))
            {
                return res;
            }
            if (ParseEventSource(token, out res))
            {
                return res;
            }
            if (ParseCycle(token, out res))
            {
                return res;
            }
            return res;
        }

        private bool ParseEnvironment(string token, out string res)
        {
            res = "";
            if (token.IndexOf("Environment") != -1)
            {
                res = token;
                return true;
            }
            return false;
        }

        private bool ParseMachine(string token, out string res)
        {
            res = "";
            if (token.IndexOf("machine") == 0)
            {
                int iDot = token.IndexOf('.');
                if (iDot == -1)
                {
                    //error ):
                    res = "-" + token;
                    return true;
                }

                const int sNum = 7;
                int aNum = iDot - sNum;
                //int nMachine = int.Parse(token.Substring(sNum, aNum));
                int nMachine;
                if (!int.TryParse(token.Substring(sNum, aNum), out nMachine))
                {
                    return false;
                }
                //string machineName = options.EnteredObjects[nMachine].Name;
                string machineName = "";

                if ((nMachine >= modelGenerator.AllMachines.Count) ||
                    (nMachine < 0))
                {
                    machineName = "machine" + nMachine;
                }
                else
                {
                    machineName = modelGenerator.AllMachines[nMachine];
                }

                res = machineName + token.Substring(iDot);

                return true;
            }
            return false;
        }

        private static bool ParseEventSource(string token, out string res)
        {
            res = "";
            if (token.IndexOf("printf") != -1)
            {
                return false;
            }
            if (token.IndexOf("source") != -1)
            {
                res = token;
                return true;
            }
            return false;
        }

        private static bool ParseCycle(string token, out string res)
        {
            res = "";
            if (token.IndexOf("CYCLE") != -1)
            {
                res = token;
                return true;
            }
            return false;
        }

        private void CompilePan()
        {
            Process gcc = new Process();
            gcc.StartInfo.FileName = "gcc.exe";
            //gcc.StartInfo.FileName = "java";
            //gcc.StartInfo.FileName = "cmd.exe";
            var defSymbol = GetCmpSymbol();
            gcc.StartInfo.Arguments = "pan.c " + defSymbol + "-o pan.exe";
            gcc.StartInfo.RedirectStandardOutput = true;
            gcc.StartInfo.UseShellExecute = false;
            gcc.StartInfo.WorkingDirectory = Params.pm.Info.GetWorkFolder();
            try
            {
                gcc.Start();

            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("gcc не установлен");
            }
            var gccOutput = gcc.StandardOutput.ReadToEnd();

            gcc.WaitForExit();
            gcc.Close();

            AddToFile(conOutputFullPath, gccOutput);

            if (!gccOutput.Trim().Equals(""))
            {
                ParseGCCError();
            }
        }

        private string GetCmpSymbol()
        {
            switch (options.CompilePanOptions)
            {
                case Options.ECompilePanOptions.Common:
                    return "";
                case Options.ECompilePanOptions.DCOLLAPSE:
                    return "-DCOLLAPSE ";
                case Options.ECompilePanOptions.DCOLLAPSE_DMA144_DSC:
                    return "-DCOLLAPSE -DMA=144 -DSC ";
                case Options.ECompilePanOptions.DMA64:
                    return "-DMA=64 ";
                case Options.ECompilePanOptions.DHC:
                    return "-DHC ";
                case Options.ECompilePanOptions.DBITSTATE:
                    return "-DBITSTATE ";
                default:
                    return "";
            }
        }

        private void ParseGCCError()
        {
            throw new NotImplementedException();
        }

        private bool CreatePan()
        {
            Process spin = new Process();
            spin.StartInfo.FileName = Spin;
            spin.StartInfo.Arguments = "-a " + modelFile; 
            spin.StartInfo.RedirectStandardOutput = true;
            spin.StartInfo.UseShellExecute = false;
            spin.StartInfo.WorkingDirectory = Params.pm.Info.GetWorkFolder();
            try
            {
                spin.Start();

            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("Неправильно установлен spin. Должен быть прописан в PATH путь к файлу spin623.exe");
            }
            var spinOutput = spin.StandardOutput.ReadToEnd();
            spin.WaitForExit();
            spin.Close();

            AddToFile(conOutputFullPath, spinOutput);

            if (!spinOutput.Trim().Equals(""))
            {
                if (ParseError(spinOutput))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ParseError(string spinOutput)
        {
            var lines = spinOutput.Split('\n');
            bool res = false;
            foreach (var l in lines)
            {
                if (l.Trim() == "")
                {
                    continue;
                }
                if (l.IndexOf("ltl") == 0)
                {
                    continue;
                }
                if (l.IndexOf("Error") != -1)
                {
                    res = true;
                    break;
                }
            }
            if (res)
            {
                report = "Verification fail! Model icorrect!\n\r" + spinOutput;
            }
            return res;
        }

        public PluginParams Params { get; private set; }

    }
}
