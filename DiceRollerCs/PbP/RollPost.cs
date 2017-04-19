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
        /// Contains the most recent saved version of the post. All rolls in the post are represented here in-order so that their results may be
        /// directly used rather than needing to re-evaluate the dice expression each time the post is viewed/previewed/edited.
        /// </summary>
        public IReadOnlyList<RollResult> Stored => _stored;

        /// <summary>
        /// Contains the current version of the post (the one being checked). This starts out empty even when deserializing, and is added to
        /// via AddRoll(). The TamperWarning flag is set based on the value of this.
        /// </summary>
        public IReadOnlyList<RollResult> Current => _current;

        /// <summary>
        /// Constructs a new, empty RollPost. This represents a new post being made. If editing an existing post,
        /// the RollPost should be constructed via deserializing the old RollPost stored in the database or other storage medium.
        /// </summary>
        public RollPost() { }

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
            _stored = ((RollResult[])info.GetValue("Stored", typeof(RollResult[]))).ToList();
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
        /// Adds a new roll to the post. This is checked against the pristine copy. If it does not cause a warning
        /// (e.g. it is adding a new roll at the end), the roll is added to the pristine copy as well. If it does
        /// cause a warning, it is only added to Current and the TamperWarning flag is set to true.
        /// <para>A roll causes a warning if Current does not begin with all of the elements of Pristine in-order.
        /// This could happen if a previous roll was edited or deleted, or if all the rolls just haven't been added yet.</para>
        /// </summary>
        /// <param name="diceExpr">The dice expression to add, will be evaluated then added to the end of Current</param>
        /// <param name="config">The configuration used for the roll. If null, RollerConfig.Default is used</param>
        public void AddRoll(string diceExpr, RollerConfig config = null)
        {
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
            // [roll:X:Y] retrieves the Value of the Yth die on the Xth roll (actual die rolls only, aka normal/fudge). First die is Y=1.
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

            rollIdx--; // make 0-based instead of 1-based
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

            int dieIdx = -1;
            int nextIdx = 2;
            List<DieResult> allRolls = null;
            if (Int32.TryParse(args[2], out dieIdx))
            {
                dieIdx--;
                nextIdx = 3;
                allRolls = Current[rollIdx].Values.Where(d => d.IsLiveDie() && d.DieType.IsRoll()).ToList();
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

            #error FINISH THIS
            switch (args[nextIdx].ToLower())
            {
                case "critical":
                    break;
                case "fumble":
                    break;
                case "success":
                    break;
                case "failure":
                    break;
            }
        }
    }
}
