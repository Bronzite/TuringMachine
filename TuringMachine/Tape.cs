using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StateMachines
{
    /// <summary>
    /// A tape that a Turning Machine operates over.
    /// </summary>
    public class Tape
    {
        /// <summary>
        /// Construct a new tape.
        /// </summary>
        /// <param name="iSize">The initial size of the tape.</param>
        public Tape(int iSize)
        {
            mTape = new int[iSize];
        }
        
        private int[] mTape;

        private int mOffsetToZero;

        private int mGrowthSize = 1024;
        /// <summary>
        /// When the head goes off the end of the tape, the number of new positions that are allocated to the tape.
        /// </summary>
        public int GrowthSize { get { return mGrowthSize; } set { mGrowthSize = value; } }

        /// <summary>
        /// Write a specific symbol to the tape.
        /// </summary>
        /// <param name="sSymbol">The symbol to write.</param>
        /// <param name="iPosition">The position to write the symbol at.</param>
        /// <param name="dicSymbol">The dictionary used to resolve the symbol to its ID.</param>
        public void WriteSymbol(string sSymbol, int iPosition, Dictionary <int,string> dicSymbol)
        {
            if(dicSymbol.ContainsValue (sSymbol))
            {
                foreach(int i in dicSymbol.Keys)
                {
                    if (dicSymbol[i].Equals(sSymbol))
                    {
                        WriteSymbol(i, iPosition);
                        return;
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Unknown symbol: {0}", sSymbol));
            }
        }

        /// <summary>
        /// Write a specific symbol to the tape.
        /// </summary>
        /// <param name="iSymbolID">The Symbol ID to write.</param>
        /// <param name="iPosition">The position to write the Symbol at.</param>
        public void WriteSymbol(int iSymbolID, int iPosition)
        {
            int iRevisedPosition = iPosition + mOffsetToZero;
            if(iRevisedPosition < 0)
            {
                GrowLeft();
                iRevisedPosition = iPosition + mOffsetToZero;
            }
            if (iRevisedPosition >= mTape.Length) GrowRight();
            mTape[iRevisedPosition] = iSymbolID;
        }

        /// <summary>
        /// Return the symbol ID at the given position.
        /// </summary>
        /// <param name="iPosition">The position to return the symbol from.</param>
        /// <returns>The ID of the symbol at the given position.</returns>
        public int ReadSymbol(int iPosition)
        {
            int iRevisedPosition = iPosition + mOffsetToZero;
            if (iRevisedPosition < 0)
            {
                GrowLeft();
                iRevisedPosition = iPosition + mOffsetToZero;
            }
            while (iRevisedPosition >= mTape.Length) GrowRight();

            return mTape[iRevisedPosition];
        }

        /// <summary>
        /// The current number of positions allocated to the tape.
        /// </summary>
        public int TapeSize { get { return mTape.Length; } }

        /// <summary>
        /// Grow the Tape by GrowLength to the left.
        /// </summary>
        private void GrowLeft()
        {
            int[] iNewArray = new int[mTape.Length + mGrowthSize];
            mOffsetToZero += mGrowthSize;
            Array.Copy(mTape, 0, iNewArray, mGrowthSize, mTape.Length);
            mTape = iNewArray;
        }

        /// <summary>
        /// Grow the Tape by GrowLength to the right..
        /// </summary>
        private void GrowRight()
        {
            int[] iNewArray = new int[mTape.Length + mGrowthSize];
            Array.Copy(mTape, 0, iNewArray, 0, mTape.Length);
            mTape = iNewArray;
        }

        /// <summary>
        /// Write the tape to the given stream.
        /// </summary>
        /// <param name="s">The stream to write the tape to.</param>
        /// <param name="dicSymbols">The dictionary used to resolve symbols to their IDs.</param>
        public void Save(Stream s, Dictionary<int,string> dicSymbols)
        {
            StreamWriter sw = new StreamWriter(s);
            for(int i=0;i<mTape.Length-1;i++)
            {
                sw.Write(dicSymbols[mTape[ i]]);
                sw.Write(',');
            }
            sw.Write(dicSymbols[mTape[mTape.Length - 1]]);
            sw.Close();
        }
    }
}
