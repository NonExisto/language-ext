﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.ClassInstances;
using static LanguageExt.Prelude;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace LanguageExt;

/// <summary>
/// Software transactional memory using Multi-Version Concurrency Control (MVCC)
/// </summary>
/// <remarks>
/// See the [concurrency section](https://github.com/louthy/language-ext/wiki/Concurrency) of the wiki for more info.
/// </remarks>
public static class STM
{
    static long refIdNext;
    static readonly AtomHashMap<long, RefState> state = AtomHashMap<long, RefState>(Traits.Eq.Comparer<EqLong, long>());
    static readonly AsyncLocal<Transaction?> transaction= new();

    static void OnChange(TrieMap<long, Change<RefState>> patch) 
    {
        foreach (var (_Key, Value) in patch)
        {
            if (Value is EntryMappedTo<RefState> update)
            {
                update.To.OnChange(update.To.UntypedValue);
            }
        }
    }

    /// <summary>
    /// Generates a new reference that can be used within a sync transaction
    /// </summary>
    internal static Ref<A> NewRef<A>(A value, Func<A, bool>? validator = null)
    {
        var id = Interlocked.Increment(ref refIdNext);
        var r = new Ref<A>(id);
        var v = new RefState<A>(0, value, validator, r);
        _ = state.Add(id, v, null);
        return r;
    }
        
    /// <summary>
    /// Run the op within a new transaction
    /// If a transaction is already running, then this becomes part of the parent transaction
    /// </summary>
    internal static R DoTransaction<R>(Func<R> op, Isolation isolation) =>
        transaction.Value == null
            ? RunTransaction(op, isolation)
            : op();
        

    /// <summary>
    /// Runs the transaction
    /// </summary>
    static R RunTransaction<R>(Func<R> op, Isolation isolation)
    {
        SpinWait sw = default;
        while (true)
        {
            // Create a new transaction with a snapshot of the current state
            var t = new Transaction(state.Items);
            transaction.Value = t;
            try
            {
                // Try to do the operations of the transaction
                return ValidateAndCommit(t, isolation, op(), long.MinValue);
            }
            catch (ConflictException)
            {
                // Conflict found, so retry
            }
            finally
            {
                // Clear the current transaction on the way out
                transaction.Value = null;
                    
                // Announce changes
                OnChange(t.changes);
            }
            // Wait one tick before trying again
            sw.SpinOnce();
        }
    }

    /// <summary>
    /// Runs the transaction
    /// </summary>
    static Eff<RT, R> RunTransaction<RT, R>(Eff<RT, R> op, Isolation isolation) =>
        getState<RT>().Bind(
            sta =>
                Eff<RT, R>.Lift(
                    env =>
                    {
                        SpinWait sw = default;
                        while (true)
                        {
                            // Create a new transaction with a snapshot of the current state
                            var t = new Transaction(state.Items);
                            transaction.Value = t;
                            try
                            {
                                // Try to do the operations of the transaction
                                var res = op.Run(env, sta.EnvIO);
                                return res.IsFail
                                           ? res
                                           : ValidateAndCommit(t, isolation, res.SuccValue, Int64.MinValue);
                            }
                            catch (ConflictException)
                            {
                                // Conflict found, so retry
                            }
                            finally
                            {
                                // Clear the current transaction on the way out
                                transaction.Value = null;

                                // Announce changes
                                OnChange(t.changes);
                            }

                            // Wait one tick before trying again
                            sw.SpinOnce();
                        }
                    }));

    /// <summary>
    /// Runs the transaction
    /// </summary>
    static Eff<R> RunTransaction<R>(Eff<R> op, Isolation isolation) =>
        lift(() =>
        {
            SpinWait sw = default;
            while (true)
            {
                // Create a new transaction with a snapshot of the current state
                var t = new Transaction(state.Items);
                transaction.Value = t;
                try
                {
                    // Try to do the operations of the transaction
                    var res = op.Run();
                    return res.IsFail 
                               ? res 
                               : ValidateAndCommit(t, isolation, res.SuccValue, Int64.MinValue);
                }
                catch (ConflictException)
                {
                    // Conflict found, so retry
                }
                finally
                {
                    // Clear the current transaction on the way out
                    transaction.Value = null;
                    
                    // Announce changes
                    OnChange(t.changes);
                }

                // Wait one tick before trying again
                sw.SpinOnce();
            }
        });
        
    /// <summary>
    /// Runs the transaction
    /// </summary>
    static async ValueTask<R> RunTransaction<R>(Func<ValueTask<R>> op, Isolation isolation)
    {
        SpinWait sw = default;
        while (true)
        {
            // Create a new transaction with a snapshot of the current state
            var t = new Transaction(state.Items);
            transaction.Value = t;
            try
            {
                // Try to do the operations of the transaction
                return ValidateAndCommit(t, isolation, await op().ConfigureAwait(false), long.MinValue);
            }
            catch (ConflictException)
            {
                // Conflict found, so retry
            }
            finally
            {
                // Clear the current transaction on the way out
                transaction.Value = null;
                    
                // Announce changes
                OnChange(t.changes);
            }
            // Wait one tick before trying again
            sw.SpinOnce();
        }
    }

    /// <summary>
    /// Runs the transaction
    /// </summary>
    static R RunTransaction<R>(Func<CommuteRef<R>> op, Isolation isolation)
    {
        SpinWait sw = default;
        while (true)
        {
            // Create a new transaction with a snapshot of the current state
            var t = new Transaction(state.Items);
            transaction.Value = t;
            try
            {
                var cref = op();

                // Try to do the operations of the transaction
                return ValidateAndCommit(t, isolation, (R)t.state[cref.Ref.Id].UntypedValue, cref.Ref.Id);
            }
            catch (ConflictException)
            {
                // Conflict found, so retry
            }
            finally
            {
                // Clear the current transaction on the way out
                transaction.Value = null;
                    
                // Announce changes
                OnChange(t.changes);
            }
            // Spin, backing off, then yield the thread to avoid deadlock 
            sw.SpinOnce();
        }
    }

    static R ValidateAndCommit<R>(Transaction t, Isolation isolation, R result, long returnRefId)
    {
        // No writing, so no validation or commit needed
        var writes = t.writes.Count;
        var commutes = t.commutes.Count;

        var anyWrites = writes     > 0;
        var anyCommutes = commutes > 0;
            
        if (!anyWrites && !anyCommutes)
        {
            return result;
        }

        // Attempt to apply the changes atomically
        _ = state.SwapInternal(s =>
        {
            if (isolation == Isolation.Serialisable)
            {
                ValidateReads(t, s);
            }

            s = anyWrites
                    ? CommitWrites(t, s)
                    : s;
            (s, result) = anyCommutes ? CommitCommutes(t, s, returnRefId, result) : (s, result);
            return s;
        });

        // Changes applied successfully
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ValidateReads(Transaction t, TrieMap<long, RefState> s)
    {
        var tlocal = t;
        var slocal = tlocal.state;
 
        // Check if something else wrote to what we were reading
        foreach (var read in tlocal.reads)
        {
            if (s[read].Version != slocal[read].Version)
            {
                throw new ConflictException();
            }
        }
    }

    static TrieMap<long, RefState> CommitWrites(Transaction t, TrieMap<long, RefState> s)
    {
        // Check if something else wrote to what we were writing
        var tlocal = t;
        var slocal = tlocal.state;
            
        foreach (var write in tlocal.writes)
        {
            var newState = slocal[write];

            if (!newState.Validate(newState))
            {
                throw new RefValidationFailedException();
            }

            s = s[write].Version == newState.Version ? s.SetItem(write, newState.Inc()) : throw new ConflictException();
        }

        return s;
    }

    static (TrieMap<long, RefState>, R) CommitCommutes<R>(Transaction t, TrieMap<long, RefState> s, long returnRefId, R result)
    {
        // Run the commutative operations
        foreach (var commute in t.commutes)
        {
            var exist = s[commute.Id];

            // Re-run the commute function with what's live now
            var nver = exist.MapAndInc(commute.Fun);

            // Validate the result
            if (!nver.Validate(nver))
            {
                throw new RefValidationFailedException();
            }

            // Save to live state
            s = s.SetItem(commute.Id, nver);

            // If it matches our return type, then make it the result
            if (returnRefId == commute.Id)
            {
                result = (R)nver.UntypedValue;
            }
        }

        return (s, result);
    }

    /// <summary>
    /// Read the value for the reference ID provided
    /// If within a transaction then the in-transaction value is returned, otherwise it's
    /// the current latest value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static object Read(long id) =>
        transaction.Value == null
            ? state.Items[id].UntypedValue
            : transaction.Value.ReadValue(id);

    /// <summary>
    /// Write the value for the reference ID provided
    /// Must be run within a transaction
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(long id, object value)
    {
        if (transaction.Value == null)
        {
            throw new InvalidOperationException("Refs can only be written to from within a `sync` transaction");
        }
        transaction.Value.WriteValue(id, value);
    }

    /// <summary>
    /// Must be called in a transaction. Sets the in-transaction-value of
    /// ref to:  
    /// 
    ///     `f(in-transaction-value-of-ref)`
    ///     
    /// and returns the in-transaction-value when complete.
    /// 
    /// At the commit point of the transaction, `f` is run *AGAIN* with the
    /// most recently committed value:
    /// 
    ///     `f(most-recently-committed-value-of-ref)`
    /// 
    /// Thus `f` should be commutative, or, failing that, you must accept
    /// last-one-in-wins behavior.
    /// 
    /// Commute allows for more concurrency than just setting the items
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static A Commute<A>(long id, Func<A, A> f)
    {
        if (transaction.Value == null)
        {
            throw new InvalidOperationException("Refs can only commute from within a transaction");
        }
        return (A)transaction.Value.Commute(id, CastCommute(f));
    }

    /// <summary>
    /// Make sure Refs are cleaned up
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Finalise(long id) =>
        state.Remove(id);

    /// <summary>
    /// Conflict exception for internal use
    /// </summary>
    sealed class ConflictException : Exception;

    /// <summary>
    /// Wraps a (A -> A) predicate as (object -> object)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Func<object, object> CastCommute<A>(Func<A, A> f) =>
        obj => f((A)obj)!;

    /// <summary>
    /// Get the currently running TransactionId
    /// </summary>
    public static long TransactionId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => transaction.Value?.transactionId ?? throw new InvalidOperationException("Transaction not running");
    }

    /// <summary>
    /// Transaction snapshot
    /// </summary>
    sealed class Transaction
    {
        static long transactionIdNext;
        public readonly long transactionId;
        public TrieMap<long, RefState> state;
        public TrieMap<long, Change<RefState>> changes;
        public readonly System.Collections.Generic.HashSet<long> reads = new();
        public readonly System.Collections.Generic.HashSet<long> writes = new();
        public readonly System.Collections.Generic.List<(long Id, Func<object, object> Fun)> commutes = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transaction(TrieMap<long, RefState> state)
        {
            this.state    = state;
            changes       = TrieMap<long, Change<RefState>>.Empty(Traits.Eq.Comparer<EqLong, long>());
            transactionId = Interlocked.Increment(ref transactionIdNext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadValue(long id)
        {
            _ = reads.Add(id);
            return state[id].UntypedValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long id, object value)
        {
            var oldState = state[id];
            var newState = oldState.SetValue(value);
            state = state.SetItem(id, newState);
            _ = writes.Add(id);
            var change = changes.Find(id);
            if (change.IsSome)
            {
                var last = (EntryMapped<RefState, RefState>)change.Value!;
                changes = changes.AddOrUpdateInPlace(id, Change<RefState>.Mapped(last.From, newState));
            }
            else
            {
                changes = changes.AddOrUpdateInPlace(id, Change<RefState>.Mapped(oldState, newState));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Commute(long id, Func<object, object> f)
        {
            var oldState = state[id];
            var newState = oldState.Map(f);
            state = state.SetItem(id, newState);
            commutes.Add((id, f));
            return newState.UntypedValue;
        }
    }

    /// <summary>
    /// The state of a Ref
    /// Includes the value and the version
    /// </summary>
    abstract record RefState(long Version)
    {
        public abstract bool Validate(RefState refState);
        public abstract RefState SetValue(object value);
        public abstract RefState SetValueAndInc(object value);
        public abstract RefState Inc();
        public abstract RefState Map(Func<object, object> f);
        public abstract RefState MapAndInc(Func<object, object> f);
        public abstract void OnChange(object value);
        public abstract object UntypedValue { get; }
    }

    sealed record RefState<A>(long Version, A Value, Func<A, bool>? Validator, Ref<A> Ref) : RefState(Version)
    {
        public override bool Validate(RefState refState) =>
            Validator?.Invoke(((RefState<A>)refState).Value) ?? true;

        public override RefState SetValue(object value) =>
            this with {Value = (A)value};

        public override RefState SetValueAndInc(object value) =>
            this with {Version = Version + 1,Value = (A)value};

        public override RefState Inc() =>
            this with {Version = Version + 1};

        public override RefState Map(Func<object, object> f) =>
            this with {Value = (A)f(Value!)};

        public override RefState MapAndInc(Func<object, object> f) =>
            this with {Version = Version + 1, Value = (A)f(Value!)};

        public override void OnChange(object value) =>
            Ref.OnChange((A)value);
            
        public override object UntypedValue =>
            Value!;
    }
}
