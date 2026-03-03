using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;

namespace Csra.TheLib.Acquire {
    public class Search : ILib.IAcquire.ISearch {
        public virtual Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
            Site<Tout>> oneMeasurement, Tout outTarget) => BinarySearch(inFrom, inTo, inMinDelta, invertingLogic, oneMeasurement, outTarget, out _);

        public virtual Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<Tout> outResult) {
            if (inFrom >= inTo) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inFrom ({inFrom}) must be less than inTo ({inTo}).");
            if (inMinDelta <= 0) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inMinDelta ({inMinDelta}) must be greater than 0.");
            double delta = (inTo - inFrom) / 2;
            Site<double> inValue = new((inFrom + inTo) / 2);
            Site<Tout> outValue;
            Site<double> inBest = new();
            Site<Tout> outBest = new();
            Site<Tout> outDevAbsBest = new();
            bool done = false;
            bool first = true;
            do {
                outValue = oneMeasurement(inValue);
                done = delta <= inMinDelta;
                delta /= 2;
                ForEachSite(site => {
                    Tout outDevAbs = Math.Abs(outValue[site] - (dynamic)outTarget);
                    if (outDevAbs < (dynamic)outDevAbsBest[site] || first) {
                        outDevAbsBest[site] = outDevAbs;
                        outBest[site] = outValue[site];
                        inBest[site] = inValue[site];
                    }
                    inValue[site] += outValue[site] > (dynamic)outTarget ^ invertingLogic ? -delta : delta;
                });
                first = false;
            } while (!done);
            outResult = outBest;
            return inBest;
        }

        public virtual Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, double inNotFoundResult) => BinarySearch(inFrom, inTo, inMinDelta, invertingLogic,
                oneMeasurement, outTripCriteria, inNotFoundResult, out _);

        public virtual Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, double inNotFoundResult, out Site<Tout> outResult) {
            if (inFrom >= inTo) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inFrom ({inFrom}) must be less than inTo ({inTo}).");
            if (inMinDelta <= 0) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inMinDelta ({inMinDelta}) must be greater than 0.");
            double delta = (inTo - inFrom) / 2;
            Site<double> inValue = new((inFrom + inTo) / 2);
            Site<Tout> outValue;
            Site<double> inBest = new(inNotFoundResult); // prime these for the case we never tripped
            Site<Tout> outBest = new();
            Site<bool> alwaysTripped = new(true); // assume worst, overwrite if we found at least one non-tripping point
            bool done = false;
            do {
                outValue = oneMeasurement(inValue);
                done = delta <= inMinDelta;
                delta /= 2;
                ForEachSite(site => {
                    if (outTripCriteria(outValue[site])) {
                        outBest[site] = outValue[site];
                        inBest[site] = inValue[site];
                        inValue[site] += invertingLogic ? delta : -delta;
                    } else {
                        alwaysTripped[site] = false;
                        inValue[site] += invertingLogic ? -delta : delta;
                    }
                });
            } while (!done);
            ForEachSite(site => {
                if (alwaysTripped[site]) {
                    inBest[site] = inNotFoundResult;
                    outBest[site] = default;
                }
            });
            outResult = outBest;
            return inBest;
        }

        public virtual Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>,
            Site<Tout>> oneMeasurement, Tout outTarget) => BinarySearch(inFrom, inTo, inMinDelta, invertingLogic, oneMeasurement, outTarget, out _);

        public virtual Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<Tout> outResult) {
            if (inFrom >= inTo) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inFrom ({inFrom}) must be less than inTo ({inTo}).");
            if (inMinDelta <= 0) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inMinDelta ({inMinDelta}) must be greater than 0.");
            Site<int> from = new(inFrom);
            Site<int> to = new(inTo);
            //Site<int> inValue; //TODO 11.00 alpha can not divide. No need to create object when alpha 2 is available.
            //https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/1661
            Site<int> inValue = new();
            Site<Tout> outValue;
            Site<int> inBest = new();
            Site<Tout> outBest = new();
            Site<Tout> outDevAbsBest = new();
            bool first = true;
            Site<int> stepSize = new();
            do {
                //inValue = (from + to) / 2; //TODO 11.00 alpha can not divide. Remove site loop when alpha 2 is available.
                //https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/1661
                ForEachSite(site => inValue[site] = (from[site] + to[site]) / 2);
                outValue = oneMeasurement(inValue);
                ForEachSite(site => {
                    Tout outDevAbs = Math.Abs(outValue[site] - (dynamic)outTarget);
                    if (outDevAbs < (dynamic)outDevAbsBest[site] || first) {
                        outDevAbsBest[site] = outDevAbs;
                        outBest[site] = outValue[site];
                        inBest[site] = inValue[site];
                    }
                    ProcessTripStep(outValue[site] > (dynamic)outTarget, from, to, invertingLogic, inValue, inBest, outValue, outBest, stepSize, site);
                });
                first = false;
            } while (stepSize.Any(x => x >= inMinDelta));
            outResult = outBest;
            return inBest;
        }

        public virtual Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, int inNotFoundResult) => BinarySearch(inFrom, inTo, inMinDelta, invertingLogic,
                oneMeasurement, outTripCriteria, inNotFoundResult, out _);

        public virtual Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, int inNotFoundResult, out Site<Tout> outResult) {
            if (inFrom >= inTo) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inFrom ({inFrom}) must be less than inTo ({inTo}).");
            if (inMinDelta <= 0) Api.Services.Alert.Error<ArgumentException>($"BinarySearch: inMinDelta ({inMinDelta}) must be greater than 0.");
            Site<int> from = new(inFrom);
            Site<int> to = new(inTo);
            //Site<int> inValue; //TODO 11.00 alpha can not divide. No need to create object when alpha 2 is available.
            //https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/1661
            Site<int> inValue = new();
            Site<Tout> outValue;
            Site<int> inBest = new(inNotFoundResult); // prime these for the case we never tripped
            Site<bool> alwaysTripped = new(true); // assume worst, overwrite if we found at least one non-tripping point
            Site<Tout> outBest = new();
            Site<int> stepSize = new();
            do {
                //inValue = (from + to) / 2; //TODO 11.00 alpha can not divide. Remove site loop when alpha 2 is available.
                //https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/1661
                ForEachSite(site => inValue[site] = (from[site] + to[site]) / 2);
                outValue = oneMeasurement(inValue);
                ForEachSite(site => {
                    bool criteria = outTripCriteria(outValue[site]);
                    ProcessTripStep(criteria, from, to, invertingLogic, inValue, inBest, outValue, outBest, stepSize, site);
                    if (!criteria) alwaysTripped[site] = false;
                });
            } while (stepSize.Any(x => x >= inMinDelta));
            ForEachSite(site => {
                if (alwaysTripped[site]) {
                    inBest[site] = inNotFoundResult;
                    outBest[site] = default;
                }
            });
            outResult = outBest;
            return inBest;
        }

        public virtual void LinearFullFromIncCount<Tin>(Tin inFrom, Tin inIncrement, int inCount, Action<Tin> oneMeasurement) =>
            LinearFullFromToCount<Tin>(inFrom, inFrom + (dynamic)inIncrement * inCount, inCount, oneMeasurement);

        public virtual Tin LinearFullFromToCount<Tin>(Tin inFrom, Tin inTo, int inCount, Action<Tin> oneMeasurement) {
            dynamic inValue = inFrom;
            dynamic increment = (inTo - inValue) / (inCount - 1); // both end points included
            for (int i = 0; i < inCount; i++) {
                oneMeasurement(inValue);
                inValue += increment;
            }
            return increment;
        }

        public virtual void LinearFullFromToInc<Tin>(Tin inFrom, Tin inTo, Tin inIncrement, Action<Tin> oneMeasurement) => LinearFullFromToCount(inFrom, inTo,
            (int)Math.Ceiling((inTo - (dynamic)inFrom) / inIncrement), oneMeasurement);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget) => LinearStopFromToCount(inFrom, inFrom + (dynamic)inIncrement * inCount, inCount, inOffset,
                inNotFoundResult, oneMeasurement, outTarget, out Site<int> _, out Site<Tout> _);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex) => LinearStopFromToCount<Tin, Tout>(inFrom, inFrom + (dynamic)inIncrement *
                inCount, inCount, inOffset, inNotFoundResult, oneMeasurement, outTarget, out closestIndex, out Site<Tout> _);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut) => LinearStopFromToCount<Tin, Tout>(inFrom,
                inFrom + (dynamic)inIncrement * inCount, inCount, inOffset, inNotFoundResult, oneMeasurement, outTarget, out closestIndex, out closestOut);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria) => LinearStopFromToCount(inFrom, inFrom + (dynamic)inIncrement * inCount, inCount,
                inOffset, inNotFoundResult, oneMeasurement, outTripCriteria, out Site<int> _, out Site<Tout> _);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex) => LinearStopFromToCount(inFrom, inFrom +
                (dynamic)inIncrement * inCount, inCount, inOffset, inNotFoundResult, oneMeasurement, outTripCriteria, out tripIndex, out Site<Tout> _);

        public virtual Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut) =>
                LinearStopFromToCount<Tin, Tout>(inFrom, inFrom + (dynamic)inIncrement * inCount, inCount, inOffset, inNotFoundResult, oneMeasurement,
                    outTripCriteria, out tripIndex, out tripOut);
        
        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget) => LinearStopFromToCount(inFrom, inTo, inCount, inOffset, inNotFoundResult, oneMeasurement, outTarget,
                out _, out _);

        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex) => LinearStopFromToCount(inFrom, inTo, inCount, inOffset, inNotFoundResult,
                oneMeasurement, outTarget, out closestIndex, out _);

        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut) {
            dynamic inValue = inFrom;
            dynamic increment = (inTo - inValue) / (inCount - 1); // both end points included
            Site<int> closestIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            Site<Tout> closestOutLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            Site<Tout> outValue1 = oneMeasurement(inValue);
            Site<Tout> delta1 = outValue1 - outTarget;
            inValue += increment;
            int index = 1;
            do {
                Site<Tout> outValue2 = oneMeasurement(inValue);
                Site<Tout> delta2 = outValue2 - outTarget;
                ForEachSite(site => {
                    if (closestIndexLocal[site] == -1) {
                        if (Math.Sign((dynamic)delta1[site]) != Math.Sign((dynamic)delta2[site])) {
                            if (Math.Abs((dynamic)delta1[site]) < Math.Abs((dynamic)delta2[site])) {
                                closestIndexLocal[site] = index - 1;
                                closestOutLocal[site] = outValue1[site];
                            } else {
                                closestIndexLocal[site] = index;
                                closestOutLocal[site] = outValue2[site];
                            }
                        }
                    }
                });
                inValue += increment;
                index++;
                delta1 = delta2; // shift value to delta1 for next iteration
            } while (inValue <= inTo && closestIndexLocal.Any(s => s == -1));
            closestIndex = closestIndexLocal;
            closestOut = closestOutLocal;
            return closestIndex.Select(CalculateClosestInputValue);
            
            // Local function to calculate the input value from the index
            Tin CalculateClosestInputValue(int index) {
                return index > -1 ? (Tin)(index * increment + inFrom + inOffset) : inNotFoundResult;
            }
        }

        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria) => LinearStopFromToCount(inFrom, inTo, inCount, inOffset, inNotFoundResult,
                oneMeasurement, outTripCriteria, out _, out _);

        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex) => LinearStopFromToCount(inFrom, inTo, inCount, inOffset,
                inNotFoundResult, oneMeasurement, outTripCriteria, out tripIndex, out _);

        public virtual Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut) {
            dynamic inValue = inFrom;
            dynamic increment = (inTo - inValue) / (inCount - 1); // both end points included
            Site<int> tripIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            Site<Tout> tripOutLocal = new();// can't directly write to out parameter within lambda, so assign to local first
            int index = 0;
            do {
                Site<Tout> outValue = oneMeasurement(inValue);
                ForEachSite(site => {
                    if (tripIndexLocal[site] == -1 && outTripCriteria(outValue[site])) {
                        tripIndexLocal[site] = index;
                        tripOutLocal[site] = outValue[site];
                    }
                });
                inValue += increment;
                index++;
            } while (inValue <= inTo && tripIndexLocal.Any(s => s == -1));
            tripIndex = tripIndexLocal;
            tripOut = tripOutLocal;
            return tripIndex.Select(CalculateTripInputValue);

            // Local function to calculate the input value from the trip index
            Tin CalculateTripInputValue(int index) {
                return index > -1 ? (Tin)(index * increment + inFrom + inOffset) : inNotFoundResult;
            }
        }
        
        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget) => LinearStopFromToCount(inFrom, to, (int)Math.Ceiling((to - (dynamic)inFrom) / inIncrement), inOffset,
                inNotFoundResult, oneMeasurement, outTarget, out _, out _);

        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex) => LinearStopFromToCount(inFrom, to, (int)Math.Ceiling((to -
                (dynamic)inFrom) / inIncrement), inOffset, inNotFoundResult, oneMeasurement, outTarget, out closestIndex, out _);

        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut) => LinearStopFromToCount(inFrom, to,
                (int)Math.Ceiling((to - (dynamic)inFrom) / inIncrement), inOffset, inNotFoundResult, oneMeasurement, outTarget, out closestIndex,
                out closestOut);

        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria) => LinearStopFromToCount(inFrom, to, (int)Math.Ceiling((to - (dynamic)inFrom) /
                inIncrement), inOffset, inNotFoundResult, oneMeasurement, outTripCriteria, out _, out _);

        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex) => LinearStopFromToCount(inFrom, to, (int)Math.Ceiling((to -
                (dynamic)inFrom) / inIncrement), inOffset, inNotFoundResult, oneMeasurement, outTripCriteria, out tripIndex, out _);

        public virtual Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin to, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
            Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut) => LinearStopFromToCount(inFrom, to,
                (int)Math.Ceiling((to - (dynamic)inFrom) / inIncrement), inOffset, inNotFoundResult, oneMeasurement, outTripCriteria, out tripIndex,
                out tripOut);

        private void ProcessTripStep<Tout>(bool criteria, Site<int> from, Site<int> to, bool invertingLogic, Site<int> inValue, Site<int> inBest,
            Site<Tout> outValue, Site<Tout> outBest, Site<int> stepSize, int site) {
            if (criteria) {
                inBest[site] = inValue[site];
                outBest[site] = outValue[site];
                if (invertingLogic) {
                    stepSize[site] = inValue[site] - from[site];
                    from[site] = inValue[site];
                } else {
                    stepSize[site] = to[site] - inValue[site];
                    to[site] = inValue[site];
                }
            } else {
                if (invertingLogic) {
                    stepSize[site] = to[site] - inValue[site];
                    to[site] = inValue[site];
                } else {
                    stepSize[site] = inValue[site] - from[site];
                    from[site] = inValue[site];
                }
            }
        }
    }
}
