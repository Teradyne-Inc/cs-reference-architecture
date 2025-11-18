using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS {
    public class Utilities : TestCodeBase {
        public const double TAU = 2.0 * Math.PI; // Because "Pi is Wrong"

        /// <summary>
        ///     ''' Creates "offline" sine wave data with distortion (2 harmonics) and noise added
        ///     ''' Data is unique per site, with exactly 1% gain added for each site after 0.
        ///     ''' THD should be about -90dB and SNR should be about 100 dB.
        ///     ''' </summary>
        ///     ''' <param name="ampl">Nominal amplitude in Volts -- Site 0 will be exact. Others grow at 1% per site. </param>
        ///     ''' <param name="freq">Frequency of sine wave. Affects only DSPWave.SampleRate</param>
        ///     ''' <param name="size">Becomes DSPWave.SampleSize</param>
        ///     ''' <param name="fund_bin">Optional. Specify what frequency bin the fundamental should be placed in. Sample rate is adjusted
        ///     accordingly.</param>
        ///     ''' <returns></returns>
        public static DSPWave SimulatedCaptureSine(double ampl, double freq, int size, int fund_bin) {
            var sim = new DSPWave();
            var harmonic_2 = new DSPWave();
            var harmonic_3 = new DSPWave();
            var noise = new DSPWave();
            var ampl_sd = new SiteDouble();

            if (fund_bin < 1 | fund_bin > size / (double)2)
                throw new System.Exception("Illegal fund_bin");

            // Create normalized sine (amplitude = 1.0)
            sim.CreateSin(TAU * fund_bin / size, 0.0, size);
            sim.SampleRate = freq * size / fund_bin; // M/N = Fi/Fs, so Fs = Fi*n/m

            // Add 90dB of distortion
            harmonic_2.CreateSin(TAU * fund_bin * 2 / size, 0.0, size);
            harmonic_3.CreateSin(TAU * fund_bin * 3 / size, 0.0, size);
            sim[-1] = sim.Add(harmonic_2.Multiply(Math.Sqrt(0.000000001 / 4.0 * 2.9)))
                           .Add(harmonic_3.Multiply(Math.Sqrt(0.000000001 / 4.0)));

            // Introduce site-specific variation
            foreach (int site in TheExec.Sites) {
                // Add site-specific noise
                noise.CreateRandom(-0.00001, 0.00001, size);
                sim[Convert.ToInt32(site)] = sim.Add(noise);
                // site-specific gain
                ampl_sd[Convert.ToInt32(site)] = ampl + Convert.ToInt32(site) * 0.1;
                sim[Convert.ToInt32(site)] = sim.Multiply(ampl_sd);
            }

            return sim;
        }

        /// <summary>
        ///     ''' Creates "offline" sine wave data with distortion (2 harmonics) and noise added
        ///     ''' Data is unique per site, with exactly 10% gain added for each site after 0.
        ///     ''' THD should be about -90dB and SNR should be about 100 dB.
        ///     ''' </summary>
        ///     ''' <param name="ampl">Nominal amplitude in Volts -- Site 0 will be exact. Others grow at 10% per site. </param>
        ///     ''' <param name="freq">Frequency of sine wave. Affects only DSPWave.SampleRate</param>
        ///     ''' <param name="size">Becomes DSPWave.SampleSize</param>
        ///     ''' <returns></returns>
        public static DSPWave SimulatedCaptureSine(double ampl, double freq, int size) {
            const int fundBin = 5;  // Somewhat arbitrary, but if caller doesn't care...
            return SimulatedCaptureSine(ampl, freq, size, fundBin);
        }

        public static PinListData CreateCaptureSine(string pins, double ampl, double freq, int size, int fund_bin) {
            var pl = new PinList();
            pl.Value = pins;
            return CreateCaptureSine(pl, ampl, freq, size, fund_bin);
        }

        public static PinListData CreateCaptureSine(PinList pinList, double ampl, double freq, int size, int fund_bin) {
            var pld = new PinListData();
            PinData pindata;

            foreach (string pinname in pinList.Value.Split(',')) {
                pindata = new PinData();
                pindata.Name = pinname;
                pindata[-1] = SimulatedCaptureSine(ampl, freq, size, fund_bin);
                pld.AddPin(pindata);
                ampl += 0.01; // Add some small per-pin variation
            }

            return pld;
        }
    }
}
