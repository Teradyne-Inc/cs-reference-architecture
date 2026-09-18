namespace Tol {

    /// <summary>
    /// The IG-XL slot type strings reported by <c>TheHdw.Config.Slots[].Type</c> and
    /// <c>TheHdw.DCVS.Pins().DCVSType</c> for the DCVS instrument family.
    /// <para>
    /// Single source of truth for these literals. Adding a DCVS instrument should mean adding one member
    /// here and handling it where the type is switched on, not repeating the string per call site.
    /// </para>
    /// </summary>
    public static class DcvsSlotType {

        /// <summary>UVS64 on UltraFLEXplus.</summary>
        public const string Uvs64 = "VS-5A";

        /// <summary>UVS256-HP on UltraFLEXplus.</summary>
        public const string Uvs256Hp = "VS-800mA";

        /// <summary>UVS64-HP on UltraFLEXplus.</summary>
        public const string Uvs64Hp = "VS-20A";

        /// <summary>HexVS on UltraFLEX.</summary>
        public const string HexVs = "HexVS";

        /// <summary>VSM on UltraFLEX.</summary>
        public const string Vsm = "VSM";

        /// <summary>
        /// Every slot type C#RA classifies as DCVS, as used by pin resolution.
        /// </summary>
        public static string[] All => [Uvs64, Uvs256Hp, Uvs64Hp, HexVs, Vsm];

        /// <summary>
        /// The subset <c>Tol.DcvsPins</c> accepts. Narrower than <see cref="All"/>: <see cref="HexVs"/> and
        /// <see cref="Vsm"/> resolve as DCVS during pin resolution but are rejected by the DcvsPins constructor,
        /// so constructing a <c>Pins</c> object over a HexVS or VSM pin throws <c>ArgumentException</c> rather than
        /// reporting through the alert service. Named here rather than left implicit in two separate string arrays.
        /// <para>
        /// Reconciling the two lists is tracked in #3332. The likely resolution is removal: the Zebra team confirmed
        /// on PR #3328 that UltraFLEX DC instruments are C/COM with no .NET layer, so C#RA cannot drive them.
        /// </para>
        /// </summary>
        public static string[] SupportedByTol => [Uvs64, Uvs256Hp, Uvs64Hp];
    }
}
