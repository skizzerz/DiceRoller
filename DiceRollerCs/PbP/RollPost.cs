using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Dice.PbP
{
    /// <summary>
    /// Represents a post (such as in a Play-by-Post) that contains one or more rolls.
    /// <para>This class contains utilities that let you store results for multiple rolls in a single post,
    /// get those results back without changing the rolled values, and check for evidence of roll tampering/cheating.</para>
    /// </summary>
    [Serializable]
    public class RollPost : ISerializable
    {
        private List<RollResult> _pristine;
        private List<RollResult> _current;

        /// <summary>
        /// Contains the "pristine" version of the post. This is used in cheat detection, as it should be a prefix of Current.
        /// If Pristine and Current diverge (as opposed to Current just having more elements), the post is flagged as being tampered with.
        /// </summary>
        public IReadOnlyList<RollResult> Pristine => _pristine;

        /// <summary>
        /// Contains the current version of the post. All rolls in the post are represented here in-order so that their results may be
        /// directly used rather than needing to re-evaluate the dice expression each time the post is viewed/previewed/edited.
        /// </summary>
        public IReadOnlyList<RollResult> Current => _current;

        /// <summary>
        /// Constructs a new, empty RollPost. This represents a new post being made. If editing an existing post,
        /// the RollPost should be constructed via deserializing the old RollPost stored in the database or other storage medium.
        /// </summary>
        public RollPost()
        {
            _pristine = new List<RollResult>();
            _current = new List<RollResult>();
        }

        /// <summary>
        /// Constructs a new RollPost using serialized data. This should be used whenever creating a RollPost based on an existing post.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RollPost(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            _pristine = ((RollResult[])info.GetValue("Pristine", typeof(RollResult[]))).ToList();
            _current = ((RollResult[])info.GetValue("Current", typeof(RollResult[]))).ToList();
        }

        /// <summary>
        /// Serializes a RollPost.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("_Version", 1);
            info.AddValue("Pristine", _pristine.ToArray(), typeof(RollResult[]));
            info.AddValue("Current", _current.ToArray(), typeof(RollResult[]));
        }
    }
}
