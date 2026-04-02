using IAT.Core.Models;
using sun.tools.tree;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services.Validation;

namespace IAT.Core.Extensions
{
    public static class ModelObjectExtensions
    {
        /// <summary>
        /// Gets the alternation priority value associated with the specified alternation group.
        /// </summary>
        /// <param name="group">The alternation group from which to retrieve the priority value. Cannot be null.</param>
        /// <returns>The integer value representing the alternation priority of the specified group.</returns>
        public static int GetAlternationPriority(this AlternationGroup group)
        {
            return group.GroupID;
        }

        /// <summary>
        /// Determines whether the specified item is a member of the alternation group.
        /// </summary>
        /// <param name="group">The alternation group to search for the specified item. Cannot be null.</param>
        /// <param name="item">The item to locate within the alternation group. Cannot be null.</param>
        /// <returns>true if the item is a member of the group; otherwise, false.</returns>
        public static bool Contains(this AlternationGroup group, Models.IContentsItem item)
        {
            return group.GroupMembers.Contains(item);
        }

        /// <summary>
        /// Returns the one-based index of the specified block within the provided contents list.   
        /// </summary>
        /// <remarks>If the block is not present in the contents list, the method returns 0. The method
        /// uses a one-based index, so the first item in the list has a block number of 1.</remarks>
        /// <param name="block">The block whose position in the contents list is to be determined.</param>
        /// <param name="contents">The list of content items in which to search for the specified block. Cannot be null.</param>
        /// <returns>The one-based index of the block in the contents list, or 0 if the block is not found.</returns>
        public static int BlockNumber(this Block block, List<IContentsItem> contents)
        {
            return contents.IndexOf(block) + 1;
        }

        /// <summary>
        /// Returns the number of items contained in the specified block.
        /// </summary>
        /// <param name="block">The block whose items are to be counted. Cannot be null.</param>
        /// <returns>The number of items in the block.</returns>
        public static int NumberOfItems(this Block block)
        {
            return block.TrialUrs.Count;
        }

        /// <summary>
        /// Retrieves the block at the specified index within the alternation group.
        /// </summary>
        /// <param name="group">The alternation group from which to retrieve the block.</param>
        /// <param name="index">The zero-based index of the block to retrieve. Must be within the bounds of the group's members and refer to
        /// a member of type Block.</param>
        /// <returns>The block at the specified index if it exists and is of type Block; otherwise, null.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is outside the valid range of group members or does not refer to a Block.</exception>
        public static Block? GetBlock(this AlternationGroup group, int index)
        {
            if (index < 0 || index >= group.GroupMembers.Count || group.GroupMembers[index] is not Block)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range for the alternation group.");
            }
            return group.GroupMembers[index] as Block;

        }

        /// <summary>
        /// Returns the alternate block in the same alternation group as the specified block, if one exists.
        /// </summary>
        /// <remarks>If the block is not part of an alternation group or the group does not contain
        /// exactly two members, the method returns null or the original block. This method is typically used to
        /// retrieve the counterpart in a binary alternation scenario.</remarks>
        /// <param name="block">The block for which to find the alternate block within its alternation group.</param>
        /// <returns>The alternate block in the same alternation group if the group contains exactly two members and an alternate
        /// exists; otherwise, returns null or the original block.</returns>
        public static Block? GetAlternateBlock(this Block block)
        {
            if (block.AlternationGroup == null)
                return null;
            if (block.AlternationGroup.GroupMembers.Count == 2) 
            {
                foreach (var member in block.AlternationGroup.GroupMembers)
                {
                    if (member is Block alternate && alternate != block)
                    {
                        return alternate;
                    }
                }
            }
            return block;
        }

        /// <summary>
        /// Determines whether the specified block contains the given trial.
        /// </summary>
        /// <param name="block">The block to search for the trial. Cannot be null.</param>
        /// <param name="trial">The trial to locate within the block. Cannot be null.</param>
        /// <returns>true if the block contains the specified trial; otherwise, false.</returns>
        public static bool Contains(this Block block, Trial trial) => block.TrialIds.Contains(trial.Id);


        /// <summary>
        /// Validates the specified block and adds any validation errors to the provided dictionary.
        /// </summary>
        /// <param name="block">The block to validate. Must not be null.</param>
        /// <param name="errors">A dictionary to which any validation errors found will be added. The key is the validated item, and the
        /// value is the corresponding validation error.</param>
        /// <exception cref="InvalidOperationException">Thrown if the block does not contain at least one trial.</exception>
        public static void Validate(this Block block, Dictionary<IValidatedItem, ValidationError> errors)
        {
            if (block.TrialIds.Count == 0)
                errors.Add(block, new ValidationError()
                {
                    Error = Error.BlockHasNoItems,
                    Container = block.BlockNumber,
                    Item = -1
                });
            if (block.Key is null)
                errors.Add(block, new ValidationError()
                {
                    Error = Error.BlockResponseKeyUndefined,
                    Container = block.BlockNumber,
                    Item = -1
                });
            foreach (var trial in block.Trials)
                trial.Validate(errors);
            throw new InvalidOperationException("A block must contain at least one trial.");
        }

        public static void Validate(this Trial trial, Dictionary<IValidatedItem, ValidationError> errors)
        {
            if (trial.StimulusId == Guid.Empty)
                errors.Add(trial, new ValidationError()
                {
                    Error = Error.ItemStimulusUndefined,
                    Container = trial.OriginatingBlock,
                    Item = trial.TrialNumber
                });
             if (trial.Key is null)
                errors.Add(trial, new ValidationError()
                {
                    Error = Error.TrialResponseKeyUndefined,
                    Container = trial.BlockNumber,
                    Item = trial.ItemIndex
                });
        }
}


