using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateMachines;

namespace TuringMachineConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            if(args.Length < 1)
            {
                Console.WriteLine("usage: TurningMachineConsole.exe -m machinefile [-i tape] [-o outputfile] [-t initialtapelength] [-g tapegrowthsize] [--verbose|-v]");
                return;
            }

            string sInputFile = "";
            string sOutputFile = "";
            string sTapeFile = "";
            int iTapeLength = 1024;
            int iGrowthSize = 1024;
            bool bVerbose = false;
            for(int i=0;i<args.Length;i++)
            {
                if (args[i].Equals("-m"))
                    sInputFile = args[i + 1];

                if (args[i].Equals("-i"))
                    sTapeFile = args[i + 1];

                if (args[i].Equals("-o"))
                    sOutputFile = args[i + 1];

                if (args[i].Equals("-t"))
                    iTapeLength = int.Parse(args[i + 1]);

                if (args[i].Equals("-g"))
                    iGrowthSize = int.Parse(args[i + 1]);

                if (args[i].Equals("-v") || args[i].Equals("--verbose"))
                    bVerbose = true;
            }

            if(sInputFile == "")
            {
                Console.WriteLine("No input file selected.");
                return;
            }

            TuringMachine tm = TuringMachine.LoadCSV(sInputFile);
            foreach(StateMachines.TuringMachine.State curState in tm.States)
            {
                Console.Write(curState.Name);
                foreach(StateMachines.TuringMachine.State.Transition transition in curState.Transitions.Values)
                {
                    string sDirection = "S";
                    if (transition.HeadMovement == TuringMachine.State.Transition.HeadMovementOptions.LEFT) sDirection = "L";
                    if (transition.HeadMovement == TuringMachine.State.Transition.HeadMovementOptions.RIGHT) sDirection = "R";

                    TuringMachine.State stateNextState = tm.GetStateByID(transition.NextState);
                    string sNextState = "HALT";
                    if (stateNextState != null) sNextState = stateNextState.Name;
                    string sTransition = string.Format("{0}/{1}/{2}/{3}",
                            tm.Symbols[transition.TriggerSymbol],
                            tm.Symbols[transition.WriteSymbol],
                            sDirection,
                            sNextState
                            );

                    Console.Write(" {0}", sTransition);
                }
                Console.WriteLine();
            }

            Tape machineTape;


            if (sTapeFile == "")
            {
                //Load Tape
                machineTape = new Tape(iTapeLength);
                machineTape.GrowthSize = iGrowthSize;
            }
            else
            {
                string sTapeData = System.IO.File.ReadAllText(sTapeFile);
                string[] TapeSymbols = sTapeData.Split(',');
                machineTape = new Tape(TapeSymbols.Length*2);
                machineTape.GrowthSize = iGrowthSize;
                int iTapeLocation = 0;
                foreach (string sCurSymbol in TapeSymbols)
                {
                    machineTape.WriteSymbol(sCurSymbol.Trim(), iTapeLocation++, tm.Symbols);
                }

            }
            
            tm.HeadPosition = iTapeLength / 2;
            DateTime LastTime = DateTime.Now;
            while (!tm.Halted)
            {
                

                TuringMachine.State curState =  tm.GetStateByID(tm.CurrentState);

                tm.Step(machineTape);
                if (bVerbose)
                {
                    if (tm.StepCount % 1000000 == 0)
                    {
                        DateTime newTime = DateTime.Now;
                        TimeSpan tsRun = newTime - LastTime;
                        Console.WriteLine("{0} steps. ({1} cycles per second.) [Tape Size: {2}]", tm.StepCount.ToString("#,000"), 1000000D / tsRun.TotalSeconds, machineTape.TapeSize);


                        LastTime = newTime;
                    }
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    machineTape.Save(ms, tm.Symbols);
                    Console.WriteLine( ASCIIEncoding.UTF8.GetString(ms.ToArray()));
                }
            }

            Console.WriteLine("Machine halted after {0} steps.", tm.StepCount);

            if(sOutputFile != "")
            {
                machineTape.Save(System.IO.File.OpenWrite(sOutputFile), tm.Symbols);
            }
        }
    }
}
