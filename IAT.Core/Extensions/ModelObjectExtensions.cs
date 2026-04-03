using IAT.Core.Models;
using IAT.Core.Models.Enumerations;
using IAT.Core.Models.Serializable;
using sun.awt.image;
using System.Security.Cryptography;

namespace IAT.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for working with alternation groups, blocks, trials, and related model objects.
    /// </summary>
    /// <remarks>This static class contains utility methods that extend core model types such as
    /// AlternationGroup, Block, and Trial. The methods facilitate common operations including membership checks,
    /// retrieval of alternates, block and trial management, and keyed direction evaluation. These extensions are
    /// intended to simplify and standardize interactions with model objects in scenarios involving alternation and
    /// trial management.</remarks>
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
            return block.Trials.Count;
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
        /// Determines the effective keyed direction for the specified trial, based on the originating block and the
        /// current block context.
        /// </summary>
        /// <param name="trial">The trial for which to determine the keyed direction. Must not be null.</param>
        /// <param name="block">The current block context used to evaluate the keyed direction. Must not be null.</param>
        /// <returns>A KeyedDirection value representing the direction to use for the trial in the context of the specified
        /// block.</returns>
        public static KeyedDirection GetKeyedDirection(this Trial trial, Block block)
        {
            if (trial.OriginatingBlock == 1)
                return trial.KeyedDirection;
            if ((trial.OriginatingBlock == 2) && (block.BlockNumber >= 2) && (block.BlockNumber <= 4))
                return trial.KeyedDirection;
            return trial.KeyedDirection.Opposite;
        }


        /// <summary>
        /// Adds the specified trial to the block if it is not already present.
        /// </summary>
        /// <remarks>If the trial or its identifier already exists in the block, this method does not add
        /// duplicates.</remarks>
        /// <param name="block">The block to which the trial will be added. Cannot be null.</param>
        /// <param name="trial">The trial to add to the block. Cannot be null.</param>
        public static void AddTrial(this Block block, Trial trial)
        {
            if (block.Trials.Count >= 0)
            {
                if (!block.Trials.Contains(trial))
                    block.Trials.Add(trial);
                if (!block.TrialIds.Contains(trial.Id)) 
                    block.TrialIds.Add(trial.Id);
            }
        }

        /// <summary>
        /// Removes the specified trial from the block and updates the associated trial identifiers.
        /// </summary>
        /// <remarks>If the specified trial does not exist in the block, no action is taken.</remarks>
        /// <param name="block">The block from which the trial will be removed. Cannot be null.</param>
        /// <param name="trial">The trial to remove from the block. Cannot be null.</param>
        public static void RemoveTrial(this Block block, Trial trial)
        {
            block.Trials.Remove(trial);
            block.TrialIds.Remove(trial.Id);
        }

        /// <summary>
        /// Removes the trial with the specified identifier from the block.
        /// </summary>
        /// <remarks>If the specified trial is not found in the block, no action is taken.</remarks>
        /// <param name="block">The block from which to remove the trial. Cannot be null.</param>
        /// <param name="trialId">The unique identifier of the trial to remove.</param>
        public static void RemoveTrial(this Block block, Guid trialId)
        {
            var trialToRemove = block.Trials.Find(t => t.Id == trialId);
            if (trialToRemove != null)
            {
                block.Trials.Remove(trialToRemove);
                block.TrialIds.Remove(trialId);
            }
        }

        /// <summary>
        /// Returns the zero-based index of the trial with the specified identifier within the block's collection of
        /// trials.
        /// </summary>
        /// <param name="block">The block containing the collection of trials to search.</param>
        /// <param name="trialId">The unique identifier of the trial to locate.</param>
        /// <returns>The zero-based index of the trial if found; otherwise, -1.</returns>
        public static int GetTrialIndex(this Block block, Guid trialId)
        {
            return block.Trials.FindIndex(t => t.Id == trialId);
        }

        /// <summary>
        /// Verifies that the decrypted ciphertext from the handshake matches the specified plain text using the
        /// provided RSA key.
        /// </summary>
        /// <remarks>This method performs a fixed-time comparison to help prevent timing attacks. The
        /// comparison is case-sensitive and uses UTF-8 encoding for the plain text.</remarks>
        /// <param name="handshake">The handshake containing the base64-encoded ciphertext to verify. The CipherText property must not be null
        /// or empty.</param>
        /// <param name="rsa">The RSA key used to decrypt the ciphertext. Must be initialized with the appropriate private key.</param>
        /// <param name="PlainText">The plain text string to compare against the decrypted ciphertext. Encoded as UTF-8 for comparison.</param>
        /// <returns>true if the decrypted ciphertext matches the specified plain text; otherwise, false.</returns>
        public static bool Verify(this Handshake handshake)
        {
            var cipherBytes = Convert.FromBase64String(handshake.CipherText ?? string.Empty);
            var plainBytes = Convert.FromBase64String(handshake.PlainText ?? string.Empty);
            var decryptedBytes = Handshake.RSA.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
            return CryptographicOperations.FixedTimeEquals(decryptedBytes, plainBytes);
        }

        /// <summary>
        /// Initializes the specified handshake with new cryptographic parameters, including a random plaintext, its
        /// encrypted ciphertext, and the public key information.
        /// </summary>
        /// <remarks>This method generates a new 32-byte random plaintext, encrypts it using the
        /// handshake's RSA public key, and sets the handshake's modulus and public key properties. The method
        /// overwrites any existing values in the handshake instance.</remarks>
        /// <param name="handshake">The handshake object to populate with generated cryptographic values. Cannot be null.</param>
        public static void Formulate(this Handshake handshake)
        {
            handshake.Modulus = Convert.ToBase64String(Handshake.RSA.ExportParameters(false).Modulus?.ToArray<byte>() ?? new byte[] { 1 });
            handshake.PublicKey = Convert.ToBase64String(Handshake.RSA.ExportRSAPublicKey());
            handshake.PlainText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}


