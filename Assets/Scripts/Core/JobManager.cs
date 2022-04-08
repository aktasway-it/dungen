using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Core
{
	/// <summary>
	/// Job manager singleton class. Nothing but a proxy object for running coroutines.
	/// </summary>
	public class JobManager : SingletonBehavior<JobManager> 
	{
		protected override void Init ()
		{
			base.Init ();
			Application.targetFrameRate = 60;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
	}

	/// <summary>
	/// Job class. Jobs are used for running coroutines instead of using MonoBehavior.StartCoroutine(). They're more flexible and powerful, allowing pausing, resuming, killing and attach/remove children jobs
	/// </summary>
	public class Job
	{
		public event Action<bool> onJobCompleted;

		/// <summary>
		/// Is the job running?
		/// </summary>
		/// <value><c>true</c> if the job is running; otherwise, <c>false</c>.</value>
		public bool IsRunning
		{
			get
			{
				return _isRunning;
			}
		}

		/// <summary>
		/// Is the job paused?
		/// </summary>
		/// <value><c>true</c> if the job is paused; otherwise, <c>false</c>.</value>
		public bool IsPaused
		{
			get
			{
				return _isPaused;
			}
		}

		private bool _isRunning;
		private bool _isPaused;

		private IEnumerator _coroutine;
		private bool _wasKilled;
		private Stack<Job> _childJobStack;

		/// <summary>
		/// Creates a new instance of the <see cref="Job"/> class.
		/// </summary>
		/// <param name="coroutine">The coroutine to be run.</param>
		/// <param name="startImmediately">If set to <c>true</c> the coroutine starts immediately.</param>
		public Job(IEnumerator coroutine, bool startImmediately = true)
		{
			_coroutine = coroutine;

			if(startImmediately)
				Start();
		}

		/// <summary>
		/// Creates a new job and starts it immediately.
		/// </summary>
		/// <param name="coroutine">The coroutine to be run.</param>
		/// <param name="startImmediately">If set to <c>true</c> the coroutine starts immediately.</param>
		public static Job Create(IEnumerator coroutine, bool startImmediately = true)
		{
			return new Job(coroutine, startImmediately);
		}

		/// <summary>
		/// Adds a child job.
		/// </summary>
		/// <param name="childJob">Child job to add.</param>
		public void AddChildJob(Job childJob)
		{
			if(_childJobStack == null)
				_childJobStack = new Stack<Job>();

			_childJobStack.Push(childJob);
		}

		/// <summary>
		/// Removes the child job.
		/// </summary>
		/// <param name="childJob">Child job to be removed.</param>
		public void RemoveChildJob(Job childJob)
		{
			if(_childJobStack.Contains(childJob))
			{
				var childStack = new Stack<Job>(_childJobStack.Count - 1);
				var currentJobs = _childJobStack.ToArray();

				Array.Reverse(currentJobs);

				for(int i = 0; i < currentJobs.Length; ++i)
				{
					var child = currentJobs[i];

					if(child != childJob)
						childStack.Push(child);
				}

				_childJobStack = childStack;
			}
		}

		/// <summary>
		/// Start the job, this is meant to be used if startImmediately was set to false in the Job constructor or when calling Create.
		/// </summary>
		public void Start()
		{
			if(_isRunning)
				return;
		
			_isRunning = true;
			JobManager.Instance.StartCoroutine(Run());
		}

		/// <summary>
		/// Starts the job and returns a refernce to the running coroutine, useful for when you need to yield for a job inside another coroutine.
		/// </summary>
		/// <returns>The as routine.</returns>
		public IEnumerator StartAsRoutine()
		{
			if(_isRunning)
				yield break;
		
			_isRunning = true;
			yield return JobManager.Instance.StartCoroutine(Run());
		}

		/// <summary>
		/// Pause the job.
		/// </summary>
		public void Pause()
		{
			_isPaused = true;
		}

		/// <summary>
		/// Resume the job.
		/// </summary>
		public void Resume()
		{
			_isPaused = false;
		}

		/// <summary>
		/// Terminates the job and its children job immediately.
		/// </summary>
		public void Kill()
		{
			if(_wasKilled)
				return;
		
			_wasKilled = true;
			_isRunning = false;
			_isPaused = false;
		}

		/// <summary>
		/// Terminates the job and its children job with a specified time delay.
		/// </summary>
		/// <param name="delayInSeconds">Delay in seconds.</param>
		public void Kill(float delayInSeconds)
		{
			if(_wasKilled)
				return;
		
			int delay = Mathf.RoundToInt(delayInSeconds * 1000);

			new Timer(delegate(object state) {
				lock(this)
				{
					Kill();
				}
			}, null, delay, Timeout.Infinite);
		}

		private IEnumerator Run()
		{
			yield return null;

			while(_isRunning)
			{
				if(_isPaused)
				{
					yield return null;
				}
				else
				{
					if(_coroutine.MoveNext())
					{
						yield return _coroutine.Current;
					}
					else
					{
						if(_childJobStack != null)
							yield return JobManager.Instance.StartCoroutine(RunChildJobs());

						_isRunning = false;
					}
				}
			}

			if(onJobCompleted != null)
				onJobCompleted(_wasKilled);
		}

		private IEnumerator RunChildJobs()
		{
			if(_childJobStack != null && _childJobStack.Count > 0)
			{
				do
				{
					Job childJob = _childJobStack.Pop();
					yield return JobManager.Instance.StartCoroutine(childJob.StartAsRoutine());
				}
				while(_childJobStack.Count > 0);
			}
		}
	}
}