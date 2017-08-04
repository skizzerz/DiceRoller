using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        /// via AddRoll().
        /// </summary>
        public IReadOnlyList<RollResult> Current => _current;

        /// <summary>
        /// Mutable version of Current. Subclasses can modify the list via this method.
        /// </summary>
        protected IList<RollResult> CurrentList { get; private set; }

        /// <summary>
        /// For unit testing, to ensure serialization roundtrips correctly.
        /// </summary>
        internal int Diverged => _diverged;

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
            int version = info.GetInt32("_Version");
            PristineList = (RollResult[])info.GetValue("Pristine", typeof(RollResult[]));
            StoredList = (RollResult[])info.GetValue("Stored", typeof(RollResult[]));
            CurrentList = new List<RollResult>();

            if (version >= 2 && context.State != StreamingContextStates.Persistence)
            {
                // when deserializing persisted data, current/diverged should have default values
                CurrentList = (RollResult[])info.GetValue("Current", typeof(RollResult[]));
                _diverged = info.GetInt32("Diverged");
            }
        }

        /// <summary>
        /// Serializes binary data to the given stream. This will *not* roundtrip (when deserializing Current will be empty).
        /// This method should be called when serializing this object to the database, to ensure that it is deserialized in the correct state.
        /// </summary>
        /// <param name="serializationStream"></param>
        public void Serialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            formatter.Serialize(serializationStream, this);
        }

        /// <summary>
        /// Deserializes binary data from the given stream, that data must have been serialized via RollPost.Serialize().
        /// </summary>
        /// <param name="serializationStream"></param>
        /// <returns></returns>
        public static RollPost Deserialize(Stream serializationStream)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Persistence));
            return (RollPost)formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// Completes deserialization of the RollPost once the entire object graph has been deserialized.
        /// <para>This method should not be directly called.</para>
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnDeserialization(object sender)
        {
            _pristine = PristineList.ToList();
            _stored = StoredList.ToList();
            _current = CurrentList.ToList();
            PristineList = _pristine;
            StoredList = _stored;
            CurrentList = _current;
        }

        /// <summary>
        /// Serializes a RollPost.
        /// <para>This method should not be directly called, use Serialize() instead.</para>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            // in v2, we roundtrip this as-is
            info.AddValue("_Version", 2);
            info.AddValue("Pristine", _pristine.ToArray(), typeof(RollResult[]));
            info.AddValue("Stored", _stored.ToArray(), typeof(RollResult[]));
            info.AddValue("Current", _current.ToArray(), typeof(RollResult[]));
            info.AddValue("Diverged", _diverged);
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

            if (!config.MacroRegistry.Contains("roll"))
            {
                config.MacroRegistry.RegisterMacro("roll", RollMacro);
            }

            int nextIdx = Current.Count;
            AST.DiceAST ast = null;
            string normalizedExpr = null;
            var pristineVersion = _pristine.ElementAtOrDefault(nextIdx);
            var storedVersion = _stored.ElementAtOrDefault(nextIdx);

            Func<RollResult> DoRoll = () =>
            {
                // ensure that our custom macro handler is attached to handle PbP-specific macros (accessing previous rolls)
                // leading -= ensures that PostMacros is only attached once
                config.ExecuteMacro -= PostMacros;
                config.ExecuteMacro += PostMacros;
                var res = Roller.Roll(ast, config);
                config.ExecuteMacro -= PostMacros; // ensure we don't leave around our custom executor past this
                return res;
            };

            try
            {
                ast = Roller.Parse(diceExpr, config);
                normalizedExpr = ast.ToString();

                // as later rolls may have macros that refer to earlier ones, we don't want to break out into a new roll
                // and then re-use a previously-rolled value. If we do, those saved macros may be incorrect.
                // As such, when we detect a divergence (pristine->stored->new roll), we ensure that we don't go back to
                // a previous state (so no stored->pristine or new roll->stored).

                if (_diverged == 0 && normalizedExpr == pristineVersion?.Expression)
                {
                    // use the pristine result for this roll
                    _current.Add(pristineVersion);
                }
                else if (_diverged == 0 && pristineVersion?.Expression == RollResult.InvalidRoll.Expression)
                {
                    // pristine was previously invalid and we now have a valid roll, so roll what we have and adjust pristine
                    var roll = DoRoll();
                    _current.Add(roll);
                    _pristine[nextIdx] = roll;
                }
                else if (_diverged <= 1 && normalizedExpr == storedVersion?.Expression)
                {
                    _current.Add(storedVersion);
                    _diverged = 1;
                }
                else if (_diverged <= 1 && storedVersion?.Expression == RollResult.InvalidRoll.Expression)
                {
                    var roll = DoRoll();
                    _current.Add(roll);
                    _stored[nextIdx] = roll;
                    _diverged = 1;
                }
                else
                {
                    // none of the versions for the current index matched, so reroll it
                    _current.Add(DoRoll());
                    _diverged = 2;
                }
            }
            catch (DiceException)
            {
                // if this roll does not parse or had a roll error, we want to still save that fact so if the user later edits their post
                // to fix the roll, it does not invalidate any rolls which happen afterwards. Additionally, if a user
                // edits their post to make a roll invalid, we want to catch that and react accordingly.
                normalizedExpr = RollResult.InvalidRoll.Expression;

                if (_diverged == 0 && normalizedExpr == pristineVersion?.Expression)
                {
                    // pristine was invalid as well, so this expression was never fixed
                    _current.Add(pristineVersion);
                }
                else if (_diverged <= 1 && normalizedExpr == storedVersion?.Expression)
                {
                    _current.Add(storedVersion);
                    _diverged = 1;
                }
                else
                {
                    // brand new invalid roll, or modified a previously-good roll to be invalid
                    _current.Add(RollResult.InvalidRoll);
                    _diverged = 2;
                }

                // re-throw our error so it can be shownt to the user
                throw;
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
            // update _stored
            _stored = _current;

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

        private void RollMacro(MacroContext context)
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

            var args = context.Arguments;

            if (args.Count == 1)
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

            if (args.Count == 2)
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

                if (args.Count == 3)
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
