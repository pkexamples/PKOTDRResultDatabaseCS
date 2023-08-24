using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// This project uses the Microsoft Entity Framework ORM Code First methodology
// to map POCO (Plain-old CLR objects) to tables and columns in a database.
// This file contains the POCO data modeling classes for persisting results to
// the database.
namespace PhotonKinetics.ResultDatabase
{
    /// <summary>
    /// Contains result set header information. Each instance is uniquely identified by the <c>FiberIDString</c>
    /// and <c>DateCreated</c>.  Instances can be associated with more than one <c>Result</c>, such as when
    /// a single measurement session produces several results.
    /// </summary>
    public class ResultSetHeader
    {

        /// <summary>
        /// This is the unique key for each record and serves as the foreign key for all <c>Results</c>
        /// acquired during the same measurement session.
        /// </summary>
        [Key]
        public long ResultSetHeaderId { get; set; }

        /// <summary>
        /// This is the unique fiber identifier.  Each instance of <c>ResultHeader</c> is uniquely
        /// identified by this string and its <c>DateCreated</c>.
        /// </summary>
        [Required]
        public string FiberIDString { get; set; }

        /// <summary>
        /// The prompt used to label the IDString.  The default value is "Fiber ID".
        /// </summary>
        [Required]
        public string FiberIDTag { get; set; } = "Fiber ID";

        /// <summary>
        ///	This is the timestamp recorded when a measurement session is initiated.  Each instance of
        /// <c>ResultHeader</c> is uniquely identified by this timestamp and its <c>FiberIDString</c>.
        /// </summary>
        [Required]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// This is the length of the fiber recorded at the start of a measurement session.  It is not a
        /// required value.
        /// </summary>
        public double? EnteredLength { get; set; }

        /// <summary>
        /// The operator's unique ID represented as a String, if provided at the start of a measurement session.
        /// </summary>
        public string OperatorID { get; set; }

        /// <summary>
        /// A collection of additional <c>HeaderLabel</c> instances that may be assigned at the start of a
        /// measurement session.
        /// </summary>
        public virtual ICollection<ResultSetLabel> Labels { get; set; }

        public ResultSetHeader()
        {
            // Default constructor
        }

    }


    /// <summary>
    /// Contains a (<c>Tag</c>, <c>Value</c>) string pair.  Multiple instances may be associated with a
    /// <c>ResultHeader</c>.
    /// </summary>
    public class ResultSetLabel
    {
        [Key]
        public long HeaderLabelId { get; set; }
        /// <summary>
        /// Gets or set the tag that identifies the data stored in the <v>Value</v> property and serves
        /// as the prompt for the label in the UI.
        /// </summary>
        [Required]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the value to be stored for the label.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the <c>ResultSetHeader</c> that this label is associated with
        /// </summary>
        public virtual ResultSetHeader Header { get; set; }

        public ResultSetLabel()
        {
            // Default constructor
        }

    }


    /// <summary>
    /// Contains information pertaining to a particular measurement instrument.
    /// </summary>
    public class Instrument
    {

        /// <summary>
        /// Gets or sets the serial number of this instrument.  The serial number serves as the unique
        /// identifier of each instrument in the database.
        /// </summary>
        [Key]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the model number of this instrument.
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// Gets or sets the collection of measurement results made by this instrument.
        /// </summary>
        public virtual ICollection<Result> Results { get; set; }

        public Instrument()
        {
            // Default constructor
        }

        public Instrument(string sn, string model)
        {
            SerialNumber = sn;
            ModelNumber = model;
        }
    }


    /// <summary>
    /// Virtual base class for all Result classes.  Enables polymorphic LINQ queries, such as a query
    /// to return all results for a particular Fiber ID.
    /// </summary>
    public abstract class Result
    {
        public enum SpoolEndType
        {
            Outside,
            Inside
        }

        [Key]
        public long ResultId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the measurement.
        /// </summary>
        [Required]
        public DateTime DateMeasured { get; set; }

        /// <summary>
        /// Gets or sets the result file path of the archived results file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the end of the spool the short fiber sample was taken from.
        /// Does not apply to long fiber measurements.
        /// </summary>
        public SpoolEndType? SpoolEnd { get; set; }

        /// <summary>
        /// Gets or sets the <c>ResultSetHeader</c> associated with this result.
        /// </summary>
        [Required]
        public virtual ResultSetHeader SetHeader { get; set; }

        /// <summary>
        /// Gets or sets the <c>Instrument</c> that was used for the measurement.
        /// </summary>
        public virtual Instrument Instrument { get; set; }

        public Result()
        {
            // Default constructor
        }
    }

    /// <summary>
    /// Container for MFD measurement results
    /// </summary>
    public class ModeFieldResult : Result
    {

        /// <summary>
        /// Gets or sets the measurement method used.
        /// 0 = Variable aperture
        /// 1 = OTDR backscatter
        /// 2 = Far field scan
        /// </summary>
        public ModeFieldMethod MfdMethod { get; set; }

        /// <summary>
        /// Gets or sets the collection of <c>ModeFieldWaveResults</c> acquired during the measurement.
        /// </summary>
        public virtual ICollection<ModeFieldWaveResult> ModeFieldResults { get; set; }

        public ModeFieldResult()
        {
            // Default constructor
        }

        public enum ModeFieldMethod
        {
            VariableAperture,
            Backscatter,
            FarFieldScan
        }
    }

    /// <summary>
    /// Container for MFD results for individual wavelengths
    /// </summary>
    public class ModeFieldWaveResult
    {
        [Key]
        public long ModeFieldWaveResultId { get; set; }

        /// <summary>
        /// Gets or sets the wavelength in nanometers of the light source used for the measurement.
        /// </summary>
        [Required]
        public double Wavelength { get; set; }

        /// <summary>
        /// Gets or sets the FOTP "standard" MFD value according to Petermann II definition for Variable Aperture,
        /// Far-field scan, or OTDR backscatter methods.
        /// </summary>
        [Required]
        public double MfdStandard { get; set; }

        /// <summary>
        /// Gets or sets the parent <c>ModeFieldResult</c> for this instance.
        /// </summary>
        public virtual ModeFieldResult ModeFieldResult { get; set; }

        public ModeFieldWaveResult()
        {
            // Default constructor
        }

        public ModeFieldWaveResult(ModeFieldResult parent)
        {
            ModeFieldResult = parent;
        }
    }

    /// <summary>
    /// Container for attenuation measurement results
    /// </summary>
    public class AttenuationResult : Result
    {

        /// <summary>
        /// Gets or sets the length used to compute the attenuation coefficient in dB/km.
        /// </summary>
        [Required]
        public double LengthUsed { get; set; }

        /// <summary>
        /// Gets or sets the method used to measure the attenuation
        /// </summary>
        [Required]
        public AttenuationMethod AttenMethod { get; set; }

        /// <summary>
        /// Gets or sets the collection of attenuation results collected in a measurement.
        /// </summary>
        public virtual ICollection<AttenuationWaveResult> AttenuationWaveResults { get; set; }

        public AttenuationResult()
        {
            // Default constructor
        }

        /// <summary>
        /// Specifier for attenuation measurement method
        /// </summary>
        /// <remarks>
        /// 0 = Cutback
        /// 1 = Backscatter
        /// 2 = Spectral Model
        /// </remarks>
        public enum AttenuationMethod
        {
            Cutback,
            Backscatter,
            SpectralModel
        }
    }

    /// <summary>
    /// Container for attenuation results for a specific wavelength
    /// </summary>
    public class AttenuationWaveResult
    {
        [Key]
        public long AttenuationWaveResultId { get; set; }

        /// <summary>
        /// Gets or sets the wavelength the attenuation coefficient is measured at in nm.
        /// </summary>
        [Required]
        public double Wavelength { get; set; }

        /// <summary>
        /// Gets or sets the attenuation coefficient in dB/km
        /// </summary>
        public double? AttenuationCoefficient { get; set; }
        public virtual AttenuationResult AttenResult { get; set; }

        public AttenuationWaveResult()
        {
            // Default constructor
        }

        /// <summary>
        /// Create a new instance with a wavelength in nm and attenuation coef in dB/km, and associate
        /// it with its parent <c>AttenuationResult</c>
        /// </summary>
        public AttenuationWaveResult(AttenuationResult parent)
        {
            AttenResult = parent;
        }
    }

    /// <summary>
    /// Container for length measurement results
    /// </summary>
    public class LengthResult : Result
    {

        /// <summary>
        /// Gets or sets the measured length in meters.
        /// </summary>
        [Required]
        public double LengthMeasured { get; set; }

        /// <summary>
        /// Gets or sets the Group Index used to compute the length.
        /// </summary>
        [Required]
        public double GroupIndex { get; set; }

        /// <summary>
        /// Gets or sets the method used in the length measurement.
        /// </summary>
        [Required]
        public Method LengthMethod { get; set; }

        /// <summary>
        /// Gets or sets the wavelength of the light source used to measure the length.
        /// </summary>
        public double? WavelengthUsed { get; set; }

        /// <summary>
        /// Specifier for length measurement method
        /// </summary>
        /// <remarks>
        /// 0 = Backscatter
        /// 1 = Phase Shift
        /// </remarks>
        public enum Method
        {
            Backscatter,
            PhaseShift,
            TimeOfFlight
        }

        public LengthResult()
        {
            // Default constructor
        }

    }

    /// <summary>
    /// This class stores results that can ONLY be obtained by OTDR Signature
    /// Analysis.  Length and end-to-end attenuation should be stored as LengthResult
    /// and AttenuationResult, with the same ResultSetHeader as the SignatureResult.
    /// Similarly, MFD modeling results and Spectral Attenuation modeling results
    /// should be stored as ModeFieldResult and AttenuationResult.
    /// </summary>
    public class SignatureResult : Result
    {
        public double Wavelength { get; set; }
        public double GroupIndex { get; set; }
        public double PulseWidthM { get; set; }
        public double PointSpacingM { get; set; }
        public double RangeKM { get; set; }
        [Required]
        public DirectionLabel Direction { get; set; } = DirectionLabel.Top;
        [Required]
        public AvgType AverageType { get; set; }
        public double? AverageLocation { get; set; }
        public double? AverageTarget { get; set; }
        public double? AverageTime { get; set; }
        public double? AverageCount { get; set; }

        // Each property of each of these will be stored in the SignatureResult table as a
        // nullable column (no DbSet added to the context for any of these)
        public SignatureEvent InsertionEvent { get; set; } = new SignatureEvent();
        public SignatureEvent EndEvent { get; set; } = new SignatureEvent();
        public SignatureEvent MaxLossEvent { get; set; } = new SignatureEvent();
        public SignatureEvent MinLossEvent { get; set; } = new SignatureEvent();
        public SignatureEvent MaxReflectanceEvent { get; set; } = new SignatureEvent();
        public WindowAttenuation MaxWindowAtten { get; set; } = new WindowAttenuation();
        public WindowAttenuation MinWindowAtten { get; set; } = new WindowAttenuation();
        public WindowUniformity MaxWindowUnif { get; set; } = new WindowUniformity();
        public WindowUniformity MinWindowUnif { get; set; } = new WindowUniformity();
        public LsaDeviation MaxLsaDeviation { get; set; } = new LsaDeviation();
        public double? Length { get; set; }
        public double? Attenuation { get; set; }
        public List<SignatureEvent> SignatureEvents { get; set; } = new List<SignatureEvent>();
        public List<WindowAttenuation> WindowAttenuations { get; set; } = new List<WindowAttenuation>();
        public List<WindowUniformity> WindowUniformities { get; set; } = new List<WindowUniformity>();

        /// <summary>
        /// Specifier for signature direction
        /// </summary>
        /// <remarks>
        /// 0 = Top
        /// 1 = Bottom
        /// 2 = Average
        /// 	'''</remarks>
        public enum DirectionLabel
        {
            Top,
            Bot,
            Avg
        }

        /// <summary>
        /// Specifier for OTDR averaging method used during acquisition
        /// </summary>
        /// <remarks>
        /// 0 = Counts
        /// 1 = Time based
        /// 2 = Noise based
        /// </remarks>
        public enum AvgType
        {
            Count,
            Time,
            Noise
        }

        public SignatureResult()
        {
            // default constructor
        }

    }

    /// <summary>
    /// Container for signature event data
    /// </summary>
    public class SignatureEvent
    {
        [Key]
        public long SignatureEventId { get; set; }
        public double? Location { get; set; }
        public double? Loss { get; set; }
        public double? Reflectance { get; set; }
    }

    /// <summary>
    /// Container for Window Attenuation data
    /// </summary>
    public class WindowAttenuation
    {
        [Key]
        public long WindowAttenuationId { get; set; }
        public double? Location { get; set; }
        public double? Attenuation { get; set; }
    }

        /// <summary>
    /// Container for Window Uniformity data
    /// </summary>
    public class WindowUniformity
    {
        [Key]
        public long WindowUniformityId { get; set; }
        public double? Location { get; set; }
        public double? Uniformity { get; set; }
    }

        /// <summary>
    /// Container for LSA Point Deviation data
    /// </summary>
    public class LsaDeviation
    {
        public double? Location { get; set; }
        public double? Deviation { get; set; }
    }
}