using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.TheLib.Execute {
    public class Digital : ILib.IExecute.IDigital {

        public void ContinueToConditionalStop(PatternInfo pattern, Action action) {
            TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.FlagWait(pattern.SetFlags, pattern.ClearFlags);
            action();
            TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.Continue(pattern.ClearFlags, pattern.SetFlags);
        }
        
        public void ForcePatternHalt(PatternInfo patternInfo) {
            TheHdw.Digital.TimeDomains(patternInfo.TimeDomain).Patgen.Halt();
        }

        public void ForcePatternHalt() {
            TheHdw.Digital.Patgen.Halt();
        }

        public void RunPattern(PatternInfo patternInfo) {
            StartPattern(patternInfo);
            WaitPatternDone(patternInfo);
        }
    
        public void RunPattern(SiteVariant sitePatterns) => throw new NotImplementedException();
        public List<PinSite<double>> RunPatternConditionalStop(PatternInfo pattern, int numberOfStops, Func<PatternInfo, int, List<PinSite<double>>> func) {
            List<PinSite<double>> returnValue = new List<PinSite<double>>();
            StartPattern(pattern);
            for (int i = 0; i < numberOfStops; i++) {
                TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.FlagWait(pattern.SetFlags, pattern.ClearFlags);
                List<PinSite<double>> stopValues = func(pattern, i);
                returnValue.AddRange(stopValues);
                TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.Continue(pattern.ClearFlags, pattern.SetFlags);
            }
            return returnValue;
        }

        public void StartPattern(PatternInfo patternInfo) {
            TheHdw.Patterns(patternInfo.Name).Start();
        }
        
        public void StartPattern(SiteVariant sitePatterns) => throw new NotImplementedException();
        public void WaitPatternDone(PatternInfo patternInfo) {
            TheHdw.Digital.TimeDomains(patternInfo.TimeDomain).Patgen.HaltWait();
        }
    }
}
