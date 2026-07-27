using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace PortBridgeUtils {
    public class SiteReg32 {

        private readonly uint[] _values;

        // constructors
        public SiteReg32() => _values = new uint[TheExec.Sites.Existing.Count];

        public SiteReg32(uint value) : this() {
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) _values[i] = value;
        }

        public SiteReg32(SiteLong sl) : this() {
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) _values[i] = (uint)sl[i]; // may need your unchecked magic here
        }

        // indexer
        public uint this[int i] {
            get => _values[i];
            set => _values[i] = value;
        }

        // serializer
        public override string ToString() => $"{string.Join(", ", _values)}";

        // methods
        public SiteLong ToSiteLong() {
            SiteLong result = new SiteLong();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = unchecked((int)_values[i]);
            return result;
        }
        // PRH: added - need to check
        public Site<int> ToSiteGeneric() {
            Site<int> result = new Site<int>();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = unchecked((int)_values[i]);
            return result;
        }

        public void Fill(uint value) {
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) _values[i] = value;
        }

        // PRH: added - need to check
        public void Fill(uint value, int site) {
            _values[site] = value;
        }

        public SiteReg32 Clone() {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = _values[i];
            return result;
        }

        // operators
        public static SiteReg32 operator +(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] + b[i];
            return result;
        }
        public static SiteReg32 operator +(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] + b;
            return result;
        }
        // PRH: added SUB - need to check
        public static SiteReg32 operator -(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] - b[i];
            return result;
        }
        public static SiteReg32 operator -(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] - b;
            return result;
        }
        public static SiteReg32 operator %(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] % b[i];
            return result;
        }

        public static SiteReg32 operator %(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] % b;
            return result;
        }

        // PRH: AND added - need to check
        public static SiteReg32 operator &(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] & b[i];
            return result;
        }
        // PRH: AND added - need to check
        public static SiteReg32 operator &(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] & b;
            return result;
        }
        // PRH: OR added - need to check
        public static SiteReg32 operator |(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] | b[i];
            return result;
        }
        // PRH: OR added - need to check
        public static SiteReg32 operator |(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] | b;
            return result;
        }
        // PRH: XOR added - need to check
        public static SiteReg32 operator ^(SiteReg32 a, SiteReg32 b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] ^ b[i];
            return result;
        }
        // PRH: XOR added - need to check
        public static SiteReg32 operator ^(SiteReg32 a, uint b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] ^ b;
            return result;
        }
        // PRH: shift right added - need to check
        public static SiteReg32 operator >>(SiteReg32 a, int b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] >> b;
            return result;
        }
        // PRH: shift left added - need to check
        public static SiteReg32 operator <<(SiteReg32 a, int b) {
            SiteReg32 result = new SiteReg32();
            for (int i = 0; i < TheExec.Sites.Existing.Count; i++) if (TheExec.Sites[i].Selected) result[i] = a[i] << b;
            return result;
        }
    }
}
