using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using GNOTDRSIGNATURELib;
using NTOPLOTDRANALYSISLib;
using PhotonKinetics.ResultDatabase;

namespace PhotonKinetics.PersistOtdrToDb
{
	public class OtdrPKResultsDbPersister
	{

		public const int SPEED_OF_LIGHT = 299792458;
		public const int NOM_SEC_TO_METERS = 100000000;

		public OtdrPKResultsDbPersister()
		{
			// default constructor
		}

		public void PersistToDB()
		{
			// All results are saved with the same ResultSetHeader and Instrument.
			// A single LengthResult record is saved, which is the shortest
			// length measured, in either TOP or BOT direction from the wavelength
			// with the lowest attenuation, in either the TOP or BOT direction.
			// End-to-end attenuation results are saved at each wavelength only for
			// the TOP direction, or for the AVG bi-directional average signature
			// if it was computed.
			// MFD results are saved for each fiber end, if results were computed.
			// Predicted attenuations from the spectral model are saved as a separate
			// AttenuationResult if a valid spectral result exists.
			// The RestultSetHeaderId (database primary key) is returned.  If no
			// results are persisted to the database, the return value is -1.

			if (Process.GetProcessesByName("GN8000FrontPanel2").Length == 0)
			{
				string errMsg = "There is no 8000 Front Panel application running.\nResults cannot be saved to the database.";
				throw new ApplicationException(errMsg);
			}

			// requires late binding; don't want to add a reference to entire Front Panel
			// could remove references to all PK COM types & declare those objects as Object
			// and use late binding for all of the COM objects
			Type gnFP2Type = Type.GetType("GN8000FrontPanel2.clsRemote");
			dynamic remote = Activator.CreateInstance(gnFP2Type);
			IOTDRAnalysis analysis = remote.Analysis as IOTDRAnalysis;
			if (analysis is null || !analysis.ResultsValid)
			{
				string errMsg = "There is no 8000 Front Panel Analysis available.\nResults cannot be saved to the database.";
				throw new ApplicationException(errMsg);
			}

			dynamic fpForm = remote.FrontPanelForm;
			var sigServer = new GNOTDRSIGNATURESERVERLib.GNOTDRSignatureServer();

			ResultSetHeader header = null;
			Instrument inst = null;
			AttenuationResult attenResult = null;
			AttenuationResult spectralAtten = null;
			ModeFieldResult mfdTopResult = null;
			ModeFieldResult mfdBotResult = null;
			double minAtten = double.MaxValue;
			int minAttenWavelength = -1;
			using (var db = new ResultsContext())
			{
				int[] waves = (int[])analysis.AvailableWavelengthValues;
				for (int waveIndex = 0, loopTo = analysis.Count - 1; waveIndex <= loopTo; waveIndex++)
				{
					IOTDRTestWavelength testWave = analysis[waves[waveIndex]];
					if (testWave.ResultsValid)
					{
						var biDirAn = testWave.BiDirAnalyzer;
						var atWaveRes = new AttenuationWaveResult();
						var mfdTopWaveRes = new ModeFieldWaveResult();
						var mfdBotWaveRes = new ModeFieldWaveResult();
						for (int dir = 0; dir <= 2; dir++)
						{
							// test to see if signature was actually acquired for each direction, and for valid results
							var testSig = testWave.get_Signatures((NTOPL_FIBER_DIR)dir) as OTDRTestSignature;
							bool hasResults = testWave.get_EventResultsValid((NTOPL_FIBER_DIR)dir) ||
								testWave.get_SlidingWindowResultsValid((NTOPL_FIBER_DIR)dir) ||
								testWave.get_LSAResultsValid((NTOPL_FIBER_DIR)dir);
							if (testSig is object && hasResults)
							{
								GNOTDRSignature sig = (GNOTDRSignature)sigServer.get_Signature(testSig.SigHandle);
								if (string.IsNullOrEmpty(sig.get_SampleIDValue(0)))
								{
									// do not persist anything if there is no Fiber ID (the first SampleIDValue)
									return;
								}
								else
								{
									// get results from the event analyzer
									var eta = biDirAn.get_EventAnalyzer((NTOPLEVENTTABLELib.NTOPL_FIBER_DIR)dir);

									// The first signature sets the header information
									if (header is null)
									{
										header = new ResultSetHeader();
										header.FiberIDString = sig.get_SampleIDValue(0);
										header.FiberIDTag = sig.get_SampleIDLabel(0);
										header.DateCreated = sig.AcqDate;
										header.EnteredLength = analysis.FiberLengthEstimate * 1000d;
										if (sig.NumSampleIDs > 1)
										{
											header.Labels = new List<ResultSetLabel>();
											for (int sampleIdIndex = 1, loopTo1 = sig.NumSampleIDs - 1; sampleIdIndex <= loopTo1; sampleIdIndex++)
												header.Labels.Add(new ResultSetLabel()
												{
													Tag = sig.get_SampleIDLabel((short)sampleIdIndex),
													Value = sig.get_SampleIDValue((short)sampleIdIndex)
												});
										}

										// Only need to fetch the instrument information once
										var optModCfg = sig.OpticalModuleCfg;
										string sn = optModCfg.SerialNumber;
										if (!string.IsNullOrEmpty(sn))
										{
											var qInst = db.Instruments.Where(i => (i.SerialNumber ?? "") == (sn ?? ""));
											// run query; if instrument exists, assign it, otherwise create it
											inst = qInst.SingleOrDefault();
											if (inst is null)
											{
												inst = new Instrument(sn, optModCfg.Model);
												db.Instruments.Add(inst);
											}
										}
									}

									var sigResult = new SignatureResult();
									// .FilePath = ? files might not actually be saved
									sigResult.SetHeader = header;
									sigResult.DateMeasured = sig.AcqDate;
									sigResult.Instrument = inst;
									sigResult.Wavelength = testWave.Wavelength;
									sigResult.GroupIndex = testWave.GroupIndex;
									sigResult.PulseWidthM = sig.PulseWidth * NOM_SEC_TO_METERS;
									sigResult.PointSpacingM = sig.PointSpacing * NOM_SEC_TO_METERS;
									sigResult.RangeKM = sig.Range * NOM_SEC_TO_METERS / 1000d;
									sigResult.Direction = (SignatureResult.DirectionLabel)dir;
									switch (sig.AverageType)
									{
										case OTDRAverageType.Counts:
											{
												sigResult.AverageType = SignatureResult.AvgType.Count;
												sigResult.AverageCount = sig.AverageCount;
												break;
											}

										case OTDRAverageType.Noise:
											{
												sigResult.AverageType = SignatureResult.AvgType.Noise;
												sigResult.AverageLocation = SecToKM(sig.AverageNoiseLocation, testWave.GroupIndex);
												sigResult.AverageTarget = sig.AverageNoiseTarget;
												break;
											}

										case OTDRAverageType.Seconds:
											{
												sigResult.AverageType = SignatureResult.AvgType.Time;
												sigResult.AverageTime = sig.AverageTime;
												break;
											}
									}

									// attenuation and length
									sigResult.Attenuation = LossPerKm(eta.Atten.Value, testWave.GroupIndex);
									if (biDirAn.get_EventResultsValid((NTOPLEVENTTABLELib.NTOPL_FIBER_DIR)dir))
									{
										sigResult.Length = SecToKM(eta.FiberLength.Value, testWave.GroupIndex);
									}

									// get buffer event location
									double buffEvtLocS = 0.0d;
									if (eta.NumBuffers > 0)
									{
										var be = eta.get_BufferEvent(0);
										buffEvtLocS = be.Loc;
										sigResult.InsertionEvent = new SignatureEvent()
										{
											Location = 0,
											Loss = be.Loss.Value,
											Reflectance = be.Refl.Value
										};
										be = eta.get_BufferEvent((NTOPLEVENTTABLELib.NTOPL_FIBER_END)1);
										sigResult.EndEvent = new SignatureEvent()
										{
											Location = FutLoc(ee.Loc, buffEvtLocS, testWave.GroupIndex),
											Loss = ee.Loss.Value,
											Reflectance = ee.Refl.Value
										};
									}

									int evtIndex;
									// get max loss event
									if (eta.MaxLossIndex > -1)
									{
										evtIndex = eta.MaxLossIndex;
										var evt = eta[evtIndex];
										var evtLoss = evt.Loss;
										var evtRefl = evt.Refl;
										sigResult.MaxLossEvent.Location = FutLoc(evt.Loc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MaxLossEvent.Loss = evtLoss.Value;
										sigResult.MaxLossEvent.Reflectance = evtRefl.Value;
									}
										var evtRefl = evt.Refl;
									}
									// get min loss event
									if (eta.MinLossIndex > -1)
									{
										evtIndex = eta.MinLossIndex;
										var evt = eta[evtIndex];
										var evtLoss = evt.Loss;
										var evtRefl = evt.Refl;
										sigResult.MinLossEvent.Location = FutLoc(evt.Loc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MinLossEvent.Loss = evtLoss.Value;
										sigResult.MinLossEvent.Reflectance = evtRefl.Value;
									}
									// get max reflectance event
									if (eta.MaxReflIndex > -1)
									{
										evtIndex = eta.MaxReflIndex;
										var evt = eta[evtIndex];
										var evtLoss = evt.Loss;
										var evtRefl = evt.Refl;
										sigResult.MaxReflectanceEvent.Location = FutLoc(evt.Loc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MaxReflectanceEvent.Loss = evtLoss.Value;
										sigResult.MaxReflectanceEvent.Reflectance = evtRefl.Value;
									}

									if (testWave.get_SlidingWindowResultsValid((NTOPL_FIBER_DIR)dir))
									{
										NTOPLATTENANALYSISLib.ISlidingWindowAnalyzer swa = testSig.SlidingWindowAnalyzer;
										var swaAttenData = swa.AttenData;
										var swaAttenXYData = swaAttenData.XYData;
										// get max sw atten
										sigResult.MaxWindowAtten.Location = FutLoc(swaAttenXYData.yMaxLoc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MaxWindowAtten.Attenuation = LossPerKm(swaAttenXYData.yMax, testWave.GroupIndex);
										// get min sw atten
										sigResult.MinWindowAtten.Location = FutLoc(swaAttenXYData.yMinLoc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MinWindowAtten.Attenuation = LossPerKm(swaAttenXYData.yMin, testWave.GroupIndex);
										// get max sw unif
										var swaUnifData = swa.UniformityData;
										var swaUnifXYData = swaUnifData.XYData;
										sigResult.MaxWindowUnif.Location = FutLoc(swaUnifXYData.yMaxLoc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MaxWindowUnif.Uniformity = LossPerKm(swaUnifXYData.yMax, testWave.GroupIndex);
										// get min sw unif
										sigResult.MinWindowUnif.Location = FutLoc(swaUnifXYData.yMinLoc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MinWindowUnif.Uniformity = LossPerKm(swaUnifXYData.yMin, testWave.GroupIndex);
									}

									if (testWave.get_LSAResultsValid((NTOPL_FIBER_DIR)dir))
									{
										NTOPLATTENANALYSISLib.ILSADeviation lsa = testSig.LSA;
										// get max LSA deviation
										sigResult.MaxLsaDeviation.Location = FutLoc(lsa.ExtremumLoc, buffEvtLocS, testWave.GroupIndex);
										sigResult.MaxLsaDeviation.Deviation = lsa.Extremum.Value;
										lsa = null;
									}

									db.Results.Add(sigResult);

									// get atten for only the top dir; if avg dir exists, overwrite top dir value in memory
									if (dir == (int)NTOPL_FIBER_DIR.NTOPL_TOP_DIR || dir == (int)NTOPL_FIBER_DIR.NTOPL_AVG_DIR)
									{
										if (attenResult is null)
										{
											attenResult = new AttenuationResult();
											attenResult.Instrument = inst;
											attenResult.SetHeader = header;
											attenResult.AttenMethod = AttenuationResult.AttenuationMethod.Backscatter;
											attenResult.DateMeasured = sig.AcqDate;
											attenResult.AttenuationWaveResults = new List<AttenuationWaveResult>();
										}

										atWaveRes.Wavelength = testWave.Wavelength;
										var numAtten = eta.Atten;
										double atten = LossPerKm(numAtten.Value, testWave.GroupIndex);
										if (atten < minAtten)
										{
											minAtten = atten;
											minAttenWavelength = testWave.Wavelength;
										}

										atWaveRes.AttenuationCoefficient = atten;
									}

									if (biDirAn.MFDResultsValid)
									{
										if (mfdTopResult is null)
										{
											mfdTopResult = new ModeFieldResult();
											mfdTopResult.SpoolEnd = Result.SpoolEndType.Outside;
											mfdTopResult.Instrument = inst;
											mfdTopResult.MfdMethod = ModeFieldResult.ModeFieldMethod.Backscatter;
											mfdTopResult.SetHeader = header;
											mfdTopResult.DateMeasured = sig.AcqDate;
											mfdTopResult.ModeFieldResults = new List<ModeFieldWaveResult>();
											mfdBotResult = new ModeFieldResult();
											mfdBotResult.SpoolEnd = Result.SpoolEndType.Inside;
											mfdBotResult.Instrument = inst;
											mfdBotResult.MfdMethod = ModeFieldResult.ModeFieldMethod.Backscatter;
											mfdBotResult.SetHeader = header;
											mfdBotResult.DateMeasured = sig.AcqDate;
											mfdBotResult.ModeFieldResults = new List<ModeFieldWaveResult>();
										}
									}
								}
							}
						}

						// add Atten wave result to attenResult
						if (attenResult is object)
						{
							attenResult.AttenuationWaveResults.Add(atWaveRes);
						}

						// get MFD results
						if (mfdTopResult is object)
						{
							mfdTopWaveRes.Wavelength = testWave.Wavelength;
							mfdBotWaveRes.Wavelength = testWave.Wavelength;
							var topMFDval = biDirAn.get_FiberMFD(NTOPLEVENTTABLELib.NTOPL_FIBER_END.NTOPL_TOP_END);
							var botMFDval = biDirAn.get_FiberMFD(NTOPLEVENTTABLELib.NTOPL_FIBER_END.NTOPL_BOT_END);
							mfdTopWaveRes.MfdStandard = topMFDval.Value;
							mfdBotWaveRes.MfdStandard = botMFDval.Value;
							mfdTopResult.ModeFieldResults.Add(mfdTopWaveRes);
							mfdBotResult.ModeFieldResults.Add(mfdBotWaveRes);
						}
					}
				} // testWave

				// get the length result from the min atten wavelength (use shorter of the top / bottom measured lengths)
				LengthResult lenResult = null;
				double minLength = double.MaxValue;
				var reportedDir = default(int);
				IOTDRTestWavelength minAttenTestWave = analysis[minAttenWavelength];
				var biDirAn2 = minAttenTestWave.BiDirAnalyzer;
				for (int dir = 0; dir <= 1; dir++)
				{
					if (biDirAn2.get_EventResultsValid((NTOPLEVENTTABLELib.NTOPL_FIBER_DIR)dir))
					{
						var evtAn = biDirAn2.get_EventAnalyzer((NTOPLEVENTTABLELib.NTOPL_FIBER_DIR)dir);
						var numLength = evtAn.FiberLength;
						double dirLength = numLength.Value;
						if (lenResult is null)
						{
							lenResult = new LengthResult();
							minLength = dirLength;
							reportedDir = dir;
						}
						else if (dirLength < minLength)
						{
							minLength = dirLength;
							reportedDir = dir;
						}
					}
				}

				double reportedLength = -1;
				if (lenResult is object)
				{
					lenResult.Instrument = inst;
					lenResult.SetHeader = header;
					lenResult.LengthMethod = LengthResult.Method.Backscatter;
					var repLenTestSig = minAttenTestWave.get_Signatures((NTOPL_FIBER_DIR)reportedDir);
					var repLenSig = sigServer.get_Signature(repLenTestSig.SigHandle);
					lenResult.DateMeasured = repLenSig.AcqDate;
					lenResult.GroupIndex = minAttenTestWave.GroupIndex;
					lenResult.WavelengthUsed = minAttenTestWave.Wavelength;
					lenResult.LengthMeasured = SecToKM(minLength, minAttenTestWave.GroupIndex);
					reportedLength = lenResult.LengthMeasured;
				}

				// get spectral model data if it exists
				if (analysis.SpectralResultsValid)
				{
					spectralAtten = new AttenuationResult();
					spectralAtten.AttenMethod = AttenuationResult.AttenuationMethod.SpectralModel;
					spectralAtten.DateMeasured = attenResult.DateMeasured;
					spectralAtten.Instrument = attenResult.Instrument;
					spectralAtten.LengthUsed = reportedLength;
					spectralAtten.SetHeader = attenResult.SetHeader;
					spectralAtten.AttenuationWaveResults = new List<AttenuationWaveResult>();
					NTOPLSPECTRALMODELLib.ISpectralModel sm = analysis.SpectralModel;
					NTOPLNUMERICLib.IXYData points = sm.PredictedAttens;
					for (int i = 0, loopTo2 = points.N - 1; i <= loopTo2; i++)
						spectralAtten.AttenuationWaveResults.Add(new AttenuationWaveResult()
						{
							Wavelength = points.get_xPoint(i),
							AttenuationCoefficient = points.get_yPoint(i)
						});
				}

				// save to database
				if (attenResult is object)
				{
					// can only set one "LengthUsed" for the group, so just use the reported length
					attenResult.LengthUsed = reportedLength;
					db.Results.Add(attenResult);
				}

				if (mfdTopResult is object)
				{
					db.Results.Add(mfdTopResult);
					db.Results.Add(mfdBotResult);
				}

				if (lenResult is object)
				{
					lenResult.LengthMeasured = reportedLength;
					db.Results.Add(lenResult);
				}

				if (spectralAtten is object)
				{
					db.Results.Add(spectralAtten);
				}

				if (db.ChangeTracker.HasChanges())
				{
					db.SaveChanges();
				}
			}
		}

		public double SecToKM(double tSeconds, double groupIndex)
		{
			return tSeconds * SPEED_OF_LIGHT / (2000 * groupIndex);
		}

		public double LossPerKm(double lossPerS, double groupIndex)
		{
			return lossPerS * 2000 * groupIndex / SPEED_OF_LIGHT;
		}

		private double FutLoc(double loc, double buffLoc, double groupIndex)
		{
			loc = loc - buffLoc;
			return SecToKM(loc, groupIndex);
		}
	}
}