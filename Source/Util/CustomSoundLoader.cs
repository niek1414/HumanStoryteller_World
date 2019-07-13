using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Verse;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace HumanStoryteller.Util {
	public enum AudioFormat
	{
		unknown = -1,
		wav = 0,
		mp3 = 1,
		aiff = 2,
		ogg = 3,
	}

internal class CustomAudioFileReader : WaveStream, ISampleProvider
{
	private WaveStream readerStream;

	private readonly SampleChannel sampleChannel;

	private readonly int destBytesPerSample;

	private readonly int sourceBytesPerSample;

	private readonly long length;

	private readonly object lockObject;

	public override WaveFormat WaveFormat => sampleChannel.WaveFormat;

	public override long Length => length;

	public override long Position
	{
		get
		{
			return SourceToDest(readerStream.Position);
		}
		set
		{
			lock (lockObject)
			{
				readerStream.Position = DestToSource(value);
			}
		}
	}

	public float Volume
	{
		get
		{
			return sampleChannel.Volume;
		}
		set
		{
			sampleChannel.Volume = value;
		}
	}

	public CustomAudioFileReader(Stream stream, AudioFormat format)
	{
		lockObject = new object();
		CreateReaderStream(stream, format);
		sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
		sampleChannel = new SampleChannel(readerStream, forceStereo: false);
		destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
		length = SourceToDest(readerStream.Length);
	}

	private void CreateReaderStream(Stream stream, AudioFormat format)
	{
		switch (format)
		{
		case AudioFormat.wav:
			readerStream = new WaveFileReader(stream);
			if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
			{
				readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
				readerStream = new BlockAlignReductionStream(readerStream);
			}
			break;
		case AudioFormat.mp3:
			readerStream = new Mp3FileReader(stream);
			break;
		case AudioFormat.aiff:
			readerStream = new AiffFileReader(stream);
			break;
		case AudioFormat.ogg:
			Tell.Err("ogg is not a supported audio file");
			//readerStream = new VorbisWaveReader(stream);
			break;
		default:
			Debug.LogWarning("Audio format " + format + " is not supported");
			break;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		WaveBuffer waveBuffer = new WaveBuffer(buffer);
		int count2 = count / 4;
		int num = Read(waveBuffer.FloatBuffer, offset / 4, count2);
		return num * 4;
	}

	public int Read(float[] buffer, int offset, int count)
	{
		lock (lockObject)
		{
			return sampleChannel.Read(buffer, offset, count);
		}
	}

	private long SourceToDest(long sourceBytes)
	{
		return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
	}

	private long DestToSource(long destBytes)
	{
		return sourceBytesPerSample * (destBytes / destBytesPerSample);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && readerStream != null)
		{
			readerStream.Dispose();
			readerStream = null;
		}
		base.Dispose(disposing);
	}
}

[StaticConstructorOnStartup]
public class Manager : MonoBehaviour
{
	private class AudioInstance
	{
		public AudioClip audioClip;

		public CustomAudioFileReader reader;

		public float[] dataToSet;

		public int samplesCount;

		public Stream streamToDisposeOnceDone;

		public int channels => reader.WaveFormat.Channels;

		public int sampleRate => reader.WaveFormat.SampleRate;

		public static implicit operator AudioClip(AudioInstance ai)
		{
			return ai.audioClip;
		}
	}

	private static readonly string[] supportedFormats;

	private static Dictionary<string, AudioClip> cache;

	private static Queue<AudioInstance> deferredLoadQueue;

	private static Queue<AudioInstance> deferredSetDataQueue;

	private static Queue<AudioInstance> deferredSetFail;

	private static Thread deferredLoaderThread;

	private static GameObject managerInstance;

	private static Dictionary<AudioClip, AudioClipLoadType> audioClipLoadType;

	private static Dictionary<AudioClip, AudioDataLoadState> audioLoadState;

	static Manager()
	{
		cache = new Dictionary<string, AudioClip>();
		deferredLoadQueue = new Queue<AudioInstance>();
		deferredSetDataQueue = new Queue<AudioInstance>();
		deferredSetFail = new Queue<AudioInstance>();
		audioClipLoadType = new Dictionary<AudioClip, AudioClipLoadType>();
		audioLoadState = new Dictionary<AudioClip, AudioDataLoadState>();
		supportedFormats = Enum.GetNames(typeof(AudioFormat));
	}

	public static AudioClip Load(string filePath, bool doStream = false, bool loadInBackground = true, bool useCache = true)
	{
		if (!IsSupportedFormat(filePath))
		{
			Debug.LogError("Could not load AudioClip at path '" + filePath + "' it's extensions marks unsupported format, supported formats are: " + string.Join(", ", Enum.GetNames(typeof(AudioFormat))));
			return null;
		}
		AudioClip value = null;
		if (useCache && cache.TryGetValue(filePath, out value) && (bool)value)
		{
			return value;
		}
		StreamReader streamReader = new StreamReader(filePath);
		value = Load(streamReader.BaseStream, GetAudioFormat(filePath), filePath, doStream, loadInBackground);
		if (useCache)
		{
			cache[filePath] = value;
		}
		return value;
	}

	public static AudioClip Load(Stream dataStream, AudioFormat audioFormat, string unityAudioClipName, bool doStream = false, bool loadInBackground = true, bool diposeDataStreamIfNotNeeded = true)
	{
		AudioClip audioClip = null;
		CustomAudioFileReader reader = null;
		try
		{
			reader = new CustomAudioFileReader(dataStream, audioFormat);
			AudioInstance audioInstance = new AudioInstance();
			audioInstance.reader = reader;
			audioInstance.samplesCount = (int)(reader.Length / (reader.WaveFormat.BitsPerSample / 8));
			AudioInstance audioInstance2 = audioInstance;
			if (!doStream)
			{
				audioClip = (audioInstance2.audioClip = AudioClip.Create(unityAudioClipName, audioInstance2.samplesCount / audioInstance2.channels, audioInstance2.channels, audioInstance2.sampleRate, doStream));
				if (diposeDataStreamIfNotNeeded)
				{
					audioInstance2.streamToDisposeOnceDone = dataStream;
				}
				SetAudioClipLoadType(audioInstance2, AudioClipLoadType.DecompressOnLoad);
				SetAudioClipLoadState(audioInstance2, AudioDataLoadState.Loading);
				if (!loadInBackground)
				{
					audioInstance2.dataToSet = new float[audioInstance2.samplesCount];
					audioInstance2.reader.Read(audioInstance2.dataToSet, 0, audioInstance2.dataToSet.Length);
					audioInstance2.audioClip.SetData(audioInstance2.dataToSet, 0);
					SetAudioClipLoadState(audioInstance2, AudioDataLoadState.Loaded);
					return audioClip;
				}
				lock (deferredLoadQueue)
				{
					deferredLoadQueue.Enqueue(audioInstance2);
				}
				RunDeferredLoaderThread();
				EnsureInstanceExists();
				return audioClip;
			}
			audioClip = (audioInstance2.audioClip = AudioClip.Create(unityAudioClipName, audioInstance2.samplesCount / audioInstance2.channels, audioInstance2.channels, audioInstance2.sampleRate, doStream, delegate(float[] target)
			{
				reader.Read(target, 0, target.Length);
			}, delegate(int target)
			{
				reader.Seek(target, SeekOrigin.Begin);
			}));
			SetAudioClipLoadType(audioInstance2, AudioClipLoadType.Streaming);
			SetAudioClipLoadState(audioInstance2, AudioDataLoadState.Loaded);
			return audioClip;
		}
		catch (Exception ex)
		{
			SetAudioClipLoadState(audioClip, AudioDataLoadState.Failed);
			Debug.LogError("Could not load AudioClip named '" + unityAudioClipName + "', exception:" + ex);
			return audioClip;
		}
	}

	private static void RunDeferredLoaderThread()
	{
		if (deferredLoaderThread == null || !deferredLoaderThread.IsAlive)
		{
			deferredLoaderThread = new Thread(DeferredLoaderMain);
			deferredLoaderThread.IsBackground = true;
			deferredLoaderThread.Start();
		}
	}

	private static void DeferredLoaderMain()
	{
		AudioInstance audioInstance = null;
		bool flag = true;
		long num = 100000L;
		while (flag || num > 0)
		{
			num--;
			lock (deferredLoadQueue)
			{
				flag = (deferredLoadQueue.Count > 0);
				if (flag)
				{
					audioInstance = deferredLoadQueue.Dequeue();
					goto IL_0051;
				}
			}
			continue;
			IL_0051:
			num = 100000L;
			try
			{
				audioInstance.dataToSet = new float[audioInstance.samplesCount];
				audioInstance.reader.Read(audioInstance.dataToSet, 0, audioInstance.dataToSet.Length);
				audioInstance.reader.Close();
				audioInstance.reader.Dispose();
				if (audioInstance.streamToDisposeOnceDone != null)
				{
					audioInstance.streamToDisposeOnceDone.Close();
					audioInstance.streamToDisposeOnceDone.Dispose();
					audioInstance.streamToDisposeOnceDone = null;
				}
				lock (deferredSetDataQueue)
				{
					deferredSetDataQueue.Enqueue(audioInstance);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				lock (deferredSetFail)
				{
					deferredSetFail.Enqueue(audioInstance);
				}
			}
		}
	}

	private void Update()
	{
		AudioInstance audioInstance = null;
		bool flag = true;
		while (true)
		{
			if (flag)
			{
				lock (deferredSetDataQueue)
				{
					flag = (deferredSetDataQueue.Count > 0);
					if (flag)
					{
						audioInstance = deferredSetDataQueue.Dequeue();
						goto IL_0045;
					}
				}
			}
			break;
			IL_0045:
			audioInstance.audioClip.SetData(audioInstance.dataToSet, 0);
			SetAudioClipLoadState(audioInstance, AudioDataLoadState.Loaded);
			audioInstance.audioClip = null;
			audioInstance.dataToSet = null;
		}
		lock (deferredSetFail)
		{
			while (deferredSetFail.Count > 0)
			{
				audioInstance = deferredSetFail.Dequeue();
				SetAudioClipLoadState(audioInstance, AudioDataLoadState.Failed);
			}
		}
	}

	private static void EnsureInstanceExists()
	{
		if (!(bool)managerInstance)
		{
			managerInstance = new GameObject("Runtime AudioClip Loader Manger singleton instance");
			managerInstance.hideFlags = HideFlags.HideAndDontSave;
			managerInstance.AddComponent<Manager>();
		}
	}

	public static void SetAudioClipLoadState(AudioClip audioClip, AudioDataLoadState newLoadState)
	{
		audioLoadState[audioClip] = newLoadState;
	}

	public static AudioDataLoadState GetAudioClipLoadState(AudioClip audioClip)
	{
		AudioDataLoadState value = AudioDataLoadState.Failed;
		if (audioClip != null)
		{
			value = audioClip.loadState;
			audioLoadState.TryGetValue(audioClip, out value);
		}
		return value;
	}

	public static void SetAudioClipLoadType(AudioClip audioClip, AudioClipLoadType newLoadType)
	{
		audioClipLoadType[audioClip] = newLoadType;
	}

	public static AudioClipLoadType GetAudioClipLoadType(AudioClip audioClip)
	{
		AudioClipLoadType value = (AudioClipLoadType)(-1);
		if (audioClip != null)
		{
			value = audioClip.loadType;
			audioClipLoadType.TryGetValue(audioClip, out value);
		}
		return value;
	}

	private static string GetExtension(string filePath)
	{
		return Path.GetExtension(filePath).Substring(1).ToLower();
	}

	public static bool IsSupportedFormat(string filePath)
	{
		return supportedFormats.Contains(GetExtension(filePath));
	}

	public static AudioFormat GetAudioFormat(string filePath)
	{
		AudioFormat result = AudioFormat.unknown;
		try
		{
			result = (AudioFormat)Enum.Parse(typeof(AudioFormat), GetExtension(filePath), ignoreCase: true);
			return result;
		}
		catch
		{
			return result;
		}
	}

	public static void ClearCache()
	{
		cache.Clear();
	}
}

}