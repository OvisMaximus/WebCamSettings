using DirectShowLib;

namespace RestoreWebCamConfig
{
	public enum PowerlineFrequency
	{
		Hz50 = 1,
		Hz60 = 2
	}
	static class Program
	{
		const string CamName = "Logitech BRIO";

		// ReSharper disable once UnusedParameter.Local
		static void Main(string[] args)
		{
			Console.WriteLine($"Initializing {CamName}");
			CameraController camController = CameraController.FindCamera(CamName);
			camController.SetManualZoom(160);
			camController.SetManualFocus(0);
			camController.SetExposure(-6);
			camController.SetPan(4);
			camController.SetTilt(-5);
			camController.SetBrightness(129);
			camController.SetContrast(139);
			camController.SetSaturation(129);
			camController.SetSharpness(0);
			camController.SetWhiteBalance(2800);
			camController.SetBackLightCompensation(0);
			camController.SetGain(66);
			camController.SetPowerLineFrequency(PowerlineFrequency.Hz50);
			camController.SetLowLightCompensation(false);
			Console.WriteLine($"{CamName} configured.");
		}
	}

	public class CameraController
	{
		private readonly IAMCameraControl _cameraControl;
		private readonly IAMVideoProcAmp _videoProcAmp;
		
		private CameraController(DsDevice videoInputDevice)
		{
			Guid iid = typeof(IBaseFilter).GUID;
			var device = videoInputDevice ?? throw 
				new ArgumentException("can not work without an device - it must not be null");
			
			device.Mon.BindToObject(null!, null, ref iid, out var source);
			_cameraControl = source as IAMCameraControl ?? throw
				new ArgumentException($"could not handle {device} as camera");
			_videoProcAmp = source as IAMVideoProcAmp ?? throw
				new ArgumentException($"could not handle {device} as video proc amp");
		}

	
		public static CameraController FindCamera(string humanReadableCameraName)
		{
			foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
			{
				if (humanReadableCameraName.Equals(ds.Name))
				{
					return new CameraController(ds);
				}
			}
			throw new FileNotFoundException($"Camera {humanReadableCameraName} not found.");
		}

		private void SetManualCamControl(CameraControlProperty property, int value)
		{
			Console.WriteLine($"Setting control parameter {property} to {value}");
			_cameraControl.Set(property, value, CameraControlFlags.Manual);
		}
		
		private void SetManualVideoProcessingProperty(VideoProcAmpProperty property, int value)
		{
			Console.WriteLine($"Setting video processing parameter {property} to {value}");
			_videoProcAmp.Set(property, value, VideoProcAmpFlags.Manual);
		}

		public void SetPowerLineFrequency(PowerlineFrequency frequency)
		{
			_videoProcAmp.Set((VideoProcAmpProperty)13, (int)frequency, VideoProcAmpFlags.Manual);
			Console.WriteLine($"Setting video processing parameter PowerLineFrequency to {frequency}");
		}

		public void SetLowLightCompensation(Boolean onOff)
		{
			int lowLightCompensationValue = onOff ? 1 : 0;
			_cameraControl.Set((CameraControlProperty)19, lowLightCompensationValue, CameraControlFlags.Manual);
			Console.WriteLine($"Setting video processing parameter PowerLineFrequency to {lowLightCompensationValue}.");
		}

		public void SetManualZoom(int zoom)
		{
			SetManualCamControl(CameraControlProperty.Zoom, zoom);
		}
		
		public void SetManualFocus(int focus)
		{
			SetManualCamControl(CameraControlProperty.Focus, focus);
		}

		public void SetExposure(int exposure)
		{
			SetManualCamControl(CameraControlProperty.Exposure, exposure);
		}

		public void SetPan(int pan)
		{
			SetManualCamControl(CameraControlProperty.Pan, pan);
		}

		public void SetTilt(int tilt)
		{
			SetManualCamControl(CameraControlProperty.Tilt, tilt);
		}

		public void SetBrightness(int brightness)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.Brightness, brightness);
		}

		public void SetContrast(int contrast)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.Contrast, contrast);
		}

		public void SetSaturation(int saturation)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.Saturation, saturation);
		}

		public void SetSharpness(int sharpness)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.Sharpness, sharpness);
		}

		public void SetWhiteBalance(int whiteBalance)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.WhiteBalance, whiteBalance);
		}

		public void SetBackLightCompensation(int backLightCompensation)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.BacklightCompensation, backLightCompensation);
		}

		public void SetGain(int gain)
		{
			SetManualVideoProcessingProperty(VideoProcAmpProperty.Gain, gain);
		}
		
	}
	
	
}