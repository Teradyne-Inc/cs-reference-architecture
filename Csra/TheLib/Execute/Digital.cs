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

        public virtual void ContinueToConditionalStop(PatternInfo pattern, Action action) {
            TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.FlagWait(pattern.SetFlags, pattern.ClearFlags);
            action();
            TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.Continue(pattern.ClearFlags, pattern.SetFlags);
        }
        
        public virtual void ForcePatternHalt(PatternInfo patternInfo) {
            TheHdw.Digital.TimeDomains(patternInfo.TimeDomain).Patgen.Halt();
        }

        public virtual void ForcePatternHalt() {
            TheHdw.Digital.Patgen.Halt();
        }

        public virtual void RunPattern(PatternInfo patternInfo) {
            StartPattern(patternInfo);
            WaitPatternDone(patternInfo);
        }
    
        public virtual void RunPattern(SiteVariant sitePatterns) => throw new NotImplementedException();
        public virtual void RunPatternConditionalStop(PatternInfo pattern, int numberOfStops, IExecutable executableObject) {
            executableObject.Clear();
            StartPattern(pattern);
            for (int i = 0; i < numberOfStops; i++) {
                TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.FlagWait(pattern.SetFlags, pattern.ClearFlags);
                executableObject.Execute();
                TheHdw.Digital.TimeDomains(pattern.TimeDomain).Patgen.Continue(pattern.ClearFlags, pattern.SetFlags);
            }
        }

        public virtual void StartPattern(PatternInfo patternInfo) {
            TheHdw.Patterns(patternInfo.Name).Start();
        }
        
        public virtual void StartPattern(SiteVariant sitePatterns) => throw new NotImplementedException();
        public virtual void WaitPatternDone(PatternInfo patternInfo) {
            TheHdw.Digital.TimeDomains(patternInfo.TimeDomain).Patgen.HaltWait();
        }
    }
}
