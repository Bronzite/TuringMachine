using System;
using System.Collections.Generic;
using System.IO;


namespace StateMachines
{
    public class TuringMachine
    {
        /// <summary>
        /// Static function that returns an instance of the Turning Machine class containing the machine described in the CSV.
        /// </summary>
        /// <param name="sFilename">Path to the CSV to load.</param>
        /// <returns>The Turning Machine described by the file.</returns>
        public static TuringMachine LoadCSV(string sFilename)
        {
            return LoadCSV(File.OpenRead(sFilename));
        }

        /// <summary>
        /// Static function that returns an instance of the TurningMachine class containing the machine described in the stream.
        /// </summary>
        /// <param name="csvStream">Stream to load from.</param>
        /// <returns>The Turning Machine described by the stream.</returns>
        public static TuringMachine LoadCSV(Stream csvStream)
        {
            TuringMachine tm = new TuringMachine();
            StreamReader sr = new StreamReader(csvStream);

            string sSymbols = sr.ReadLine();
            string[] sSymbolList = sSymbols.Split(',');
            foreach(string sCurSymbol in sSymbolList)
            {
                tm.AddSymbol(sCurSymbol);
            }
            List<string> lstStateLines = new List<string>();

            while (!sr.EndOfStream)
            {
                string sStateLine = sr.ReadLine();
                string[] sParameters = sStateLine.Split(',');
                State newState = tm.CreateState(sParameters[0]);
                tm.AddState(newState);
                lstStateLines.Add(sStateLine);
            }
            foreach(string sStateLine in lstStateLines)
            { 
                string[] sParameters = sStateLine.Split(',');
                State newState = tm.mStates[tm.mStateNames[sParameters[0]]];
                
                for (int i =1; i<sParameters.Length;i+=4)
                {
                    int iTriggerSymbol = tm.GetIndexOfSymbol(sParameters[i]);
                    int iWriteSymbol = tm.GetIndexOfSymbol(sParameters[i + 1]);
                    char c = sParameters[i + 2].Trim().ToCharArray()[0];
                    int iNextState = tm.GetIndexOfState(sParameters[i + 3]);
                    State.Transition newTransition = new State.Transition(iTriggerSymbol, iWriteSymbol, c, iNextState);
                    newState.Transitions.Add(newTransition.TriggerSymbol, newTransition);
                }
                
            }

            return tm;
        }

        /// <summary>
        /// Generic Constructor
        /// </summary>
        public TuringMachine()
        {
            mSymbols = new Dictionary<int, string>();
            mStates = new Dictionary<int, State>();
            mStateNames = new Dictionary<string, int>();
        }

        /// <summary>
        /// Creates a TurningMachine class with an alphabet of the passed symbols.
        /// </summary>
        /// <param name="ieSymbols">Symbol alphabet to initialize the Turingmachine with.</param>
        public TuringMachine(IEnumerable<string> ieSymbols):this()
        {
            int iSymbolIndex = 0;
            foreach (string curSymbol in ieSymbols)
            {
                if (!mSymbols.ContainsValue(curSymbol))
                    mSymbols.Add(iSymbolIndex++, curSymbol);
                else
                    throw new Exception(string.Format("Duplicate Symbol: {0}",curSymbol));
            }
        }

        private Dictionary<string, int> mStateNames;
        
        /// <summary>
        /// Add a symbol to the Turing Machine's alphabet.
        /// </summary>
        /// <param name="sSymbol">The Symbol to Add</param>
        private void AddSymbol(string sSymbol)
        {
            if (!mSymbols.ContainsValue(sSymbol))
                mSymbols.Add(mSymbols.Count, sSymbol);
            else
                throw new Exception("Duplicate Symbol");
        }

        /// <summary>
        /// Find the index number of the state with name s.
        /// </summary>
        /// <param name="s">The name of the state to find.</param>
        /// <returns>The index of the state with name s, or zero if the state was not found.</returns>
        public int GetIndexOfState(string s)
        {
            if (mStateNames.ContainsKey(s))
                return mStateNames[s];
            else
                return 0;
        }

        /// <summary>
        /// Return the State with the given ID number.
        /// </summary>
        /// <param name="iID">The ID number of the state to recover.</param>
        /// <returns>The State with the given ID number, or null if it could not be found.</returns>
        public State GetStateByID(int iID)
        {
            if (mStates.ContainsKey (iID))
                return mStates[iID];
            else
                return null;
        }

        /// <summary>
        /// Return the index within the alphabet of the given symbol.
        /// </summary>
        /// <param name="s">The symbol to find the index of.</param>
        /// <returns>The index of the given symbol, or -1 if the symbol could not be found.</returns>
        public int GetIndexOfSymbol(string s)
        {
            for(int i=0; i<mSymbols.Count;i++)
            {
                if (mSymbols[i].Equals(s))
                    return i;
            }
            return -1;
        }

        private Dictionary<int, string> mSymbols;
        /// <summary>
        /// The Alphabet of Symbols in this Turning Machine.
        /// </summary>
        public Dictionary<int,string> Symbols { get { return mSymbols; } set { mSymbols = value; } }

        private Dictionary<int, State> mStates;
        /// <summary>
        /// The collection of States in this Turning Machine.
        /// </summary>
        public IEnumerable<State> States { get { return mStates.Values; } }

        private string mStartState ="";
        /// <summary>
        /// The name of the State the Turning Machine should begin execution in.
        /// </summary>
        public string StartState { get { return mStartState; } set { mStartState = value; } }

        private int mCurrentState;
        /// <summary>
        /// The index of the State the Turning Machine is currently in.
        /// </summary>
        public int CurrentState { get { return mCurrentState; } set { mCurrentState = value; } }

        private int mHeadPosition;
        /// <summary>
        /// The index of the location on the Tape the head is currently positioned at.
        /// </summary>
        public int HeadPosition { get { return mHeadPosition; } set { mHeadPosition = value; } }

        private bool mHalted;
        /// <summary>
        /// True if the Turning Machine has halted.
        /// </summary>
        public bool Halted { get { return mHalted; } set { mHalted = true; } }

        private int mStepCount;
        /// <summary>
        /// The number of steps this Turning Machine has take since it was last restarted.
        /// </summary>
        public int StepCount { get { return mStepCount; } }

        /// <summary>
        /// Step the Turning Machine over the given tape.
        /// </summary>
        /// <param name="oTape">The Tape the Turning Machine should work over.</param>
        public void Step(Tape oTape)
        {
            if(!mHalted)
            {
                State sCurrentState = mStates[mCurrentState];
                int iSymbol = oTape.ReadSymbol(mHeadPosition);
                State.Transition curTransitions = sCurrentState.Transitions[iSymbol];
                oTape.WriteSymbol(curTransitions.WriteSymbol,mHeadPosition);
                if (curTransitions.HeadMovement == State.Transition.HeadMovementOptions.LEFT) mHeadPosition--;
                if (curTransitions.HeadMovement == State.Transition.HeadMovementOptions.RIGHT) mHeadPosition++;
                mCurrentState = curTransitions.NextState;
                if (mCurrentState < 1) mHalted = true;
                mStepCount++;
            }
        }

        /// <summary>
        /// Add a new state to the Turning Machine.
        /// </summary>
        /// <param name="oState">The state to add.</param>
        public void AddState(State oState)
        {
            int iNewID = mStates.Count+1;
            oState.ID = iNewID;
            if (oState.Name == "") oState.Name = oState.ID.ToString();

            if (!mStates.ContainsKey(oState.ID))
            {
                mStates.Add(oState.ID, oState);
                mStateNames.Add(oState.Name, oState.ID);
                if(mStartState == "")
                {
                    mStartState = oState.Name;
                    mCurrentState = 1;
                }
            }
            else
            {
                throw new Exception(string.Format("ID {0} already in machine.", iNewID));
            }
        }

        /// <summary>
        /// Create and return a new state with the given name.
        /// </summary>
        /// <param name="sName">The name of the new state.</param>
        /// <returns>An inert state with the given name.</returns>
        public State CreateState(string sName)
        {
            State retval = new State();
            retval.Name = sName;
            return retval;
        }

        /// <summary>
        /// A state within the Turning Machine.
        /// </summary>
        public class State
        {
            /// <summary>
            /// Default Constructor
            /// </summary>
            public State()
            {
                mTransitions = new Dictionary<int, Transition>();
            }

            private int mID;
            /// <summary>
            /// The ID number of the state.
            /// </summary>
            public int ID { get { return mID; } set { mID = value; } }

            private string mName;
            /// <summary>
            /// The Name of the state.
            /// </summary>
            public string Name { get { return mName; } set { mName = value; } }

            private Dictionary<int, Transition> mTransitions;
            /// <summary>
            /// The set of Transitions the state can make.
            /// </summary>
            public Dictionary<int,Transition> Transitions { get { return mTransitions; } set { mTransitions = value; } }

            /// <summary>
            /// A transition from one state to another.
            /// </summary>
            public class Transition
                {
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="iTriggerSymbol">The symbol that selects this transition.</param>
                /// <param name="iWriteSymbol">The symbol to write into the current head location.</param>
                /// <param name="hmoHeadMovement">The direction to move the head.</param>
                /// <param name="iNextState">The ID number of the state to transition to.</param>
                    public Transition (int iTriggerSymbol, int iWriteSymbol, HeadMovementOptions hmoHeadMovement, int iNextState)
                    {
                        mTriggerSymbol = iTriggerSymbol;
                        mWriteSymbol = iWriteSymbol;
                        mHeadMovement = hmoHeadMovement;
                        mNextState = iNextState;
                    }
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="iTriggerSymbol">The symbol that selects this transition.</param>
                /// <param name="iWriteSymbol">The symbol to write into the current head location.</param>
                /// <param name="hmoHeadMovement">The direction to move the head.</param>
                /// <param name="iNextState">The ID number of the state to transition to.</param>
                public Transition(int iTriggerSymbol, int iWriteSymbol, int iHeadMovement,int iNextState):this(iTriggerSymbol,iWriteSymbol,(HeadMovementOptions)iHeadMovement,iNextState){}
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="iTriggerSymbol">The symbol that selects this transition.</param>
                /// <param name="iWriteSymbol">The symbol to write into the current head location.</param>
                /// <param name="hmoHeadMovement">The direction to move the head.</param>
                /// <param name="iNextState">The ID number of the state to transition to.</param>
                public Transition(int iTriggerSymbol, int iWriteSymbol, char cHeadMovement, int iNextState) : this(iTriggerSymbol, iWriteSymbol, HeadMovementOptions.STAY, iNextState)
                    {
                        if (char.ToUpper(cHeadMovement) == 'L') mHeadMovement = HeadMovementOptions.LEFT;
                        else if (char.ToUpper(cHeadMovement) == 'R') mHeadMovement = HeadMovementOptions.RIGHT;
                        else mHeadMovement = HeadMovementOptions.STAY;
                    }

                /// <summary>
                /// The set of possible head movements.
                /// </summary>
                public enum HeadMovementOptions: int { LEFT=0, RIGHT=1, STAY = 2};

                    private int mTriggerSymbol;
                /// <summary>
                /// The symbol that selects this transition.
                /// </summary>
                public int TriggerSymbol { get { return mTriggerSymbol; } set { mTriggerSymbol = value; } }

                    private HeadMovementOptions mHeadMovement;
                /// <summary>
                /// The direction to move the head when this transition begins.
                /// </summary>
                public HeadMovementOptions HeadMovement { get { return mHeadMovement; } set { mHeadMovement = value; } }

                    private int mWriteSymbol;
                /// <summary>
                /// The index of the symbol to write when this transition is made.
                /// </summary>
                    public int WriteSymbol { get { return mWriteSymbol; } set { mWriteSymbol = value; } }

                /// <summary>
                /// The ID of the next state to transition to.
                /// </summary>
                    private int mNextState;
                    public int NextState { get { return mNextState; } set { mNextState = value; } }


            }
        }
        
    }
}
