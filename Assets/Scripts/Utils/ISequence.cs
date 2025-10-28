using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util.Extensions
{
    /// <summary>
    /// Interface defining a sequence of actions, which can be started and ended manually.
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        /// Event triggered when the sequence has finished.
        /// </summary>
        public event Action OnFinished;
        
        /// <summary>
        /// Starts the sequence by invoking its begin action.
        /// </summary>
        public void Begin();

        /// <summary>
        /// Forcefully ends the sequence, bypassing the normal finish procedure.
        /// </summary>
        public void End();
    }

    /// <summary>
    /// A customizable sequence of actions that allows you to specify custom begin and end actions.
    /// </summary>
    public class CustomSequence : ISequence
    {
        public event Action OnFinished;

        private Action begin;
        private Action end;
        private bool started;

        /// <summary>
        /// Initializes a new sequence where a single action is executed on Begin, followed by finishing the sequence.
        /// </summary>
        /// <param name="action">The action to execute when the sequence begins.</param>
        public CustomSequence(Action action)
        {
            begin = action;
            begin += End; // Automatically call End after the action completes
            end = FinishSequence; // End action
        }

        /// <summary>
        /// Initializes a sequence with customizable begin and end actions.
        /// </summary>
        /// <param name="beginAction">The action to execute when the sequence begins.</param>
        /// <param name="endAction">The action to execute when the sequence ends.</param>
        public CustomSequence(Action beginAction, Action endAction)
        {
            begin = beginAction;
            end = endAction;
            end += FinishSequence; // Automatically finish the sequence when end action completes
        }

        /// <summary>
        /// Starts the sequence by invoking the specified begin action.
        /// </summary>
        public void Begin()
        {
            if (started) return; // Prevent starting if the sequence has already started
            started = true;
            begin?.Invoke();
        }

        /// <summary>
        /// Ends the sequence by invoking the specified end action.
        /// </summary>
        public void End()
        {
            if (!started) return; // Prevent ending if the sequence hasn't started
            end?.Invoke();
        }

        /// <summary>
        /// Sets a custom end action for this sequence.
        /// </summary>
        /// <param name="action">The custom action to invoke when the sequence ends.</param>
        /// <returns>Returns the current sequence instance for method chaining.</returns>
        public CustomSequence SetEndAction(Action action)
        {
            end = action;
            return this;
        }

        /// <summary>
        /// Marks the sequence as finished and triggers the OnFinished event.
        /// </summary>
        public void FinishSequence()
        {
            started = false;
            OnFinished?.Invoke();
        }

        /// <summary>
        /// Creates an empty sequence that does nothing when executed.
        /// </summary>
        /// <returns>A new instance of an empty sequence.</returns>
        public static CustomSequence EmptySequence() => new CustomSequence(null);
    }

    /// <summary>
    /// A sequence that runs another sequence after performing an initial setup action.
    /// </summary>
    public class SequenceWithSetup : ISequence
    {
        private Action SetupAction;
        private ISequence Sequence;
        public event Action OnFinished;

        /// <summary>
        /// Initializes a sequence that will first perform the setup action, then begin the nested sequence.
        /// </summary>
        /// <param name="setup">The setup action to perform before the sequence begins.</param>
        /// <param name="sequence">The sequence that will run after the setup is complete.</param>
        public SequenceWithSetup(Action setup, ISequence sequence)
        {
            SetupAction = setup;
            Sequence = sequence;
            sequence.OnFinished += Finish; // Notify when the nested sequence finishes
        }
        
        /// <summary>
        /// Begins the sequence by first running the setup action and then starting the nested sequence.
        /// </summary>
        public void Begin()
        {
            SetupAction?.Invoke();
            Sequence?.Begin();
        }

        /// <summary>
        /// Ends the nested sequence by invoking its end method.
        /// </summary>
        public void End() => Sequence?.End();

        /// <summary>
        /// Invoked when the nested sequence finishes, triggers the OnFinished event.
        /// </summary>
        private void Finish() => OnFinished?.Invoke();
    }

    /// <summary>
    /// A sequence that runs multiple other sequences in parallel.
    /// </summary>
    public class ParallelSequences : ISequence
    {
        public event Action OnFinished;

        private List<ISequence> sequences = new();
        private int sequencesLeft;
        private bool running => sequencesLeft > 0;

        /// <summary>
        /// Adds a sequence to be executed in parallel with others.
        /// </summary>
        /// <param name="sequence">The sequence to add to the parallel group.</param>
        /// <returns>Returns the current ParallelSequences instance for method chaining.</returns>
        public ParallelSequences Add(ISequence sequence)
        {
            sequences.Add(sequence);
            sequence.OnFinished += OnSequenceFinished;
            return this;
        }

        /// <summary>
        /// Starts all sequences in parallel.
        /// </summary>
        public void Begin()
        {
            if (running) return; // Prevent starting if sequences are already running
            sequencesLeft = sequences.Count;
            for (int i = 0; i < sequences.Count; i++)
                sequences[i]?.Begin();
            if(!running) AllSequencesFinished();
        }

        /// <summary>
        /// Ends all sequences in parallel.
        /// </summary>
        public void End()
        {
            if (!running) return; // Prevent ending if sequences are not running
            for (int i = 0; i < sequences.Count; i++)
                sequences[i]?.End();
        }

        /// <summary>
        /// Called when a sequence finishes, decreases the count of remaining sequences.
        /// </summary>
        private void OnSequenceFinished()
        {
            sequencesLeft--;
            if (running) return; // If there are still sequences running, do nothing
            AllSequencesFinished();
        }

        /// <summary>
        /// Called when all sequences are finished, triggers the OnFinished event.
        /// </summary>
        private void AllSequencesFinished()
        {
            sequencesLeft = 0;
            OnFinished?.Invoke();
        }

        /// <summary>
        /// Clears the list of sequences and removes the event handlers.
        /// </summary>
        private void ClearSequence()
        {
            for (int i = 0; i < sequences.Count; i++)
                sequences[i].OnFinished -= OnSequenceFinished;
            sequences.Clear();
        }
    }

    /// <summary>
    /// A sequence that only runs if a specified condition is true.
    /// </summary>
    public class ConditionalSequence : ISequence
    {
        private Func<bool> condition;
        private ISequence Sequence;
        public event Action OnFinished;

        private bool playedSequence;

        /// <summary>
        /// Initializes a conditional sequence that will only run if the condition is true.
        /// </summary>
        /// <param name="sequenceCondition">A function that returns true if the sequence should run.</param>
        /// <param name="sequence">The sequence to execute if the condition is met.</param>
        public ConditionalSequence(Func<bool> sequenceCondition, ISequence sequence)
        {
            condition = sequenceCondition;
            Sequence = sequence;
            sequence.OnFinished += Finish; // Notify when the sequence finishes
        }
        
        /// <summary>
        /// Starts the sequence if the condition returns true, otherwise ends immediately.
        /// </summary>
        public void Begin()
        {
            playedSequence = condition?.Invoke() ?? true;
            if (playedSequence) Sequence?.Begin();
            else End();
        }

        /// <summary>
        /// Ends the sequence if it was played, or finishes immediately if it was skipped.
        /// </summary>
        public void End()
        {
            if (playedSequence) Sequence?.End();
            else Finish();
        }

        /// <summary>
        /// Marks the conditional sequence as finished and triggers the OnFinished event.
        /// </summary>
        private void Finish()
        {
            playedSequence = false;
            OnFinished?.Invoke();
        }
    }

    public class BranchedSequence : ISequence
    {
        private ISequence mainSequence;
        private ISequence altSequence;

        private Func<bool> contidion;

        private ISequence currentSequence;
        
        public event Action OnFinished;

        public BranchedSequence(ISequence main, ISequence alt, Func<bool> contidion)
        {
            mainSequence = main;
            altSequence = alt;
            this.contidion = contidion;
        }
        
        public void Begin()
        {
            currentSequence = contidion?.Invoke() ?? true ? mainSequence : altSequence;
            currentSequence.OnFinished += OnSequenceEnd;
            currentSequence.Begin();
        }

        public void End() => currentSequence?.End();

        private void OnSequenceEnd()
        {
            currentSequence.OnFinished -= OnSequenceEnd;
            currentSequence = null;
            OnFinished?.Invoke();
        }
    }

    /// <summary>
    /// Play the main sequence. when it is done,
    /// it will finish this sequence then call the followUp sequence.
    /// </summary>
    public class FollowUpSequence : ISequence
    {
        private ISequence sequence;
        private ISequence followUp;
        
        public event Action OnFinished;

        public FollowUpSequence(ISequence mainSequence, ISequence followUpSequence)
        {
            sequence = mainSequence;
            followUp = followUpSequence;
        }
        
        public void Begin()
        {
            sequence.OnFinished += OnMainSequenceFinished;
            sequence.Begin();
        }

        /// <summary>
        /// End both the main and followUp sequence
        /// </summary>
        public void End()
        {
            sequence.End();
            followUp.End();
        }

        private void OnMainSequenceFinished()
        {
            sequence.OnFinished -= OnMainSequenceFinished;
            OnFinished?.Invoke();
            followUp.Begin();
        }
    }
}
