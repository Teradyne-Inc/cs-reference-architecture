using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib.Execute {
    public class Search : ILib.IExecute.ISearch {
        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget) =>
            LinearFullProcess(outValues, inFrom, inIncrement, inOffset, outTarget, out _, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
            out Site<int> closestIndex) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, outTarget, out closestIndex, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
            out Site<int> closestIndex, out Site<Tout> closestOut) {
            Site<int> closestIndexLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            Site<Tout> closestOutLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            ForEachSite(site => {
                List<dynamic> deltas = outValues.Select(v => Math.Abs(v[site] - (dynamic)outTarget)).ToList();
                dynamic closestDeltaSoFar = deltas[0];
                int closestIndexSoFar = 0;
                for (int i = 1; i < outValues.Count; i++) {
                    if (Math.Abs(deltas[i]) < Math.Abs(closestDeltaSoFar)) {
                        closestDeltaSoFar = deltas[i];
                        closestIndexSoFar = i;
                    }
                }
                closestIndexLocal[site] = closestIndexSoFar;
                closestOutLocal[site] = outValues[closestIndexSoFar][site];
            });
            closestIndex = closestIndexLocal;
            closestOut = closestOutLocal;
            return closestIndex.Select(t => (Tin)(t * (dynamic)inIncrement + inFrom + inOffset));
        }

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget) =>
            LinearFullProcess(outValues, inFrom, inIncrement, inOffset, outTarget, out _, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
            out Site<int> closestIndex) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, outTarget, out closestIndex, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
            out Site<int> closestIndex, out Site<Tout> closestOut) {
            Site<int> closestIndexLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            ForEachSite(site => { // should be using DSP for this actually, but difficult with generics & delegates
                Samples<Tout> deltas = TerMath.Abs(outValues[site] - (dynamic)outTarget);
                deltas.Min(out int thisIndex);
                closestIndexLocal[site] = thisIndex;
            });
            closestIndex = closestIndexLocal;
            closestOut = outValues.Select((ov, i) => ov[i]);
            return closestIndex.Select(t => (Tin)(t * (dynamic)inIncrement + inFrom + inOffset));
        }

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
            Func<Tout, bool> outTripCriteria) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, inNotFoundResult, outTripCriteria, out _, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
            Func<Tout, bool> outTripCriteria, out Site<int> tripIndex) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, inNotFoundResult,
                outTripCriteria, out tripIndex, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut) {
            Site<int> tripIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            Site<Tout> tripOutLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            ForEachSite(site => { // this could be done with a convoluted & unreadable LINQ statement
                for (int index = 0; index < outValues.Count; index++) {
                    if (tripIndexLocal[site] == -1 && outTripCriteria(outValues[index][site])) tripIndexLocal[site] = index;
                }
                if (tripIndexLocal[site] != -1) tripOutLocal[site] = outValues[tripIndexLocal[site]][site];
            });
            tripIndex = tripIndexLocal;
            tripOut = tripOutLocal;
            return tripIndexLocal.Select(CalculateTripInputValue);
            
            // Local function to calculate the input value from the trip index
            Tin CalculateTripInputValue(int index) {
                return index > -1 ? (Tin)(index * (dynamic)inIncrement + inFrom + inOffset) : inNotFoundResult;
            }
        }

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
            Func<Tout, bool> outTripCriteria) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, inNotFoundResult, outTripCriteria, out _, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
            Func<Tout, bool> outTripCriteria, out Site<int> tripIndex) => LinearFullProcess(outValues, inFrom, inIncrement, inOffset, inNotFoundResult,
                outTripCriteria, out tripIndex, out _);

        public virtual Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut) {
            Site<int> tripIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            ForEachSite(site => { // should be using DSP for this actually, but difficult with generics & delegates
                tripIndexLocal[site] = outValues[site].Select(s => outTripCriteria(s)).IndexOf(true);
            });
            tripIndex = tripIndexLocal;
            tripOut = outValues.Select((ov, i) => ov[i]);
            return tripIndexLocal.Select(CalculateTripInputValue);
            
            // Local function to calculate the input value from the trip index
            Tin CalculateTripInputValue(int index) {
                return index > -1 ? (Tin)(index * (dynamic)inIncrement + inFrom + inOffset) : inNotFoundResult;
            }
        }
    }
}
