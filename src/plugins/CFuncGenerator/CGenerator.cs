namespace CFuncGenerator;

class CGenerator
    {
        public CGenerator(IParams p)
        {
            iParams = p;
        }

        private IParams iParams;
        private Indent indent;

        public bool GenerateAll()
        {
            bool res = true;
            foreach (var stateMachine in iParams.Machines.AsParallel().Where(stateMachine => !GenerateOneMachine(stateMachine)))
            {
                res = false;
            }
            return res;
        }

        private bool GenerateOneMachine(StateMachine stateMachine)
        {
            //Check the file.
            var pathHeader = iParams.WorkDirectory + @"\" + GetHeaderName(stateMachine);
            if (!GoodHeader(stateMachine, pathHeader))
            {
                if (!GenInitHeader(stateMachine, pathHeader))
                {
                    return false;
                }
            }
            else
            {
                if (!IterativeGenHeader(stateMachine, pathHeader))
                {
                    return false;
                }
            }

            var pathModule = iParams.WorkDirectory + @"\" + GetModuleName(stateMachine);
            if (!GoodModule(stateMachine, pathModule))
            {
                if (!GenInitModule(stateMachine, pathModule))
                {
                    return false;
                }
            }
            else
            {
                if (!IterativeGenModule(stateMachine, pathModule))
                {
                    return false;
                }
            }
            return false;
        }

        private bool GenInitModule(StateMachine stateMachine, string pathModule)
        {
            try
            {
                var code = new List<string>();
                code.AddRange(CreateInclude(stateMachine));
                //code.AddRange();
                code.AddRange(CreateUserDefinitions(stateMachine));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private IEnumerable<string> CreateInclude(StateMachine stateMachine)
        {
            var res = new List<string>();
            res.Add("/*User includes*/");
            res.Add("");    
            res.Add("/*End user includes*/");
            res.Add("");
            res.Add("#include \"" + GetHeaderName(stateMachine) + "\"");
            res.Add("");

            return res;
        }

        private IEnumerable<string> CreateUserDefinitions(StateMachine stateMachine)
        {
            var res = new List<string>();
            res.Add("/*User definitions*/");
            res.Add("");
            res.Add("/*End user definitions*/");
            res.Add("");
            return res;
        }


        private string GetHeaderName(StateMachine stateMachine)
        {
            return stateMachine.Name + ".h";
        }
        
        private string GetModuleName(StateMachine stateMachine)
        {
            return stateMachine.Name + ".c";
        }

        private bool IterativeGenModule(StateMachine stateMachine, string pathModule)
        {
            throw new NotImplementedException();
        }

        private bool GenInitHeader(StateMachine stateMachine, string pathHeader)
        {
            try
            {
                var code = new List<string>();
                code.AddRange(CreateOnce(stateMachine));

                code.AddRange(CreatePrototype(stateMachine));

                code.AddRange(CreateEndif(stateMachine));

                WriteCode(pathHeader, code);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private IEnumerable<string> CreatePrototype(StateMachine stateMachine)
        {
            List<string> res = new List<string>();
            res.Add("void " + stateMachine.Name + "();");
            res.Add("");    
            return res;
        }

        private static void WriteCode(string pathHeader, List<string> code)
        {
            using (var sw = new StreamWriter(pathHeader))
            {
                foreach (var line in code)
                {
                    sw.WriteLine(line);
                }
            }
        }

        private IEnumerable<string> CreateEndif(StateMachine stateMachine)
        {
            var res = new List<string>();
            res.Add("#endif");
            res.Add("");    
            return res;
        }

        private IEnumerable<string> CreateOnce(StateMachine stateMachine)
        {
            List<string> res = new List<string>();
            res.Add("#ifndef _" + stateMachine.Name + "_H");
            res.Add("#define _" + stateMachine.Name + "_H");
            res.Add("");
            return res;
        }

        private bool IterativeGenHeader(StateMachine stateMachine, string pathHeader)
        {
            return true;
        }

        private bool GoodModule(StateMachine stateMachine, string pathModule)
        {
            return false;
            if (!File.Exists(pathModule))
            {
                return false;
            }
            return true;
        }

        private bool GoodHeader(StateMachine stateMachine, string pathHeader)
        {
            return false;
            if (!File.Exists(pathHeader))
            {
                return false;
            }
            return true;
        }

    }