using System;
using UnityEngine;
using UnityEngine.UI;
using Wikitude;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using Wooga.Lambda.Control.Concurrent;
using Wooga.Lambda.Control.Monad;
using Wooga.Lambda.Data;
using Wooga.Lambda.Logging;
using Debug = UnityEngine.Debug;

public class ScenePickingController : SampleController
{
	public InstantTracker Tracker;

	/* The augmentation prefab that should be placed whenever a successful hit was registered. */
	public GameObject Augmentation;

	/* Button that toggles between Initializing and Tracking state. */
	public Button          ToggleStateButton;
	public Text            ToggleStateButtonText;
	public Text            MessageBox;
	public ToastController Toast;

	/* The slider used to define the DeviceHeightAboveGround property. */
	public GameObject HeightSlider;
	public Text       HeightLabel;

	/* The state in which the tracker currently is. */

	/* Flag to determine if we are currently in the process of chaning from one InstantTrackingState to another.
		*/
	private bool _changingState = false;

	/* Renders the grid used when initializing the tracker, indicating the ground plane. */
	private GridRenderer _gridRenderer;

	/* The currently rendered augmentations */
	private List<GameObject> _augmentations = new List<GameObject>();
	private InstantTrackable _trackable;

	private void Awake()
	{
		Application.targetFrameRate = 60;

		_gridRenderer         = GetComponent<GridRenderer>();
		_gridRenderer.enabled = false;

		_trackable = Tracker.GetComponentInChildren<InstantTrackable>();
		Tracker.OnScreenConversionComputed.AddListener(OnScreenConversionComputed);
	}

	protected override void Start()
	{
		base.Start();

		MessageBox.text = "Starting the SDK";
		/* The Wikitude SDK needs to be fully started before we can query for ARKit / ARCore support
		 * SDK initialization happens during start, so we wait one frame in a coroutine
		 */
		var stop=Stopwatch.StartNew();
		CheckPlatformAssistedTrackingSupport();
		stop.Stop();
		Debug.Log($"Took time to do so :{stop.ElapsedTicks.ToString()}.\nOr:{stop.Elapsed.ToString()}.");
	}

	/*if (Tracker.SMARTEnabled)
	{
		Tracker.IsPlatformAssistedTrackingSupported((SmartAvailability smartAvailability) => { this.UpdateTrackingMessage(smartAvailability); });
	}*/
	static int Count = 0;
	private bool CheckPlatformAssistedTrackingSupport()
	{
		Debug.Log($"Count:{Count}");
		lock (Tracker)
		{
		if (++Count >= 100) return false;




		{
			Either.Catch(() => (Action) (() => Tracker.IsPlatformAssistedTrackingSupported(UpdateTrackingMessage))).
						 SuccessOr(() => Async.Sleep(30).Then(() => Async.Return(CheckPlatformAssistedTrackingSupport)).RunSynchronously());
		}

		return true;
		}
	}


	private void UpdateTrackingMessage(SmartAvailability smartAvailability)
	{
		if (Tracker.SMARTEnabled)
		{
			string sdk;
			if (Application.platform == RuntimePlatform.Android)
			{
				sdk = "ARCore";
			} else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				sdk = "ARKit";
			} else
			{
				MessageBox.text = "Running without platform assisted tracking support.";
				return;
			}

			switch (smartAvailability)
			{
			case SmartAvailability.IndeterminateQueryFailed:
			{
				MessageBox.text = "Platform support query failed. Running without platform assisted tracking support.";
				break;
			}
			case SmartAvailability.CheckingQueryOngoing:
			{
				MessageBox.text = "Platform support query ongoing.";
				break;
			}
			case SmartAvailability.Unsupported:
			{
				MessageBox.text = "Running without platform assisted tracking support.";
				break;
			}
			case SmartAvailability.SupportedUpdateRequired:
			case SmartAvailability.Supported:
			{
				string runningWithMessage = "Running with platform assisted tracking support (" + sdk + ").";

				if (_currentTrackerState == InstantTrackingState.Tracking)
				{
					MessageBox.text = runningWithMessage;
				} else
				{
					MessageBox.text = runningWithMessage + "\n Move your phone around until the target turns green, which is when you can start tracking.";
				}

				break;
			}
			}
		} else
		{
			MessageBox.text = "Running without platform assisted tracking support.";
		}
	}

	protected override void Update()
	{
		base.Update();

		/* If we register a screen tap while we're tracking, convert the screen tap to a coordinate in the map. */
		if (IsTracking && Input.GetMouseButtonUp(0))
		{
			/* The result of this operation is not instantaneous, so we need to wait for
			 * the OnScreenConversionComputed callback to get the results. */
			Tracker.ConvertScreenCoordinate(Input.mousePosition);
		}

		/* Change the color of the grid to indicate if tracking can be started or not. */
		if (_currentTrackerState == InstantTrackingState.Initializing)
		{
			if (Tracker.CanStartTracking())
			{
				_gridRenderer.TargetColor = Color.green;
			} else
			{
				_gridRenderer.TargetColor = GridRenderer.DefaultTargetColor;
			}
		} else
		{
			_gridRenderer.TargetColor = GridRenderer.DefaultTargetColor;
		}
	}


	public void OnStateChanged(InstantTrackingState newState)
	{
		_currentTrackerState = newState;

		if (_currentTrackerState == InstantTrackingState.Initializing)
		{
			ToggleStateButtonText.text = "Start Tracking";
			HeightSlider.SetActive(true);
		} else
		{
			ToggleStateButtonText.text = "Start Initialization";
			HeightSlider.SetActive(false);
		}

		_changingState = false;
	}

	public void OnScreenConversionComputed(bool success, Vector2 screenCoordinate, Vector3 pointCloudCoordinate)
	{
		if (success)
		{
			/* The pointCloudCoordinate values are in the local space of the trackable. */
			var newAugmentation = GameObject.Instantiate(Augmentation, _trackable.transform) as GameObject;
			newAugmentation.transform.localPosition = pointCloudCoordinate;
			newAugmentation.transform.localScale    = Vector3.one;
			_augmentations.Add(newAugmentation);
		} else
		{
			Toast.DisplayMessage("No point found at the touched position.", 2.0f);
		}
	}

	public void OnToggleStateButtonPressed()
	{
		if (!_changingState)
		{
			if (_currentTrackerState == InstantTrackingState.Initializing)
			{
				if (Tracker.CanStartTracking())
				{
					ToggleStateButtonText.text = "Switching State...";
					_changingState             = true;
					Tracker.SetState(InstantTrackingState.Tracking);
				}
			} else
			{
				/* Clear all the previous augmentations */
				foreach (var augmentation in _augmentations)
				{
					Destroy(augmentation);
				}

				_augmentations.Clear();

				ToggleStateButtonText.text = "Switching State...";
				_changingState             = true;
				Tracker.SetState(InstantTrackingState.Initializing);
			}
		}
	}

	public void OnInitializationStarted(InstantTarget target)
	{
		SetSceneEnabled(true);
	}

	public void OnInitializationStopped(InstantTarget target)
	{
		SetSceneEnabled(false);
	}

	public void OnSceneRecognized(InstantTarget target)
	{
		SetSceneEnabled(true);
		IsTracking = true;
	}

	public void OnSceneLost(InstantTarget target)
	{
		SetSceneEnabled(false);
		IsTracking = false;
	}

	private void SetSceneEnabled(bool enabled)
	{
		_gridRenderer.enabled = enabled;
		/* Because the InstantTrackable has the Auto Toggle Visibility option enabled
		 * and because all the augmentations are set as children to it, we don't need to hide them.
		 */
	}

	public void OnHeightValueChanged(float newHeightValue)
	{
		HeightLabel.text                = string.Format("{0:0.##} m", newHeightValue);
		Tracker.DeviceHeightAboveGround = newHeightValue;
	}

	public void OnError(Error error)
	{
		_changingState             = false;
		ToggleStateButtonText.text = "Start Tracking";
		PrintError("Instant Tracker error!", error);
	}

	public void OnFailedStateChange(InstantTrackingState failedState, Error error)
	{
		PrintError("Failed to change state to " + failedState, error);
	}
}
