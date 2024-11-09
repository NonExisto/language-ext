namespace LanguageExt
{
    /// <summary>
    /// Event sent from the `AtomHashMap` type whenever an operation successfully modifies the underlying data 
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    public delegate void AtomHashMapChangeEvent<K, V>(HashMapPatch<K, V> Patch);
}
