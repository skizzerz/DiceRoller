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
    public class RollPost : ISerializable, IDeserializationCallback
    {
        private List<RollResult> _pristine = new List<RollResult>();
        private List<RollResult> _stored = new List<RollResult>();
        private List<RollResult> _current = new List<RollResult>();
        private int _diverged = 0;

        /// <summary>
        /// Contains the "pristine" version of the post. This is used in cheat detection, as it should be a prefix of Current.
        /// If Pristine and Current diverge (as opposed to Current just having more elements), the post is flagged as being tampered with.
        /// </summary>
        public IReadOnlyList<RollResult> Pristine => _pristine;

        /// <summary>
        /// Mutable version of Pristine. Subclasses can modify the list via this method.
        /// </summary>
        protected IList<RollResult> PristineList { get; private set; }

        /// <summary>
        /// Contains the most recent saved version of the post. All rolls in the post are represented here in-order so that their results may be
        /// directly used rather than needing to re-evaluate the dice expression each time the post is viewed/previewed/edited.
        /// </summary>
        public IReadOnlyList<RollResult> Stored => _stored;

        /// <summary>
        /// Mutable version of Stored. Subclasses can modify the list via this method.
        /// </summary>
        protected IList<RollResult> StoredList { get; private set; }

        /// <summary>
        /// Contains the current version of the post (the one being checked). This starts out empty even when deserializing, and is added to
        /// via AddRoll(). The TamperWarning flag is set based on the value of this.
        /// </summary>
        public IReadOnlyList<RollResult> Current => _current;

        /// <summary>
        /// Mutable version of Current. Subclasses can modify the list via this method.
        /// </summary>
        protected IList<RollResult> CurrentList { get; private set; }

        /// <summary>
        /// Constructs a new, empty RollPost. This represents a new post being made. If editing an existing post,
        /// the RollPost should be constructed via deserializing the old RollPost stored in the database or other storage medium.
        /// </summary>
        public RollPost() {
            PristineList = _pristine;
            StoredList = _stored;
            CurrentList = _current;
        }

        /// <summary>
        /// Constructs a new RollPost using serialized data. This should be used whenever creating a RollPost based on an existing post.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RollPost(SerializationInfo info, StreamingContext context)
            : this()
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            // array is deserialized before contents, so cannot do .ToList() to save directly in _pristine and _stored
            // as such, save to these lists instead for the time being, then fix when all is done.
            PristineList = (RollResult[])info.GetValue("Pristine", typeof(RollResult[]));
            StoredList = (RollResult[])info.GetValue("Stored", typeof(RollResult[]));
        }

        /// <summary>
        /// Completes deserialization of the RollPost once the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnDeserialization(object sender)
        {
            _pristine = PristineList.ToList();
            _stored = StoredList.ToList();
            PristineList = _pristine;
            StoredList = _stored;
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
            // not a typo: we save Current as Stored, overwriting the previous value of Stored.
            // If Validate() succeeds, Pristine is set to Current. As such, it is already up-to-date on save time.
            info.AddValue("Stored", _current.ToArray(), typeof(RollResult[]));
        }

        /// <summary>
        /// Adds a new roll to the post using the DefaultConfig if the roll needs to be evaluated.
        /// </summary>
        /// <param name="diceExpr">The dice expression to add, will be evaluated then added to the end of Current</param>
        public void AddRoll(string diceExpr)
        {
            AddRoll(diceExpr, null);
        }

        /// <summary>
        /// Adds a new roll to the post using the given config if the roll needs to be evaluated.
        /// </summary>
        /// <param name="diceExpr">The dice expression to add, will be evaluated then added to the end of Current</param>
        /// <param name="config">The configuration used for the roll. If null, RollerConfig.Default is used</param>
        public void AddRoll(string diceExpr, RollerConfig config)
        {
            if (config == null)
            {
                config = Roller.DefaultConfig;
            }

            int nextIdx = Current.Count;
            var ast = Roller.Parse(diceExpr, config);
            string normalizedExpr = ast.ToString();
            var pristineVersion = _pristine.ElementAtOrDefault(nextIdx);
            var storedVersion = _stored.ElementAtOrDefault(nextIdx);

            // as later rolls may have macros that refer to earlier ones, we don't want to break out into a new roll
            // and then re-use a previously-rolled value. If we do, those saved macros may be incorrect.
            // As such, when we detect a divergence (pristine->stored->new roll), we ensure that we don't go back to
            // a previous state (so no stored->pristine or new roll->stored).

            if (_diverged == 0 && normalizedExpr == pristineVersion?.Expression)
            {
                // use the pristine result for this roll
                _current.Add(pristineVersion);
            }
            else if (_diverged <= 1 && normalizedExpr == storedVersion?.Expression)
            {
                _current.Add(storedVersion);
                _diverged = 1;
            }
            else
            {
                // none of the versions for the current index matched, so reroll it
                // ensure that our custom macro handler is attached to handle PbP-specific macros (accessing previous rolls)
                config.ExecuteMacro -= PostMacros;
                config.ExecuteMacro += PostMacros;
                _current.Add(Roller.Roll(ast, config));
                config.ExecuteMacro -= PostMacros; // ensure we don't leave around our custom executor past this
                _diverged = 2;
            }
        }

        /// <summary>
        /// Checks if Current has diverged from Pristine. If it has, this returns false and indicates that there was
        /// some tampering of the dice expressions (cheating). If it hasn't, this returns true to indicate that everything is good.
        /// <para>Additionally, if this succeeds, Pristine is set to Current.</para>
        /// <para>If you wish to add your own checks, extend RollPost in your own class and override Validate. Call the base
        /// Validate() only if your own checks all succeed. Otherwise, you may accidentally overwrite Pristine on validation failure.</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate()
        {
            if (_current.Count < _pristine.Count)
            {
                // rolls were removed
                return false;
            }

            if (!_pristine.SequenceEqual(_current.Take(_pristine.Count)))
            {
                // beginning of each doesn't match
                return false;
            }

            // if we get here, all is well
            _pristine = _current;

            return true;
        }

        /// <summary>
        /// Executes PbP-specific macros, which are documented on the DiceRoller wiki in the Dice Reference.
        /// </summary>
        /// <param name="context"></param>
        private void PostMacros(MacroContext context)
        {
            string[] args = context.Param.Split(':');

            switch (args[0].ToLower())
            {
                case "roll":
                    RollMacro(context, args);
                    break;
            }
        }

        private void RollMacro(MacroContext context, string[] args)
        {
            // [roll:X] retrieves the Value of the Xth roll (first roll in the post is X=1). Can only retrieve values of past rolls.
            // [roll:X:Y] retrieves the Value of the Yth die on the Xth roll (actual die rolls only, aka normal/fudge/group). First die is Y=1.
            // [roll:X:critical] is the number of dice in the Xth roll that are critical.
            // [roll:X:Y:critical] is 1 if the Yth die is critical and 0 otherwise.
            // [roll:X:fumble] and [roll:X:Y:fumble] work the same way
            // as do [roll:X:success], [roll:X:Y:success], [roll:X:failure], and [roll:X:Y:failure]
            // for success/failure, it only counts number of successes or number of failures, returning an integer >= 0 for each. In other words,
            // [roll:X:success] doesn't deduct 1 whenever it sees a failure roll, unlike [roll:X] which will give successes - failures.
            // All other formulations of the macro are an error (which we pass down, as someone else may have their own roll macro which implements extended features)

            if (args.Length == 1)
            {
                return; // no X
            }


            if (!Int32.TryParse(args[1], out int rollIdx))
            {
                return; // invalid X
            }

            if (rollIdx < 0)
            {
                // negative X means we count backwards from the current post
                rollIdx += Current.Count;
            }
            else
            {
                rollIdx--; // make 0-based instead of 1-based
            }

            if (rollIdx < 0 || rollIdx >= Current.Count)
            {
                return; // X is too big or small
            }

            if (args.Length == 2)
            {
                // only have 2 args, return the value of the Xth roll
                context.Value = Current[rollIdx].Value;
                context.ValueType = Current[rollIdx].ResultType;
                return;
            }

            int nextIdx = 2;
            var allRolls = Current[rollIdx].Values.Where(d => d.IsLiveDie() && d.DieType.IsRoll()).ToList();
            if (Int32.TryParse(args[2], out int dieIdx))
            {
                dieIdx--;
                nextIdx = 3;
                if (dieIdx < 0 || dieIdx >= allRolls.Count)
                {
                    return; // Y is too big or small
                }

                if (args.Length == 3)
                {
                    // only have 3 args, X and Y. Return the value of the Yth roll
                    context.Value = allRolls[dieIdx].Value;
                    return;
                }
            }
            else
            {
                dieIdx = -1;
            }

            DieFlags flag;
            switch (args[nextIdx].ToLower())
            {
                case "critical":
                    flag = DieFlags.Critical;
                    break;
                case "fumble":
                    flag = DieFlags.Fumble;
                    break;
                case "success":
                    flag = DieFlags.Success;
                    break;
                case "failure":
                    flag = DieFlags.Failure;
                    break;
                default:
                    return; // unrecognized flag
            }

            if (dieIdx >= 0)
            {
                context.Value = allRolls[dieIdx].Flags.HasFlag(flag) ? 1 : 0;
            }
            else
            {
                context.Value = allRolls.Count(d => d.Flags.HasFlag(flag));
            }
        }
    }
}
