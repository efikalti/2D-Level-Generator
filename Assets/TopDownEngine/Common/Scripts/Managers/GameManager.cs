﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A list of the possible TopDown Engine base events
    /// </summary>
    public enum TopDownEngineEventTypes
	{
		LevelStart,
		LevelComplete,
		LevelEnd,
		Pause,
		UnPause,
		PlayerDeath,
        RespawnStarted,
        RespawnComplete,
        StarPicked,
		GameOver
	}

	/// <summary>
	/// A type of events used to signal level start and end (for now)
	/// </summary>
	public struct TopDownEngineEvent
	{
		public TopDownEngineEventTypes EventType;
		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>
		public TopDownEngineEvent(TopDownEngineEventTypes eventType)
		{
			EventType = eventType;
		}

        static TopDownEngineEvent e;
        public static void Trigger(TopDownEngineEventTypes eventType)
        {
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    } 

	/// <summary>
	/// A list of the methods available to change the current score
	/// </summary>
	public enum PointsMethods
	{
		Add,
		Set
	}

	/// <summary>
	/// A type of event used to signal changes to the current score
	/// </summary>
	public struct TopDownEnginePointEvent
	{
		public PointsMethods PointsMethod;
		public int Points;
        /// <summary>
        /// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEnginePointEvent"/> struct.
        /// </summary>
        /// <param name="pointsMethod">Points method.</param>
        /// <param name="points">Points.</param>
        public TopDownEnginePointEvent(PointsMethods pointsMethod, int points)
		{
			PointsMethod = pointsMethod;
			Points = points;
        }

        static TopDownEnginePointEvent e;
        public static void Trigger(PointsMethods pointsMethod, int points)
        {
            e.PointsMethod = pointsMethod;
            e.Points = points;
            MMEventManager.TriggerEvent(e);
        }
    }

	/// <summary>
	/// A list of the methods available to change the current score
	/// </summary>
	public enum TimeScaleMethods
	{
		Set,
		For,
		Reset
	}

    /// <summary>
    /// A list of the possible pause methods
    /// </summary>
	public enum PauseMethods
	{
		PauseMenu,
		NoPauseMenu
	}

    /// <summary>
    /// A class to store points of entry into levels, one per level.
    /// </summary>
	public class PointsOfEntryStorage
	{
		public string LevelName;
		public int PointOfEntryIndex;
		public Character.FacingDirections FacingDirection;

		public PointsOfEntryStorage(string levelName, int pointOfEntryIndex, Character.FacingDirections facingDirection)
		{
			LevelName = levelName;
			FacingDirection = facingDirection;
			PointOfEntryIndex = pointOfEntryIndex;
		}
	}

	/// <summary>
	/// The game manager is a persistent singleton that handles points and time
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/Game Manager")]
	public class GameManager : 	PersistentSingleton<GameManager>, 
								MMEventListener<MMGameEvent>, 
								MMEventListener<TopDownEngineEvent>, 
								MMEventListener<TopDownEnginePointEvent>
	{		
		/// the target frame rate for the game
		public int TargetFrameRate=300;
        [Header("Lives")]
        /// the maximum amount of lives the character can currently have
        public int MaximumLives = 0;
        /// the current number of lives 
        public int CurrentLives = 0;
        /// the name of the scene to redirect to when all lives are lost
        public string GameOverScene;
        /// the current number of game points
        public int Points { get; private set; }
		/// true if the game is currently paused
		public bool Paused { get; set; } 
		// true if we've stored a map position at least once
		public bool StoredLevelMapPosition{ get; set; }
		/// the current player
		public Vector2 LevelMapPosition { get; set; }
        /// the list of points of entry and exit
        public List<PointsOfEntryStorage> PointsOfEntry;
        /// the stored selected character
        public Character StoredCharacter { get; set; }

        // storage
        protected Stack<float> _savedTimeScale;
		protected bool _inventoryOpen = false;
		protected bool _pauseMenuOpen = false;
		protected InventoryInputManager _inventoryInputManager;
        protected int _initialMaximumLives;
        protected int _initialCurrentLives;

        /// <summary>
        /// On Awake we initialize our list of points of entry
        /// </summary>
        protected override void Awake()
		{
			base.Awake ();
			PointsOfEntry = new List<PointsOfEntryStorage> ();
		}

	    /// <summary>
	    /// On Start(), sets the target framerate to whatever's been specified
	    /// </summary>
	    protected virtual void Start()
	    {
			Application.targetFrameRate = TargetFrameRate;
			_savedTimeScale = new Stack<float> ();
            _initialCurrentLives = CurrentLives;
            _initialMaximumLives = MaximumLives;
        }
					
		/// <summary>
		/// this method resets the whole game manager
		/// </summary>
		public virtual void Reset()
		{
			Points = 0;
			Time.timeScale = 1f;
			Paused = false;
		}
        /// <summary>
        /// Use this method to decrease the current number of lives
        /// </summary>
        public virtual void LoseLife()
        {
            CurrentLives--;
        }

        /// <summary>
        /// Use this method when a life (or more) is gained
        /// </summary>
        /// <param name="lives">Lives.</param>
        public virtual void GainLives(int lives)
        {
            CurrentLives += lives;
            if (CurrentLives > MaximumLives)
            {
                CurrentLives = MaximumLives;
            }
        }

        /// <summary>
        /// Use this method to increase the max amount of lives, and optionnally the current amount as well
        /// </summary>
        /// <param name="lives">Lives.</param>
        /// <param name="increaseCurrent">If set to <c>true</c> increase current.</param>
        public virtual void AddLives(int lives, bool increaseCurrent)
        {
            MaximumLives += lives;
            if (increaseCurrent)
            {
                CurrentLives += lives;
            }
        }

        /// <summary>
        /// Resets the number of lives to their initial values.
        /// </summary>
        public virtual void ResetLives()
        {
            CurrentLives = _initialCurrentLives;
            MaximumLives = _initialMaximumLives;
        }

        /// <summary>
        /// Adds the points in parameters to the current game points.
        /// </summary>
        /// <param name="pointsToAdd">Points to add.</param>
        public virtual void AddPoints(int pointsToAdd)
		{
			Points += pointsToAdd;
		}
		
		/// <summary>
		/// use this to set the current points to the one you pass as a parameter
		/// </summary>
		/// <param name="points">Points.</param>
		public virtual void SetPoints(int points)
		{
			Points = points;
		}
		
		/// <summary>
		/// sets the timescale to the one in parameters
		/// </summary>
		/// <param name="newTimeScale">New time scale.</param>
		public virtual void SetTimeScale(float newTimeScale)
		{
			_savedTimeScale.Push(Time.timeScale);
			Time.timeScale = newTimeScale;
		}
		
		/// <summary>
		/// Resets the time scale to the last saved time scale.
		/// </summary>
		public virtual void ResetTimeScale()
		{
			if (_savedTimeScale.Count > 0)
			{
				Time.timeScale = _savedTimeScale.Peek();
				_savedTimeScale.Pop ();	
			}
			else
			{
				Time.timeScale = 1f;
			}
		}

        /// <summary>
        /// Enables the inventory input manager if found
        /// </summary>
        /// <param name="status"></param>
		protected virtual void SetActiveInventoryInputManager(bool status)
		{
			_inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager> ();
			if (_inventoryInputManager != null)
			{
				_inventoryInputManager.enabled = status;
			}
		}

		/// <summary>
		/// Pauses the game or unpauses it depending on the current state
		/// </summary>
		public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
		{	
			if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen)
			{
				return;
			}

			// if time is not already stopped		
			if (Time.timeScale>0.0f)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
                Instance.Paused=true;
				if ((GUIManager.Instance!= null) && (pauseMethod == PauseMethods.PauseMenu))
				{
					GUIManager.Instance.SetPauseScreen(true);	
					_pauseMenuOpen = true;
					SetActiveInventoryInputManager (false);
				}
				if (pauseMethod == PauseMethods.NoPauseMenu)
				{
					_inventoryOpen = true;
				}
			}
			else
			{
				UnPause(pauseMethod);
			}		
			LevelManager.Instance.ToggleCharacterPause();
		}
        
        /// <summary>
        /// Unpauses the game
        /// </summary>
        public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            Instance.Paused = false;
			if ((GUIManager.Instance!= null) && (pauseMethod == PauseMethods.PauseMenu))
			{ 
				GUIManager.Instance.SetPauseScreen(false);
				_pauseMenuOpen = false;
				SetActiveInventoryInputManager (true);
			}
			if (_inventoryOpen)
			{
				_inventoryOpen = false;
			}
		}
        
        /// <summary>
        /// Stores the points of entry for the level whose name you pass as a parameter.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="entryIndex">Entry index.</param>
        /// <param name="exitIndex">Exit index.</param>
        public virtual void StorePointsOfEntry(string levelName, int entryIndex, Character.FacingDirections facingDirection)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						point.PointOfEntryIndex = entryIndex;
						return;
					}
				}	
			}

			PointsOfEntry.Add (new PointsOfEntryStorage (levelName, entryIndex, facingDirection));
		}

		/// <summary>
		/// Gets point of entry info for the level whose scene name you pass as a parameter
		/// </summary>
		/// <returns>The points of entry.</returns>
		/// <param name="levelName">Level name.</param>
		public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						return point;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Clears the stored point of entry infos for the level whose name you pass as a parameter
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void ClearPointOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						PointsOfEntry.Remove (point);
					}
				}
			}
		}

		/// <summary>
		/// Clears all points of entry.
		/// </summary>
		public virtual void ClearAllPointsOfEntry()
		{
			PointsOfEntry.Clear ();
		}

        /// <summary>
		/// Deletes all save files
		/// </summary>
		public virtual void ResetAllSaves()
        {
            SaveLoadManager.DeleteSaveFolder("InventoryEngine");
            SaveLoadManager.DeleteSaveFolder("TopDownEngine");
            SaveLoadManager.DeleteSaveFolder("MMAchievements");
        }

        /// <summary>
        /// Stores the selected character for use in upcoming levels
        /// </summary>
        /// <param name="selectedCharacter">Selected character.</param>
        public virtual void StoreSelectedCharacter(Character selectedCharacter)
        {
            StoredCharacter = selectedCharacter;
        }

        /// <summary>
        /// Clears the selected character.
        /// </summary>
        public virtual void ClearSelectedCharacter()
        {
            StoredCharacter = null;
        }

        /// <summary>
        /// Catches MMGameEvents and acts on them, playing the corresponding sounds
        /// </summary>
        /// <param name="gameEvent">MMGameEvent event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "inventoryOpens":
					Pause (PauseMethods.NoPauseMenu);
					break;

				case "inventoryCloses":
					Pause (PauseMethods.NoPauseMenu);
					break;
			}
		}

        /// <summary>
        /// Catches TopDownEngineEvents and acts on them, playing the corresponding sounds
        /// </summary>
        /// <param name="engineEvent">TopDownEngineEvent event.</param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.Pause:
					Pause ();
					break;

				case TopDownEngineEventTypes.UnPause:
					UnPause ();
					break;
			}
		}

        /// <summary>
        /// Catches TopDownEnginePointsEvents and acts on them, playing the corresponding sounds
        /// </summary>
        /// <param name="pointEvent">TopDownEnginePointEvent event.</param>
        public virtual void OnMMEvent(TopDownEnginePointEvent pointEvent)
		{
			
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<TopDownEngineEvent> ();
			this.MMEventStartListening<TopDownEnginePointEvent> ();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<TopDownEngineEvent> ();
			this.MMEventStopListening<TopDownEnginePointEvent> ();
		}
	}
}