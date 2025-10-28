using System;
using System.Collections.Generic;
using UnityEngine;
using Util.Extensions;

namespace Util
{
    /// <summary>
    /// Manages a sequence of animations or actions, allowing for the addition, insertion, and control of multiple sequences.
    /// </summary>
    public class SequenceManager : ISequence
    {
        private List<ISequence> sequences = new(); // List of all sequences in the manager
        private int current = 0; // Index of the current sequence being executed
        private bool skipAll; // Flag to indicate if all sequences should be skipped
        
        public event Action OnSequenceChanged; // Action to be invoked when the current sequence changes
        public event Action OnFinished; // Event triggered when all sequences have finished

        public bool running { get; private set; } // Indicates if the sequence manager is currently running
        public int sequenceCount => sequences?.Count ?? 0;
        public ISequence currentSequence => sequences.Count > 0 ? sequences[current] : null; // Retrieves the current sequence or null if none exists

        /// <summary>
        /// Adds a sequence to the list of sequences in the manager.
        /// </summary>
        /// <param name="sequence">The sequence to be added.</param>
        private void AddSequence(ISequence sequence) => sequences.Add(sequence);

        /// <summary>
        /// Adds an animation (sequence) to the sequence manager.
        /// </summary>
        /// <param name="sequence">The animation (sequence) to be added.</param>
        /// <returns>Returns the current SequenceManager instance for method chaining.</returns>
        public SequenceManager Add(ISequence sequence)
        {
            AddSequence(sequence);
            return this;
        }

        /// <summary>
        /// Inserts a sequence directly after a target sequence. 
        /// The insert will fail if the target sequence has already been completed.
        /// </summary>
        /// <param name="sequence">The sequence to be inserted after the target sequence.</param>
        /// <param name="afterSequence">The target sequence that the new sequence will be inserted after.</param>
        /// <returns>Returns the current SequenceManager instance for method chaining.</returns>
        public SequenceManager InsertAfter(ISequence sequence, ISequence afterSequence)
        {
            int id = sequences.IndexOf(afterSequence); // Find the index of the target sequence
            if (id < 0 || id >= sequences.Count - 1) 
            {
                // If the target sequence doesn't exist or it's the last one, just add the new sequence at the end
                AddSequence(sequence);
            }
            else
            {
                if (id < current)
                {
                    current++;
                    Debug.LogWarning("Target sequence already passed");
                }
                // If the target sequence is still to be executed, insert the new sequence after it
                sequences.Insert(id + 1, sequence);
            }
            
            return this;
        }
        
        /// <summary>
        /// Inserts a sequence at a specific index in the sequence list.
        /// </summary>
        /// <param name="sequence">The sequence to insert at the specified index.</param>
        /// <param name="id">The index at which to insert the sequence.</param>
        /// <returns>Returns the current SequenceManager instance for method chaining.</returns>
        public SequenceManager InsertSequence(ISequence sequence, int id)
        {
            sequences.Insert(id, sequence);
            return this;
        }

        /// <summary>
        /// Checks if a specific sequence exists in the sequence manager.
        /// </summary>
        /// <param name="sequence">The sequence to check for.</param>
        /// <returns>Returns true if the sequence is in the list; otherwise, false.</returns>
        public bool Contains(ISequence sequence) => sequences.Contains(sequence);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified sequence in the collection.
        /// </summary>
        /// <param name="sequence">The sequence to locate in the sequence list.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of the specified sequence if found; otherwise, -1.
        /// </returns>
        public int IndexOf(ISequence sequence) => sequences.IndexOf(sequence);
        
        /// <summary>
        /// Returns the zero-based index of the last occurrence of the specified sequence in the collection.
        /// </summary>
        /// <param name="sequence">The sequence to locate in the sequence list.</param>
        /// <returns>
        /// The zero-based index of the last occurrence of the specified sequence if found; otherwise, -1.
        /// </returns>
        public int LastIndexOf(ISequence sequence) => sequences.LastIndexOf(sequence);

        /// <summary>
        /// Clears all sequences from the manager and resets the state.
        /// </summary>
        public void ClearSequence()
        {
            if(current >= 0 && current < sequences.Count) 
            {
                // Unsubscribe from the current sequence's OnFinished event
                sequences[current].OnFinished -= GoToNext;
            }
            sequences.Clear(); // Clears the sequence list
            current = 0; // Reset the current sequence index
        }

        /// <summary>
        /// Begins executing the sequences from the start.
        /// </summary>
        public void Begin()
        {
            if (running) return; // Prevent re-starting if already running
            current = 0; // Reset to the first sequence
            skipAll = false; // Reset the skipAll flag
            running = true; // Mark as running
            PlaySequence(); // Start the first sequence
        }
        
        /// <summary>
        /// Ends the whole sequence, skipping any remaining sequences.
        /// </summary>
        public void End()
        {
            if (!running) return; // Only end if running
            skipAll = true; // Set the flag to skip all remaining sequences
            EndCurrent(); // End the current sequence immediately
        }

        /// <summary>
        /// Ends the currently running sequence.
        /// </summary>
        public void EndCurrent()
        {
            if (!running) return; // Only end if running
            currentSequence?.End(); // Call End on the current sequence if it exists
        }
        
        /// <summary>
        /// Starts or resumes the execution of the current sequence.
        /// </summary>
        private void PlaySequence()
        {
            // Ensure current index is valid
            if (current < 0 || current >= sequences.Count)
            {
                FinishSequence();
                return;
            }

            ISequence animation = currentSequence;
            if (animation == null) return; // Return if no sequence exists at the current index
            animation.OnFinished += GoToNext; // Subscribe to the OnFinished event of the current sequence
            animation.Begin(); // Begin the current sequence
            OnSequenceChanged?.Invoke(); // Notify that the sequence has changed
            if (skipAll) animation.End(); // If skipping, immediately end the current sequence
        }

        /// <summary>
        /// Moves to the next sequence in the list once the current sequence is finished.
        /// </summary>
        private void GoToNext()
        {
            currentSequence.OnFinished -= GoToNext; // Unsubscribe from the current sequence's OnFinished event
            current++; // Move to the next sequence
            PlaySequence();
        }

        /// <summary>
        /// Reset properties and call OnFinished event.
        /// </summary>
        private void FinishSequence()
        {
            current = 0; // Reset current sequence index
            running = false; // All sequences are finished, mark as not running
            skipAll = false; // Reset the skip flag
            OnFinished?.Invoke(); // Trigger the OnFinished event
        }
    }
}
